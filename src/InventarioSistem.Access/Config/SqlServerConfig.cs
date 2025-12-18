using System;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access.Config;

public class SqlServerConfig
{
    public string? ConnectionString { get; set; }
    public bool UseLocalDb { get; set; } = true; // Padrão: LocalDB

    // Quando o usuário escolhe MDF em rede, usamos cache local e sincronizamos.
    public bool UseMdfCache { get; set; }
    public string? OriginalMdfPath { get; set; }

    private const string ConfigFileName = "sqlserver.config.json";

    // Local gravável por usuário (evita falhas de permissão quando o app está em Program Files / diretório protegido)
    private static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InventoryLocal");

    private static string ConfigFile => Path.Combine(ConfigDirectory, ConfigFileName);

    // Local legado (versões antigas): ao lado do executável
    private static string LegacyConfigFile =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

    public static SqlServerConfig Load()
    {
        // Compatibilidade: se existir config legado, prefere ler dele.
        // A partir daqui, Save() sempre grava no caminho novo.
        var effectiveConfigPath = File.Exists(ConfigFile) ? ConfigFile : (File.Exists(LegacyConfigFile) ? LegacyConfigFile : ConfigFile);

        if (!File.Exists(effectiveConfigPath))
        {
            // Primeira execução - usar LocalDB por padrão
            var config = new SqlServerConfig
            {
                ConnectionString = LocalDbManager.GetConnectionString(),
                UseLocalDb = true
            };
            config.Save();
            InventoryLogger.Info("SqlServerConfig", "Arquivo de configuração criado com LocalDB padrão");
            return config;
        }

        try
        {
            var json = File.ReadAllText(effectiveConfigPath);
            var config = JsonSerializer.Deserialize<SqlServerConfig>(json) ?? new SqlServerConfig();
            
            // Se não tem connection string, usar LocalDB
            if (string.IsNullOrEmpty(config.ConnectionString))
            {
                InventoryLogger.Warn("SqlServerConfig", "Connection string vazia no config — usando LocalDB padrão");
                config.ConnectionString = LocalDbManager.GetConnectionString();
                config.UseLocalDb = true;
                config.Save();
                return config;
            }

            // Validar formato da connection string
            try
            {
                _ = new SqlConnectionStringBuilder(config.ConnectionString);
                InventoryLogger.Info("SqlServerConfig", $"Connection string carregada e validada (UseLocalDb={config.UseLocalDb})");
                return config;
            }
            catch (Exception validationEx)
            {
                InventoryLogger.Error("SqlServerConfig", 
                    $"Connection string carregada do config é MALFORMADA: {validationEx.Message}\n" +
                    $"String: {config.ConnectionString.Substring(0, Math.Min(80, config.ConnectionString.Length))}...",
                    validationEx);
                
                // Restaurar LocalDB como fallback
                config.ConnectionString = LocalDbManager.GetConnectionString();
                config.UseLocalDb = true;
                config.Save();
                InventoryLogger.Warn("SqlServerConfig", "Config resetado para LocalDB como fallback");
                return config;
            }
        }
        catch (Exception loadEx)
        {
            InventoryLogger.Error("SqlServerConfig", $"Erro ao carregar config: {loadEx.Message}", loadEx);
            return new SqlServerConfig
            {
                ConnectionString = LocalDbManager.GetConnectionString(),
                UseLocalDb = true
            };
        }
    }

    public void Save()
    {
        // Garantir diretório gravável
        Directory.CreateDirectory(ConfigDirectory);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigFile, json);
    }
}

