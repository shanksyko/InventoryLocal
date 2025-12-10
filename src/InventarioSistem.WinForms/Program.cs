using System;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
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

            // Initialize user store
            var factory = new AccessConnectionFactory();
            var userStore = new UserStore(factory);

            // Ensure Users table exists
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao inicializar tabela de usuários:\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
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
    }
}
