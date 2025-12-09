using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Logging;
using InventarioSistem.Core.Utils;

namespace InventarioSistem.Access;

public partial class AccessInventoryStore
{
    //
    //  COMPUTADORES
    //
    public void AddComputer(Computer computer)
    {
        ArgumentNullException.ThrowIfNull(computer);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Computadores
            (Host, SerialNumber, Proprietario, Departamento, Matricula)
            VALUES (?, ?, ?, ?, ?)
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.SerialNumber);
        command.Parameters.Add("Proprietario", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Proprietario);
        command.Parameters.Add("Departamento", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Departamento);
        command.Parameters.Add("Matricula", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Matricula);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Computador inserido: Host='{computer.Host}', NS='{computer.SerialNumber}', Proprietario='{computer.Proprietario}', Departamento='{computer.Departamento}', Matricula='{computer.Matricula}'");
    }

    public List<Computer> GetAllComputers()
    {
        var computers = new List<Computer>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Host, SerialNumber, Proprietario, Departamento, Matricula FROM Computadores";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            computers.Add(new Computer
            {
                Id = reader.GetInt32(0),
                Host = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                SerialNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Proprietario = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Departamento = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Matricula = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            });
        }

        return computers;
    }

    public void UpdateComputer(Computer computer)
    {
        ArgumentNullException.ThrowIfNull(computer);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Computadores
            SET Host = ?, SerialNumber = ?, Proprietario = ?, Departamento = ?, Matricula = ?
            WHERE Id = ?
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.SerialNumber);
        command.Parameters.Add("Proprietario", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Proprietario);
        command.Parameters.Add("Departamento", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Departamento);
        command.Parameters.Add("Matricula", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(computer.Matricula);
        command.Parameters.Add("Id", OdbcType.Int).Value = computer.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Computador atualizado (Id={computer.Id}): Host='{computer.Host}', NS='{computer.SerialNumber}', Proprietario='{computer.Proprietario}', Departamento='{computer.Departamento}', Matricula='{computer.Matricula}'");
    }

    //
    //  TABLETS
    //
    public void AddTablet(Tablet tablet)
    {
        ArgumentNullException.ThrowIfNull(tablet);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Tablets
            (Host, SerialNumber, Local, Responsavel, Imeis)
            VALUES (?, ?, ?, ?, ?)
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.SerialNumber);
        command.Parameters.Add("Local", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Local);
        command.Parameters.Add("Responsavel", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Responsavel);
        command.Parameters.Add("Imeis", OdbcType.VarChar).Value = DeviceStringUtils.ImeisToString(tablet.Imeis);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Tablet inserido: Host='{tablet.Host}', NS='{tablet.SerialNumber}', Local='{tablet.Local}', Responsavel='{tablet.Responsavel}', IMEIs='{string.Join(";", tablet.Imeis ?? new())}'");
    }

    public List<Tablet> GetAllTablets()
    {
        var tablets = new List<Tablet>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Host, SerialNumber, Local, Responsavel, Imeis FROM Tablets";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var imeisRaw = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);

            tablets.Add(new Tablet
            {
                Id = reader.GetInt32(0),
                Host = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                SerialNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Local = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Responsavel = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Imeis = DeviceStringUtils.ImeisFromString(imeisRaw)
            });
        }

        return tablets;
    }

    public void UpdateTablet(Tablet tablet)
    {
        ArgumentNullException.ThrowIfNull(tablet);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Tablets
            SET Host = ?, SerialNumber = ?, Local = ?, Responsavel = ?, Imeis = ?
            WHERE Id = ?
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.SerialNumber);
        command.Parameters.Add("Local", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Local);
        command.Parameters.Add("Responsavel", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(tablet.Responsavel);
        command.Parameters.Add("Imeis", OdbcType.VarChar).Value = DeviceStringUtils.ImeisToString(tablet.Imeis);
        command.Parameters.Add("Id", OdbcType.Int).Value = tablet.Id;

        command.ExecuteNonQuery();

        var imeisText = string.Join(";", tablet.Imeis ?? new());
        InventoryLogger.Info("AccessInventoryStore",
            $"Tablet atualizado (Id={tablet.Id}): Host='{tablet.Host}', NS='{tablet.SerialNumber}', Local='{tablet.Local}', Responsavel='{tablet.Responsavel}', IMEIs='{imeisText}'");
    }

    //
    //  COLETORES ANDROID
    //
    public void AddColetor(ColetorAndroid coletor)
    {
        ArgumentNullException.ThrowIfNull(coletor);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO ColetoresAndroid
            (Host, SerialNumber, MacAddress, IpAddress, Local)
            VALUES (?, ?, ?, ?, ?)
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.SerialNumber);
        command.Parameters.Add("MacAddress", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.MacAddress);
        command.Parameters.Add("IpAddress", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.IpAddress);
        command.Parameters.Add("Local", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.Local);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Coletor inserido: Host='{coletor.Host}', NS='{coletor.SerialNumber}', MAC='{coletor.MacAddress}', IP='{coletor.IpAddress}', Local='{coletor.Local}'");
    }

    public List<ColetorAndroid> GetAllColetores()
    {
        var coletores = new List<ColetorAndroid>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Host, SerialNumber, MacAddress, IpAddress, Local FROM ColetoresAndroid";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            coletores.Add(new ColetorAndroid
            {
                Id = reader.GetInt32(0),
                Host = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                SerialNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                MacAddress = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                IpAddress = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Local = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            });
        }

        return coletores;
    }

    public void UpdateColetor(ColetorAndroid coletor)
    {
        ArgumentNullException.ThrowIfNull(coletor);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE ColetoresAndroid
            SET Host = ?, SerialNumber = ?, MacAddress = ?, IpAddress = ?, Local = ?
            WHERE Id = ?
        ";

        command.Parameters.Add("Host", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.Host);
        command.Parameters.Add("SerialNumber", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.SerialNumber);
        command.Parameters.Add("MacAddress", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.MacAddress);
        command.Parameters.Add("IpAddress", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.IpAddress);
        command.Parameters.Add("Local", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(coletor.Local);
        command.Parameters.Add("Id", OdbcType.Int).Value = coletor.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Coletor atualizado (Id={coletor.Id}): Host='{coletor.Host}', NS='{coletor.SerialNumber}', MAC='{coletor.MacAddress}', IP='{coletor.IpAddress}', Local='{coletor.Local}'");
    }

    //
    //  CELULARES
    //
    public void AddCelular(Celular celular)
    {
        ArgumentNullException.ThrowIfNull(celular);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Celulares
            (Hostname, Modelo, Numero, Proprietario, Imei1, Imei2)
            VALUES (?, ?, ?, ?, ?, ?)
        ";

        command.Parameters.Add("Hostname", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Hostname);
        command.Parameters.Add("Modelo", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Modelo);
        command.Parameters.Add("Numero", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Numero);
        command.Parameters.Add("Proprietario", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Proprietario);
        command.Parameters.Add("Imei1", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Imei1);
        command.Parameters.Add("Imei2", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Imei2);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Celular inserido: Host='{celular.Hostname}', Modelo='{celular.Modelo}', Numero='{celular.Numero}', Proprietario='{celular.Proprietario}', IMEI1='{celular.Imei1}', IMEI2='{celular.Imei2}'");
    }

    public List<Celular> GetAllCelulares()
    {
        var celulares = new List<Celular>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Hostname, Modelo, Numero, Proprietario, Imei1, Imei2 FROM Celulares";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            celulares.Add(new Celular
            {
                Id = reader.GetInt32(0),
                Hostname = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Modelo = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Numero = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Proprietario = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Imei1 = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Imei2 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }

        return celulares;
    }

    public void UpdateCelular(Celular celular)
    {
        ArgumentNullException.ThrowIfNull(celular);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Celulares
            SET Hostname = ?, Modelo = ?, Numero = ?, Proprietario = ?, Imei1 = ?, Imei2 = ?
            WHERE Id = ?
        ";

        command.Parameters.Add("Hostname", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Hostname);
        command.Parameters.Add("Modelo", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Modelo);
        command.Parameters.Add("Numero", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Numero);
        command.Parameters.Add("Proprietario", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Proprietario);
        command.Parameters.Add("Imei1", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Imei1);
        command.Parameters.Add("Imei2", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Imei2);
        command.Parameters.Add("Id", OdbcType.Int).Value = celular.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Celular atualizado (Id={celular.Id}): Host='{celular.Hostname}', Modelo='{celular.Modelo}', Numero='{celular.Numero}', Proprietario='{celular.Proprietario}', IMEI1='{celular.Imei1}', IMEI2='{celular.Imei2}'");
    }
}
