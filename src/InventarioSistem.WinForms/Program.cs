using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
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
                // Initialize SQL Server infrastructure
                var sqlConfig = new SqlServerConfig();
                _sqlServerFactory = new SqlServerConnectionFactory(sqlConfig);
                _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);

                // Test connection and initialize database if needed
                try
                {
                    var dbManager = new SqlServerDatabaseManager(sqlConfig);
                    
                    // Test if connection string is configured
                    if (string.IsNullOrWhiteSpace(sqlConfig.GetConnectionString()))
                    {
                        // Database not configured - show database setup dialog
                        ShowDatabaseSetupDialog(sqlConfig, dbManager);
                    }

                    // Ensure schema is created
                    var schemaManager = new SqlServerSchemaManager(_sqlServerFactory);
                    schemaManager.EnsureRequiredTables();
                    
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
                            true);

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

                // Show login form (placeholder - needs migration of LoginForm)
                // For now, create a main form with a default logged-in user
                try
                {
                    // TODO: Create SqlServerLoginForm
                    // For now, bypass login and use admin user
                    var inventoryStore = new SqlServerInventoryStore(_sqlServerFactory);
                    Application.Run(new MainForm(_sqlServerFactory, inventoryStore, _sqlServerUserStore));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao abrir formulário principal:\n\n{ex.Message}",
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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

        private static void ShowDatabaseSetupDialog(SqlServerConfig config, SqlServerDatabaseManager dbManager)
        {
            var result = MessageBox.Show(
                "Banco de dados SQL Server não configurado.\n\n" +
                "Deseja configurar agora?",
                "Configuração de Banco de Dados",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // TODO: Create database configuration dialog
                MessageBox.Show(
                    "Funcionalidade de configuração será adicionada em breve.\n\n" +
                    "Por favor, configure a string de conexão manualmente no arquivo sqlserver.config.json",
                    "Configuração Manual",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}

