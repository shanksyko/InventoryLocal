using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace InventarioSistem.Access;

/// <summary>
/// Gerenciador de SQL Server LocalDB (sem instalação necessária)
/// LocalDB é uma versão leve do SQL Server que vem com .NET
/// </summary>
public static class LocalDbManager
{
    private const string LocalDbInstanceName = "InventoryLocal";
    private const string DatabaseName = "InventoryLocal";
    private static readonly string LocalDbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "InventoryLocal"
    );

    /// <summary>
    /// Gera a connection string para LocalDB
    /// </summary>
    public static string GetConnectionString()
    {
        var dbPath = Path.Combine(LocalDbPath, $"{DatabaseName}.mdf");
        return $"Server=(localdb)\\{LocalDbInstanceName};AttachDbFileName={dbPath};Integrated Security=true;TrustServerCertificate=true;";
    }

    /// <summary>
    /// Inicializa o LocalDB - cria banco automaticamente se não existir
    /// </summary>
    public static bool Initialize()
    {
        try
        {
            // Criar diretório se não existir
            if (!Directory.Exists(LocalDbPath))
                Directory.CreateDirectory(LocalDbPath);

            // Tentar conexão - vai criar banco automaticamente se não existir
            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            conn.Close();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao inicializar LocalDB: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Valida se LocalDB está disponível
    /// </summary>
    public static bool IsLocalDbAvailable()
    {
        try
        {
            using var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            conn.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Caminho onde o banco de dados será armazenado
    /// </summary>
    public static string GetDatabasePath()
    {
        return Path.Combine(LocalDbPath, $"{DatabaseName}.mdf");
    }

    /// <summary>
    /// Informações sobre o LocalDB
    /// </summary>
    public static string GetInfo()
    {
        return $@"
╔════════════════════════════════════════════════════════════════╗
║                   Informações do LocalDB                        ║
╚════════════════════════════════════════════════════════════════╝

📁 Caminho do banco:        {GetDatabasePath()}
🗄️  Nome da instância:       (localdb)\{LocalDbInstanceName}
📊 Nome do banco:            {DatabaseName}
✅ Integrado Security:       Sim (sem usuario/senha necessário)

🎯 Vantagens:
   • Sem instalação necessária
   • Já vem com .NET
   • Banco local no computador
   • Perfito para desenvolvimento
   • Zero configuração

⚙️  Compatível com:
   • Entity Framework Core
   • Dapper
   • Microsoft.Data.SqlClient
   • SQL Server Management Studio
";
    }
}
