using System;
using System.IO;
using System.Text.Json;

namespace InventarioSistem.Access.Config;

public class AccessConfig
{
    public string? DatabasePath { get; set; }

    private static string ConfigFile =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public static AccessConfig Load()
    {
        if (!File.Exists(ConfigFile))
            return new AccessConfig();

        var json = File.ReadAllText(ConfigFile);
        return JsonSerializer.Deserialize<AccessConfig>(json) ?? new AccessConfig();
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
