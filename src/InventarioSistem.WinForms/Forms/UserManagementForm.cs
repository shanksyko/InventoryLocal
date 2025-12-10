using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário para admin gerenciar usuários
/// </summary>
public class UserManagementForm : Form
{
    private readonly UserStore _userStore;
    private DataGridView _gridUsers = null!;
    private Button _btnNovoUsuario = null!;
    private Button _btnEditarUsuario = null!;
    private Button _btnExcluirUsuario = null!;
    private Button _btnFechar = null!;

    public UserManagementForm(UserStore userStore)
    {
        _userStore = userStore;
        InitializeUI();
        LoadUsersAsync();
    }

    private void InitializeUI()
    {
        Text = "Gerenciar Usuários";
        Size = new System.Drawing.Size(700, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new System.Drawing.Font("Segoe UI", 9F);
        BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

        // Buttons
        _btnNovoUsuario = new Button
        {
            Text = "Novo Usuário",
            AutoSize = true,
            Location = new System.Drawing.Point(10, 10)
        };
        _btnNovoUsuario.Click += (_, _) => NovoUsuario();

        _btnEditarUsuario = new Button
        {
            Text = "Editar Selecionado",
            AutoSize = true,
            Location = new System.Drawing.Point(120, 10)
        };
        _btnEditarUsuario.Click += (_, _) => EditarUsuario();

        _btnExcluirUsuario = new Button
        {
            Text = "Excluir",
            AutoSize = true,
            Location = new System.Drawing.Point(270, 10)
        };
        _btnExcluirUsuario.Click += (_, _) => ExcluirUsuario();

        _btnFechar = new Button
        {
            Text = "Fechar",
            AutoSize = true,
            Location = new System.Drawing.Point(600, 10)
        };
        _btnFechar.Click += (_, _) => Close();

        // Grid
        _gridUsers = new DataGridView
        {
            Location = new System.Drawing.Point(10, 50),
            Size = new System.Drawing.Size(680, 420),
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

        Controls.Add(_btnNovoUsuario);
        Controls.Add(_btnEditarUsuario);
        Controls.Add(_btnExcluirUsuario);
        Controls.Add(_btnFechar);
        Controls.Add(_gridUsers);
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
            LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro ao excluir: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
