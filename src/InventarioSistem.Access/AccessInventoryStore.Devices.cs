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
            (Host, SerialNumber, Proprietario, Departamento, Matricula, Monitores)
            VALUES (?, ?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Host", computer.Host);
        AddTextParameter(command, "SerialNumber", computer.SerialNumber);
        AddTextParameter(command, "Proprietario", computer.Proprietario);
        AddTextParameter(command, "Departamento", computer.Departamento);
        AddTextParameter(command, "Matricula", computer.Matricula);
        AddTextParameter(command, "Monitores", computer.Monitores);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Computador inserido: Host='{computer.Host}', NS='{computer.SerialNumber}', Proprietario='{computer.Proprietario}', Departamento='{computer.Departamento}', Matricula='{computer.Matricula}', Monitores='{computer.Monitores}'");
    }

    public List<Computer> GetAllComputers()
    {
        var computers = new List<Computer>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Host, SerialNumber, Proprietario, Departamento, Matricula, Monitores FROM Computadores";

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
                Matricula = GetStringSafe(reader, 5),
                Monitores = reader.FieldCount > 6 ? GetStringSafe(reader, 6) : string.Empty
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
            SET Host = ?, SerialNumber = ?, Proprietario = ?, Departamento = ?, Matricula = ?, Monitores = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Host", computer.Host);
        AddTextParameter(command, "SerialNumber", computer.SerialNumber);
        AddTextParameter(command, "Proprietario", computer.Proprietario);
        AddTextParameter(command, "Departamento", computer.Departamento);
        AddTextParameter(command, "Matricula", computer.Matricula);
        AddTextParameter(command, "Monitores", computer.Monitores);
        command.Parameters.Add("Id", OdbcType.Int).Value = computer.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Computador atualizado (Id={computer.Id}): Host='{computer.Host}', NS='{computer.SerialNumber}', Proprietario='{computer.Proprietario}', Departamento='{computer.Departamento}', Matricula='{computer.Matricula}', Monitores='{computer.Monitores}'");
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
            (CellName, Imei1, Imei2, Modelo, Numero, Roaming, Usuario, Matricula, Cargo, Setor, Email, Senha)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "CellName", celular.CellName);
        AddTextParameter(command, "Imei1", celular.Imei1);
        AddTextParameter(command, "Imei2", celular.Imei2);
        AddTextParameter(command, "Modelo", celular.Modelo);
        AddTextParameter(command, "Numero", celular.Numero);
        AddTextParameter(command, "Roaming", celular.Roaming);
        AddTextParameter(command, "Usuario", celular.Usuario);
        AddTextParameter(command, "Matricula", celular.Matricula);
        AddTextParameter(command, "Cargo", celular.Cargo);
        AddTextParameter(command, "Setor", celular.Setor);
        AddTextParameter(command, "Email", celular.Email);
        AddTextParameter(command, "Senha", celular.Senha);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Celular inserido: CellName='{celular.CellName}', Modelo='{celular.Modelo}', Numero='{celular.Numero}', Usuario='{celular.Usuario}', IMEI1='{celular.Imei1}', IMEI2='{celular.Imei2}'");
    }

    public List<Celular> GetAllCelulares()
    {
        var celulares = new List<Celular>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"SELECT Id, CellName, Imei1, Imei2, Modelo, Numero, Roaming, Usuario, Matricula, Cargo, Setor, Email, Senha FROM Celulares";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            celulares.Add(new Celular
            {
                Id = reader.GetInt32(0),
                CellName = GetStringSafe(reader, 1),
                Imei1 = GetStringSafe(reader, 2),
                Imei2 = GetStringSafe(reader, 3),
                Modelo = GetStringSafe(reader, 4),
                Numero = GetStringSafe(reader, 5),
                Roaming = reader.FieldCount > 6 ? GetStringSafe(reader, 6) : string.Empty,
                Usuario = reader.FieldCount > 7 ? GetStringSafe(reader, 7) : string.Empty,
                Matricula = reader.FieldCount > 8 ? GetStringSafe(reader, 8) : string.Empty,
                Cargo = reader.FieldCount > 9 ? GetStringSafe(reader, 9) : string.Empty,
                Setor = reader.FieldCount > 10 ? GetStringSafe(reader, 10) : string.Empty,
                Email = reader.FieldCount > 11 ? GetStringSafe(reader, 11) : string.Empty,
                Senha = reader.FieldCount > 12 ? GetStringSafe(reader, 12) : string.Empty
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
            SET CellName = ?, Imei1 = ?, Imei2 = ?, Modelo = ?, Numero = ?, Roaming = ?, Usuario = ?, Matricula = ?, Cargo = ?, Setor = ?, Email = ?, Senha = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "CellName", celular.CellName);
        AddTextParameter(command, "Imei1", celular.Imei1);
        AddTextParameter(command, "Imei2", celular.Imei2);
        AddTextParameter(command, "Modelo", celular.Modelo);
        AddTextParameter(command, "Numero", celular.Numero);
        AddTextParameter(command, "Roaming", celular.Roaming);
        AddTextParameter(command, "Usuario", celular.Usuario);
        AddTextParameter(command, "Matricula", celular.Matricula);
        AddTextParameter(command, "Cargo", celular.Cargo);
        AddTextParameter(command, "Setor", celular.Setor);
        AddTextParameter(command, "Email", celular.Email);
        AddTextParameter(command, "Senha", celular.Senha);
        command.Parameters.Add("Id", OdbcType.Int).Value = celular.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Celular atualizado (Id={celular.Id}): CellName='{celular.CellName}', Modelo='{celular.Modelo}', Numero='{celular.Numero}', Usuario='{celular.Usuario}', IMEI1='{celular.Imei1}', IMEI2='{celular.Imei2}'");
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
            (Nome, TipoModelo, SerialNumber, LocalAtual, LocalAnterior)
            VALUES (?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Nome", impressora.Nome);
        AddTextParameter(command, "TipoModelo", impressora.TipoModelo);
        AddTextParameter(command, "SerialNumber", impressora.SerialNumber);
        AddTextParameter(command, "LocalAtual", impressora.LocalAtual);
        AddTextParameter(command, "LocalAnterior", impressora.LocalAnterior);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Impressora inserida: Nome='{impressora.Nome}', Modelo='{impressora.TipoModelo}', NS='{impressora.SerialNumber}', LocalAtual='{impressora.LocalAtual}', LocalAnterior='{impressora.LocalAnterior}'");
    }

    public List<Impressora> GetAllImpressoras()
    {
        var impressoras = new List<Impressora>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Nome, TipoModelo, SerialNumber, LocalAtual, LocalAnterior FROM Impressoras";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            impressoras.Add(new Impressora
            {
                Id = reader.GetInt32(0),
                Nome = GetStringSafe(reader, 1),
                TipoModelo = GetStringSafe(reader, 2),
                SerialNumber = GetStringSafe(reader, 3),
                LocalAtual = GetStringSafe(reader, 4),
                LocalAnterior = GetStringSafe(reader, 5)
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
            SET Nome = ?, TipoModelo = ?, SerialNumber = ?, LocalAtual = ?, LocalAnterior = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Nome", impressora.Nome);
        AddTextParameter(command, "TipoModelo", impressora.TipoModelo);
        AddTextParameter(command, "SerialNumber", impressora.SerialNumber);
        AddTextParameter(command, "LocalAtual", impressora.LocalAtual);
        AddTextParameter(command, "LocalAnterior", impressora.LocalAnterior);
        command.Parameters.Add("Id", OdbcType.Int).Value = impressora.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Impressora atualizada (Id={impressora.Id}): Nome='{impressora.Nome}', Modelo='{impressora.TipoModelo}', NS='{impressora.SerialNumber}', LocalAtual='{impressora.LocalAtual}', LocalAnterior='{impressora.LocalAnterior}'");
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
            (Responsavel, Ipei, MacAddress, Numero, Local)
            VALUES (?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Responsavel", dect.Responsavel);
        AddTextParameter(command, "Ipei", dect.Ipei);
        AddTextParameter(command, "MacAddress", dect.MacAddress);
        AddTextParameter(command, "Numero", dect.Numero);
        AddTextParameter(command, "Local", dect.Local);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"DECT inserido: Numero='{dect.Numero}', IPEI='{dect.Ipei}', MAC='{dect.MacAddress}', Local='{dect.Local}', Resp='{dect.Responsavel}'");
    }

    public List<DectPhone> GetAllDects()
    {
        var dects = new List<DectPhone>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Responsavel, Ipei, MacAddress, Numero, Local FROM Dects";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            dects.Add(new DectPhone
            {
                Id = reader.GetInt32(0),
                Responsavel = GetStringSafe(reader, 1),
                Ipei = GetStringSafe(reader, 2),
                MacAddress = GetStringSafe(reader, 3),
                Numero = GetStringSafe(reader, 4),
                Local = GetStringSafe(reader, 5)
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
            SET Responsavel = ?, Ipei = ?, MacAddress = ?, Numero = ?, Local = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Responsavel", dect.Responsavel);
        AddTextParameter(command, "Ipei", dect.Ipei);
        AddTextParameter(command, "MacAddress", dect.MacAddress);
        AddTextParameter(command, "Numero", dect.Numero);
        AddTextParameter(command, "Local", dect.Local);
        command.Parameters.Add("Id", OdbcType.Int).Value = dect.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"DECT atualizado (Id={dect.Id}): Numero='{dect.Numero}', IPEI='{dect.Ipei}', MAC='{dect.MacAddress}', Local='{dect.Local}', Resp='{dect.Responsavel}'");
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
            (Responsavel, MacAddress, Numero, Local)
            VALUES (?, ?, ?, ?)
        ";

        AddTextParameter(command, "Responsavel", phone.Responsavel);
        AddTextParameter(command, "MacAddress", phone.MacAddress);
        AddTextParameter(command, "Numero", phone.Numero);
        AddTextParameter(command, "Local", phone.Local);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Telefone Cisco inserido: Numero='{phone.Numero}', MAC='{phone.MacAddress}', Local='{phone.Local}', Resp='{phone.Responsavel}'");
    }

    public List<CiscoPhone> GetAllTelefonesCisco()
    {
        var telefones = new List<CiscoPhone>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Responsavel, MacAddress, Numero, Local FROM TelefonesCisco";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            telefones.Add(new CiscoPhone
            {
                Id = reader.GetInt32(0),
                Responsavel = GetStringSafe(reader, 1),
                MacAddress = GetStringSafe(reader, 2),
                Numero = GetStringSafe(reader, 3),
                Local = GetStringSafe(reader, 4)
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
            SET Responsavel = ?, MacAddress = ?, Numero = ?, Local = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Responsavel", phone.Responsavel);
        AddTextParameter(command, "MacAddress", phone.MacAddress);
        AddTextParameter(command, "Numero", phone.Numero);
        AddTextParameter(command, "Local", phone.Local);
        command.Parameters.Add("Id", OdbcType.Int).Value = phone.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Telefone Cisco atualizado (Id={phone.Id}): Numero='{phone.Numero}', MAC='{phone.MacAddress}', Local='{phone.Local}', Resp='{phone.Responsavel}'");
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
    //  TELEVISOR
    //
    public void AddTelevisor(Televisor tv)
    {
        ArgumentNullException.ThrowIfNull(tv);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Televisores (Modelo, SerialNumber, Local)
            VALUES (?, ?, ?)
        ";

        AddTextParameter(command, "Modelo", tv.Modelo);
        AddTextParameter(command, "SerialNumber", tv.SerialNumber);
        AddTextParameter(command, "Local", tv.Local);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Televisor inserido: Modelo='{tv.Modelo}', NS='{tv.SerialNumber}', Local='{tv.Local}'");
    }

    public List<Televisor> GetAllTelevisores()
    {
        var televisores = new List<Televisor>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Modelo, SerialNumber, Local FROM Televisores";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            televisores.Add(new Televisor
            {
                Id = reader.GetInt32(0),
                Modelo = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3)
            });
        }

        return televisores;
    }

    public void UpdateTelevisor(Televisor tv)
    {
        ArgumentNullException.ThrowIfNull(tv);

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Televisores
            SET Modelo = ?, SerialNumber = ?, Local = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Modelo", tv.Modelo);
        AddTextParameter(command, "SerialNumber", tv.SerialNumber);
        AddTextParameter(command, "Local", tv.Local);
        command.Parameters.Add("Id", OdbcType.Int).Value = tv.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Televisor atualizado (Id={tv.Id}): Modelo='{tv.Modelo}', NS='{tv.SerialNumber}', Local='{tv.Local}'");
    }

    public void DeleteTelevisor(int id)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Televisores WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore", $"Televisor excluído (Id={id}).");
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
            (Modelo, SerialNumber, Local, Ip, DataBateria, DataNobreak, ProximasVerificacoes)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        ";

        AddTextParameter(command, "Modelo", relogio.Modelo);
        AddTextParameter(command, "SerialNumber", relogio.SerialNumber);
        AddTextParameter(command, "Local", relogio.Local);
        AddTextParameter(command, "Ip", relogio.Ip);
        AddDateParameter(command, "DataBateria", relogio.DataBateria);
        AddDateParameter(command, "DataNobreak", relogio.DataNobreak);
        AddDateParameter(command, "ProximasVerificacoes", relogio.ProximasVerificacoes);

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Relógio de ponto inserido: Modelo='{relogio.Modelo}', NS='{relogio.SerialNumber}', IP='{relogio.Ip}', Local='{relogio.Local}', DataBateria='{relogio.DataBateria:yyyy-MM-dd}', DataNobreak='{relogio.DataNobreak:yyyy-MM-dd}', ProximaVerificacao='{relogio.ProximasVerificacoes:yyyy-MM-dd}'");
    }

    public List<RelogioPonto> GetAllRelogiosPonto()
    {
        var relogios = new List<RelogioPonto>();

        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Id, Modelo, SerialNumber, Local, Ip, DataBateria, DataNobreak, ProximasVerificacoes FROM RelogiosPonto";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            relogios.Add(new RelogioPonto
            {
                Id = reader.GetInt32(0),
                Modelo = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                Ip = GetStringSafe(reader, 4),
                DataBateria = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                DataNobreak = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ProximasVerificacoes = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
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
            SET Modelo = ?, SerialNumber = ?, Local = ?, Ip = ?, DataBateria = ?, DataNobreak = ?, ProximasVerificacoes = ?
            WHERE Id = ?
        ";

        AddTextParameter(command, "Modelo", relogio.Modelo);
        AddTextParameter(command, "SerialNumber", relogio.SerialNumber);
        AddTextParameter(command, "Local", relogio.Local);
        AddTextParameter(command, "Ip", relogio.Ip);
        AddDateParameter(command, "DataBateria", relogio.DataBateria);
        AddDateParameter(command, "DataNobreak", relogio.DataNobreak);
        AddDateParameter(command, "ProximasVerificacoes", relogio.ProximasVerificacoes);
        command.Parameters.Add("Id", OdbcType.Int).Value = relogio.Id;

        command.ExecuteNonQuery();

        InventoryLogger.Info("AccessInventoryStore",
            $"Relógio de ponto atualizado (Id={relogio.Id}): Modelo='{relogio.Modelo}', NS='{relogio.SerialNumber}', IP='{relogio.Ip}', Local='{relogio.Local}', DataBateria='{relogio.DataBateria:yyyy-MM-dd}', DataNobreak='{relogio.DataNobreak:yyyy-MM-dd}', ProximaVerificacao='{relogio.ProximasVerificacoes:yyyy-MM-dd}'");
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

    private static void AddDateParameter(OdbcCommand command, string name, DateTime? value)
    {
        var parameter = command.Parameters.Add(name, OdbcType.DateTime);
        parameter.Value = value.HasValue ? value.Value : (object)DBNull.Value;
    }

    private static string GetStringSafe(OdbcDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);

    private static object NormalizeToDbValue(string? value)
    {
        var normalized = DeviceStringUtils.NormalizeString(value);
        return string.IsNullOrWhiteSpace(normalized) ? DBNull.Value : normalized;
    }
}
