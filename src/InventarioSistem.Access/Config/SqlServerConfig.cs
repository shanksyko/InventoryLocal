using System;
using System.IO;
using System.Text.Json;

namespace InventarioSistem.Access.Config;

public class SqlServerConfig
{
    public string? ConnectionString { get; set; }
    public bool UseLocalDb { get; set; } = true; // Padrão: LocalDB

    private static string ConfigFile =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sqlserver.config.json");

    public static SqlServerConfig Load()
    {
        if (!File.Exists(ConfigFile))
        {
            // Primeira execução - usar LocalDB por padrão
            var config = new SqlServerConfig
            {
                ConnectionString = LocalDbManager.GetConnectionString(),
                UseLocalDb = true
            };
            config.Save();
            return config;
        }

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var config = JsonSerializer.Deserialize<SqlServerConfig>(json) ?? new SqlServerConfig();
            
            // Se não tem connection string, usar LocalDB
            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                config.ConnectionString = LocalDbManager.GetConnectionString();
                config.UseLocalDb = true;
                config.Save();
            }
            
            return config;
        }
        catch
        {
            return new SqlServerConfig
            {
                ConnectionString = LocalDbManager.GetConnectionString(),
                UseLocalDb = true
            };
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

