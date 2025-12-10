using System;
using System.IO;
using System.Text.Json;

namespace InventarioSistem.Access.Config;

public class SqlServerConfig
{
    public string? ConnectionString { get; set; }

    private static string ConfigFile =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sqlserver.config.json");

    public static SqlServerConfig Load()
    {
        if (!File.Exists(ConfigFile))
            return new SqlServerConfig();

        try
        {
            var json = File.ReadAllText(ConfigFile);
            return JsonSerializer.Deserialize<SqlServerConfig>(json) ?? new SqlServerConfig();
        }
        catch
        {
            return new SqlServerConfig();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigFile, json);
    }
}
