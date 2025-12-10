using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário para admin gerenciar usuários
/// </summary>
public class UserManagementForm : Form
{
    private readonly UserStore _userStore;
    private readonly User? _currentUser;
    private readonly bool _isVisualizer;
    private DataGridView _gridUsers = null!;
    private Button _btnNovoUsuario = null!;
    private Button _btnEditarUsuario = null!;
    private Button _btnExcluirUsuario = null!;
    private Button _btnResetSenha = null!;
    private Button _btnFechar = null!;

    public UserManagementForm(UserStore userStore, User? currentUser = null)
    {
        _userStore = userStore;
        _currentUser = currentUser;
        _isVisualizer = _currentUser?.Role == UserRole.Visualizador;
        InitializeUI();
        LoadUsersAsync();
    }

    private void InitializeUI()
    {
        Text = "Gerenciar Usuários";
        Size = new System.Drawing.Size(900, 550);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new System.Drawing.Size(700, 400);

        // Buttons (organizados em FlowLayout para evitar sobreposição)
        _btnNovoUsuario = new Button
        {
            Text = "Novo Usuário",
            AutoSize = true
        };
        _btnNovoUsuario.Click += (_, _) => NovoUsuario();

        _btnEditarUsuario = new Button
        {
            Text = "Editar Selecionado",
            AutoSize = true
        };
        _btnEditarUsuario.Click += (_, _) => EditarUsuario();

        _btnExcluirUsuario = new Button
        {
            Text = "Excluir",
            AutoSize = true
        };
        _btnExcluirUsuario.Click += (_, _) => ExcluirUsuario();

        _btnResetSenha = new Button
        {
            Text = "Reset de Senha",
            AutoSize = true,
            BackColor = System.Drawing.Color.FromArgb(220, 140, 40),
            ForeColor = System.Drawing.Color.White
        };
        _btnResetSenha.Click += (_, _) => ResetarSenha();

        _btnFechar = new Button
        {
            Text = "Fechar",
            AutoSize = true
        };
        _btnFechar.Click += (_, _) => Close();

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(10, 10, 10, 5),
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };
        buttonsPanel.Controls.AddRange(new Control[]
        {
            _btnNovoUsuario,
            _btnEditarUsuario,
            _btnExcluirUsuario,
            _btnResetSenha,
            _btnFechar
        });

        // Grid
        _gridUsers = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = System.Drawing.Color.White,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.FixedSingle
        };

        _gridUsers.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
            new DataGridViewTextBoxColumn { HeaderText = "Usuário", DataPropertyName = "Username", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Nome", DataPropertyName = "FullName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Função", DataPropertyName = "Role", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
            new DataGridViewTextBoxColumn { HeaderText = "Ativo", DataPropertyName = "IsActive", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
            new DataGridViewTextBoxColumn { HeaderText = "Último Login", DataPropertyName = "LastLogin", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells }
        });

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.Controls.Add(buttonsPanel, 0, 0);
        mainPanel.Controls.Add(_gridUsers, 0, 1);

        Controls.Add(mainPanel);

        // Visualizador: deixa botões visíveis porém desabilitados (read-only)
        if (_isVisualizer)
        {
            _btnNovoUsuario.Enabled = false;
            _btnEditarUsuario.Enabled = false;
            _btnExcluirUsuario.Enabled = false;
            _btnResetSenha.Enabled = false;
        }
    }

    private async void LoadUsersAsync()
    {
        try
        {
            // Garante que a tabela Users existe antes de carregar
            await _userStore.EnsureUsersTableAsync();
            
            var users = await _userStore.GetAllUsersAsync();
            _gridUsers.DataSource = new BindingSource { DataSource = users.ToList() };
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void NovoUsuario()
    {
        using var form = new UserEditForm(_userStore, null);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadUsersAsync();
        }
    }

    private void EditarUsuario()
    {
        if (_gridUsers.CurrentRow?.DataBoundItem is not User selected)
        {
            MessageBox.Show(this, "Selecione um usuário para editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new UserEditForm(_userStore, selected);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadUsersAsync();
        }
    }

    private void ExcluirUsuario()
    {
        if (_gridUsers.CurrentRow?.DataBoundItem is not User selected)
        {
            MessageBox.Show(this, "Selecione um usuário para excluir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, $"Deseja realmente excluir '{selected.Username}'?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            _userStore.DeleteUserAsync(selected.Id).GetAwaiter().GetResult();
            AuditLog.LogUserDeletion(selected.Username, "admin");
            LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro ao excluir: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetarSenha()
    {
        if (_gridUsers.CurrentRow?.DataBoundItem is not User selected)
        {
            MessageBox.Show(this, "Selecione um usuário para resetar a senha.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new PasswordResetDialog(selected.Username);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            ResetarSenhaAsync(selected, dialog.NovaSenha);
        }
    }

    private async void ResetarSenhaAsync(User selected, string novaSenha)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(novaSenha))
            {
                MessageBox.Show(this, "Senha não pode ser vazia.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var hash = User.HashPassword(novaSenha);
            await _userStore.UpdatePasswordAsync(selected.Id, hash);
            
            AuditLog.LogPasswordChange(selected.Username, "admin");
            MessageBox.Show(this, $"Senha do usuário '{selected.Username}' resetada com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro ao resetar senha: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
