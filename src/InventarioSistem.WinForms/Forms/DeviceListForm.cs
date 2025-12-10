using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;

namespace InventarioSistem.WinForms.Forms;

public class DeviceListForm : Form
{
    private readonly SqlServerInventoryStore _store;
    private readonly BindingSource _binding = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly ComboBox _typeFilter = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _searchBox = new() { Width = 220, PlaceholderText = "Buscar em qualquer coluna" };

    private List<dynamic> _allDevices = new();
    private readonly string[] _deviceTypes = new[]
    {
        "Computer", "Tablet", "ColetorAndroid", "Celular", "Impressora",
        "Dect", "TelefoneCisco", "Televisor", "RelogioPonto", "Monitor", "Nobreak"
    };

    public DeviceListForm(SqlServerInventoryStore store)
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

        buttons.Controls.Add(new Label { Text = "Tipo:", AutoSize = true, Padding = new Padding(10, 12, 0, 0) });
        buttons.Controls.Add(_typeFilter);

        buttons.Controls.Add(new Label { Text = "Buscar:", AutoSize = true, Padding = new Padding(10, 12, 0, 0) });
        buttons.Controls.Add(_searchBox);

        ConfigureFilters();

        Controls.Add(_grid);
        Controls.Add(buttons);

        Shown += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _allDevices = (await _store.ListAsync()).ToList();
        ApplyFilters();
    }

    private dynamic? SelectedDevice => _binding.Current as dynamic;

    private async Task AddAsync()
    {
        MessageBox.Show(this, "Abrir form de edição de dispositivo", "Não implementado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.CompletedTask;
    }

    private async Task EditAsync()
    {
        var selected = SelectedDevice;
        if (selected == null)
        {
            MessageBox.Show(this, "Selecione um dispositivo", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MessageBox.Show(this, "Editar formulário não implementado", "Não implementado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.CompletedTask;
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

        try
        {
            int deviceId = (int)selected.Id;
            await _store.DeleteAsync(deviceId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Erro ao excluir: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigureFilters()
    {
        _typeFilter.Items.Clear();
        _typeFilter.Items.Add("Todos");
        _typeFilter.Items.AddRange(_deviceTypes);

        _typeFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        _searchBox.TextChanged += (_, _) => ApplyFilters();

        _typeFilter.SelectedIndex = 0;
    }

    private void ApplyFilters()
    {
        IEnumerable<dynamic> query = _allDevices;

        // Filter by type
        if (_typeFilter.SelectedItem is string selectedText && selectedText != "Todos")
        {
            query = query.Where(d => d.Type?.ToString() == selectedText);
        }

        // Filter by search term
        var term = _searchBox.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.ToLowerInvariant();
            query = query.Where(d =>
                (d.SerialNumber != null && d.SerialNumber.ToString().ToLowerInvariant().Contains(normalized)) ||
                (d.Type != null && d.Type.ToString().ToLowerInvariant().Contains(normalized)));
        }

        _binding.DataSource = query.ToList();
    }
}
