using System;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

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
    private readonly UserStore _userStore;

    public User? LoggedInUser { get; private set; }

    public LoginForm(UserStore userStore)
    {
        _userStore = userStore;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "Login - Inventory System";
        Size = new System.Drawing.Size(350, 250);
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
            Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Location = new System.Drawing.Point(70, 20)
        };

        // Username
        var lblUsername = new Label
        {
            Text = "Usuário:",
            AutoSize = true,
            Location = new System.Drawing.Point(30, 70)
        };

        _txtUsername = new TextBox
        {
            Location = new System.Drawing.Point(30, 90),
            Size = new System.Drawing.Size(290, 24)
        };

        // Password
        var lblPassword = new Label
        {
            Text = "Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(30, 120)
        };

        _txtPassword = new TextBox
        {
            Location = new System.Drawing.Point(30, 140),
            Size = new System.Drawing.Size(290, 24),
            UseSystemPasswordChar = true
        };

        // Message
        _lblMessage = new Label
        {
            Text = string.Empty,
            ForeColor = System.Drawing.Color.Red,
            AutoSize = true,
            Location = new System.Drawing.Point(30, 170)
        };

        // Buttons
        _btnLogin = new Button
        {
            Text = "Entrar",
            Size = new System.Drawing.Size(100, 35),
            Location = new System.Drawing.Point(140, 195),
            BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
        };
        _btnLogin.Click += (_, _) => LoginAsync();

        _btnCancel = new Button
        {
            Text = "Cancelar",
            Size = new System.Drawing.Size(100, 35),
            Location = new System.Drawing.Point(220, 195)
        };
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblTitle);
        Controls.Add(lblUsername);
        Controls.Add(_txtUsername);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_lblMessage);
        Controls.Add(_btnLogin);
        Controls.Add(_btnCancel);
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
                _lblMessage.Text = "Usuário e senha são obrigatórios.";
                return;
            }

            var user = await _userStore.GetUserByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                _lblMessage.Text = "Usuário não encontrado ou inativo.";
                return;
            }

            if (!user.VerifyPassword(password))
            {
                _lblMessage.Text = "Senha incorreta.";
                return;
            }

            // Atualiza último login
            await _userStore.UpdateLastLoginAsync(user.Id);

            LoggedInUser = user;
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
