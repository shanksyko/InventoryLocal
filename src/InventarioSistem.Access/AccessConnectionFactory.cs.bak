using System.Data.Odbc;
using InventarioSistem.Access.Db;

namespace InventarioSistem.Access;

public class AccessConnectionFactory
{
    private readonly string? _databasePathOverride;

    public AccessConnectionFactory(string? databasePath = null, string? password = null)
    {
        _databasePathOverride = databasePath;
        Password = password;
    }

    public string DatabasePath => _databasePathOverride ?? AccessDatabaseManager.ResolveActiveDatabasePath();

    public string? Password { get; }

    public string ConnectionString
    {
        get
        {
            var passwordPart = string.IsNullOrEmpty(Password) ? string.Empty : $";PWD={Password}";
            return $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={DatabasePath}{passwordPart};Uid=Admin;";
        }
    }

    public OdbcConnection CreateConnection()
    {
        return new OdbcConnection(ConnectionString);
    }
}
