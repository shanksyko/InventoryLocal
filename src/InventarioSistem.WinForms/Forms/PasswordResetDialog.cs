using System;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Diálogo para reset de senha de usuário
/// </summary>
public class PasswordResetDialog : Form
{
    private TextBox _txtNovaSenha = null!;
    private TextBox _txtConfirmarSenha = null!;
    private Button _btnConfirmar = null!;
    private Button _btnCancelar = null!;
    private Label _lblMensagem = null!;
    private Label _lblRequisitos = null!;

    private readonly User _user;
    private readonly UserStore _userStore;
    private readonly bool _isFirstLogin;

    public string NovaSenha { get; private set; } = string.Empty;

    public PasswordResetDialog(User user, UserStore userStore, bool isFirstLogin = false)
    {
        _user = user;
        _userStore = userStore;
        _isFirstLogin = isFirstLogin;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = _isFirstLogin ? "Primeiro Login - Alterar Senha" : "Reset de Senha";
        Size = new System.Drawing.Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Título
        var lblTitulo = new Label
        {
            Text = _isFirstLogin 
                ? $"Bem-vindo! Por segurança, altere sua senha inicial."
                : $"Resetar senha do usuário: {_user.Username}",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 15),
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
            MaximumSize = new System.Drawing.Size(450, 0)
        };

        // Requisitos de senha
        _lblRequisitos = new Label
        {
            Text = PasswordValidator.GetPasswordRequirements(),
            AutoSize = true,
            Location = new System.Drawing.Point(20, 50),
            Font = new System.Drawing.Font("Segoe UI", 8.5F),
            ForeColor = System.Drawing.Color.FromArgb(64, 64, 64)
        };

        // Nova senha
        var lblNovaSenha = new Label
        {
            Text = "Nova Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 160)
        };

        _txtNovaSenha = new TextBox
        {
            Location = new System.Drawing.Point(20, 180),
            Size = new System.Drawing.Size(450, 24),
            UseSystemPasswordChar = true
        };

        // Confirmar senha
        var lblConfirmar = new Label
        {
            Text = "Confirmar Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 210)
        };

        _txtConfirmarSenha = new TextBox
        {
            Location = new System.Drawing.Point(20, 230),
            Size = new System.Drawing.Size(450, 24),
            UseSystemPasswordChar = true
        };

        // Mensagem de erro
        _lblMensagem = new Label
        {
            Text = string.Empty,
            AutoSize = true,
            Location = new System.Drawing.Point(20, 265),
            ForeColor = System.Drawing.Color.Red,
            MaximumSize = new System.Drawing.Size(450, 0)
        };

        // Botões
        _btnConfirmar = new Button
        {
            Text = "Confirmar",
            Size = new System.Drawing.Size(120, 35),
            Location = new System.Drawing.Point(250, 315),
            BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
        };
        _btnConfirmar.Click += async (_, _) => await ConfirmarResetAsync();

        _btnCancelar = new Button
        {
            Text = "Cancelar",
            Size = new System.Drawing.Size(120, 35),
            Location = new System.Drawing.Point(370, 315),
            BackColor = System.Drawing.Color.FromArgb(240, 240, 240),
            Enabled = !_isFirstLogin // Não permite cancelar no primeiro login
        };
        _btnCancelar.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblTitulo);
        Controls.Add(_lblRequisitos);
        Controls.Add(lblNovaSenha);
        Controls.Add(_txtNovaSenha);
        Controls.Add(lblConfirmar);
        Controls.Add(_txtConfirmarSenha);
        Controls.Add(_lblMensagem);
        Controls.Add(_btnConfirmar);
        Controls.Add(_btnCancelar);

        // Focus no campo de senha
        Shown += (_, _) => _txtNovaSenha.Focus();
    }

    private async Task ConfirmarResetAsync()
    {
        _btnConfirmar.Enabled = false;
        _lblMensagem.Text = string.Empty;

        try
        {
            var novaSenha = _txtNovaSenha.Text;
            var confirmar = _txtConfirmarSenha.Text;

            // Validar senha vazia
            if (string.IsNullOrWhiteSpace(novaSenha))
            {
                _lblMensagem.Text = "A senha não pode ser vazia.";
                return;
            }

            // Validar se as senhas coincidem
            if (novaSenha != confirmar)
            {
                _lblMensagem.Text = "As senhas não coincidem.";
                return;
            }

            // Validar complexidade da senha
            var (isValid, errorMessage) = PasswordValidator.ValidatePassword(novaSenha);
            if (!isValid)
            {
                _lblMensagem.Text = errorMessage;
                return;
            }

            // Atualizar senha no banco
            _user.PasswordHash = User.HashPassword(novaSenha);
            await _userStore.UpdatePasswordAsync(_user.Id, _user.PasswordHash);

            NovaSenha = novaSenha;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            _lblMensagem.Text = $"Erro ao alterar senha: {ex.Message}";
        }
        finally
        {
            _btnConfirmar.Enabled = true;
        }
    }
}
