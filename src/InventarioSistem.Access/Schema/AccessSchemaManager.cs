using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using InventarioSistem.Access;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access.Schema;

/// <summary>
/// Responsável por garantir que o banco Access selecionado
/// contenha as tabelas padrão do InventarioSistem.
/// </summary>
public static class AccessSchemaManager
{
    private const string LoggerSource = "AccessSchema";

    private static readonly string[] RequiredTables =
    {
        "Devices",
        "Computadores",
        "Tablets",
        "ColetoresAndroid",
        "Celulares",
        "Impressoras",
        "Dects",
        "TelefonesCisco",
        "Televisores",
        "RelogiosPonto",
        "Monitores",
        "Nobreaks"
    };

    // Lista completa de colunas suportadas no Devices
    private static readonly Dictionary<string, string> DeviceColumns = new()
    {
        { "Id", "COUNTER PRIMARY KEY" },
        { "Tipo", "TEXT(50)" },
        { "Patrimonio", "TEXT(100)" },
        { "Marca", "TEXT(100)" },
        { "Hostname", "TEXT(255)" },
        { "Modelo", "TEXT(255)" },
        { "NumeroSerie", "TEXT(100)" },
        { "Imei", "TEXT(30)" },
        { "SerialNumber", "TEXT(255)" },
        { "Local", "TEXT(255)" },
        { "Localizacao", "TEXT(255)" },
        { "Responsavel", "TEXT(255)" },

        { "SistemaOperacional", "TEXT(100)" },
        { "Processador", "TEXT(100)" },
        { "MemoriaRamGb", "INTEGER" },
        { "ArmazenamentoGb", "INTEGER" },
        { "VersaoAndroid", "TEXT(50)" },
        { "LinhaTelefonica", "TEXT(50)" },

        { "Observacoes", "MEMO" },
        { "AtualizadoEm", "DATETIME" },

        { "TipoModelo", "TEXT(255)" },
        { "LocalizacaoAtual", "TEXT(255)" },
        { "LocalizacaoAnterior", "TEXT(255)" },

        { "Numero", "TEXT(255)" },
        { "IPEI", "TEXT(255)" },
        { "MacAddress", "TEXT(255)" },
        { "IP", "TEXT(255)" },

        { "IMEI1", "TEXT(255)" },
        { "IMEI2", "TEXT(255)" },
        { "Roaming", "YESNO" },
        { "Usuario", "TEXT(255)" },
        { "Matricula", "TEXT(255)" },
        { "Cargo", "TEXT(255)" },
        { "Setor", "TEXT(255)" },
        { "Email", "TEXT(255)" },
        { "Senha", "TEXT(255)" },

        { "ComputadorVinculado", "TEXT(255)" },
        { "Status", "TEXT(255)" },

        { "FabricanteScanner", "TEXT(100)" },
        { "PossuiCarregadorBase", "YESNO" },
        { "PossuiTeclado", "YESNO" },
        { "Corporativo", "YESNO" },

        { "DataBateria", "DATETIME" },
        { "DataNobreak", "DATETIME" },
        { "ProximaVerificacao", "DATETIME" }
    };

    /// <summary>
    /// Verifica se todas as tabelas obrigatórias existem no banco atual.
    /// Usa AccessConnectionFactory.CreateConnection() para conectar ao banco ativo.
    /// </summary>
    public static bool HasAllRequiredTables()
    {
        var factory = new AccessConnectionFactory();
        using var conn = factory.CreateConnection();
        conn.Open();
        var schema = conn.GetSchema("Tables");

        bool HasTable(string name)
        {
            // Filtra apenas tabelas de usuário (TABLE) e compara por nome.
            var rows = schema.Rows.Cast<DataRow>()
                .Where(r =>
                    string.Equals(r["TABLE_TYPE"]?.ToString(), "TABLE", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r["TABLE_NAME"]?.ToString(), name, StringComparison.OrdinalIgnoreCase));

            return rows.Any();
        }

        foreach (var table in RequiredTables)
        {
            if (!HasTable(table))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Cria as tabelas obrigatórias que ainda não existirem.
    /// Não apaga nem altera tabelas já existentes.
    /// </summary>
    public static void EnsureRequiredTables() => EnsureRequiredTables(new AccessConnectionFactory());

    public static void EnsureRequiredTables(AccessConnectionFactory factory)
    {
        InventoryLogger.Info(LoggerSource, "Iniciando verificação/criação do schema Access.");

        var errors = new List<Exception>();

        using var conn = factory.CreateConnection();
        conn.Open();

        var schema = conn.GetSchema("Tables");

        bool HasTable(string name)
        {
            var rows = schema.Rows.Cast<DataRow>()
                .Where(r =>
                    string.Equals(r["TABLE_TYPE"]?.ToString(), "TABLE", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r["TABLE_NAME"]?.ToString(), name, StringComparison.OrdinalIgnoreCase));

            return rows.Any();
        }

        void CreateTable(string name, string createSql)
        {
            if (HasTable(name))
                return;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        void EnsureColumns(string tableName, IEnumerable<(string ColumnName, string Definition)> columns)
        {
            bool ColumnExists(string columnName)
            {
                var restrictions = new[] { null, null, tableName, null };
                var columnSchema = conn.GetSchema("Columns", restrictions);
                return columnSchema.Rows.Cast<DataRow>()
                    .Any(r => string.Equals(r["COLUMN_NAME"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var (columnName, definition) in columns)
            {
                if (ColumnExists(columnName))
                    continue;

                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN {definition}";
                alter.ExecuteNonQuery();
            }
        }

        bool EnsureSafe(string tableName, Action action)
        {
            try
            {
                InventoryLogger.Info(LoggerSource, $"Verificando tabela '{tableName}'...");
                action();
                InventoryLogger.Info(LoggerSource, $"Tabela '{tableName}' ok (existente ou criada).");
                return true;
            }
            catch (Exception ex)
            {
                InventoryLogger.Error(LoggerSource, $"Falha ao garantir tabela '{tableName}'.", ex);
                errors.Add(ex);
                return false;
            }
        }

        void EnsureDevicesTable()
        {
            // 1 – Criar tabela Devices se não existir
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Devices WHERE 1=0";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    using var create = conn.CreateCommand();
                    create.CommandText = "CREATE TABLE Devices (Id COUNTER PRIMARY KEY, Tipo TEXT(50))";
                    create.ExecuteNonQuery();
                }
            }

            // 2 – Adicionar colunas faltantes automaticamente
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var schemaCmd = conn.CreateCommand())
            {
                schemaCmd.CommandText = "SELECT * FROM Devices";
                using var reader = schemaCmd.ExecuteReader(CommandBehavior.SchemaOnly);
                var table = reader.GetSchemaTable();
                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var colName = row["ColumnName"] as string;
                        if (!string.IsNullOrWhiteSpace(colName))
                        {
                            existing.Add(colName);
                        }
                    }
                }
            }

            foreach (var col in DeviceColumns)
            {
                if (existing.Contains(col.Key))
                {
                    continue;
                }

                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE Devices ADD COLUMN {col.Key} {col.Value}";
                alter.ExecuteNonQuery();
            }
        }

        void EnsureRelogiosPontoTable()
        {
            CreateTable("RelogiosPonto", @"
                CREATE TABLE RelogiosPonto (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Modelo TEXT(100),
                    SerialNumber TEXT(100),
                    Local TEXT(100),
                    Ip TEXT(50),
                    DataBateria DATETIME,
                    DataNobreak DATETIME,
                    ProximasVerificacoes DATETIME
                )
            ");

            EnsureColumns("RelogiosPonto", new (string, string)[]
            {
                ("Modelo", "Modelo TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Local", "Local TEXT(100)"),
                ("Ip", "Ip TEXT(50)"),
                ("DataBateria", "DataBateria DATETIME"),
                ("DataNobreak", "DataNobreak DATETIME"),
                ("ProximasVerificacoes", "ProximasVerificacoes DATETIME"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        }

        void EnsureMonitoresTable()
        {
            CreateTable("Monitores", @"
                CREATE TABLE Monitores (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Modelo TEXT(100),
                    SerialNumber TEXT(100),
                    Local TEXT(100),
                    Responsavel TEXT(100),
                    ComputadorVinculado TEXT(100)
                )
            ");

            EnsureColumns("Monitores", new (string, string)[]
            {
                ("Modelo", "Modelo TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Local", "Local TEXT(100)"),
                ("Responsavel", "Responsavel TEXT(100)"),
                ("ComputadorVinculado", "ComputadorVinculado TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        }

        void EnsureNobreaksTable()
        {
            CreateTable("Nobreaks", @"
                CREATE TABLE Nobreaks (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Hostname TEXT(100),
                    Local TEXT(100),
                    IpAddress TEXT(50),
                    Modelo TEXT(100),
                    Status TEXT(50),
                    SerialNumber TEXT(100)
                )
            ");

            EnsureColumns("Nobreaks", new (string, string)[]
            {
                ("Hostname", "Hostname TEXT(100)"),
                ("Local", "Local TEXT(100)"),
                ("IpAddress", "IpAddress TEXT(50)"),
                ("Modelo", "Modelo TEXT(100)"),
                ("Status", "Status TEXT(50)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        }

        EnsureSafe("Devices", EnsureDevicesTable);

        EnsureSafe("Computadores", () =>
        {
            CreateTable("Computadores", @"
                CREATE TABLE Computadores (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Host TEXT(100),
                    SerialNumber TEXT(100),
                    Proprietario TEXT(100),
                    Departamento TEXT(100),
                    Matricula TEXT(50)
                )
            ");

            EnsureColumns("Computadores", new (string, string)[]
            {
                ("Host", "Host TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Proprietario", "Proprietario TEXT(100)"),
                ("Departamento", "Departamento TEXT(100)"),
                ("Matricula", "Matricula TEXT(50)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("Tablets", () =>
        {
            CreateTable("Tablets", @"
                CREATE TABLE Tablets (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Host TEXT(100),
                    SerialNumber TEXT(100),
                    Local TEXT(100),
                    Responsavel TEXT(100),
                    Imeis TEXT(255)
                )
            ");

            EnsureColumns("Tablets", new (string, string)[]
            {
                ("Host", "Host TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Local", "Local TEXT(100)"),
                ("Responsavel", "Responsavel TEXT(100)"),
                ("Imeis", "Imeis TEXT(255)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("ColetoresAndroid", () =>
        {
            CreateTable("ColetoresAndroid", @"
                CREATE TABLE ColetoresAndroid (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Host TEXT(100),
                    SerialNumber TEXT(100),
                    MacAddress TEXT(50),
                    IpAddress TEXT(50),
                    Local TEXT(100)
                )
            ");

            EnsureColumns("ColetoresAndroid", new (string, string)[]
            {
                ("Host", "Host TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("MacAddress", "MacAddress TEXT(50)"),
                ("IpAddress", "IpAddress TEXT(50)"),
                ("Local", "Local TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("Celulares", () =>
        {
            CreateTable("Celulares", @"
                CREATE TABLE Celulares (
                    Id AUTOINCREMENT PRIMARY KEY,
                    CellName TEXT(100),
                    Imei1 TEXT(50),
                    Imei2 TEXT(50),
                    Modelo TEXT(100),
                    Numero TEXT(50),
                    Roaming YESNO,
                    Usuario TEXT(100),
                    Matricula TEXT(50),
                    Cargo TEXT(100),
                    Setor TEXT(100),
                    Email TEXT(150),
                    Senha TEXT(150)
                )
            ");

            EnsureColumns("Celulares", new (string, string)[]
            {
                ("CellName", "CellName TEXT(100)"),
                ("Imei1", "Imei1 TEXT(50)"),
                ("Imei2", "Imei2 TEXT(50)"),
                ("Modelo", "Modelo TEXT(100)"),
                ("Numero", "Numero TEXT(50)"),
                ("Roaming", "Roaming YESNO"),
                ("Usuario", "Usuario TEXT(100)"),
                ("Matricula", "Matricula TEXT(50)"),
                ("Cargo", "Cargo TEXT(100)"),
                ("Setor", "Setor TEXT(100)"),
                ("Email", "Email TEXT(150)"),
                ("Senha", "Senha TEXT(150)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("Impressoras", () =>
        {
            CreateTable("Impressoras", @"
                CREATE TABLE Impressoras (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Nome TEXT(200),
                    TipoModelo TEXT(100),
                    SerialNumber TEXT(100),
                    LocalAtual TEXT(100),
                    LocalAnterior TEXT(100)
                )
            ");

            EnsureColumns("Impressoras", new (string, string)[]
            {
                ("Nome", "Nome TEXT(200)"),
                ("TipoModelo", "TipoModelo TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("LocalAtual", "LocalAtual TEXT(100)"),
                ("LocalAnterior", "LocalAnterior TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("Dects", () =>
        {
            CreateTable("Dects", @"
                CREATE TABLE Dects (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Responsavel TEXT(100),
                    Ipei TEXT(100),
                    MacAddress TEXT(50),
                    Numero TEXT(50),
                    Local TEXT(100)
                )
            ");

            EnsureColumns("Dects", new (string, string)[]
            {
                ("Responsavel", "Responsavel TEXT(100)"),
                ("Ipei", "Ipei TEXT(100)"),
                ("MacAddress", "MacAddress TEXT(50)"),
                ("Numero", "Numero TEXT(50)"),
                ("Local", "Local TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("TelefonesCisco", () =>
        {
            CreateTable("TelefonesCisco", @"
                CREATE TABLE TelefonesCisco (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Responsavel TEXT(100),
                    MacAddress TEXT(50),
                    Numero TEXT(50),
                    Local TEXT(100)
                )
            ");

            EnsureColumns("TelefonesCisco", new (string, string)[]
            {
                ("Responsavel", "Responsavel TEXT(100)"),
                ("MacAddress", "MacAddress TEXT(50)"),
                ("Numero", "Numero TEXT(50)"),
                ("Local", "Local TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("Televisores", () =>
        {
            CreateTable("Televisores", @"
                CREATE TABLE Televisores (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Modelo TEXT(100),
                    SerialNumber TEXT(100),
                    Local TEXT(100)
                )
            ");

            EnsureColumns("Televisores", new (string, string)[]
            {
                ("Modelo", "Modelo TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Local", "Local TEXT(100)"),
                ("CreatedAt", "CreatedAt DATETIME")
            });
        });

        EnsureSafe("RelogiosPonto", EnsureRelogiosPontoTable);
        EnsureSafe("Monitores", EnsureMonitoresTable);
        EnsureSafe("Nobreaks", EnsureNobreaksTable);

        if (errors.Count > 0)
        {
            throw new AggregateException("Falha ao garantir as tabelas obrigatórias do Access.", errors);
        }

        InventoryLogger.Info(LoggerSource, "Verificação/criação do schema Access concluída.");
    }
}
