using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access;

/// <summary>
/// Continuation of SqlServerInventoryStore with device methods for:
/// Celulares, Impressoras, Dects, TelefonesCisco, Televisores, RelogiosPonto, Monitores, Nobreaks
/// </summary>
public partial class SqlServerInventoryStore
{
    // ===== CELULARES =====

    public async Task AddCelularAsync(Celular celular, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(celular);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Celulares]
            ([CellName], [Imei1], [Imei2], [Modelo], [Numero], [Roaming], [Usuario], [Matricula], [Cargo], [Setor], [Email], [Senha], [CreatedAt])
            VALUES (@CellName, @Imei1, @Imei2, @Modelo, @Numero, @Roaming, @Usuario, @Matricula, @Cargo, @Setor, @Email, @Senha, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@CellName", celular.CellName ?? "");
        command.Parameters.AddWithValue("@Imei1", celular.Imei1 ?? "");
        command.Parameters.AddWithValue("@Imei2", celular.Imei2 ?? "");
        command.Parameters.AddWithValue("@Modelo", celular.Modelo ?? "");
        command.Parameters.AddWithValue("@Numero", celular.Numero ?? "");
        command.Parameters.AddWithValue("@Roaming", celular.Roaming);
        command.Parameters.AddWithValue("@Usuario", celular.Usuario ?? "");
        command.Parameters.AddWithValue("@Matricula", celular.Matricula ?? "");
        command.Parameters.AddWithValue("@Cargo", celular.Cargo ?? "");
        command.Parameters.AddWithValue("@Setor", celular.Setor ?? "");
        command.Parameters.AddWithValue("@Email", celular.Email ?? "");
        command.Parameters.AddWithValue("@Senha", celular.Senha ?? "");
        command.Parameters.AddWithValue("@CreatedAt", celular.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        celular.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Celular inserido: {celular.CellName}");
    }

    public async Task<List<Celular>> GetAllCelularesAsync(CancellationToken cancellationToken = default)
    {
        var celulares = new List<Celular>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT [Id], [CellName], [Imei1], [Imei2], [Modelo], [Numero], [Roaming], [Usuario], [Matricula], [Cargo], [Setor], [Email], [Senha], [CreatedAt]
            FROM [Celulares] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            celulares.Add(new Celular
            {
                Id = reader.GetInt32(0),
                CellName = GetStringSafe(reader, 1),
                Imei1 = GetStringSafe(reader, 2),
                Imei2 = GetStringSafe(reader, 3),
                Modelo = GetStringSafe(reader, 4),
                Numero = GetStringSafe(reader, 5),
                Roaming = GetBoolSafe(reader, 6),
                Usuario = GetStringSafe(reader, 7),
                Matricula = GetStringSafe(reader, 8),
                Cargo = GetStringSafe(reader, 9),
                Setor = GetStringSafe(reader, 10),
                Email = GetStringSafe(reader, 11),
                Senha = GetStringSafe(reader, 12),
                CreatedAt = GetDateTimeSafe(reader, 13)
            });
        }
        return celulares;
    }

    public async Task UpdateCelularAsync(Celular celular, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(celular);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Celulares]
            SET [CellName] = @CellName, [Imei1] = @Imei1, [Imei2] = @Imei2, [Modelo] = @Modelo,
                [Numero] = @Numero, [Roaming] = @Roaming, [Usuario] = @Usuario, [Matricula] = @Matricula,
                [Cargo] = @Cargo, [Setor] = @Setor, [Email] = @Email, [Senha] = @Senha
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@CellName", celular.CellName ?? "");
        command.Parameters.AddWithValue("@Imei1", celular.Imei1 ?? "");
        command.Parameters.AddWithValue("@Imei2", celular.Imei2 ?? "");
        command.Parameters.AddWithValue("@Modelo", celular.Modelo ?? "");
        command.Parameters.AddWithValue("@Numero", celular.Numero ?? "");
        command.Parameters.AddWithValue("@Roaming", celular.Roaming);
        command.Parameters.AddWithValue("@Usuario", celular.Usuario ?? "");
        command.Parameters.AddWithValue("@Matricula", celular.Matricula ?? "");
        command.Parameters.AddWithValue("@Cargo", celular.Cargo ?? "");
        command.Parameters.AddWithValue("@Setor", celular.Setor ?? "");
        command.Parameters.AddWithValue("@Email", celular.Email ?? "");
        command.Parameters.AddWithValue("@Senha", celular.Senha ?? "");
        command.Parameters.AddWithValue("@Id", celular.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Celular atualizado (Id={celular.Id})");
    }

    public async Task DeleteCelularAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Celulares] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Celular deletado: Id={id}");
    }

    public List<Celular> GetAllCelulares() => GetAllCelularesAsync().GetAwaiter().GetResult();
    public void AddCelular(Celular celular) => AddCelularAsync(celular).GetAwaiter().GetResult();
    public void UpdateCelular(Celular celular) => UpdateCelularAsync(celular).GetAwaiter().GetResult();
    public void DeleteCelular(int id) => DeleteCelularAsync(id).GetAwaiter().GetResult();

    // ===== IMPRESSORAS =====

    public async Task AddImpressoraAsync(Impressora impressora, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(impressora);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Impressoras]
            ([Nome], [TipoModelo], [SerialNumber], [LocalAtual], [LocalAnterior], [CreatedAt])
            VALUES (@Nome, @TipoModelo, @SerialNumber, @LocalAtual, @LocalAnterior, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Nome", impressora.Nome ?? "");
        command.Parameters.AddWithValue("@TipoModelo", impressora.TipoModelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", impressora.SerialNumber ?? "");
        command.Parameters.AddWithValue("@LocalAtual", impressora.LocalAtual ?? "");
        command.Parameters.AddWithValue("@LocalAnterior", impressora.LocalAnterior ?? "");
        command.Parameters.AddWithValue("@CreatedAt", impressora.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        impressora.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Impressora inserida: {impressora.Nome}");
    }

    public async Task<List<Impressora>> GetAllImpressorasAsync(CancellationToken cancellationToken = default)
    {
        var impressoras = new List<Impressora>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Nome], [TipoModelo], [SerialNumber], [LocalAtual], [LocalAnterior], [CreatedAt] FROM [Impressoras] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            impressoras.Add(new Impressora
            {
                Id = reader.GetInt32(0),
                Nome = GetStringSafe(reader, 1),
                TipoModelo = GetStringSafe(reader, 2),
                SerialNumber = GetStringSafe(reader, 3),
                LocalAtual = GetStringSafe(reader, 4),
                LocalAnterior = GetStringSafe(reader, 5),
                CreatedAt = GetDateTimeSafe(reader, 6)
            });
        }
        return impressoras;
    }

    public async Task UpdateImpressoraAsync(Impressora impressora, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(impressora);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Impressoras]
            SET [Nome] = @Nome, [TipoModelo] = @TipoModelo, [SerialNumber] = @SerialNumber,
                [LocalAtual] = @LocalAtual, [LocalAnterior] = @LocalAnterior
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Nome", impressora.Nome ?? "");
        command.Parameters.AddWithValue("@TipoModelo", impressora.TipoModelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", impressora.SerialNumber ?? "");
        command.Parameters.AddWithValue("@LocalAtual", impressora.LocalAtual ?? "");
        command.Parameters.AddWithValue("@LocalAnterior", impressora.LocalAnterior ?? "");
        command.Parameters.AddWithValue("@Id", impressora.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Impressora atualizada (Id={impressora.Id})");
    }

    public async Task DeleteImpressoraAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Impressoras] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Impressora deletada: Id={id}");
    }

    public List<Impressora> GetAllImpressoras() => GetAllImpressorasAsync().GetAwaiter().GetResult();
    public void AddImpressora(Impressora impressora) => AddImpressoraAsync(impressora).GetAwaiter().GetResult();
    public void UpdateImpressora(Impressora impressora) => UpdateImpressoraAsync(impressora).GetAwaiter().GetResult();
    public void DeleteImpressora(int id) => DeleteImpressoraAsync(id).GetAwaiter().GetResult();

    // ===== DECTS =====

    public async Task AddDectAsync(DectPhone dect, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dect);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Dects]
            ([Responsavel], [Ipei], [MacAddress], [Numero], [Local], [Modelo], [CreatedAt])
            VALUES (@Responsavel, @Ipei, @MacAddress, @Numero, @Local, @Modelo, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Responsavel", dect.Responsavel ?? "");
        command.Parameters.AddWithValue("@Ipei", dect.Ipei ?? "");
        command.Parameters.AddWithValue("@MacAddress", dect.MacAddress ?? "");
        command.Parameters.AddWithValue("@Numero", dect.Numero ?? "");
        command.Parameters.AddWithValue("@Local", dect.Local ?? "");
        command.Parameters.AddWithValue("@Modelo", dect.Modelo ?? "");
        command.Parameters.AddWithValue("@CreatedAt", dect.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        dect.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"DECT inserido");
    }

    public async Task<List<DectPhone>> GetAllDectsAsync(CancellationToken cancellationToken = default)
    {
        var dects = new List<DectPhone>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Responsavel], [Ipei], [MacAddress], [Numero], [Local], [Modelo], [CreatedAt] FROM [Dects] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            dects.Add(new DectPhone
            {
                Id = reader.GetInt32(0),
                Responsavel = GetStringSafe(reader, 1),
                Ipei = GetStringSafe(reader, 2),
                MacAddress = GetStringSafe(reader, 3),
                Numero = GetStringSafe(reader, 4),
                Local = GetStringSafe(reader, 5),
                Modelo = GetStringSafe(reader, 6),
                CreatedAt = GetDateTimeSafe(reader, 7)
            });
        }
        return dects;
    }

    public async Task UpdateDectAsync(DectPhone dect, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dect);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Dects]
            SET [Responsavel] = @Responsavel, [Ipei] = @Ipei, [MacAddress] = @MacAddress,
                [Numero] = @Numero, [Local] = @Local, [Modelo] = @Modelo
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Responsavel", dect.Responsavel ?? "");
        command.Parameters.AddWithValue("@Ipei", dect.Ipei ?? "");
        command.Parameters.AddWithValue("@MacAddress", dect.MacAddress ?? "");
        command.Parameters.AddWithValue("@Numero", dect.Numero ?? "");
        command.Parameters.AddWithValue("@Local", dect.Local ?? "");
        command.Parameters.AddWithValue("@Modelo", dect.Modelo ?? "");
        command.Parameters.AddWithValue("@Id", dect.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"DECT atualizado (Id={dect.Id})");
    }

    public async Task DeleteDectAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Dects] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"DECT deletado: Id={id}");
    }

    public List<DectPhone> GetAllDects() => GetAllDectsAsync().GetAwaiter().GetResult();
    public void AddDect(DectPhone dect) => AddDectAsync(dect).GetAwaiter().GetResult();
    public void UpdateDect(DectPhone dect) => UpdateDectAsync(dect).GetAwaiter().GetResult();
    public void DeleteDect(int id) => DeleteDectAsync(id).GetAwaiter().GetResult();

    // ===== TELEFONES CISCO =====

    public async Task AddTelefoneCiscoAsync(CiscoPhone telefone, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telefone);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [TelefonesCisco]
            ([Responsavel], [MacAddress], [Numero], [Local], [IpAddress], [Serial], [CreatedAt])
            VALUES (@Responsavel, @MacAddress, @Numero, @Local, @IpAddress, @Serial, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Responsavel", telefone.Responsavel ?? "");
        command.Parameters.AddWithValue("@MacAddress", telefone.MacAddress ?? "");
        command.Parameters.AddWithValue("@Numero", telefone.Numero ?? "");
        command.Parameters.AddWithValue("@Local", telefone.Local ?? "");
        command.Parameters.AddWithValue("@IpAddress", telefone.IpAddress ?? "");
        command.Parameters.AddWithValue("@Serial", telefone.Serial ?? "");
        command.Parameters.AddWithValue("@CreatedAt", telefone.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        telefone.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Telefone Cisco inserido");
    }

    public async Task<List<CiscoPhone>> GetAllTelefonesCiscoAsync(CancellationToken cancellationToken = default)
    {
        var telefones = new List<CiscoPhone>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Responsavel], [MacAddress], [Numero], [Local], [IpAddress], [Serial], [CreatedAt] FROM [TelefonesCisco] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            telefones.Add(new CiscoPhone
            {
                Id = reader.GetInt32(0),
                Responsavel = GetStringSafe(reader, 1),
                MacAddress = GetStringSafe(reader, 2),
                Numero = GetStringSafe(reader, 3),
                Local = GetStringSafe(reader, 4),
                IpAddress = GetStringSafe(reader, 5),
                Serial = GetStringSafe(reader, 6),
                CreatedAt = GetDateTimeSafe(reader, 7)
            });
        }
        return telefones;
    }

    public async Task UpdateTelefoneCiscoAsync(CiscoPhone telefone, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telefone);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [TelefonesCisco]
            SET [Responsavel] = @Responsavel, [MacAddress] = @MacAddress, [Numero] = @Numero,
                [Local] = @Local, [IpAddress] = @IpAddress, [Serial] = @Serial
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Responsavel", telefone.Responsavel ?? "");
        command.Parameters.AddWithValue("@MacAddress", telefone.MacAddress ?? "");
        command.Parameters.AddWithValue("@Numero", telefone.Numero ?? "");
        command.Parameters.AddWithValue("@Local", telefone.Local ?? "");
        command.Parameters.AddWithValue("@IpAddress", telefone.IpAddress ?? "");
        command.Parameters.AddWithValue("@Serial", telefone.Serial ?? "");
        command.Parameters.AddWithValue("@Id", telefone.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Telefone Cisco atualizado (Id={telefone.Id})");
    }

    public async Task DeleteTelefoneCiscoAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [TelefonesCisco] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Telefone Cisco deletado: Id={id}");
    }

    public List<CiscoPhone> GetAllTelefonesCisco() => GetAllTelefonesCiscoAsync().GetAwaiter().GetResult();
    public void AddTelefoneCisco(CiscoPhone telefone) => AddTelefoneCiscoAsync(telefone).GetAwaiter().GetResult();
    public void UpdateTelefoneCisco(CiscoPhone telefone) => UpdateTelefoneCiscoAsync(telefone).GetAwaiter().GetResult();
    public void DeleteTelefoneCisco(int id) => DeleteTelefoneCiscoAsync(id).GetAwaiter().GetResult();

    // ===== TELEVISORES =====

    public async Task AddTelevisorAsync(Televisor tv, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tv);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Televisores]
            ([Modelo], [SerialNumber], [Local], [CreatedAt])
            VALUES (@Modelo, @SerialNumber, @Local, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Modelo", tv.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", tv.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", tv.Local ?? "");
        command.Parameters.AddWithValue("@CreatedAt", tv.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        tv.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Televisor inserido");
    }

    public async Task<List<Televisor>> GetAllTelevisoresAsync(CancellationToken cancellationToken = default)
    {
        var tvs = new List<Televisor>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Modelo], [SerialNumber], [Local], [CreatedAt] FROM [Televisores] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tvs.Add(new Televisor
            {
                Id = reader.GetInt32(0),
                Modelo = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                CreatedAt = GetDateTimeSafe(reader, 4)
            });
        }
        return tvs;
    }

    public async Task UpdateTelevisorAsync(Televisor tv, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tv);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Televisores]
            SET [Modelo] = @Modelo, [SerialNumber] = @SerialNumber, [Local] = @Local
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Modelo", tv.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", tv.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", tv.Local ?? "");
        command.Parameters.AddWithValue("@Id", tv.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Televisor atualizado (Id={tv.Id})");
    }

    public async Task DeleteTelevisorAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Televisores] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Televisor deletado: Id={id}");
    }

    public List<Televisor> GetAllTelevisores() => GetAllTelevisoresAsync().GetAwaiter().GetResult();
    public void AddTelevisor(Televisor tv) => AddTelevisorAsync(tv).GetAwaiter().GetResult();
    public void UpdateTelevisor(Televisor tv) => UpdateTelevisorAsync(tv).GetAwaiter().GetResult();
    public void DeleteTelevisor(int id) => DeleteTelevisorAsync(id).GetAwaiter().GetResult();

    // ===== RELOGIOS PONTO =====

    public async Task AddRelogiosPontoAsync(RelogioPonto relogio, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relogio);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [RelogiosPonto]
            ([Modelo], [SerialNumber], [Local], [Ip], [DataBateria], [DataNobreak], [ProximasVerificacoes], [CreatedAt])
            VALUES (@Modelo, @SerialNumber, @Local, @Ip, @DataBateria, @DataNobreak, @ProximasVerificacoes, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Modelo", relogio.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", relogio.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", relogio.Local ?? "");
        command.Parameters.AddWithValue("@Ip", relogio.Ip ?? "");
        command.Parameters.AddWithValue("@DataBateria", relogio.DataBateria is not null ? relogio.DataBateria : DBNull.Value);
        command.Parameters.AddWithValue("@DataNobreak", relogio.DataNobreak is not null ? relogio.DataNobreak : DBNull.Value);
        command.Parameters.AddWithValue("@ProximasVerificacoes", relogio.ProximasVerificacoes is not null ? relogio.ProximasVerificacoes : DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", relogio.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        relogio.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Relógio Ponto inserido");
    }

    public async Task<List<RelogioPonto>> GetAllRelogiosPontoAsync(CancellationToken cancellationToken = default)
    {
        var relogios = new List<RelogioPonto>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Modelo], [SerialNumber], [Local], [Ip], [DataBateria], [DataNobreak], [ProximasVerificacoes], [CreatedAt] FROM [RelogiosPonto] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            relogios.Add(new RelogioPonto
            {
                Id = reader.GetInt32(0),
                Modelo = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                Ip = GetStringSafe(reader, 4),
                DataBateria = GetDateTimeSafe(reader, 5),
                DataNobreak = GetDateTimeSafe(reader, 6),
                ProximasVerificacoes = GetDateTimeSafe(reader, 7),
                CreatedAt = GetDateTimeSafe(reader, 8)
            });
        }
        return relogios;
    }

    public async Task UpdateRelogiosPontoAsync(RelogioPonto relogio, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relogio);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [RelogiosPonto]
            SET [Modelo] = @Modelo, [SerialNumber] = @SerialNumber, [Local] = @Local,
                [Ip] = @Ip, [DataBateria] = @DataBateria, [DataNobreak] = @DataNobreak, [ProximasVerificacoes] = @ProximasVerificacoes
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Modelo", relogio.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", relogio.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", relogio.Local ?? "");
        command.Parameters.AddWithValue("@Ip", relogio.Ip ?? "");
        command.Parameters.AddWithValue("@DataBateria", relogio.DataBateria is not null ? relogio.DataBateria : DBNull.Value);
        command.Parameters.AddWithValue("@DataNobreak", relogio.DataNobreak is not null ? relogio.DataNobreak : DBNull.Value);
        command.Parameters.AddWithValue("@ProximasVerificacoes", relogio.ProximasVerificacoes is not null ? relogio.ProximasVerificacoes : DBNull.Value);
        command.Parameters.AddWithValue("@Id", relogio.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Relógio Ponto atualizado (Id={relogio.Id})");
    }

    public async Task DeleteRelogiosPontoAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [RelogiosPonto] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Relógio Ponto deletado: Id={id}");
    }

    public List<RelogioPonto> GetAllRelogiosPonto() => GetAllRelogiosPontoAsync().GetAwaiter().GetResult();
    public void AddRelogioPonto(RelogioPonto relogio) => AddRelogiosPontoAsync(relogio).GetAwaiter().GetResult();
    public void UpdateRelogioPonto(RelogioPonto relogio) => UpdateRelogiosPontoAsync(relogio).GetAwaiter().GetResult();
    public void DeleteRelogioPonto(int id) => DeleteRelogiosPontoAsync(id).GetAwaiter().GetResult();

    // ===== MONITORES =====

    public async Task AddMonitorAsync(InventarioSistem.Core.Devices.Monitor monitor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Monitores]
            ([Modelo], [SerialNumber], [Local], [Responsavel], [ComputadorVinculado], [CreatedAt])
            VALUES (@Modelo, @SerialNumber, @Local, @Responsavel, @ComputadorVinculado, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Modelo", monitor.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", monitor.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", monitor.Local ?? "");
        command.Parameters.AddWithValue("@Responsavel", monitor.Responsavel ?? "");
        command.Parameters.AddWithValue("@ComputadorVinculado", monitor.ComputadorVinculado ?? "");
        command.Parameters.AddWithValue("@CreatedAt", monitor.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        monitor.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Monitor inserido");
    }

    public async Task<List<InventarioSistem.Core.Devices.Monitor>> GetAllMonitoresAsync(CancellationToken cancellationToken = default)
    {
        var monitores = new List<InventarioSistem.Core.Devices.Monitor>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Modelo], [SerialNumber], [Local], [Responsavel], [ComputadorVinculado], [CreatedAt] FROM [Monitores] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            monitores.Add(new InventarioSistem.Core.Devices.Monitor
            {
                Id = reader.GetInt32(0),
                Modelo = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                Responsavel = GetStringSafe(reader, 4),
                ComputadorVinculado = GetStringSafe(reader, 5),
                CreatedAt = GetDateTimeSafe(reader, 6)
            });
        }
        return monitores;
    }

    public async Task UpdateMonitorAsync(InventarioSistem.Core.Devices.Monitor monitor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Monitores]
            SET [Modelo] = @Modelo, [SerialNumber] = @SerialNumber, [Local] = @Local,
                [Responsavel] = @Responsavel, [ComputadorVinculado] = @ComputadorVinculado
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Modelo", monitor.Modelo ?? "");
        command.Parameters.AddWithValue("@SerialNumber", monitor.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", monitor.Local ?? "");
        command.Parameters.AddWithValue("@Responsavel", monitor.Responsavel ?? "");
        command.Parameters.AddWithValue("@ComputadorVinculado", monitor.ComputadorVinculado ?? "");
        command.Parameters.AddWithValue("@Id", monitor.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Monitor atualizado (Id={monitor.Id})");
    }

    public async Task DeleteMonitorAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Monitores] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Monitor deletado: Id={id}");
    }

    public List<InventarioSistem.Core.Devices.Monitor> GetAllMonitores() => GetAllMonitoresAsync().GetAwaiter().GetResult();
    public void AddMonitor(InventarioSistem.Core.Devices.Monitor monitor) => AddMonitorAsync(monitor).GetAwaiter().GetResult();
    public void UpdateMonitor(InventarioSistem.Core.Devices.Monitor monitor) => UpdateMonitorAsync(monitor).GetAwaiter().GetResult();
    public void DeleteMonitor(int id) => DeleteMonitorAsync(id).GetAwaiter().GetResult();

    // ===== NOBREAKS =====

    public async Task AddNobreakAsync(Nobreak nobreak, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nobreak);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Nobreaks]
            ([Hostname], [Local], [IpAddress], [Modelo], [Status], [SerialNumber], [CreatedAt])
            VALUES (@Hostname, @Local, @IpAddress, @Modelo, @Status, @SerialNumber, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Hostname", nobreak.Hostname ?? "");
        command.Parameters.AddWithValue("@Local", nobreak.Local ?? "");
        command.Parameters.AddWithValue("@IpAddress", nobreak.IpAddress ?? "");
        command.Parameters.AddWithValue("@Modelo", nobreak.Modelo ?? "");
        command.Parameters.AddWithValue("@Status", nobreak.Status ?? "");
        command.Parameters.AddWithValue("@SerialNumber", nobreak.SerialNumber ?? "");
        command.Parameters.AddWithValue("@CreatedAt", nobreak.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        nobreak.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Nobreak inserido");
    }

    public async Task<List<Nobreak>> GetAllNobreaksAsync(CancellationToken cancellationToken = default)
    {
        var nobreaks = new List<Nobreak>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Hostname], [Local], [IpAddress], [Modelo], [Status], [SerialNumber], [CreatedAt] FROM [Nobreaks] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            nobreaks.Add(new Nobreak
            {
                Id = reader.GetInt32(0),
                Hostname = GetStringSafe(reader, 1),
                Local = GetStringSafe(reader, 2),
                IpAddress = GetStringSafe(reader, 3),
                Modelo = GetStringSafe(reader, 4),
                Status = GetStringSafe(reader, 5),
                SerialNumber = GetStringSafe(reader, 6),
                CreatedAt = GetDateTimeSafe(reader, 7)
            });
        }
        return nobreaks;
    }

    public async Task UpdateNobreakAsync(Nobreak nobreak, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nobreak);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Nobreaks]
            SET [Hostname] = @Hostname, [Local] = @Local, [IpAddress] = @IpAddress,
                [Modelo] = @Modelo, [Status] = @Status, [SerialNumber] = @SerialNumber
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Hostname", nobreak.Hostname ?? "");
        command.Parameters.AddWithValue("@Local", nobreak.Local ?? "");
        command.Parameters.AddWithValue("@IpAddress", nobreak.IpAddress ?? "");
        command.Parameters.AddWithValue("@Modelo", nobreak.Modelo ?? "");
        command.Parameters.AddWithValue("@Status", nobreak.Status ?? "");
        command.Parameters.AddWithValue("@SerialNumber", nobreak.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Id", nobreak.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Nobreak atualizado (Id={nobreak.Id})");
    }

    public async Task DeleteNobreakAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Nobreaks] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Nobreak deletado: Id={id}");
    }

    public List<Nobreak> GetAllNobreaks() => GetAllNobreaksAsync().GetAwaiter().GetResult();
    public void AddNobreak(Nobreak nobreak) => AddNobreakAsync(nobreak).GetAwaiter().GetResult();
    public void UpdateNobreak(Nobreak nobreak) => UpdateNobreakAsync(nobreak).GetAwaiter().GetResult();
    public void DeleteNobreak(int id) => DeleteNobreakAsync(id).GetAwaiter().GetResult();
}
