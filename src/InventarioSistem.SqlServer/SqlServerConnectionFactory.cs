using System;
using System.Data.SqlClient;

namespace InventarioSistem.SqlServer;

/// <summary>
/// Factory para criar conexões com SQL Server Express.
/// Gerencia a connection string e valida a conexão.
/// </summary>
public class SqlServerConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string server, string database, string userId = "sa", string password = "")
    {
        if (string.IsNullOrWhiteSpace(server))
            throw new ArgumentException("Servidor SQL não pode ser vazio.", nameof(server));
        
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentException("Nome do banco não pode ser vazio.", nameof(database));

        _connectionString = BuildConnectionString(server, database, userId, password);
    }

    /// <summary>
    /// Factory alternativo usando connection string pronta.
    /// </summary>
    public SqlServerConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string não pode ser vazia.", nameof(connectionString));
        
        _connectionString = connectionString;
    }

    public string ConnectionString => _connectionString;

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Testa a conexão com o servidor SQL.
    /// </summary>
    public bool TestConnection()
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Constrói a connection string com parâmetros de segurança.
    /// </summary>
    private static string BuildConnectionString(string server, string database, string userId, string password)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            UserID = userId,
            Password = password,
            ConnectTimeout = 15,
            Encrypt = true,
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        return builder.ConnectionString;
    }
}
