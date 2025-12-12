using System;
using Microsoft.Data.Sqlite;

namespace InventarioSistem.Access;

/// <summary>
/// Factory para conexões SQLite
/// </summary>
public class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
