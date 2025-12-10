using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Entities;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                // Try to resolve database path, if it fails, show error
                string? dbPath = null;
                try
                {
                    dbPath = AccessDatabaseManager.ResolveActiveDatabasePath();
                }
                catch (FileNotFoundException)
                {
                    // Banco não configurado - criar um novo
                    var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InventarioSistem.accdb");
                    
                    // Create new empty database
                    AccessDatabaseManager.CreateNewDatabase(defaultPath);
                    
                    AccessDatabaseManager.SetActiveDatabasePath(defaultPath);
                    dbPath = defaultPath;
                }

                // Initialize user store
                var factory = new AccessConnectionFactory();
                var userStore = new UserStore(factory);

                // Ensure all required tables exist
                try
                {
                    AccessSchemaManager.EnsureRequiredTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao criar tabelas do banco:\n\n{ex.Message}",
                        "Erro no Schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Ensure Users table exists
                userStore.EnsureUsersTableAsync().Wait();
                
                // Check if any users exist, create default admin if not
                var users = userStore.GetAllUsersAsync().Result;
                if (users == null || !users.Any())
                {
                    var defaultAdmin = new User
                    {
                        Username = "admin",
                        PasswordHash = User.HashPassword("admin123"),
                        FullName = "Administrador",
                        Email = "admin@inventory.local",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        Provider = "Local"
                    };

                    userStore.AddUserAsync(defaultAdmin).Wait();
                    
                    MessageBox.Show(
                        "Primeiro acesso detectado. Usuário padrão criado:\n\n" +
                        "Usuário: admin\n" +
                        "Senha: admin123\n\n" +
                        "Altere a senha após o primeiro login.",
                        "Primeiro Acesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // Show login form
                using (var loginForm = new LoginForm(userStore))
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // User cancelled login
                    }

                    var loggedInUser = loginForm.LoggedInUser;
                    if (loggedInUser != null)
                    {
                        Application.Run(new MainForm(loggedInUser));
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

