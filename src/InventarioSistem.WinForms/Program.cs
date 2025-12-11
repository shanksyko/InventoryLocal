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
                        // Database not configured - show database setup dialog
                        ShowDatabaseSetupDialog(sqlConfig);
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

        private static void ShowDatabaseSetupDialog(SqlServerConfig config)
        {
            var result = MessageBox.Show(
                "Banco de dados SQL Server não configurado.\n\n" +
                "Deseja configurar agora?",
                "Configuração de Banco de Dados",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using var form = new Form
                {
                    Text = "Configurar SQL Server",
                    Size = new System.Drawing.Size(600, 200),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var label = new Label
                {
                    Text = "Connection String do SQL Server:",
                    Location = new System.Drawing.Point(10, 20),
                    AutoSize = true
                };

                var textBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 45),
                    Size = new System.Drawing.Size(560, 20),
                    Text = "Server=localhost\\SQLEXPRESS;Database=InventoryDB;Integrated Security=true;TrustServerCertificate=true;"
                };

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(400, 100)
                };

                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(490, 100)
                };

                form.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    config.ConnectionString = textBox.Text.Trim();
                    config.Save();
                    
                    // Update factory with new connection string
                    _sqlServerFactory = new SqlServerConnectionFactory(config.ConnectionString);
                    _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);
                    
                    MessageBox.Show(
                        "Configuração salva com sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "A aplicação será fechada pois o banco de dados não foi configurado.",
                        "Configuração Cancelada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Environment.Exit(0);
                }
            }
            else
            {
                MessageBox.Show(
                    "A aplicação será fechada pois o banco de dados não foi configurado.",
                    "Configuração Necessária",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
        }
    }
}

