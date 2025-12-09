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
        "RelogiosPonto"
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
            CreateTable("Devices", @"
                CREATE TABLE Devices (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Tipo TEXT(50),
                    Patrimonio TEXT(100),
                    Marca TEXT(100),
                    Modelo TEXT(100),
                    NumeroSerie TEXT(100),
                    Imei TEXT(30),
                    SistemaOperacional TEXT(100),
                    Processador TEXT(100),
                    MemoriaRamGb INTEGER,
                    ArmazenamentoGb INTEGER,
                    VersaoAndroid TEXT(50),
                    LinhaTelefonica TEXT(50),
                    Responsavel TEXT(100),
                    Localizacao TEXT(100),
                    Observacoes MEMO,
                    AtualizadoEm DATETIME,
                    FabricanteScanner TEXT(100),
                    PossuiCarregadorBase YESNO,
                    PossuiTeclado YESNO,
                    Corporativo YESNO
                )
            ");

            EnsureColumns("Devices", new (string, string)[]
            {
                ("Tipo", "Tipo TEXT(50)"),
                ("Patrimonio", "Patrimonio TEXT(100)"),
                ("Marca", "Marca TEXT(100)"),
                ("Modelo", "Modelo TEXT(100)"),
                ("NumeroSerie", "NumeroSerie TEXT(100)"),
                ("Imei", "Imei TEXT(30)"),
                ("SistemaOperacional", "SistemaOperacional TEXT(100)"),
                ("Processador", "Processador TEXT(100)"),
                ("MemoriaRamGb", "MemoriaRamGb INTEGER"),
                ("ArmazenamentoGb", "ArmazenamentoGb INTEGER"),
                ("VersaoAndroid", "VersaoAndroid TEXT(50)"),
                ("LinhaTelefonica", "LinhaTelefonica TEXT(50)"),
                ("Responsavel", "Responsavel TEXT(100)"),
                ("Localizacao", "Localizacao TEXT(100)"),
                ("Observacoes", "Observacoes MEMO"),
                ("AtualizadoEm", "AtualizadoEm DATETIME"),
                ("FabricanteScanner", "FabricanteScanner TEXT(100)"),
                ("PossuiCarregadorBase", "PossuiCarregadorBase YESNO"),
                ("PossuiTeclado", "PossuiTeclado YESNO"),
                ("Corporativo", "Corporativo YESNO")
            });
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
                ("ProximasVerificacoes", "ProximasVerificacoes DATETIME")
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
                    Matricula TEXT(50),
                    Monitores TEXT(255)
                )
            ");

            EnsureColumns("Computadores", new (string, string)[]
            {
                ("Host", "Host TEXT(100)"),
                ("SerialNumber", "SerialNumber TEXT(100)"),
                ("Proprietario", "Proprietario TEXT(100)"),
                ("Departamento", "Departamento TEXT(100)"),
                ("Matricula", "Matricula TEXT(50)"),
                ("Monitores", "Monitores TEXT(255)")
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
                ("Imeis", "Imeis TEXT(255)")
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
                ("Local", "Local TEXT(100)")
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
                ("Senha", "Senha TEXT(150)")
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
                ("LocalAnterior", "LocalAnterior TEXT(100)")
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
                ("Local", "Local TEXT(100)")
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
                ("Local", "Local TEXT(100)")
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
                ("Local", "Local TEXT(100)")
            });
        });

        EnsureSafe("RelogiosPonto", EnsureRelogiosPontoTable);

        if (errors.Count > 0)
        {
            throw new AggregateException("Falha ao garantir as tabelas obrigatórias do Access.", errors);
        }

        InventoryLogger.Info(LoggerSource, "Verificação/criação do schema Access concluída.");
    }
}
