using System;
using System.Windows.Forms;

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

    public string NovaSenha { get; private set; } = string.Empty;

    public PasswordResetDialog(string username)
    {
        InitializeUI(username);
    }

    private void InitializeUI(string username)
    {
        Text = "Reset de Senha";
        Size = new System.Drawing.Size(400, 220);
        StartPosition = FormStartPosition.CenterParent;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Título
        var lblTitulo = new Label
        {
            Text = $"Resetar senha do usuário: {username}",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 15),
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
        };

        // Nova senha
        var lblNovaSenha = new Label
        {
            Text = "Nova Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 50)
        };

        _txtNovaSenha = new TextBox
        {
            Location = new System.Drawing.Point(20, 70),
            Size = new System.Drawing.Size(350, 24),
            UseSystemPasswordChar = true
        };

        // Confirmar senha
        var lblConfirmar = new Label
        {
            Text = "Confirmar Senha:",
            AutoSize = true,
            Location = new System.Drawing.Point(20, 100)
        };

        _txtConfirmarSenha = new TextBox
        {
            Location = new System.Drawing.Point(20, 120),
            Size = new System.Drawing.Size(350, 24),
            UseSystemPasswordChar = true
        };

        // Mensagem de erro
        _lblMensagem = new Label
        {
            Text = string.Empty,
            AutoSize = true,
            Location = new System.Drawing.Point(20, 150),
            ForeColor = System.Drawing.Color.Red
        };

        // Botões
        _btnConfirmar = new Button
        {
            Text = "Confirmar",
            Size = new System.Drawing.Size(100, 30),
            Location = new System.Drawing.Point(170, 155),
            BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
        };
        _btnConfirmar.Click += (_, _) => ConfirmarReset();

        _btnCancelar = new Button
        {
            Text = "Cancelar",
            Size = new System.Drawing.Size(100, 30),
            Location = new System.Drawing.Point(270, 155),
            BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
        };
        _btnCancelar.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblTitulo);
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

    private void ConfirmarReset()
    {
        var novaSenha = _txtNovaSenha.Text;
        var confirmar = _txtConfirmarSenha.Text;

        if (string.IsNullOrWhiteSpace(novaSenha))
        {
            _lblMensagem.Text = "A senha não pode ser vazia.";
            return;
        }

        if (novaSenha.Length < 6)
        {
            _lblMensagem.Text = "A senha deve ter pelo menos 6 caracteres.";
            return;
        }

        if (novaSenha != confirmar)
        {
            _lblMensagem.Text = "As senhas não coincidem.";
            return;
        }

        NovaSenha = novaSenha;
        DialogResult = DialogResult.OK;
    }
}
