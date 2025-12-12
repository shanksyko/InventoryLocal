using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace InventarioSistem.Access;

/// <summary>
/// Gerenciador SQLite - Banco de dados embarcado, sem instalação necessária
/// </summary>
public static class SqliteDbManager
{
    private static readonly string SqlitePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "InventoryLocal"
    );

    private const string DatabaseFile = "InventoryLocal.db";

    /// <summary>
    /// Obtém a connection string para SQLite
    /// </summary>
    public static string GetConnectionString()
    {
        var dbPath = Path.Combine(SqlitePath, DatabaseFile);
        return $"Data Source={dbPath};";
    }

    /// <summary>
    /// Cria banco SQLite e schema (usa AppData local por padrão)
    /// </summary>
    public static string CreateSqliteDatabase(Action<string>? logAction = null)
    {
        return CreateSqliteDatabase(null, logAction);
    }

    /// <summary>
    /// Cria banco SQLite e schema em caminho customizado (local ou rede)
    /// </summary>
    public static string CreateSqliteDatabase(string? customDbPath, Action<string>? logAction = null)
    {
        void Log(string msg) => logAction?.Invoke(msg);

        try
        {
            string dbPath;
            
            if (!string.IsNullOrWhiteSpace(customDbPath))
            {
                // Validar e usar caminho customizado
                dbPath = customDbPath;
                
                // Se for diretório, adicionar nome padrão
                if (Directory.Exists(dbPath) || !dbPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    dbPath = Path.Combine(dbPath, DatabaseFile);
                }
                
                // Criar diretório se não existir
                var dir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Log($"📁 Diretório criado: {dir}");
                }
            }
            else
            {
                // Usar AppData local (padrão)
                if (!Directory.Exists(SqlitePath))
                {
                    Directory.CreateDirectory(SqlitePath);
                    Log($"📁 Diretório criado: {SqlitePath}");
                }
                
                dbPath = Path.Combine(SqlitePath, DatabaseFile);
            }

            Log($"📄 Banco SQLite: {dbPath}");

            // Validar permissão de escrita
            try
            {
                var testFile = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch
            {
                throw new UnauthorizedAccessException($"Sem permissão de escrita em: {Path.GetDirectoryName(dbPath)}");
            }

            // Criar/conectar ao banco
            var connString = $"Data Source={dbPath};";
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();
                Log("✅ Conectado ao SQLite");

                // Criar tabelas (compatível com schema SQL Server)
                var factory = new SqliteConnectionFactory(connString);
                Schema.SqliteSchemaManager.EnsureRequiredTables(factory);
                Log("✅ Estrutura criada");

                // Criar usuário admin
                EnsureAdmin(conn, Log);

                conn.Close();
            }

            Log($"🔗 Connection String: {connString}");
            Log("🎉 Banco SQLite pronto para uso!");
            return connString;
        }
        catch (Exception ex)
        {
            Log($"❌ Erro: {ex.Message}");
            throw;
        }
    }

    private static void EnsureAdmin(SqliteConnection conn, Action<string> Log)
    {
        try
        {
            Log("👤 Garantindo usuário admin...");
            
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
            var count = (int?)(long?)checkCmd.ExecuteScalar() ?? 0;

            if (count == 0)
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedAt, LastPasswordChange)
                    VALUES (@username, @passwordHash, @fullName, @role, 1, datetime('now'), datetime('now'))";

                insertCmd.Parameters.AddWithValue("@username", "admin");
                insertCmd.Parameters.AddWithValue("@passwordHash", Core.Entities.User.HashPassword("L9l337643k#$"));
                insertCmd.Parameters.AddWithValue("@fullName", "Administrador");
                insertCmd.Parameters.AddWithValue("@role", "Admin");

                insertCmd.ExecuteNonQuery();
                Log("✅ Usuário admin criado (admin / L9l337643k#$)");
            }
        }
        catch (Exception ex)
        {
            Log($"⚠️  Erro ao criar admin: {ex.Message}");
        }
    }

    public static string GetDatabasePath()
    {
        return Path.Combine(SqlitePath, DatabaseFile);
    }
}
