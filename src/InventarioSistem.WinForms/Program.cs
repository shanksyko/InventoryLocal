using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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

                // Se LocalDB não está disponível ou é primeira execução, mostrar formulário
                if (!localDbAvailable || isFirstRun)
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
                                    bool hasExistingData = false;
                                    if (!isFirstRun && !string.IsNullOrWhiteSpace(sqlConfig.ConnectionString))
                                    {
                                        try
                                        {
                                            hasExistingData = DatabaseConfigForm.HasExistingData(sqlConfig.ConnectionString);
                                        }
                                        catch { }
                                    }

                                    // Se há dados, oferecer migração
                                    if (hasExistingData && sqlConfig.ConnectionString != connString)
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
                                                sqlConfig.ConnectionString,
                                                connString))
                                            {
                                                migrationForm.ShowDialog();
                                            }
                                        }
                                    }

                                    // Salvar configuração
                                    sqlConfig.ConnectionString = connString;
                                    sqlConfig.UseLocalDb = (mode == "localdb");
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
                else
                {
                    // LocalDB está disponível e configurado
                    sqlConfig.ConnectionString = LocalDbManager.GetConnectionString();
                    sqlConfig.UseLocalDb = true;
                }

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
                        Application.Run(new MainForm(_sqlServerFactory, inventoryStore, _sqlServerUserStore));
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


