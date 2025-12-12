using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access.Schema;

/// <summary>
/// Responsável por garantir que o banco SQL Server
/// contenha as tabelas padrão do InventarioSistem.
/// </summary>
public static class SqlServerSchemaManager
{
    private const string LoggerSource = "SqlServerSchema";

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
        "RelogiosPonto",
        "Monitores",
        "Nobreaks",
        "Users"
    };

    /// <summary>
    /// Cria as tabelas obrigatórias que ainda não existirem.
    /// </summary>
    public static void EnsureRequiredTables(SqlServerConnectionFactory factory)
    {
        InventoryLogger.Info(LoggerSource, "Iniciando verificação/criação do schema SQL Server.");

        var errors = new List<Exception>();

        using var conn = factory.CreateConnection();
        conn.Open();

        bool TableExists(string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @tableName";
            cmd.Parameters.AddWithValue("@tableName", tableName);
            return ((int?)cmd.ExecuteScalar() ?? 0) > 0;
        }

        bool ColumnExists(string tableName, string columnName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";
            cmd.Parameters.AddWithValue("@tableName", tableName);
            cmd.Parameters.AddWithValue("@columnName", columnName);
            return ((int?)cmd.ExecuteScalar() ?? 0) > 0;
        }

        void CreateTable(string name, string createSql)
        {
            if (TableExists(name))
                return;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }

        void EnsureColumns(string tableName, IEnumerable<(string ColumnName, string Definition)> columns)
        {
            foreach (var (columnName, definition) in columns)
            {
                if (ColumnExists(tableName, columnName))
                    continue;

                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE [{tableName}] ADD {definition}";
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

        // Criar tabelas
        EnsureSafe("Computadores", () =>
        {
            CreateTable("Computadores", @"
                CREATE TABLE [Computadores] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Host] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [Proprietario] VARCHAR(100),
                    [Departamento] VARCHAR(100),
                    [Matricula] VARCHAR(50),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Computadores", new (string, string)[]
            {
                ("Host", "[Host] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("Proprietario", "[Proprietario] VARCHAR(100)"),
                ("Departamento", "[Departamento] VARCHAR(100)"),
                ("Matricula", "[Matricula] VARCHAR(50)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Tablets", () =>
        {
            CreateTable("Tablets", @"
                CREATE TABLE [Tablets] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Host] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [Local] VARCHAR(100),
                    [Responsavel] VARCHAR(100),
                    [Imeis] VARCHAR(MAX),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Tablets", new (string, string)[]
            {
                ("Host", "[Host] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("Responsavel", "[Responsavel] VARCHAR(100)"),
                ("Imeis", "[Imeis] VARCHAR(MAX)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("ColetoresAndroid", () =>
        {
            CreateTable("ColetoresAndroid", @"
                CREATE TABLE [ColetoresAndroid] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Host] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [MacAddress] VARCHAR(50),
                    [IpAddress] VARCHAR(50),
                    [Local] VARCHAR(100),
                    [CreatedAt] DATETIME,
                    [AppGwsFg] BIT,
                    [AppGwsRm] BIT,
                    [AppInspection] BIT,
                    [AppCuringTbr] BIT,
                    [AppCuringPcr] BIT,
                    [AppInspectionTbr] BIT,
                    [AppQuimico] BIT,
                    [AppBuildingTbr] BIT,
                    [AppBuildingPcr] BIT,
                    [OsWinCe] BIT,
                    [OsAndroid81] BIT,
                    [OsAndroid10] BIT
                )
            ");

            EnsureColumns("ColetoresAndroid", new (string, string)[]
            {
                ("Host", "[Host] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("MacAddress", "[MacAddress] VARCHAR(50)"),
                ("IpAddress", "[IpAddress] VARCHAR(50)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME"),
                ("AppGwsFg", "[AppGwsFg] BIT"),
                ("AppGwsRm", "[AppGwsRm] BIT"),
                ("AppInspection", "[AppInspection] BIT"),
                ("AppCuringTbr", "[AppCuringTbr] BIT"),
                ("AppCuringPcr", "[AppCuringPcr] BIT"),
                ("AppInspectionTbr", "[AppInspectionTbr] BIT"),
                ("AppQuimico", "[AppQuimico] BIT"),
                ("AppBuildingTbr", "[AppBuildingTbr] BIT"),
                ("AppBuildingPcr", "[AppBuildingPcr] BIT"),
                ("OsWinCe", "[OsWinCe] BIT"),
                ("OsAndroid81", "[OsAndroid81] BIT"),
                ("OsAndroid10", "[OsAndroid10] BIT")
            });
        });

        EnsureSafe("Celulares", () =>
        {
            CreateTable("Celulares", @"
                CREATE TABLE [Celulares] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [CellName] VARCHAR(100),
                    [Imei1] VARCHAR(50),
                    [Imei2] VARCHAR(50),
                    [Modelo] VARCHAR(100),
                    [Numero] VARCHAR(50),
                    [Roaming] BIT,
                    [Usuario] VARCHAR(100),
                    [Matricula] VARCHAR(50),
                    [Cargo] VARCHAR(100),
                    [Setor] VARCHAR(100),
                    [Email] VARCHAR(150),
                    [Senha] VARCHAR(150),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Celulares", new (string, string)[]
            {
                ("CellName", "[CellName] VARCHAR(100)"),
                ("Imei1", "[Imei1] VARCHAR(50)"),
                ("Imei2", "[Imei2] VARCHAR(50)"),
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("Numero", "[Numero] VARCHAR(50)"),
                ("Roaming", "[Roaming] BIT"),
                ("Usuario", "[Usuario] VARCHAR(100)"),
                ("Matricula", "[Matricula] VARCHAR(50)"),
                ("Cargo", "[Cargo] VARCHAR(100)"),
                ("Setor", "[Setor] VARCHAR(100)"),
                ("Email", "[Email] VARCHAR(150)"),
                ("Senha", "[Senha] VARCHAR(150)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Impressoras", () =>
        {
            CreateTable("Impressoras", @"
                CREATE TABLE [Impressoras] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Nome] VARCHAR(200),
                    [TipoModelo] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [LocalAtual] VARCHAR(100),
                    [LocalAnterior] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Impressoras", new (string, string)[]
            {
                ("Nome", "[Nome] VARCHAR(200)"),
                ("TipoModelo", "[TipoModelo] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("LocalAtual", "[LocalAtual] VARCHAR(100)"),
                ("LocalAnterior", "[LocalAnterior] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Dects", () =>
        {
            CreateTable("Dects", @"
                CREATE TABLE [Dects] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Responsavel] VARCHAR(100),
                    [Ipei] VARCHAR(100),
                    [MacAddress] VARCHAR(50),
                    [Numero] VARCHAR(50),
                    [Local] VARCHAR(100),
                    [Modelo] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Dects", new (string, string)[]
            {
                ("Responsavel", "[Responsavel] VARCHAR(100)"),
                ("Ipei", "[Ipei] VARCHAR(100)"),
                ("MacAddress", "[MacAddress] VARCHAR(50)"),
                ("Numero", "[Numero] VARCHAR(50)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("TelefonesCisco", () =>
        {
            CreateTable("TelefonesCisco", @"
                CREATE TABLE [TelefonesCisco] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Responsavel] VARCHAR(100),
                    [MacAddress] VARCHAR(50),
                    [Numero] VARCHAR(50),
                    [Local] VARCHAR(100),
                    [IpAddress] VARCHAR(50),
                    [Serial] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("TelefonesCisco", new (string, string)[]
            {
                ("Responsavel", "[Responsavel] VARCHAR(100)"),
                ("MacAddress", "[MacAddress] VARCHAR(50)"),
                ("Numero", "[Numero] VARCHAR(50)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("IpAddress", "[IpAddress] VARCHAR(50)"),
                ("Serial", "[Serial] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Televisores", () =>
        {
            CreateTable("Televisores", @"
                CREATE TABLE [Televisores] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Modelo] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [Local] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Televisores", new (string, string)[]
            {
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("RelogiosPonto", () =>
        {
            CreateTable("RelogiosPonto", @"
                CREATE TABLE [RelogiosPonto] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Modelo] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [Local] VARCHAR(100),
                    [Ip] VARCHAR(50),
                    [DataBateria] DATETIME,
                    [DataNobreak] DATETIME,
                    [ProximasVerificacoes] DATETIME,
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("RelogiosPonto", new (string, string)[]
            {
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("Ip", "[Ip] VARCHAR(50)"),
                ("DataBateria", "[DataBateria] DATETIME"),
                ("DataNobreak", "[DataNobreak] DATETIME"),
                ("ProximasVerificacoes", "[ProximasVerificacoes] DATETIME"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Monitores", () =>
        {
            CreateTable("Monitores", @"
                CREATE TABLE [Monitores] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Modelo] VARCHAR(100),
                    [SerialNumber] VARCHAR(100),
                    [Local] VARCHAR(100),
                    [Responsavel] VARCHAR(100),
                    [ComputadorVinculado] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Monitores", new (string, string)[]
            {
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("Responsavel", "[Responsavel] VARCHAR(100)"),
                ("ComputadorVinculado", "[ComputadorVinculado] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Nobreaks", () =>
        {
            CreateTable("Nobreaks", @"
                CREATE TABLE [Nobreaks] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Hostname] VARCHAR(100),
                    [Local] VARCHAR(100),
                    [IpAddress] VARCHAR(50),
                    [Modelo] VARCHAR(100),
                    [Status] VARCHAR(50),
                    [SerialNumber] VARCHAR(100),
                    [CreatedAt] DATETIME
                )
            ");

            EnsureColumns("Nobreaks", new (string, string)[]
            {
                ("Hostname", "[Hostname] VARCHAR(100)"),
                ("Local", "[Local] VARCHAR(100)"),
                ("IpAddress", "[IpAddress] VARCHAR(50)"),
                ("Modelo", "[Modelo] VARCHAR(100)"),
                ("Status", "[Status] VARCHAR(50)"),
                ("SerialNumber", "[SerialNumber] VARCHAR(100)"),
                ("CreatedAt", "[CreatedAt] DATETIME")
            });
        });

        EnsureSafe("Users", () =>
        {
            CreateTable("Users", @"
                CREATE TABLE [Users] (
                    [Id] INT PRIMARY KEY IDENTITY(1,1),
                    [Username] VARCHAR(100) UNIQUE NOT NULL,
                    [PasswordHash] VARCHAR(MAX) NOT NULL,
                    [FullName] VARCHAR(200),
                    [Role] VARCHAR(50) NOT NULL,
                    [IsActive] BIT DEFAULT 1,
                    [CreatedAt] DATETIME DEFAULT GETUTCDATE(),
                    [LastLogin] DATETIME,
                    [LastPasswordChange] DATETIME
                )
            ");

            EnsureColumns("Users", new (string, string)[]
            {
                ("Username", "[Username] VARCHAR(100)"),
                ("PasswordHash", "[PasswordHash] VARCHAR(MAX)"),
                ("FullName", "[FullName] VARCHAR(200)"),
                ("Role", "[Role] VARCHAR(50)"),
                ("IsActive", "[IsActive] BIT"),
                ("CreatedAt", "[CreatedAt] DATETIME"),
                ("LastLogin", "[LastLogin] DATETIME"),
                ("LastPasswordChange", "[LastPasswordChange] DATETIME")
            });

            // Criar usuário admin se não existir
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
            var adminExists = ((int?)checkCmd.ExecuteScalar() ?? 0) > 0;

            if (!adminExists)
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedAt, LastPasswordChange)
                    VALUES (@username, @passwordHash, @fullName, @role, 1, GETUTCDATE(), GETUTCDATE())";
                
                insertCmd.Parameters.AddWithValue("@username", "admin");
                insertCmd.Parameters.AddWithValue("@passwordHash", Core.Entities.User.HashPassword("L9l337643k#$"));
                insertCmd.Parameters.AddWithValue("@fullName", "Administrador");
                insertCmd.Parameters.AddWithValue("@role", "Admin");
                
                insertCmd.ExecuteNonQuery();
                InventoryLogger.Info(LoggerSource, "Usuário admin criado com sucesso.");
            }
        });

        if (errors.Count > 0)
        {
            throw new AggregateException("Falha ao garantir as tabelas obrigatórias do SQL Server.", errors);
        }

        InventoryLogger.Info(LoggerSource, "Verificação/criação do schema SQL Server concluída.");
    }
}
