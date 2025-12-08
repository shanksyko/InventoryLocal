using System;
using System.Collections.Generic;
using System.Data.Odbc;
using InventarioSistem.Core.Devices;
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
            (Modelo, Numero, Proprietario, Imeis)
            VALUES (?, ?, ?, ?)
        ";

        command.Parameters.Add("Modelo", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Modelo);
        command.Parameters.Add("Numero", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Numero);
        command.Parameters.Add("Proprietario", OdbcType.VarChar).Value = DeviceStringUtils.NormalizeString(celular.Proprietario);
        command.Parameters.Add("Imeis", OdbcType.VarChar).Value = DeviceStringUtils.ImeisToString(celular.Imeis);

        command.ExecuteNonQuery();
    }

    public List<Celular> GetAllCelulares()
    {
        var celulares = new List<Celular>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Modelo, Numero, Proprietario, Imeis FROM Celulares";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var imeisRaw = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);

            celulares.Add(new Celular
            {
                Id = reader.GetInt32(0),
                Modelo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Numero = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Proprietario = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Imeis = DeviceStringUtils.ImeisFromString(imeisRaw)
            });
        }

        return celulares;
    }
}
