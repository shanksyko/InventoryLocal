using System.Data.Odbc;

namespace InventarioSistem.Access;

public class AccessConnectionFactory
{
    public AccessConnectionFactory(string databasePath, string? password = null)
    {
        DatabasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        Password = password;
    }

    public string DatabasePath { get; }

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
