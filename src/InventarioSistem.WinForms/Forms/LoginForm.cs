using System;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário de login do sistema
/// </summary>
public class LoginForm : Form
{
    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private Button _btnLogin = null!;
    private Button _btnCancel = null!;
    private Label _lblMessage = null!;
    private UserStore? _userStore;

    public User? LoggedInUser { get; private set; }
    public string? EnteredPassword { get; private set; }

        public LoginForm(UserStore? userStore)
    {
        _userStore = userStore;
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

    private void CreateDatabase()
    {
        var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InventarioSistem.accdb");
        try
        {
            AccessDatabaseManager.CreateNewDatabase(defaultPath);
            AccessDatabaseManager.SetActiveDatabasePath(defaultPath);
            var factory = new AccessConnectionFactory();
            _userStore = new UserStore(factory);
            _lblMessage.ForeColor = System.Drawing.Color.Green;
            _lblMessage.Text = "Banco criado e selecionado com sucesso.";
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            _lblMessage.ForeColor = System.Drawing.Color.Red;
            _lblMessage.Text = "Não foi possível criar o banco: driver Access ausente. Instale o Microsoft Access Database Engine ou selecione um .accdb existente.";
        }
        catch (Exception ex)
        {
            _lblMessage.ForeColor = System.Drawing.Color.Red;
            _lblMessage.Text = $"Erro ao criar banco: {ex.Message}";
        }
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
                    AuditLog.LogLogin("admin", true);
                    DialogResult = DialogResult.OK;
                    return;
                }

                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                AuditLog.LogLogin(username, false);
                return;
            }

            var user = await _userStore.GetUserByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                AuditLog.LogLogin(username, false);
                return;
            }

            if (!user.VerifyPassword(password))
            {
                _lblMessage.Text = "Usuário e/ou senha incorreto.";
                AuditLog.LogLogin(username, false);
                return;
            }

            // Atualiza último login
            await _userStore.UpdateLastLoginAsync(user.Id);

            LoggedInUser = user;
            EnteredPassword = password;
            AuditLog.LogLogin(username, true);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            _lblMessage.Text = $"Erro: {ex.Message}";
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
