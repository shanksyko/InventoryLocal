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

    /// <summary>
    /// Cria um novo arquivo .mdf no caminho especificado com estrutura e usuário admin
    /// </summary>
    public static string CreateMdfDatabase(string mdfPath, Action<string>? logAction = null)
    {
        void Log(string msg) => logAction?.Invoke(msg);

        try
        {
            Log($"📄 Caminho MDF: {mdfPath}");
            var ldfPath = Path.ChangeExtension(mdfPath, ".ldf");
            Log($"📄 Caminho LDF: {ldfPath}");
            
            // Validar caminho
            var directory = Path.GetDirectoryName(mdfPath);
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Caminho inválido para o arquivo .mdf");

            // Criar diretório se não existir
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Log($"📁 Diretório criado: {directory}");
                }
                catch (Exception ex)
                {
                    Log($"⚠️  Erro ao criar diretório na primeira tentativa: {ex.Message}");
                    Log($"🔄 Tentando novamente com pausa...");
                    try
                    {
                        System.Threading.Thread.Sleep(500);
                        Directory.CreateDirectory(directory);
                        Log($"📁 Diretório criado (retry): {directory}");
                    }
                    catch (Exception retryEx)
                    {
                        Log($"❌ Falha ao criar diretório (ambas tentativas): {retryEx.Message}");
                        throw;
                    }
                }
            }

            var dbName = Path.GetFileNameWithoutExtension(mdfPath);

            // Se já existir o database, apenas reutiliza e garante schema/admin
            var createConnString = $"Data Source=(LocalDB)\\mssqllocaldb;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=30;";

            using (var conn = new SqlConnection(createConnString))
            {
                conn.Open();
                Log("✅ Conectado ao LocalDB");

                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandTimeout = 30;
                    checkCmd.CommandText = "SELECT db_id(@name)";
                    checkCmd.Parameters.AddWithValue("@name", dbName);
                    var dbIdObj = checkCmd.ExecuteScalar();
                    var exists = dbIdObj != null && dbIdObj != DBNull.Value;

                    if (exists)
                    {
                        var ldfPathCheck = Path.ChangeExtension(mdfPath, ".ldf");
                        var mdfExists = File.Exists(mdfPath);
                        var ldfExists = File.Exists(ldfPathCheck);

                        if (mdfExists && ldfExists)
                        {
                            Log("ℹ️  Banco já existia com arquivos físicos. Reutilizando e garantindo estrutura/usuário...");
                            var existingConn = $"Data Source=(LocalDB)\\mssqllocaldb;Database={dbName};Integrated Security=true;TrustServerCertificate=true;";
                            EnsureSchemaAndAdmin(existingConn, Log);
                            Log($"🔎 Database name: {dbName}");
                            return existingConn;
                        }
                        else
                        {
                            Log("⚠️  Banco consta na instância, mas arquivos .mdf/.ldf não existem. Recriando do zero...");
                            using (var dropCmd = conn.CreateCommand())
                            {
                                dropCmd.CommandTimeout = 60;
                                dropCmd.CommandText = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]";
                                try
                                {
                                    dropCmd.ExecuteNonQuery();
                                    Log("🗑️  Banco antigo removido da instância.");
                                }
                                catch (Exception ex)
                                {
                                    Log($"⚠️  Falha ao remover banco antigo: {ex.Message}. Tentando prosseguir com criação forçada...");
                                }
                            }
                        }
                    }
                }

                // Se arquivo já existe, deletar
                if (File.Exists(mdfPath))
                {
                    try
                    {
                        File.Delete(mdfPath);
                        Log("🗑️  Arquivo existente removido");
                    }
                    catch (IOException ex)
                    {
                        Log($"⚠️  Não foi possível remover arquivo: {ex.Message}");
                        // Continuar mesmo assim
                    }
                }

                // Também remover arquivo .ldf se existir
                ldfPath = Path.ChangeExtension(mdfPath, ".ldf");
                if (File.Exists(ldfPath))
                {
                    try
                    {
                        File.Delete(ldfPath);
                        Log("🗑️  Arquivo de log removido");
                    }
                    catch (IOException ex)
                    {
                        Log($"⚠️  Não foi possível remover .ldf: {ex.Message}");
                    }
                }

                Log("⚙️  Criando banco de dados...");

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 120; // 2 minutos para criar BD
                    cmd.CommandText = $@"
                        CREATE DATABASE [{dbName}]
                        ON PRIMARY (
                            NAME = {dbName}_Data,
                            FILENAME = '{mdfPath}'
                        )
                        LOG ON (
                            NAME = {dbName}_Log,
                            FILENAME = '{ldfPath}'
                        )";
                    cmd.ExecuteNonQuery();
                }
            }

            // Usar Database={dbName} para garantir schema/admin sem depender de Attach durante criação
            var ensureConn = $"Data Source=(LocalDB)\\mssqllocaldb;Database={dbName};Integrated Security=true;TrustServerCertificate=true;";
            Log("📊 Garantindo estrutura do banco (via Database)...");
            EnsureSchemaAndAdmin(ensureConn, Log);
            // Retornar AttachDbFileName para compatibilidade de runtime
            var finalConnString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={mdfPath};Integrated Security=true;TrustServerCertificate=true;";
            Log($"🔗 Conexão final (AttachDbFileName): {finalConnString}");
            Log("🎉 Banco de dados pronto para uso!");
            return finalConnString;
        }
        catch (Exception ex)
        {
            Log($"❌ Erro: {ex.Message}");
            throw new Exception($"Erro ao criar arquivo .mdf: {ex.Message}", ex);
        }
    }

    private static void EnsureSchemaAndAdmin(string connectionString, Action<string> Log)
    {
        try
        {
            // Criar estrutura de tabelas
            Log("📊 Criando/garantindo estrutura de tabelas...");
            var factory = new SqlServerConnectionFactory(connectionString);
            Schema.SqlServerSchemaManager.EnsureRequiredTables(factory);
            Log("✅ Estrutura ok");
        }
        catch (Exception ex)
        {
            Log($"⚠️  Erro ao garantir schema: {ex.Message}");
            // Continuar mesmo assim - talvez schema já exista
        }

        // Criar usuário admin
        Log("👤 Garantindo usuário administrador...");
        try
        {
            using var conn = new SqlConnection(connectionString + (connectionString.Contains("Connect Timeout") ? "" : ";Connect Timeout=30;"));
            conn.Open();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandTimeout = 30;
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
            var count = (int?)checkCmd.ExecuteScalar() ?? 0;

            if (count == 0)
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandTimeout = 30;
                insertCmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedAt, LastPasswordChange)
                    VALUES (@username, @passwordHash, @fullName, @role, 1, GETUTCDATE(), GETUTCDATE())";

                insertCmd.Parameters.AddWithValue("@username", "admin");
                insertCmd.Parameters.AddWithValue("@passwordHash", Core.Entities.User.HashPassword("L9l337643k#$"));
                insertCmd.Parameters.AddWithValue("@fullName", "Administrador");
                insertCmd.Parameters.AddWithValue("@role", "Admin");

                insertCmd.ExecuteNonQuery();
                Log("✅ Usuário admin criado (Usuário: admin | Senha: L9l337643k#$)");
            }
            else
            {
                using var updateCmd = conn.CreateCommand();
                updateCmd.CommandTimeout = 30;
                updateCmd.CommandText = @"
                    UPDATE Users
                    SET Role = 'Admin', IsActive = 1
                    WHERE Username = 'admin'";
                updateCmd.ExecuteNonQuery();
                Log("ℹ️  Usuário admin já existia — role/ativo garantidos (Admin / Ativo)");
            }
        }
        catch (Exception ex)
        {
            Log($"⚠️  Erro ao garantir admin: {ex.Message}");
            // Erro ao criar usuário não deve bloquear
        }
    }
}

