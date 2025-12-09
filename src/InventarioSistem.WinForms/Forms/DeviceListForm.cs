using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using DeviceEntity = InventarioSistem.Core.Entities.Device;
using DeviceType = InventarioSistem.Core.Models.DeviceType;

namespace InventarioSistem.WinForms.Forms;

public class DeviceListForm : Form
{
    private readonly AccessInventoryStore _store;
    private readonly BindingSource _binding = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly ComboBox _typeFilter = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _searchBox = new() { Width = 220, PlaceholderText = "Buscar em qualquer coluna" };

    private List<DeviceEntity> _allDevices = new();

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

    private DeviceEntity? SelectedDevice => _binding.Current as DeviceEntity;

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

    private void ConfigureFilters()
    {
        _typeFilter.Items.Clear();
        _typeFilter.Items.Add("Todos");
        _typeFilter.Items.AddRange(new object[]
        {
            DeviceType.Computador,
            DeviceType.Tablet,
            DeviceType.Coletor,
            DeviceType.Celular,
            DeviceType.Impressora,
            DeviceType.Dect,
            DeviceType.Cisco,
            DeviceType.Televisor,
            DeviceType.RelogioPonto,
            DeviceType.Monitor,
            DeviceType.Nobreak
        });

        _typeFilter.SelectedIndexChanged += (_, _) => ApplyFilters();
        _searchBox.TextChanged += (_, _) => ApplyFilters();

        _typeFilter.SelectedIndex = 0;
    }

    private void ApplyFilters()
    {
        IEnumerable<DeviceEntity> query = _allDevices;

        if (_typeFilter.SelectedItem is string selectedType && selectedType != "Todos")
        {
            query = query.Where(d => string.Equals(d.Tipo, selectedType, StringComparison.OrdinalIgnoreCase));
        }

        var term = _searchBox.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.ToLowerInvariant();
            query = query.Where(d =>
                Contains(d.Hostname, normalized) ||
                Contains(d.Modelo, normalized) ||
                Contains(d.SerialNumber, normalized) ||
                Contains(d.Local, normalized) ||
                Contains(d.Responsavel, normalized) ||
                Contains(d.TipoModelo, normalized) ||
                Contains(d.LocalizacaoAtual, normalized) ||
                Contains(d.LocalizacaoAnterior, normalized) ||
                Contains(d.Numero, normalized) ||
                Contains(d.MacAddress, normalized) ||
                Contains(d.IPEI, normalized) ||
                Contains(d.IP, normalized) ||
                Contains(d.IMEI1, normalized) ||
                Contains(d.IMEI2, normalized) ||
                Contains(d.Usuario, normalized) ||
                Contains(d.Matricula, normalized) ||
                Contains(d.Cargo, normalized) ||
                Contains(d.Setor, normalized) ||
                Contains(d.Email, normalized) ||
                Contains(d.ComputadorVinculado, normalized) ||
                Contains(d.Status, normalized));
        }

        _binding.DataSource = query.ToList();
    }

    private static bool Contains(string? value, string term)
    {
        return !string.IsNullOrWhiteSpace(value) && value!.ToLowerInvariant().Contains(term);
    }
}
