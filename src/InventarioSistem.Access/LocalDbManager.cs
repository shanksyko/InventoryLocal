using System;
using System.IO;
using System.Diagnostics;
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
            // Validar se LocalDB está disponível
            if (!IsLocalDbAvailable())
            {
                // Tentar criar a instância do LocalDB
                if (!EnsureLocalDbInstance())
                    throw new Exception("Não foi possível inicializar LocalDB. Verifique a instalação do .NET Runtime.");
            }

            // Criar diretório se não existir
            if (!Directory.Exists(LocalDbPath))
                Directory.CreateDirectory(LocalDbPath);

            // Tentar conexão - vai criar banco automaticamente se não existir
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                conn.Close();
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao inicializar LocalDB: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tenta garantir que a instância LocalDB existe
    /// </summary>
    private static bool EnsureLocalDbInstance()
    {
        try
        {
            // Tentar encontrar sqllocaldb.exe
            var sqlLocalDbPath = GetSqlLocalDbPath();
            if (string.IsNullOrEmpty(sqlLocalDbPath))
                return false;

            // Tentar criar instância se não existir
            var processInfo = new ProcessStartInfo
            {
                FileName = sqlLocalDbPath,
                Arguments = $"create {LocalDbInstanceName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                if (process != null)
                {
                    process.WaitForExit(5000);
                    return process.ExitCode == 0;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Localiza o executável sqllocaldb.exe
    /// </summary>
    private static string? GetSqlLocalDbPath()
    {
        // Procurar em locais padrão
        string[] possiblePaths = new[]
        {
            @"C:\Program Files\Microsoft SQL Server\160\Tools\Binn\sqllocaldb.exe",
            @"C:\Program Files (x86)\Microsoft SQL Server\160\Tools\Binn\sqllocaldb.exe",
            @"C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqllocaldb.exe",
            @"C:\Program Files (x86)\Microsoft SQL Server\150\Tools\Binn\sqllocaldb.exe",
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    /// <summary>
    /// Valida se LocalDB está disponível
    /// </summary>
    public static bool IsLocalDbAvailable()
    {
        try
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                conn.Close();
            }
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
