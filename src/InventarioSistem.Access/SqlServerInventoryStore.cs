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

    public async Task<List<Computer>> GetAllComputersAsync(CancellationToken cancellationToken = default)
    {
        var computers = new List<Computer>();
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Id], [Host], [SerialNumber], [Proprietario], [Departamento], [Matricula], [CreatedAt] FROM [Computadores] ORDER BY [Id]";

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
    public List<Computer> GetAllComputers() => GetAllComputersAsync().GetAwaiter().GetResult();
    public void AddComputer(Computer computer) => AddComputerAsync(computer).GetAwaiter().GetResult();
    public void UpdateComputer(Computer computer) => UpdateComputerAsync(computer).GetAwaiter().GetResult();
    public void DeleteComputer(int id) => DeleteComputerAsync(id).GetAwaiter().GetResult();

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

    public List<Tablet> GetAllTablets() => GetAllTabletsAsync().GetAwaiter().GetResult();
    public void AddTablet(Tablet tablet) => AddTabletAsync(tablet).GetAwaiter().GetResult();
    public void UpdateTablet(Tablet tablet) => UpdateTabletAsync(tablet).GetAwaiter().GetResult();
    public void DeleteTablet(int id) => DeleteTabletAsync(id).GetAwaiter().GetResult();

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

    public List<ColetorAndroid> GetAllColetores() => GetAllColetoresAsync().GetAwaiter().GetResult();
    public void AddColetor(ColetorAndroid coletor) => AddColetorAsync(coletor).GetAwaiter().GetResult();
    public void UpdateColetor(ColetorAndroid coletor) => UpdateColetorAsync(coletor).GetAwaiter().GetResult();
    public void DeleteColetor(int id) => DeleteColetorAsync(id).GetAwaiter().GetResult();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

