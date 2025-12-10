using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Schema;

namespace InventarioSistem.Access.Db;

public static class SqlServerDatabaseManager
{
    /// <summary>
    /// Define a connection string do SQL Server e persiste em sqlserver.config.json.
    /// </summary>
    public static void SetConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        var config = new SqlServerConfig
        {
            ConnectionString = connectionString
        };
        config.Save();

        // Garante que o banco tenha toda a estrutura obrigatória
        SqlServerSchemaManager.EnsureRequiredTables(new SqlServerConnectionFactory(connectionString));
    }

    /// <summary>
    /// Cria um novo banco de dados no SQL Server.
    /// Formato recomendado para connectionString: "Server=localhost\SQLEXPRESS;Integrated Security=true;"
    /// </summary>
    public static string CreateNewDatabase(string masterConnectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(masterConnectionString))
            throw new ArgumentException("Master connection string cannot be empty.", nameof(masterConnectionString));

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name cannot be empty.", nameof(databaseName));

        // Valida nome do banco (apenas alphanumericos, underscore, hífen)
        if (!Regex.IsMatch(databaseName, @"^[a-zA-Z0-9_\-]+$"))
            throw new ArgumentException("Database name contains invalid characters.", nameof(databaseName));

        try
        {
            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                // Verifica se o banco já existe
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = 
                        $"SELECT COUNT(*) FROM sys.databases WHERE name = N'{SanitizeSqlString(databaseName)}'";
                    var result = (int?)checkCommand.ExecuteScalar() ?? 0;

                    if (result > 0)
                        throw new InvalidOperationException($"Database '{databaseName}' already exists.");
                }

                // Cria o banco
                using (var createCommand = connection.CreateCommand())
                {
                    createCommand.CommandText = $"CREATE DATABASE [{databaseName}]";
                    createCommand.ExecuteNonQuery();
                }
            }

            // Constrói a connection string para o novo banco
            var newConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = databaseName
            }.ConnectionString;

            // Define como ativo
            SetConnectionString(newConnectionString);

            // Cria as tabelas padrão
            SqlServerSchemaManager.EnsureRequiredTables(new SqlServerConnectionFactory(newConnectionString));

            return newConnectionString;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create database '{databaseName}'.", ex);
        }
    }

    /// <summary>
    /// Retorna um resumo textual do banco: contagem de registros por tabela principal.
    /// </summary>
    public static string GetDatabaseSummary(string connectionString)
    {
        var sb = new StringBuilder();

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var tableNames = new[]
                {
                    "Computadores", "Tablets", "ColetoresAndroid", "Celulares",
                    "Impressoras", "Dects", "TelefonesCisco", "Televisores",
                    "RelogiosPonto", "Monitores", "Nobreaks"
                };

                sb.AppendLine($"SQL Server Database Summary");
                sb.AppendLine($"Connection: {connection.DataSource}");
                sb.AppendLine($"Database: {connection.Database}");
                sb.AppendLine();

                foreach (var tableName in tableNames)
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
                            var count = (int?)command.ExecuteScalar() ?? 0;
                            sb.AppendLine($"{tableName}: {count} registros");
                        }
                    }
                    catch
                    {
                        sb.AppendLine($"{tableName}: N/A (tabela não existe)");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Erro ao obter resumo: {ex.Message}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Testa a conexão com SQL Server.
    /// </summary>
    public static bool TestConnection(string connectionString)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitiza string para SQL (proteção básica contra SQL injection).
    /// </summary>
    private static string SanitizeSqlString(string input)
    {
        return input.Replace("'", "''");
    }
}
