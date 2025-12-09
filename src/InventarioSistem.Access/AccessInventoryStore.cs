using System.Data.Common;
using System.Data.Odbc;
using System.Text;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.Access;

public partial class AccessInventoryStore
{
    private readonly AccessConnectionFactory _factory;

    public AccessInventoryStore(AccessConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        // Usa o gerenciador de esquema centralizado para garantir todas as tabelas obrigatórias.
        await Task.Run(() => Schema.AccessSchemaManager.EnsureRequiredTables(_factory), cancellationToken);
    }

    public async Task<int> AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Devices
            (Tipo, Patrimonio, Marca, Modelo, NumeroSerie, Imei, SistemaOperacional, Processador, MemoriaRamGb, ArmazenamentoGb, VersaoAndroid, LinhaTelefonica, Responsavel, Localizacao, Observacoes, AtualizadoEm, FabricanteScanner, PossuiCarregadorBase, PossuiTeclado, Corporativo)
            VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?);";

        AddDeviceParameters(command, device);

        await command.ExecuteNonQueryAsync(cancellationToken);

        command.CommandText = "SELECT @@IDENTITY";
        var idObject = await command.ExecuteScalarAsync(cancellationToken);
        device.Id = Convert.ToInt32(idObject);
        return device.Id;
    }

    public async Task<Device?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Devices WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapDevice(reader);
    }

    public async Task<IReadOnlyCollection<Device>> ListAsync(DeviceType? type = null, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        if (type is null)
        {
            command.CommandText = "SELECT * FROM Devices ORDER BY Localizacao, Patrimonio";
        }
        else
        {
            command.CommandText = "SELECT * FROM Devices WHERE Tipo = ? ORDER BY Patrimonio";
            command.Parameters.Add("Tipo", OdbcType.VarChar).Value = type.ToString();
        }

        var items = new List<Device>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var device = MapDevice(reader);
            if (device != null)
            {
                items.Add(device);
            }
        }

        return items;
    }

    public async Task<IReadOnlyCollection<Device>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = StringUtilities.RemoveDiacritics(term ?? string.Empty).ToUpperInvariant();
        var devices = await ListAsync(null, cancellationToken);
        return devices.Where(d =>
            StringUtilities.ContainsInsensitive(d.Patrimonio, normalizedTerm) ||
            StringUtilities.ContainsInsensitive(d.Marca, normalizedTerm) ||
            StringUtilities.ContainsInsensitive(d.Modelo, normalizedTerm) ||
            StringUtilities.ContainsInsensitive(d.NumeroSerie, normalizedTerm) ||
            StringUtilities.ContainsInsensitive(d.Responsavel, normalizedTerm) ||
            (!string.IsNullOrWhiteSpace(d.Imei) && StringUtilities.ContainsInsensitive(d.Imei!, normalizedTerm))).ToList();
    }

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE Devices SET
            Tipo=?, Patrimonio=?, Marca=?, Modelo=?, NumeroSerie=?, Imei=?, SistemaOperacional=?, Processador=?, MemoriaRamGb=?, ArmazenamentoGb=?, VersaoAndroid=?, LinhaTelefonica=?, Responsavel=?, Localizacao=?, Observacoes=?, AtualizadoEm=?, FabricanteScanner=?, PossuiCarregadorBase=?, PossuiTeclado=?, Corporativo=?
            WHERE Id = ?";

        AddDeviceParameters(command, device);
        command.Parameters.Add("Id", OdbcType.Int).Value = device.Id;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Devices WHERE Id = ?";
        command.Parameters.Add("Id", OdbcType.Int).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IDictionary<DeviceType, int>> CountByTypeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Tipo, COUNT(*) AS Total FROM Devices GROUP BY Tipo";
        var result = new Dictionary<DeviceType, int>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var tipoText = reader.GetString(reader.GetOrdinal("Tipo"));
            if (Enum.TryParse<DeviceType>(tipoText, out var type))
            {
                result[type] = reader.GetInt32(reader.GetOrdinal("Total"));
            }
        }

        return result;
    }

    public async Task<IReadOnlyCollection<Device>> DevicesMissingImeiAsync(CancellationToken cancellationToken = default)
    {
        var devices = await ListAsync(null, cancellationToken);
        return devices.Where(d => string.IsNullOrWhiteSpace(d.Imei)).ToList();
    }

    public async Task<IReadOnlyCollection<Device>> DevicesByLocationAsync(string location, CancellationToken cancellationToken = default)
    {
        var devices = await ListAsync(null, cancellationToken);
        return devices.Where(d => StringUtilities.ContainsInsensitive(d.Localizacao, location)).ToList();
    }

    private static void AddDeviceParameters(OdbcCommand command, Device device)
    {
        var orderedValues = new object?[]
        {
            device.Type.ToString(),
            StringUtilities.NullIfWhiteSpace(device.Patrimonio),
            StringUtilities.NullIfWhiteSpace(device.Marca),
            StringUtilities.NullIfWhiteSpace(device.Modelo),
            StringUtilities.NullIfWhiteSpace(device.NumeroSerie),
            StringUtilities.NullIfWhiteSpace(ImeiUtility.Normalize(device.Imei)),
            (device as Computer)?.SistemaOperacional,
            (device as Computer)?.Processador,
            (device as Computer)?.MemoriaRamGb ?? 0,
            (device as Computer)?.ArmazenamentoGb ?? 0,
            device switch
            {
                Tablet tablet => tablet.VersaoAndroid,
                Celular celular => celular.VersaoAndroid,
                ColetorAndroid coletor => coletor.VersaoAndroid,
                _ => null
            },
            device switch
            {
                Tablet tablet => tablet.LinhaTelefonica,
                Celular celular => celular.LinhaTelefonica,
                _ => null
            },
            StringUtilities.NullIfWhiteSpace(device.Responsavel),
            StringUtilities.NullIfWhiteSpace(device.Localizacao),
            StringUtilities.NullIfWhiteSpace(device.Observacoes),
            device.AtualizadoEm,
            (device as ColetorAndroid)?.FabricanteScanner,
            (device as ColetorAndroid)?.PossuiCarregadorBase ?? false,
            (device as Tablet)?.PossuiTeclado ?? false,
            (device as Celular)?.Corporativo ?? false
        };

        foreach (var value in orderedValues)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    private static Device? MapDevice(DbDataReader reader)
    {
        var tipoText = reader.GetString(reader.GetOrdinal("Tipo"));
        if (!Enum.TryParse<DeviceType>(tipoText, out var type))
        {
            return null;
        }

        Device device = type switch
        {
            DeviceType.Computer => new Computer
            {
                SistemaOperacional = reader["SistemaOperacional"] as string ?? string.Empty,
                Processador = reader["Processador"] as string ?? string.Empty,
                MemoriaRamGb = Convert.ToInt32(reader["MemoriaRamGb"] is DBNull ? 0 : reader["MemoriaRamGb"]),
                ArmazenamentoGb = Convert.ToInt32(reader["ArmazenamentoGb"] is DBNull ? 0 : reader["ArmazenamentoGb"])
            },
            DeviceType.Tablet => new Tablet
            {
                VersaoAndroid = reader["VersaoAndroid"] as string ?? string.Empty,
                LinhaTelefonica = reader["LinhaTelefonica"] as string ?? string.Empty,
                PossuiTeclado = reader["PossuiTeclado"] is bool teclado && teclado
            },
            DeviceType.ColetorAndroid => new ColetorAndroid
            {
                VersaoAndroid = reader["VersaoAndroid"] as string ?? string.Empty,
                FabricanteScanner = reader["FabricanteScanner"] as string ?? string.Empty,
                PossuiCarregadorBase = reader["PossuiCarregadorBase"] is bool baseCarga && baseCarga
            },
            DeviceType.Celular => new Celular
            {
                LinhaTelefonica = reader["LinhaTelefonica"] as string ?? string.Empty,
                Corporativo = reader["Corporativo"] is bool corporativo && corporativo,
                VersaoAndroid = reader["VersaoAndroid"] as string ?? string.Empty
            },
            DeviceType.Impressora => new Impressora(),
            DeviceType.Dect => new Dect(),
            DeviceType.TelefoneCisco => new TelefoneCisco(),
            DeviceType.Televisor => new Televisor(),
            _ => null!
        };

        if (device == null)
        {
            return null;
        }

        device.Id = Convert.ToInt32(reader["Id"]);
        device.Patrimonio = reader["Patrimonio"] as string ?? string.Empty;
        device.Marca = reader["Marca"] as string ?? string.Empty;
        device.Modelo = reader["Modelo"] as string ?? string.Empty;
        device.NumeroSerie = reader["NumeroSerie"] as string ?? string.Empty;
        device.Imei = reader["Imei"] as string;
        device.Responsavel = reader["Responsavel"] as string ?? string.Empty;
        device.Localizacao = reader["Localizacao"] as string ?? string.Empty;
        device.Observacoes = reader["Observacoes"] as string;
        device.AtualizadoEm = reader["AtualizadoEm"] is DateTime date ? date : DateTime.UtcNow;

        return device;
    }
}
