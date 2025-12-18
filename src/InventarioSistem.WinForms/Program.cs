using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Logging;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms
{
    internal static class Program
    {
        private static SqlServerConnectionFactory? _sqlServerFactory;
        private static SqlServerUserStore? _sqlServerUserStore;

        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                // 🔧 ETAPA 1: INICIALIZAR LocalDB AUTOMATICAMENTE
                bool localDbAvailable = false;
                try
                {
                    if (!LocalDbManager.IsLocalDbAvailable())
                    {
                        // Tentar inicializar LocalDB
                        try
                        {
                            LocalDbManager.Initialize();
                            localDbAvailable = true;
                        }
                        catch
                        {
                            // LocalDB não disponível - usar SQL Server ou Arquivo
                            localDbAvailable = false;
                        }
                    }
                    else
                    {
                        localDbAvailable = true;
                    }

                    if (localDbAvailable)
                    {
                        InventoryLogger.Info("Program", LocalDbManager.GetInfo());
                    }
                }
                catch (Exception ex)
                {
                    InventoryLogger.Error("Program", $"LocalDB não disponível: {ex.Message}");
                    localDbAvailable = false;
                }

                // 🔧 ETAPA 2: CONFIGURAR MODO DE BANCO DE DADOS
                var sqlConfig = SqlServerConfig.Load();
                bool isFirstRun = string.IsNullOrWhiteSpace(sqlConfig.ConnectionString);
                bool localDbConfiguredButUnavailable = sqlConfig.UseLocalDb && !localDbAvailable;

                bool connectionInvalid = false;
                if (!isFirstRun)
                {
                    try
                    {
                        _ = new SqlConnectionStringBuilder(sqlConfig.ConnectionString);
                    }
                    catch
                    {
                        connectionInvalid = true;
                    }
                }

                // Só abre o configurador se não houver connection string ou se a configuração aponta para LocalDB e ele não estiver disponível.
                if (isFirstRun || localDbConfiguredButUnavailable || connectionInvalid)
                {
                    bool configured = false;
                    while (!configured)
                    {
                        using (var configForm = new DatabaseConfigForm())
                        {
                            if (configForm.ShowDialog() != DialogResult.OK)
                            {
                                var result = MessageBox.Show(
                                    "Você precisa configurar o banco de dados para usar a aplicação.\n\n" +
                                    "Deseja tentar novamente?",
                                    "Configuração Obrigatória",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.No)
                                {
                                    return;
                                }
                                continue;
                            }

                            var connString = configForm.GetConnectionString();
                            var mode = configForm.GetMode();
                            var useMdfCache = configForm.GetUseMdfCache();
                            var originalMdfPath = configForm.GetOriginalMdfPath();

                            if (!string.IsNullOrEmpty(connString))
                            {
                                try
                                {
                                    // Validar conexão
                                    using (var testConn = new SqlServerConnectionFactory(connString).CreateConnection())
                                    {
                                        testConn.Open();
                                    }

                                    var modeText = mode switch
                                    {
                                        "localdb" => "LocalDB (Automático)",
                                        "sqlserver" => "SQL Server",
                                        "filemdf" => "Arquivo .mdf",
                                        _ => "Desconhecido"
                                    };

                                    // Verificar se há dados no banco anterior
                                    var previousConnString = sqlConfig.ConnectionString;
                                    bool hasExistingData = false;
                                    if (!isFirstRun && !string.IsNullOrWhiteSpace(previousConnString))
                                    {
                                        try
                                        {
                                            hasExistingData = DatabaseConfigForm.HasExistingData(previousConnString);
                                        }
                                        catch { }
                                    }

                                    // Se há dados, oferecer migração
                                    if (hasExistingData
                                        && !string.IsNullOrWhiteSpace(previousConnString)
                                        && !string.Equals(previousConnString, connString, StringComparison.Ordinal))
                                    {
                                        var migrateResult = MessageBox.Show(
                                            $"Foram detectados dados no banco anterior.\n\n" +
                                            $"Deseja migrar os dados para o novo destino?\n\n" +
                                            $"Origem: {(sqlConfig.UseLocalDb ? "LocalDB" : "Outro banco")}\n" +
                                            $"Destino: {modeText}",
                                            "Migração de Dados",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);

                                        if (migrateResult == DialogResult.Yes)
                                        {
                                            using (var migrationForm = new DatabaseMigrationForm(
                                                previousConnString,
                                                connString))
                                            {
                                                migrationForm.ShowDialog();
                                            }
                                        }
                                    }

                                    // Salvar configuração
                                    sqlConfig.ConnectionString = connString;
                                    sqlConfig.UseLocalDb = (mode == "localdb" || mode == "filemdf");
                                    sqlConfig.UseMdfCache = useMdfCache;
                                    sqlConfig.OriginalMdfPath = originalMdfPath;
                                    sqlConfig.Save();
                                    configured = true;

                                    MessageBox.Show(
                                        $"✅ Configuração salva com sucesso!\n\n" +
                                        $"Modo: {modeText}\n\n" +
                                        $"A aplicação iniciará agora.",
                                        "Sucesso",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(
                                        $"❌ Erro ao validar conexão:\n\n{ex.Message}\n\n" +
                                        $"Verifique os dados e tente novamente.",
                                        "Erro de Conexão",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
                // Caso já esteja configurado, seguimos em frente sem abrir o configurador.

                // Se estiver configurado para MDF em rede com cache, garantir cache atualizado agora.
                if (sqlConfig.UseMdfCache && !string.IsNullOrWhiteSpace(sqlConfig.OriginalMdfPath))
                {
                    try
                    {
                        var cached = MdfCacheManager.EnsureCacheReady(sqlConfig.OriginalMdfPath, msg => InventoryLogger.Info("Program", msg));
                        sqlConfig.ConnectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={cached};Integrated Security=true;TrustServerCertificate=true;";
                        sqlConfig.UseLocalDb = true;
                        sqlConfig.Save();
                    }
                    catch (Exception ex)
                    {
                        InventoryLogger.Error("Program", $"Falha ao preparar cache do MDF: {ex.Message}");
                    }
                }

                // Sincronizar cache de volta ao sair (best-effort)
                Application.ApplicationExit += (_, _) =>
                {
                    try
                    {
                        if (sqlConfig.UseMdfCache && !string.IsNullOrWhiteSpace(sqlConfig.OriginalMdfPath))
                        {
                            var cachedMdf = MdfCacheManager.GetCachedMdfPath(sqlConfig.OriginalMdfPath);
                            MdfCacheManager.TrySyncBack(sqlConfig.OriginalMdfPath, cachedMdf, msg => InventoryLogger.Info("Program", msg));
                        }
                    }
                    catch
                    {
                        // best-effort
                    }
                };

                // 🗄️ ETAPA 3: INICIALIZAR FACTORY E USER STORE
                _sqlServerFactory = new SqlServerConnectionFactory(sqlConfig.ConnectionString);
                _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);

                // 🗄️ ETAPA 4: VALIDAR BANCO E CRIAR SCHEMA
                try
                {
                    SqlServerSchemaManager.EnsureRequiredTables(_sqlServerFactory);
                    InventoryLogger.Info("Program", "Banco de dados SQL Server inicializado com sucesso");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao inicializar banco de dados:\n\n{ex.Message}",
                        "Erro de Banco de Dados",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Check if any users exist, create default admin if needed
                try
                {
                    // Check for users (simplified check)
                    var adminUser = _sqlServerUserStore.GetUser("admin");
                    if (adminUser == null)
                    {
                        _sqlServerUserStore.CreateUser(
                            "admin",
                            "L9l337643k#$",
                            "Administrador",
                            true,
                            "Admin");

                        MessageBox.Show(
                            "Primeiro acesso detectado. Usuário padrão criado:\n\n" +
                            "Usuário: admin\n" +
                            "Senha: L9l337643k#$\n\n" +
                            "Altere a senha após o primeiro login.",
                            "Primeiro Acesso",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    InventoryLogger.Error("Program", $"Erro ao verificar/criar usuário padrão: {ex.Message}");
                }

                // Initialize inventory store
                var inventoryStore = new SqlServerInventoryStore(_sqlServerFactory);

                // Show login form
                using (var loginForm = new LoginForm(_sqlServerFactory, _sqlServerUserStore))
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // User cancelled login
                    }

                    var loggedInUser = LoginForm.LoggedInUser;
                    if (loggedInUser != null)
                    {
                        Application.Run(new MainForm(_sqlServerFactory, inventoryStore, _sqlServerUserStore, loggedInUser));
                    }
                    else
                    {
                        MessageBox.Show(
                            "Falha ao obter informações do usuário logado.",
                            "Erro",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao inicializar a aplicação:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Erro Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}


