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

        AddTextParameter(command, "Host", computer.Host);
        AddTextParameter(command, "SerialNumber", computer.SerialNumber);
        AddTextParameter(command, "Proprietario", computer.Proprietario);
        AddTextParameter(command, "Departamento", computer.Departamento);
        AddTextParameter(command, "Matricula", computer.Matricula);

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
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Proprietario = GetStringSafe(reader, 3),
                Departamento = GetStringSafe(reader, 4),
                Matricula = GetStringSafe(reader, 5)
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

        AddTextParameter(command, "Host", computer.Host);
        AddTextParameter(command, "SerialNumber", computer.SerialNumber);
        AddTextParameter(command, "Proprietario", computer.Proprietario);
        AddTextParameter(command, "Departamento", computer.Departamento);
        AddTextParameter(command, "Matricula", computer.Matricula);
        command.Parameters.Add("Id", OdbcType.Int).Value = computer.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Computador atualizado (Id={computer.Id}): Host='{computer.Host}', NS='{computer.SerialNumber}', Proprietario='{computer.Proprietario}', Departamento='{computer.Departamento}', Matricula='{computer.Matricula}'");
    }

    public void DeleteComputer(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Computadores WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Computador excluído (Id={id}).");
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

        AddTextParameter(command, "Host", tablet.Host);
        AddTextParameter(command, "SerialNumber", tablet.SerialNumber);
        AddTextParameter(command, "Local", tablet.Local);
        AddTextParameter(command, "Responsavel", tablet.Responsavel);
        AddTextParameter(command, "Imeis", DeviceStringUtils.ImeisToString(tablet.Imeis));

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
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                Responsavel = GetStringSafe(reader, 4),
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

        AddTextParameter(command, "Host", tablet.Host);
        AddTextParameter(command, "SerialNumber", tablet.SerialNumber);
        AddTextParameter(command, "Local", tablet.Local);
        AddTextParameter(command, "Responsavel", tablet.Responsavel);
        AddTextParameter(command, "Imeis", DeviceStringUtils.ImeisToString(tablet.Imeis));
        command.Parameters.Add("Id", OdbcType.Int).Value = tablet.Id;

        command.ExecuteNonQuery();

        var imeisText = string.Join(";", tablet.Imeis ?? new());
        InventoryLogger.Info("AccessInventoryStore",
            $"Tablet atualizado (Id={tablet.Id}): Host='{tablet.Host}', NS='{tablet.SerialNumber}', Local='{tablet.Local}', Responsavel='{tablet.Responsavel}', IMEIs='{imeisText}'");
    }

    public void DeleteTablet(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Tablets WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Tablet excluído (Id={id}).");
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

        AddTextParameter(command, "Host", coletor.Host);
        AddTextParameter(command, "SerialNumber", coletor.SerialNumber);
        AddTextParameter(command, "MacAddress", coletor.MacAddress);
        AddTextParameter(command, "IpAddress", coletor.IpAddress);
        AddTextParameter(command, "Local", coletor.Local);

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
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                MacAddress = GetStringSafe(reader, 3),
                IpAddress = GetStringSafe(reader, 4),
                Local = GetStringSafe(reader, 5)
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

        AddTextParameter(command, "Host", coletor.Host);
        AddTextParameter(command, "SerialNumber", coletor.SerialNumber);
        AddTextParameter(command, "MacAddress", coletor.MacAddress);
        AddTextParameter(command, "IpAddress", coletor.IpAddress);
        AddTextParameter(command, "Local", coletor.Local);
        command.Parameters.Add("Id", OdbcType.Int).Value = coletor.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Coletor atualizado (Id={coletor.Id}): Host='{coletor.Host}', NS='{coletor.SerialNumber}', MAC='{coletor.MacAddress}', IP='{coletor.IpAddress}', Local='{coletor.Local}'");
    }

    public void DeleteColetor(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM ColetoresAndroid WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Coletor excluído (Id={id}).");
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

        AddTextParameter(command, "Hostname", celular.Hostname);
        AddTextParameter(command, "Modelo", celular.Modelo);
        AddTextParameter(command, "Numero", celular.Numero);
        AddTextParameter(command, "Proprietario", celular.Proprietario);
        AddTextParameter(command, "Imei1", celular.Imei1);
        AddTextParameter(command, "Imei2", celular.Imei2);

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
                Hostname = GetStringSafe(reader, 1),
                Modelo = GetStringSafe(reader, 2),
                Numero = GetStringSafe(reader, 3),
                Proprietario = GetStringSafe(reader, 4),
                Imei1 = GetStringSafe(reader, 5),
                Imei2 = GetStringSafe(reader, 6)
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

        AddTextParameter(command, "Hostname", celular.Hostname);
        AddTextParameter(command, "Modelo", celular.Modelo);
        AddTextParameter(command, "Numero", celular.Numero);
        AddTextParameter(command, "Proprietario", celular.Proprietario);
        AddTextParameter(command, "Imei1", celular.Imei1);
        AddTextParameter(command, "Imei2", celular.Imei2);
        command.Parameters.Add("Id", OdbcType.Int).Value = celular.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Celular atualizado (Id={celular.Id}): Host='{celular.Hostname}', Modelo='{celular.Modelo}', Numero='{celular.Numero}', Proprietario='{celular.Proprietario}', IMEI1='{celular.Imei1}', IMEI2='{celular.Imei2}'");
    }

    public void DeleteCelular(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Celulares WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Celular excluído (Id={id}).");
    }

    //
    //  IMPRESSORAS
    //
    public void AddImpressora(Impressora impressora)
    {
        ArgumentNullException.ThrowIfNull(impressora);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Impressoras
            (Hostname, Modelo, NumeroSerie, Local, Responsavel)
            VALUES (?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Hostname", impressora.Hostname);
        AddTextParameter(command, "Modelo", impressora.Modelo);
        AddTextParameter(command, "NumeroSerie", impressora.NumeroSerie);
        AddTextParameter(command, "Local", impressora.Local);
        AddTextParameter(command, "Responsavel", impressora.Responsavel);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Impressora inserida: Host='{impressora.Hostname}', Modelo='{impressora.Modelo}', NS='{impressora.NumeroSerie}', Local='{impressora.Local}', Resp='{impressora.Responsavel}'");
    }

    public List<Impressora> GetAllImpressoras()
    {
        var impressoras = new List<Impressora>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Hostname, Modelo, NumeroSerie, Local, Responsavel FROM Impressoras";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            impressoras.Add(new Impressora
            {
                Id = reader.GetInt32(0),
                Hostname = GetStringSafe(reader, 1),
                Modelo = GetStringSafe(reader, 2),
                NumeroSerie = GetStringSafe(reader, 3),
                Local = GetStringSafe(reader, 4),
                Responsavel = GetStringSafe(reader, 5)
            });
        }

        return impressoras;
    }

    public void UpdateImpressora(Impressora impressora)
    {
        ArgumentNullException.ThrowIfNull(impressora);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Impressoras
            SET Hostname = ?, Modelo = ?, NumeroSerie = ?, Local = ?, Responsavel = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Hostname", impressora.Hostname);
        AddTextParameter(command, "Modelo", impressora.Modelo);
        AddTextParameter(command, "NumeroSerie", impressora.NumeroSerie);
        AddTextParameter(command, "Local", impressora.Local);
        AddTextParameter(command, "Responsavel", impressora.Responsavel);
        command.Parameters.Add("Id", OdbcType.Int).Value = impressora.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Impressora atualizada (Id={impressora.Id}): Host='{impressora.Hostname}', Modelo='{impressora.Modelo}', NS='{impressora.NumeroSerie}', Local='{impressora.Local}', Resp='{impressora.Responsavel}'");
    }

    public void DeleteImpressora(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Impressoras WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Impressora excluída (Id={id}).");
    }

    //
    //  DECT
    //
    public void AddDect(DectPhone dect)
    {
        ArgumentNullException.ThrowIfNull(dect);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Dects
            (Hostname, NumeroSerie, Ramal, Responsavel)
            VALUES (?, ?, ?, ?)
        ";

        AddTextParameter(command, "Hostname", dect.Hostname);
        AddTextParameter(command, "NumeroSerie", dect.NumeroSerie);
        AddTextParameter(command, "Ramal", dect.Ramal);
        AddTextParameter(command, "Responsavel", dect.Responsavel);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"DECT inserido: Host='{dect.Hostname}', Ramal='{dect.Ramal}', NS='{dect.NumeroSerie}', Resp='{dect.Responsavel}'");
    }

    public List<DectPhone> GetAllDects()
    {
        var dects = new List<DectPhone>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Hostname, NumeroSerie, Ramal, Responsavel FROM Dects";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            dects.Add(new DectPhone
            {
                Id = reader.GetInt32(0),
                Hostname = GetStringSafe(reader, 1),
                NumeroSerie = GetStringSafe(reader, 2),
                Ramal = GetStringSafe(reader, 3),
                Responsavel = GetStringSafe(reader, 4)
            });
        }

        return dects;
    }

    public void UpdateDect(DectPhone dect)
    {
        ArgumentNullException.ThrowIfNull(dect);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Dects
            SET Hostname = ?, NumeroSerie = ?, Ramal = ?, Responsavel = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Hostname", dect.Hostname);
        AddTextParameter(command, "NumeroSerie", dect.NumeroSerie);
        AddTextParameter(command, "Ramal", dect.Ramal);
        AddTextParameter(command, "Responsavel", dect.Responsavel);
        command.Parameters.Add("Id", OdbcType.Int).Value = dect.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"DECT atualizado (Id={dect.Id}): Host='{dect.Hostname}', Ramal='{dect.Ramal}', NS='{dect.NumeroSerie}', Resp='{dect.Responsavel}'");
    }

    public void DeleteDect(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Dects WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"DECT excluído (Id={id}).");
    }

    //
    //  TELEFONES CISCO
    //
    public void AddTelefoneCisco(CiscoPhone phone)
    {
        ArgumentNullException.ThrowIfNull(phone);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO TelefonesCisco
            (Hostname, MacAddress, IpAddress, Ramal, Responsavel)
            VALUES (?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Hostname", phone.Hostname);
        AddTextParameter(command, "MacAddress", phone.MacAddress);
        AddTextParameter(command, "IpAddress", phone.IpAddress);
        AddTextParameter(command, "Ramal", phone.Ramal);
        AddTextParameter(command, "Responsavel", phone.Responsavel);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Telefone Cisco inserido: Host='{phone.Hostname}', MAC='{phone.MacAddress}', IP='{phone.IpAddress}', Ramal='{phone.Ramal}', Resp='{phone.Responsavel}'");
    }

    public List<CiscoPhone> GetAllTelefonesCisco()
    {
        var telefones = new List<CiscoPhone>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Hostname, MacAddress, IpAddress, Ramal, Responsavel FROM TelefonesCisco";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            telefones.Add(new CiscoPhone
            {
                Id = reader.GetInt32(0),
                Hostname = GetStringSafe(reader, 1),
                MacAddress = GetStringSafe(reader, 2),
                IpAddress = GetStringSafe(reader, 3),
                Ramal = GetStringSafe(reader, 4),
                Responsavel = GetStringSafe(reader, 5)
            });
        }

        return telefones;
    }

    public void UpdateTelefoneCisco(CiscoPhone phone)
    {
        ArgumentNullException.ThrowIfNull(phone);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE TelefonesCisco
            SET Hostname = ?, MacAddress = ?, IpAddress = ?, Ramal = ?, Responsavel = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Hostname", phone.Hostname);
        AddTextParameter(command, "MacAddress", phone.MacAddress);
        AddTextParameter(command, "IpAddress", phone.IpAddress);
        AddTextParameter(command, "Ramal", phone.Ramal);
        AddTextParameter(command, "Responsavel", phone.Responsavel);
        command.Parameters.Add("Id", OdbcType.Int).Value = phone.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Telefone Cisco atualizado (Id={phone.Id}): Host='{phone.Hostname}', MAC='{phone.MacAddress}', IP='{phone.IpAddress}', Ramal='{phone.Ramal}', Resp='{phone.Responsavel}'");
    }

    public void DeleteTelefoneCisco(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM TelefonesCisco WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Telefone Cisco excluído (Id={id}).");
    }

    //
    //  RELÓGIOS DE PONTO
    //
    public void AddRelogioPonto(RelogioPonto relogio)
    {
        ArgumentNullException.ThrowIfNull(relogio);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO RelogiosPonto
            (Hostname, Modelo, NumeroSerie, IpAddress, Local, Responsavel)
            VALUES (?, ?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Hostname", relogio.Hostname);
        AddTextParameter(command, "Modelo", relogio.Modelo);
        AddTextParameter(command, "NumeroSerie", relogio.NumeroSerie);
        AddTextParameter(command, "IpAddress", relogio.IpAddress);
        AddTextParameter(command, "Local", relogio.Local);
        AddTextParameter(command, "Responsavel", relogio.Responsavel);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Relógio de ponto inserido: Host='{relogio.Hostname}', Modelo='{relogio.Modelo}', NS='{relogio.NumeroSerie}', IP='{relogio.IpAddress}', Local='{relogio.Local}', Resp='{relogio.Responsavel}'");
    }

    public List<RelogioPonto> GetAllRelogiosPonto()
    {
        var relogios = new List<RelogioPonto>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Hostname, Modelo, NumeroSerie, IpAddress, Local, Responsavel FROM RelogiosPonto";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relogios.Add(new RelogioPonto
            {
                Id = reader.GetInt32(0),
                Hostname = GetStringSafe(reader, 1),
                Modelo = GetStringSafe(reader, 2),
                NumeroSerie = GetStringSafe(reader, 3),
                IpAddress = GetStringSafe(reader, 4),
                Local = GetStringSafe(reader, 5),
                Responsavel = GetStringSafe(reader, 6)
            });
        }

        return relogios;
    }

    public void UpdateRelogioPonto(RelogioPonto relogio)
    {
        ArgumentNullException.ThrowIfNull(relogio);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE RelogiosPonto
            SET Hostname = ?, Modelo = ?, NumeroSerie = ?, IpAddress = ?, Local = ?, Responsavel = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Hostname", relogio.Hostname);
        AddTextParameter(command, "Modelo", relogio.Modelo);
        AddTextParameter(command, "NumeroSerie", relogio.NumeroSerie);
        AddTextParameter(command, "IpAddress", relogio.IpAddress);
        AddTextParameter(command, "Local", relogio.Local);
        AddTextParameter(command, "Responsavel", relogio.Responsavel);
        command.Parameters.Add("Id", OdbcType.Int).Value = relogio.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Relógio de ponto atualizado (Id={relogio.Id}): Host='{relogio.Hostname}', Modelo='{relogio.Modelo}', NS='{relogio.NumeroSerie}', IP='{relogio.IpAddress}', Local='{relogio.Local}', Resp='{relogio.Responsavel}'");
    }

    public void DeleteRelogioPonto(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM RelogiosPonto WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Relógio de ponto excluído (Id={id}).");
    }

    private static void AddTextParameter(OdbcCommand command, string name, string? value)
    {
        var parameter = command.Parameters.Add(name, OdbcType.VarChar);
        parameter.Value = NormalizeToDbValue(value);
    }

    private static string GetStringSafe(OdbcDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);

    private static object NormalizeToDbValue(string? value)
    {
        var normalized = DeviceStringUtils.NormalizeString(value);
        return string.IsNullOrWhiteSpace(normalized) ? DBNull.Value : normalized;
    }
}
