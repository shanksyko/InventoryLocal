using System;
using System.Collections.Generic;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário de login para SQL Server
/// </summary>
public class LoginForm : Form
{
    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private Button _btnLogin = null!;
    private Button _btnCancel = null!;
    private Label _lblMessage = null!;
    private SqlServerUserStore? _userStore;
    private SqlServerConnectionFactory? _sqlFactory;

    // Rate limiting
    private static Dictionary<string, (int attempts, DateTime blockedUntil)> _loginAttempts = new();
    private const int MAX_ATTEMPTS = 5;
    private const int BLOCK_MINUTES = 15;

    public static User? LoggedInUser { get; set; }
    public static string? EnteredPassword { get; set; }

    private bool IsRateLimited(string username, out int remainingMinutes)
    {
        remainingMinutes = 0;
        
        if (_loginAttempts.TryGetValue(username, out var attempt))
        {
            if (DateTime.Now < attempt.blockedUntil)
            {
                remainingMinutes = (int)Math.Ceiling((attempt.blockedUntil - DateTime.Now).TotalMinutes);
                return true;
            }
            
            // Bloqueio expirado, limpar
            if (DateTime.Now >= attempt.blockedUntil && attempt.attempts >= MAX_ATTEMPTS)
            {
                _loginAttempts.Remove(username);
            }
        }
        
        return false;
    }

    private void RegisterFailedAttempt(string username)
    {
        if (_loginAttempts.TryGetValue(username, out var attempt))
        {
            attempt.attempts++;
            
            if (attempt.attempts >= MAX_ATTEMPTS)
            {
                attempt.blockedUntil = DateTime.Now.AddMinutes(BLOCK_MINUTES);
            }
            
            _loginAttempts[username] = attempt;
        }
        else
        {
            _loginAttempts[username] = (1, DateTime.MinValue);
        }
    }

    private void ResetLoginAttempts(string username)
    {
        if (_loginAttempts.ContainsKey(username))
        {
            _loginAttempts.Remove(username);
        }
    }

    public LoginForm(SqlServerConnectionFactory? sqlFactory = null, SqlServerUserStore? SqlServerUserStore = null)
    {
        var config = new SqlServerConfig();
        _sqlFactory = sqlFactory ?? new SqlServerConnectionFactory(config.ConnectionString);
        _userStore = SqlServerUserStore;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "Login - Inventory System";
        Size = new System.Drawing.Size(400, 260);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Ícone do formulário
        try
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                Icon = new System.Drawing.Icon(iconPath);
            }
        }
        catch { /* Ignora se não conseguir carregar o ícone */ }

        // Título
        var lblTitle = new Label
        {
            Text = "Inventory System",
            Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Location = new System.Drawing.Point(90, 12),
            ForeColor = System.Drawing.Color.FromArgb(50, 100, 160)
        };

        // Username
        var lblUsername = new Label
        {
            Text = "Usuário:",
            AutoSize = true,
            Location = new System.Drawing.Point(30, 55)
        };

        _txtUsername = new TextBox
        {
            Location = new System.Drawing.Point(30, 75),
            Size = new System.Drawing.Size(340, 22),
            BackColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Password
        var lblPassword = new Label
        {
            Text = "Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(30, 105)
        };

        _txtPassword = new TextBox
        {
            Location = new System.Drawing.Point(30, 125),
            Size = new System.Drawing.Size(340, 22),
            UseSystemPasswordChar = true,
            BackColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Message
        _lblMessage = new Label
        {
            Text = string.Empty,
            ForeColor = System.Drawing.Color.Red,
            AutoSize = true,
            Location = new System.Drawing.Point(30, 155)
        };

        // Buttons
        _btnLogin = new Button
        {
            Text = "Entrar",
            Size = new System.Drawing.Size(85, 28),
            Location = new System.Drawing.Point(30, 175),
            BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
        };
        _btnLogin.Click += (_, _) => LoginAsync();

        _btnCancel = new Button
        {
            Text = "Cancelar",
            Size = new System.Drawing.Size(85, 28),
            Location = new System.Drawing.Point(140, 175),
            BackColor = System.Drawing.Color.FromArgb(240, 240, 240),
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI", 9F)
        };
        _btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;


        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(_txtUsername);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_lblMessage);
        Controls.Add(_btnLogin);
        Controls.Add(_btnCancel);

        // Focus on username field when form loads
        Shown += (_, _) => _txtUsername.Focus();
    }

    private async void LoginAsync()
    {
        _btnLogin.Enabled = false;

        try
        {
            var username = _txtUsername.Text.Trim();
            var password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                RegisterFailedAttempt(username);
                return;
            }

            // Verificar rate limiting
            if (IsRateLimited(username, out int remainingMinutes))
            {
                _lblMessage.Text = $"Conta bloqueada por {remainingMinutes} minutos devido a múltiplas tentativas falhadas.";
                InventoryLogger.Info("LoginForm", $"Login bloqueado por rate limit: {username}");
                return;
            }

            if (_userStore == null)
            {
                // Offline mode: accept default admin credentials
                if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase) && password == "L9l337643k#$")
                {
                    LoggedInUser = new User
                    {
                        Username = "admin",
                        FullName = "Administrador",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        Provider = "Local"
                    };
                    EnteredPassword = password;
                    ResetLoginAttempts(username);
                    InventoryLogger.Info("LoginForm", $"Login bem-sucedido (offline): {username}");
                    DialogResult = DialogResult.OK;
                    return;
                }

                RegisterFailedAttempt(username);
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                InventoryLogger.Info("LoginForm", $"Falha de login (offline, credenciais inválidas): {username}");
                return;
            }

            // Validate against SQL Server
            var user = await _userStore.GetUserAsync(username);
            if (user == null)
            {
                RegisterFailedAttempt(username);
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                InventoryLogger.Info("LoginForm", $"Falha de login (usuário não encontrado): {username}");
                return;
            }

            if (!user.Value.IsActive)
            {
                RegisterFailedAttempt(username);
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                InventoryLogger.Info("LoginForm", $"Falha de login (usuário inativo): {username}");
                return;
            }

            // Verify password
            bool passwordValid = await _userStore.ValidateUserAsync(username, password);
            if (!passwordValid)
            {
                RegisterFailedAttempt(username);
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                InventoryLogger.Info("LoginForm", $"Falha de login (senha incorreta): {username}");
                return;
            }

            // Login bem-sucedido
            ResetLoginAttempts(username);

            LoggedInUser = new User
            {
                Id = user.Value.Id,
                Username = user.Value.Username,
                FullName = user.Value.FullName,
                IsActive = user.Value.IsActive,
                CreatedAt = DateTime.Now,
                Role = UserRole.Admin,
                Provider = "SqlServer"
            };
            EnteredPassword = password;
            InventoryLogger.Info("LoginForm", $"Login bem-sucedido: {username}");
            
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            _lblMessage.Text = $"Erro: {ex.Message}";
            InventoryLogger.Error("LoginForm", $"Erro durante login: {ex.Message}");
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
