using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using InventarioSistem.Access;

namespace InventarioSistem.Access.Schema;

/// <summary>
/// Responsável por garantir que o banco Access selecionado
/// contenha as tabelas padrão do InventarioSistem.
/// </summary>
public static class AccessSchemaManager
{
    private static readonly string[] RequiredTables =
    {
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

        void EnsureRelogiosPontoTable()
        {
            CreateTable("RelogiosPonto", @"
                CREATE TABLE RelogiosPonto (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Patrimonio TEXT(100),
                    Marca TEXT(100),
                    Modelo TEXT(100),
                    NumeroSerie TEXT(100),
                    Imei TEXT(30),
                    Responsavel TEXT(100),
                    Localizacao TEXT(100),
                    Observacoes MEMO,
                    AtualizadoEm DATETIME
                )
            ");

            EnsureColumns("RelogiosPonto", new (string, string)[]
            {
                ("Patrimonio", "Patrimonio TEXT(100)"),
                ("Marca", "Marca TEXT(100)"),
                ("Modelo", "Modelo TEXT(100)"),
                ("NumeroSerie", "NumeroSerie TEXT(100)"),
                ("Imei", "Imei TEXT(30)"),
                ("Responsavel", "Responsavel TEXT(100)"),
                ("Localizacao", "Localizacao TEXT(100)"),
                ("Observacoes", "Observacoes MEMO"),
                ("AtualizadoEm", "AtualizadoEm DATETIME")
            });
        }

        // Computadores
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
            ("Matricula", "Matricula TEXT(50)")
        });

        // Tablets
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

        // ColetoresAndroid
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

        // Celulares
        CreateTable("Celulares", @"
            CREATE TABLE Celulares (
                Id AUTOINCREMENT PRIMARY KEY,
                Hostname TEXT(100),
                Modelo TEXT(100),
                Numero TEXT(50),
                Proprietario TEXT(100),
                Imei1 TEXT(50),
                Imei2 TEXT(50)
            )
        ");

        EnsureColumns("Celulares", new (string, string)[]
        {
            ("Hostname", "Hostname TEXT(100)"),
            ("Modelo", "Modelo TEXT(100)"),
            ("Numero", "Numero TEXT(50)"),
            ("Proprietario", "Proprietario TEXT(100)"),
            ("Imei1", "Imei1 TEXT(50)"),
            ("Imei2", "Imei2 TEXT(50)")
        });

        // Impressoras
        CreateTable("Impressoras", @"
            CREATE TABLE Impressoras (
                Id AUTOINCREMENT PRIMARY KEY,
                Patrimonio TEXT(100),
                Marca TEXT(100),
                Modelo TEXT(100),
                NumeroSerie TEXT(100),
                Imei TEXT(30),
                Responsavel TEXT(100),
                Localizacao TEXT(100),
                Observacoes MEMO,
                AtualizadoEm DATETIME
            )
        ");

        EnsureColumns("Impressoras", new (string, string)[]
        {
            ("Patrimonio", "Patrimonio TEXT(100)"),
            ("Marca", "Marca TEXT(100)"),
            ("Modelo", "Modelo TEXT(100)"),
            ("NumeroSerie", "NumeroSerie TEXT(100)"),
            ("Imei", "Imei TEXT(30)"),
            ("Responsavel", "Responsavel TEXT(100)"),
            ("Localizacao", "Localizacao TEXT(100)"),
            ("Observacoes", "Observacoes MEMO"),
            ("AtualizadoEm", "AtualizadoEm DATETIME")
        });

        // Dects
        CreateTable("Dects", @"
            CREATE TABLE Dects (
                Id AUTOINCREMENT PRIMARY KEY,
                Patrimonio TEXT(100),
                Marca TEXT(100),
                Modelo TEXT(100),
                NumeroSerie TEXT(100),
                Imei TEXT(30),
                Responsavel TEXT(100),
                Localizacao TEXT(100),
                Observacoes MEMO,
                AtualizadoEm DATETIME
            )
        ");

        EnsureColumns("Dects", new (string, string)[]
        {
            ("Patrimonio", "Patrimonio TEXT(100)"),
            ("Marca", "Marca TEXT(100)"),
            ("Modelo", "Modelo TEXT(100)"),
            ("NumeroSerie", "NumeroSerie TEXT(100)"),
            ("Imei", "Imei TEXT(30)"),
            ("Responsavel", "Responsavel TEXT(100)"),
            ("Localizacao", "Localizacao TEXT(100)"),
            ("Observacoes", "Observacoes MEMO"),
            ("AtualizadoEm", "AtualizadoEm DATETIME")
        });

        // TelefonesCisco
        CreateTable("TelefonesCisco", @"
            CREATE TABLE TelefonesCisco (
                Id AUTOINCREMENT PRIMARY KEY,
                Patrimonio TEXT(100),
                Marca TEXT(100),
                Modelo TEXT(100),
                NumeroSerie TEXT(100),
                Imei TEXT(30),
                Responsavel TEXT(100),
                Localizacao TEXT(100),
                Observacoes MEMO,
                AtualizadoEm DATETIME
            )
        ");

        EnsureColumns("TelefonesCisco", new (string, string)[]
        {
            ("Patrimonio", "Patrimonio TEXT(100)"),
            ("Marca", "Marca TEXT(100)"),
            ("Modelo", "Modelo TEXT(100)"),
            ("NumeroSerie", "NumeroSerie TEXT(100)"),
            ("Imei", "Imei TEXT(30)"),
            ("Responsavel", "Responsavel TEXT(100)"),
            ("Localizacao", "Localizacao TEXT(100)"),
            ("Observacoes", "Observacoes MEMO"),
            ("AtualizadoEm", "AtualizadoEm DATETIME")
        });

        // Televisores
        CreateTable("Televisores", @"
            CREATE TABLE Televisores (
                Id AUTOINCREMENT PRIMARY KEY,
                Hostname TEXT(100),
                Modelo TEXT(100),
                NumeroSerie TEXT(100),
                Local TEXT(100),
                Responsavel TEXT(100)
            )
        ");

        EnsureColumns("Televisores", new (string, string)[]
        {
            ("Hostname", "Hostname TEXT(100)"),
            ("Modelo", "Modelo TEXT(100)"),
            ("NumeroSerie", "NumeroSerie TEXT(100)"),
            ("Local", "Local TEXT(100)"),
            ("Responsavel", "Responsavel TEXT(100)")
        });

        EnsureRelogiosPontoTable();
    }
}
