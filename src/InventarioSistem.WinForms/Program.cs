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
                // 🔧 ETAPA 1: CONFIGURAÇÃO INICIAL - OBRIGATÓRIA NA PRIMEIRA EXECUÇÃO
                var sqlConfig = SqlServerConfig.Load();
                bool isFirstRun = string.IsNullOrWhiteSpace(sqlConfig.ConnectionString);

                if (isFirstRun)
                {
                    ShowWelcomeMessage();
                    
                    // Mostrar formulário de configuração até que seja bem-sucedido
                    bool configured = false;
                    while (!configured)
                    {
                        using (var setupForm = new DatabaseSetupForm())
                        {
                            if (setupForm.ShowDialog() != DialogResult.OK)
                            {
                                var result = MessageBox.Show(
                                    "Você precisa configurar o banco de dados para usar a aplicação.\n\n" +
                                    "Deseja tentar novamente?",
                                    "Configuração Obrigatória",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (result == DialogResult.No)
                                {
                                    MessageBox.Show(
                                        "A aplicação será fechada.",
                                        "Configuração Obrigatória",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                                    return;
                                }
                                continue;
                            }

                            var connString = setupForm.GetConnectionString();
                            if (!string.IsNullOrEmpty(connString))
                            {
                                try
                                {
                                    // Validar conexão
                                    using (var testConn = new SqlServerConnectionFactory(connString).CreateConnection())
                                    {
                                        testConn.Open();
                                    }

                                    sqlConfig.ConnectionString = connString;
                                    sqlConfig.Save();
                                    configured = true;

                                    MessageBox.Show(
                                        "✅ Configuração salva com sucesso!\n\n" +
                                        "A aplicação iniciará agora.",
                                        "Sucesso",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(
                                        $"❌ Erro ao validar conexão:\n\n{ex.Message}\n\n" +
                                        "Verifique os dados e tente novamente.",
                                        "Erro de Conexão",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }

                // 🗄️ ETAPA 2: INICIALIZAR FACTORY E USER STORE
                _sqlServerFactory = new SqlServerConnectionFactory(sqlConfig.ConnectionString);
                _sqlServerUserStore = new SqlServerUserStore(_sqlServerFactory);

                // 🗄️ ETAPA 3: VALIDAR BANCO E CRIAR SCHEMA
                try
                {
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

        private static void ShowWelcomeMessage()
        {
            MessageBox.Show(
                "🎉 Bem-vindo ao Inventory System!\n\n" +
                "Esta é sua primeira execução.\n\n" +
                "Você será guiado através da configuração do banco de dados SQL Server.\n\n" +
                "Certifique-se de que:\n" +
                "✅ SQL Server Express está instalado\n" +
                "✅ O serviço SQL Server está em execução\n" +
                "✅ O arquivo create-schema.sql está disponível\n\n" +
                "Clique OK para continuar.",
                "Primeiro Acesso - Configuração",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}


