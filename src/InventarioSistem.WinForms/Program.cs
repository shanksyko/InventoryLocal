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
                // Initialize SQL Server infrastructure
                var sqlConfig = SqlServerConfig.Load();
                _sqlServerFactory = new SqlServerConnectionFactory(sqlConfig.ConnectionString);
                _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);

                // Test connection and initialize database if needed
                try
                {
                    // Test if connection string is configured
                    if (string.IsNullOrWhiteSpace(sqlConfig.ConnectionString))
                    {
                        // Database not configured - show new advanced setup dialog
                        using (var setupForm = new DatabaseSetupForm())
                        {
                            if (setupForm.ShowDialog() != DialogResult.OK)
                            {
                                MessageBox.Show(
                                    "Configuração cancelada. A aplicação será fechada.",
                                    "Cancelado",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                return;
                            }

                            var connString = setupForm.GetConnectionString();
                            if (!string.IsNullOrEmpty(connString))
                            {
                                sqlConfig.ConnectionString = connString;
                                sqlConfig.Save();
                                _sqlServerFactory = new SqlServerConnectionFactory(connString);
                                _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);
                            }
                        }
                    }

                    // Ensure schema is created
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


