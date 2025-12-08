using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public class DeviceListForm : Form
{
    private readonly AccessInventoryStore _store;
    private readonly BindingSource _binding = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

    public DeviceListForm(AccessInventoryStore store)
    {
        _store = store;
        Text = "Dispositivos";
        Width = 900;
        Height = 500;

        var addButton = new Button { Text = "Novo", Dock = DockStyle.Left, Width = 100 };
        addButton.Click += async (_, _) => await AddAsync();

        var editButton = new Button { Text = "Editar", Dock = DockStyle.Left, Width = 100 };
        editButton.Click += async (_, _) => await EditAsync();

        var deleteButton = new Button { Text = "Excluir", Dock = DockStyle.Left, Width = 100 };
        deleteButton.Click += async (_, _) => await DeleteAsync();

        var refreshButton = new Button { Text = "Atualizar", Dock = DockStyle.Left, Width = 100 };
        refreshButton.Click += async (_, _) => await LoadAsync();

        _grid.DataSource = _binding;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            Height = 45
        };

        buttons.Controls.Add(addButton);
        buttons.Controls.Add(editButton);
        buttons.Controls.Add(deleteButton);
        buttons.Controls.Add(refreshButton);

        Controls.Add(_grid);
        Controls.Add(buttons);

        Shown += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var data = await _store.ListAsync();
        _binding.DataSource = data.ToList();
    }

    private Device? SelectedDevice => _binding.Current as Device;

    private async Task AddAsync()
    {
        using var editor = new DeviceEditForm(_store, null);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        var selected = SelectedDevice;
        if (selected == null)
        {
            MessageBox.Show(this, "Selecione um dispositivo", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var editor = new DeviceEditForm(_store, selected);
        if (editor.ShowDialog(this) == DialogResult.OK)
        {
            await LoadAsync();
        }
    }

    private async Task DeleteAsync()
    {
        var selected = SelectedDevice;
        if (selected == null)
        {
            MessageBox.Show(this, "Selecione um dispositivo", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, "Confirma exclusão?", "Inventário", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        await _store.DeleteAsync(selected.Id);
        await LoadAsync();
    }
}
