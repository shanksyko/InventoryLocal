using System;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário para editar/criar usuários
/// </summary>
public class UserEditForm : Form
{
    private readonly SqlServerUserStore _userStore;
    private readonly User? _existingUser;
    private TextBox _txtUsername = null!;
    private TextBox _txtPassword = null!;
    private TextBox _txtFullName = null!;
    private TextBox _txtEmail = null!;
    private ComboBox _cmbRole = null!;
    private CheckBox _chkIsActive = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    private Label _lblPasswordHint = null!;

    public UserEditForm(SqlServerUserStore SqlServerUserStore, User? existingUser)
    {
        _userStore = SqlServerUserStore;
        _existingUser = existingUser;
        InitializeUI();
        PopulateFields();
    }

    private void InitializeUI()
    {
        Text = _existingUser != null ? "Editar Usuário" : "Novo Usuário";
        Size = new System.Drawing.Size(450, 380);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Username
        var lblUsername = new Label { Text = "Usuário:", AutoSize = true, Location = new System.Drawing.Point(20, 20) };
        _txtUsername = new TextBox
        {
            Location = new System.Drawing.Point(20, 40),
            Size = new System.Drawing.Size(350, 24),
            ReadOnly = _existingUser != null
        };

        // Password
        var lblPassword = new Label { Text = "Senha:", AutoSize = true, Location = new System.Drawing.Point(20, 75) };
        _txtPassword = new TextBox
        {
            Location = new System.Drawing.Point(20, 95),
            Size = new System.Drawing.Size(350, 24),
            UseSystemPasswordChar = true
        };
        _lblPasswordHint = new Label
        {
            Text = _existingUser != null ? "Deixe em branco para não alterar" : "Senha para login local",
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic),
            ForeColor = System.Drawing.Color.Gray,
            Location = new System.Drawing.Point(20, 120)
        };

        // Full Name
        var lblFullName = new Label { Text = "Nome Completo:", AutoSize = true, Location = new System.Drawing.Point(20, 145) };
        _txtFullName = new TextBox { Location = new System.Drawing.Point(20, 165), Size = new System.Drawing.Size(350, 24) };

        // Email
        var lblEmail = new Label { Text = "Email:", AutoSize = true, Location = new System.Drawing.Point(20, 195) };
        _txtEmail = new TextBox { Location = new System.Drawing.Point(20, 215), Size = new System.Drawing.Size(350, 24) };

        // Role
        var lblRole = new Label { Text = "Função:", AutoSize = true, Location = new System.Drawing.Point(20, 245) };
        _cmbRole = new ComboBox
        {
            Location = new System.Drawing.Point(20, 265),
            Size = new System.Drawing.Size(150, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbRole.Items.AddRange(new object[] { UserRole.Admin, UserRole.Visualizador });

        // IsActive
        _chkIsActive = new CheckBox
        {
            Text = "Ativo",
            Location = new System.Drawing.Point(180, 265),
            AutoSize = true,
            Checked = true
        };

        // Buttons (agrupados para evitar sobreposição em diferentes DPIs)
        _btnSave = new Button
        {
            Text = "Salvar",
            Size = new System.Drawing.Size(100, 35),
            BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0)
        };
        _btnSave.Click += (_, _) => SaveAsync();

        _btnCancel = new Button
        {
            Text = "Cancelar",
            Size = new System.Drawing.Size(100, 35),
            Margin = new Padding(0)
        };
        _btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(15, 10, 15, 10)
        };

        var buttonsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        buttonsFlow.Controls.AddRange(new Control[] { _btnSave, _btnCancel });
        footerPanel.Controls.Add(buttonsFlow);

        Controls.Add(lblUsername);
        Controls.Add(_txtUsername);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_lblPasswordHint);
        Controls.Add(lblFullName);
        Controls.Add(_txtFullName);
        Controls.Add(lblEmail);
        Controls.Add(_txtEmail);
        Controls.Add(lblRole);
        Controls.Add(_cmbRole);
        Controls.Add(_chkIsActive);
        Controls.Add(footerPanel);
    }

    private void PopulateFields()
    {
        if (_existingUser != null)
        {
            _txtUsername.Text = _existingUser.Username;
            _txtFullName.Text = _existingUser.FullName ?? string.Empty;
            _txtEmail.Text = _existingUser.Email ?? string.Empty;
            _cmbRole.SelectedItem = _existingUser.Role;
            _chkIsActive.Checked = _existingUser.IsActive;
        }
        else
        {
            _cmbRole.SelectedItem = UserRole.Visualizador;
        }
    }

    private async void SaveAsync()
    {
        try
        {
            var username = _txtUsername.Text.Trim();
            var password = _txtPassword.Text;
            var fullName = _txtFullName.Text.Trim();
            var email = _txtEmail.Text.Trim();
            var role = (UserRole)(_cmbRole.SelectedItem ?? UserRole.Visualizador);
            var isActive = _chkIsActive.Checked;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(this, "Usuário é obrigatório.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_existingUser == null)
            {
                // Novo usuário
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(this, "Senha é obrigatória para novo usuário.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validar complexidade da senha
                var (isValid, errorMessage) = PasswordValidator.ValidatePassword(password);
                if (!isValid)
                {
                    MessageBox.Show(this, errorMessage, "Validação de Senha", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = User.HashPassword(password),
                    FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    Role = role,
                    IsActive = isActive,
                    Provider = "Local"
                };

                await _userStore.AddUserAsync(username, password, fullName ?? "", isActive);
                MessageBox.Show(this, "Usuário criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Editar usuário existente
                _existingUser.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
                _existingUser.Email = string.IsNullOrWhiteSpace(email) ? null : email;
                _existingUser.Role = role;
                _existingUser.IsActive = isActive;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    // Validar complexidade da nova senha
                    var (isValid, errorMessage) = PasswordValidator.ValidatePassword(password);
                    if (!isValid)
                    {
                        MessageBox.Show(this, errorMessage, "Validação de Senha", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    await _userStore.UpdatePasswordAsync(_existingUser.Id.ToString(), User.HashPassword(password));
                }

                await _userStore.UpdateUserAsync(_existingUser.Id.ToString(), username, fullName ?? "", isActive);
                MessageBox.Show(this, "Usuário atualizado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
