using Microsoft.Data.SqlClient;
using InventarioSistem.Access.Config;

namespace InventarioSistem.Access;

public class SqlServerConnectionFactory
{
    private readonly string? _connectionStringOverride;

    public SqlServerConnectionFactory(string? connectionString = null)
    {
        _connectionStringOverride = connectionString;
    }

    public string ConnectionString
    {
        get
        {
            if (!string.IsNullOrEmpty(_connectionStringOverride))
                return _connectionStringOverride;

            var config = SqlServerConfig.Load();
            if (!string.IsNullOrEmpty(config.ConnectionString))
                return config.ConnectionString;

            throw new InvalidOperationException(
                "SQL Server connection string not configured. " +
                "Use SqlServerDatabaseManager.SetConnectionString() to configure.");
        }
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}
