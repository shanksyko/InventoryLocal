using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Logging;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.Access;

/// <summary>
/// SQL Server implementation of the inventory store.
/// Full replacement for AccessInventoryStore with native async support.
/// </summary>
public partial class SqlServerInventoryStore : IDisposable
{
    private readonly SqlServerConnectionFactory _factory;

    public SqlServerInventoryStore(SqlServerConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => 
            Schema.SqlServerSchemaManager.EnsureRequiredTables(_factory), 
            cancellationToken);
    }

    public void InvalidateCache()
    {
        CacheManager.Instance.RemoveByPrefix("devices_");
    }

    // ===== HELPER METHODS =====
    private static string GetStringSafe(IDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static DateTime? GetDateTimeSafe(IDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static bool GetBoolSafe(IDataReader reader, int ordinal)
    {
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }

    private static int? GetIntSafe(IDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    // ===== COMPUTADORES =====

    public async Task AddComputerAsync(Computer computer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computer);

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Computadores] 
            ([Host], [SerialNumber], [Proprietario], [Departamento], [Matricula], [CreatedAt])
            VALUES (@Host, @SerialNumber, @Proprietario, @Departamento, @Matricula, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Host", computer.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", computer.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Proprietario", computer.Proprietario ?? "");
        command.Parameters.AddWithValue("@Departamento", computer.Departamento ?? "");
        command.Parameters.AddWithValue("@Matricula", computer.Matricula ?? "");
        command.Parameters.AddWithValue("@CreatedAt", computer.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        computer.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Computador inserido: Host='{computer.Host}', NS='{computer.SerialNumber}'");
    }

    public async Task<List<Computer>> GetAllComputersAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var computers = new List<Computer>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        // Limitar a 1000 registros por padrão para melhor performance com grandes volumes
        var limitClause = limit.HasValue ? $"TOP {limit.Value}" : "TOP 1000";
        command.CommandText = $"SELECT {limitClause} [Id], [Host], [SerialNumber], [Proprietario], [Departamento], [Matricula], [CreatedAt] FROM [Computadores] ORDER BY [Id] DESC";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            computers.Add(new Computer
            {
                Id = reader.GetInt32(0),
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Proprietario = GetStringSafe(reader, 3),
                Departamento = GetStringSafe(reader, 4),
                Matricula = GetStringSafe(reader, 5),
                CreatedAt = GetDateTimeSafe(reader, 6)
            });
        }
        return computers;
    }

    public async Task UpdateComputerAsync(Computer computer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computer);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Computadores]
            SET [Host] = @Host, [SerialNumber] = @SerialNumber, [Proprietario] = @Proprietario,
                [Departamento] = @Departamento, [Matricula] = @Matricula
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Host", computer.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", computer.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Proprietario", computer.Proprietario ?? "");
        command.Parameters.AddWithValue("@Departamento", computer.Departamento ?? "");
        command.Parameters.AddWithValue("@Matricula", computer.Matricula ?? "");
        command.Parameters.AddWithValue("@Id", computer.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Computador atualizado (Id={computer.Id}): Host='{computer.Host}'");
    }

    public async Task DeleteComputerAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Computadores] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Computador deletado: Id={id}");
    }

    // Sync wrappers
    public List<Computer> GetAllComputers(int? limit = null) => Task.Run(() => GetAllComputersAsync(limit)).GetAwaiter().GetResult();
    public void AddComputer(Computer computer) => Task.Run(() => AddComputerAsync(computer)).GetAwaiter().GetResult();
    public void UpdateComputer(Computer computer) => Task.Run(() => UpdateComputerAsync(computer)).GetAwaiter().GetResult();
    public void DeleteComputer(int id) => Task.Run(() => DeleteComputerAsync(id)).GetAwaiter().GetResult();

    // ===== TABLETS =====

    public async Task AddTabletAsync(Tablet tablet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tablet);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Tablets]
            ([Host], [SerialNumber], [Local], [Responsavel], [Imeis], [CreatedAt])
            VALUES (@Host, @SerialNumber, @Local, @Responsavel, @Imeis, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Host", tablet.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", tablet.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", tablet.Local ?? "");
        command.Parameters.AddWithValue("@Responsavel", tablet.Responsavel ?? "");
        command.Parameters.AddWithValue("@Imeis", string.Join(";", tablet.Imeis ?? new()));
        command.Parameters.AddWithValue("@CreatedAt", tablet.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        tablet.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Tablet inserido: Host='{tablet.Host}', NS='{tablet.SerialNumber}'");
    }

    public async Task<List<Tablet>> GetAllTabletsAsync(CancellationToken cancellationToken = default)
    {
        var tablets = new List<Tablet>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Host], [SerialNumber], [Local], [Responsavel], [Imeis], [CreatedAt] FROM [Tablets] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var imeisStr = GetStringSafe(reader, 5);
            var imeis = string.IsNullOrEmpty(imeisStr) ? new List<string>() : imeisStr.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();

            tablets.Add(new Tablet
            {
                Id = reader.GetInt32(0),
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                Local = GetStringSafe(reader, 3),
                Responsavel = GetStringSafe(reader, 4),
                Imeis = imeis,
                CreatedAt = GetDateTimeSafe(reader, 6)
            });
        }
        return tablets;
    }

    public async Task UpdateTabletAsync(Tablet tablet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tablet);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Tablets]
            SET [Host] = @Host, [SerialNumber] = @SerialNumber, [Local] = @Local,
                [Responsavel] = @Responsavel, [Imeis] = @Imeis
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Host", tablet.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", tablet.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Local", tablet.Local ?? "");
        command.Parameters.AddWithValue("@Responsavel", tablet.Responsavel ?? "");
        command.Parameters.AddWithValue("@Imeis", string.Join(";", tablet.Imeis ?? new()));
        command.Parameters.AddWithValue("@Id", tablet.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Tablet atualizado (Id={tablet.Id})");
    }

    public async Task DeleteTabletAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [Tablets] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Tablet deletado: Id={id}");
    }

    public List<Tablet> GetAllTablets() => Task.Run(() => GetAllTabletsAsync()).GetAwaiter().GetResult();
    public void AddTablet(Tablet tablet) => Task.Run(() => AddTabletAsync(tablet)).GetAwaiter().GetResult();
    public void UpdateTablet(Tablet tablet) => Task.Run(() => UpdateTabletAsync(tablet)).GetAwaiter().GetResult();
    public void DeleteTablet(int id) => Task.Run(() => DeleteTabletAsync(id)).GetAwaiter().GetResult();

    // ===== COLETORES ANDROID =====

    public async Task AddColetorAsync(ColetorAndroid coletor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(coletor);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [ColetoresAndroid]
            ([Host], [SerialNumber], [MacAddress], [IpAddress], [Local], [CreatedAt], 
             [AppGwsFg], [AppGwsRm], [AppInspection], [AppCuringTbr], [AppCuringPcr],
             [AppInspectionTbr], [AppQuimico], [AppBuildingTbr], [AppBuildingPcr],
             [OsWinCe], [OsAndroid81], [OsAndroid10])
            VALUES (@Host, @SerialNumber, @MacAddress, @IpAddress, @Local, @CreatedAt,
                    @AppGwsFg, @AppGwsRm, @AppInspection, @AppCuringTbr, @AppCuringPcr,
                    @AppInspectionTbr, @AppQuimico, @AppBuildingTbr, @AppBuildingPcr,
                    @OsWinCe, @OsAndroid81, @OsAndroid10);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Host", coletor.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", coletor.SerialNumber ?? "");
        command.Parameters.AddWithValue("@MacAddress", coletor.MacAddress ?? "");
        command.Parameters.AddWithValue("@IpAddress", coletor.IpAddress ?? "");
        command.Parameters.AddWithValue("@Local", coletor.Local ?? "");
        command.Parameters.AddWithValue("@CreatedAt", coletor.CreatedAt ?? DateTime.Now);
        command.Parameters.AddWithValue("@AppGwsFg", coletor.AppGwsFg);
        command.Parameters.AddWithValue("@AppGwsRm", coletor.AppGwsRm);
        command.Parameters.AddWithValue("@AppInspection", coletor.AppInspection);
        command.Parameters.AddWithValue("@AppCuringTbr", coletor.AppCuringTbr);
        command.Parameters.AddWithValue("@AppCuringPcr", coletor.AppCuringPcr);
        command.Parameters.AddWithValue("@AppInspectionTbr", coletor.AppInspectionTbr);
        command.Parameters.AddWithValue("@AppQuimico", coletor.AppQuimico);
        command.Parameters.AddWithValue("@AppBuildingTbr", coletor.AppBuildingTbr);
        command.Parameters.AddWithValue("@AppBuildingPcr", coletor.AppBuildingPcr);
        command.Parameters.AddWithValue("@OsWinCe", coletor.OsWinCe);
        command.Parameters.AddWithValue("@OsAndroid81", coletor.OsAndroid81);
        command.Parameters.AddWithValue("@OsAndroid10", coletor.OsAndroid10);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        coletor.Id = Convert.ToInt32(result);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Coletor inserido: Host='{coletor.Host}'");
    }

    public async Task<List<ColetorAndroid>> GetAllColetoresAsync(CancellationToken cancellationToken = default)
    {
        var coletores = new List<ColetorAndroid>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT [Id], [Host], [SerialNumber], [MacAddress], [IpAddress], [Local], [CreatedAt],
                   [AppGwsFg], [AppGwsRm], [AppInspection], [AppCuringTbr], [AppCuringPcr],
                   [AppInspectionTbr], [AppQuimico], [AppBuildingTbr], [AppBuildingPcr],
                   [OsWinCe], [OsAndroid81], [OsAndroid10]
            FROM [ColetoresAndroid] ORDER BY [Id]";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            coletores.Add(new ColetorAndroid
            {
                Id = reader.GetInt32(0),
                Host = GetStringSafe(reader, 1),
                SerialNumber = GetStringSafe(reader, 2),
                MacAddress = GetStringSafe(reader, 3),
                IpAddress = GetStringSafe(reader, 4),
                Local = GetStringSafe(reader, 5),
                CreatedAt = GetDateTimeSafe(reader, 6),
                AppGwsFg = GetBoolSafe(reader, 7),
                AppGwsRm = GetBoolSafe(reader, 8),
                AppInspection = GetBoolSafe(reader, 9),
                AppCuringTbr = GetBoolSafe(reader, 10),
                AppCuringPcr = GetBoolSafe(reader, 11),
                AppInspectionTbr = GetBoolSafe(reader, 12),
                AppQuimico = GetBoolSafe(reader, 13),
                AppBuildingTbr = GetBoolSafe(reader, 14),
                AppBuildingPcr = GetBoolSafe(reader, 15),
                OsWinCe = GetBoolSafe(reader, 16),
                OsAndroid81 = GetBoolSafe(reader, 17),
                OsAndroid10 = GetBoolSafe(reader, 18)
            });
        }
        return coletores;
    }

    public async Task UpdateColetorAsync(ColetorAndroid coletor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(coletor);
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [ColetoresAndroid]
            SET [Host] = @Host, [SerialNumber] = @SerialNumber, [MacAddress] = @MacAddress,
                [IpAddress] = @IpAddress, [Local] = @Local,
                [AppGwsFg] = @AppGwsFg, [AppGwsRm] = @AppGwsRm, [AppInspection] = @AppInspection,
                [AppCuringTbr] = @AppCuringTbr, [AppCuringPcr] = @AppCuringPcr,
                [AppInspectionTbr] = @AppInspectionTbr, [AppQuimico] = @AppQuimico,
                [AppBuildingTbr] = @AppBuildingTbr, [AppBuildingPcr] = @AppBuildingPcr,
                [OsWinCe] = @OsWinCe, [OsAndroid81] = @OsAndroid81, [OsAndroid10] = @OsAndroid10
            WHERE [Id] = @Id";

        command.Parameters.AddWithValue("@Host", coletor.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", coletor.SerialNumber ?? "");
        command.Parameters.AddWithValue("@MacAddress", coletor.MacAddress ?? "");
        command.Parameters.AddWithValue("@IpAddress", coletor.IpAddress ?? "");
        command.Parameters.AddWithValue("@Local", coletor.Local ?? "");
        command.Parameters.AddWithValue("@AppGwsFg", coletor.AppGwsFg);
        command.Parameters.AddWithValue("@AppGwsRm", coletor.AppGwsRm);
        command.Parameters.AddWithValue("@AppInspection", coletor.AppInspection);
        command.Parameters.AddWithValue("@AppCuringTbr", coletor.AppCuringTbr);
        command.Parameters.AddWithValue("@AppCuringPcr", coletor.AppCuringPcr);
        command.Parameters.AddWithValue("@AppInspectionTbr", coletor.AppInspectionTbr);
        command.Parameters.AddWithValue("@AppQuimico", coletor.AppQuimico);
        command.Parameters.AddWithValue("@AppBuildingTbr", coletor.AppBuildingTbr);
        command.Parameters.AddWithValue("@AppBuildingPcr", coletor.AppBuildingPcr);
        command.Parameters.AddWithValue("@OsWinCe", coletor.OsWinCe);
        command.Parameters.AddWithValue("@OsAndroid81", coletor.OsAndroid81);
        command.Parameters.AddWithValue("@OsAndroid10", coletor.OsAndroid10);
        command.Parameters.AddWithValue("@Id", coletor.Id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Coletor atualizado (Id={coletor.Id})");
    }

    public async Task DeleteColetorAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM [ColetoresAndroid] WHERE [Id] = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Coletor deletado: Id={id}");
    }

    public List<ColetorAndroid> GetAllColetores() => Task.Run(() => GetAllColetoresAsync()).GetAwaiter().GetResult();
    public void AddColetor(ColetorAndroid coletor) => Task.Run(() => AddColetorAsync(coletor)).GetAwaiter().GetResult();
    public void UpdateColetor(ColetorAndroid coletor) => Task.Run(() => UpdateColetorAsync(coletor)).GetAwaiter().GetResult();
    public void DeleteColetor(int id) => Task.Run(() => DeleteColetorAsync(id)).GetAwaiter().GetResult();

    // ===== UTILITY METHODS =====
    
    /// <summary>
    /// List all devices (generic method for cross-type queries)
    /// </summary>
    public async Task<List<dynamic>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<dynamic>();
        
        // Get all device types and return as dynamic list (limit to 1000 per type for performance)
        var computers = await GetAllComputersAsync(1000, cancellationToken);
        foreach (var c in computers)
        {
            result.Add(new { c.Id, Type = "Computer", c.Host, c.SerialNumber, c.CreatedAt });
        }

        var tablets = await GetAllTabletsAsync(cancellationToken);
        foreach (var t in tablets)
        {
            result.Add(new { t.Id, Type = "Tablet", t.Host, t.SerialNumber, t.CreatedAt });
        }

        var coletores = await GetAllColetoresAsync(cancellationToken);
        foreach (var c in coletores)
        {
            result.Add(new { c.Id, Type = "ColetorAndroid", SerialNumber = (string?)null, c.CreatedAt });
        }

        var celulares = await GetAllCelularesAsync(cancellationToken);
        foreach (var c in celulares)
        {
            result.Add(new { c.Id, Type = "Celular", SerialNumber = (string?)null, c.CreatedAt });
        }

        var impressoras = await GetAllImpressorasAsync(cancellationToken);
        foreach (var i in impressoras)
        {
            result.Add(new { i.Id, Type = "Impressora", SerialNumber = (string?)null, i.CreatedAt });
        }

        var dects = await GetAllDectsAsync(cancellationToken);
        foreach (var d in dects)
        {
            result.Add(new { d.Id, Type = "Dect", SerialNumber = (string?)null, d.CreatedAt });
        }

        var ciscos = await GetAllTelefonesCiscoAsync(cancellationToken);
        foreach (var t in ciscos)
        {
            result.Add(new { t.Id, Type = "TelefoneCisco", SerialNumber = (string?)null, t.CreatedAt });
        }

        var televisores = await GetAllTelevisoresAsync(cancellationToken);
        foreach (var t in televisores)
        {
            result.Add(new { t.Id, Type = "Televisor", SerialNumber = (string?)null, t.CreatedAt });
        }

        var relogios = await GetAllRelogiosPontoAsync(cancellationToken);
        foreach (var r in relogios)
        {
            result.Add(new { r.Id, Type = "RelogioPonto", SerialNumber = (string?)null, r.CreatedAt });
        }

        var monitores = await GetAllMonitoresAsync(cancellationToken);
        foreach (var m in monitores)
        {
            result.Add(new { m.Id, Type = "Monitor", SerialNumber = (string?)null, m.CreatedAt });
        }

        var nobreaks = await GetAllNobreaksAsync(cancellationToken);
        foreach (var n in nobreaks)
        {
            result.Add(new { n.Id, Type = "Nobreak", n.SerialNumber, n.CreatedAt });
        }

        return result;
    }

    /// <summary>
    /// Count devices grouped by type
    /// </summary>
    public async Task<Dictionary<string, int>> CountByTypeAsync(CancellationToken cancellationToken = default)
    {
        var counts = new Dictionary<string, int>();

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        async Task<int> CountTable(string tableName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result ?? 0);
        }

        counts["Computadores"] = await CountTable("Computadores");
        counts["Tablets"] = await CountTable("Tablets");
        counts["ColetoresAndroid"] = await CountTable("ColetoresAndroid");
        counts["Celulares"] = await CountTable("Celulares");
        counts["Impressoras"] = await CountTable("Impressoras");
        counts["Dects"] = await CountTable("Dects");
        counts["TelefonesCisco"] = await CountTable("TelefonesCisco");
        counts["Televisores"] = await CountTable("Televisores");
        counts["RelogiosPonto"] = await CountTable("RelogiosPonto");
        counts["Monitores"] = await CountTable("Monitores");
        counts["Nobreaks"] = await CountTable("Nobreaks");

        return counts;
    }

    /// <summary>
    /// Delete device by ID (searches all tables)
    /// </summary>
    public async Task DeleteAsync(int deviceId, CancellationToken cancellationToken = default)
    {
        string[] tables = { "Computadores", "Tablets", "ColetoresAndroid", "Celulares", "Impressoras", 
                          "Dects", "TelefonesCisco", "Televisores", "RelogiosPonto", "Monitores", "Nobreaks" };

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        foreach (var table in tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM [{table}] WHERE [Id] = @Id";
            command.Parameters.AddWithValue("@Id", deviceId);
            var affected = await command.ExecuteNonQueryAsync(cancellationToken);
            
            if (affected > 0)
            {
                InvalidateCache();
                InventoryLogger.Info("SqlServerInventoryStore", $"Dispositivo deletado: Id={deviceId}");
                return;
            }
        }
    }

    /// <summary>
    /// Get devices missing IMEI
    /// </summary>
    public async Task<List<dynamic>> DevicesMissingImeiAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<dynamic>();
        var celulares = await GetAllCelularesAsync(cancellationToken);
        
        foreach (var c in celulares)
        {
            if (string.IsNullOrWhiteSpace(c.Numero))
            {
                result.Add(new { c.Id, Type = "Celular", Numero = (string?)null, c.CreatedAt });
            }
        }

        return result;
    }

    /// <summary>
    /// Group devices by location
    /// </summary>
    public async Task<Dictionary<string, int>> DevicesByLocationAsync(CancellationToken cancellationToken = default)
    {
        var all = await ListAsync(cancellationToken);
        var groups = new Dictionary<string, int>();

        foreach (var device in all)
        {
            string? location = (string?)device.Location ?? (string?)device.Local;
            if (!string.IsNullOrWhiteSpace(location))
            {
                if (!groups.ContainsKey(location))
                    groups[location] = 0;
                groups[location]++;
            }
        }

        return groups;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

