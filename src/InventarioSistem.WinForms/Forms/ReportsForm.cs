using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public class ReportsForm : Form
{
    private readonly SqlServerInventoryStore _store;
    private readonly ListView _totals = new() { Dock = DockStyle.Top, Height = 150, View = View.Details, FullRowSelect = true };
    private readonly ListBox _missingImei = new() { Dock = DockStyle.Fill };
    private readonly TextBox _locationFilter = new() { PlaceholderText = "Localização" };
    private readonly Label _locationCount = new() { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft };

    public ReportsForm(SqlServerInventoryStore store)
    {
        _store = store;
        Text = "Relatórios";
        Width = 640;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        // Ícone do formulário
        try
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                Icon = new System.Drawing.Icon(iconPath);
            }
        }
        catch { /* Ignora se não conseguir carregar o ícone */ }

        _totals.Columns.Add("Tipo", 200);
        _totals.Columns.Add("Total", 80);

        var filterButton = new Button { Text = "Filtrar por local", Dock = DockStyle.Right, Width = 150 };
        filterButton.Click += async (_, _) => await LoadLocationAsync();

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 35 };
        _locationFilter.Dock = DockStyle.Fill;
        topPanel.Controls.Add(_locationFilter);
        topPanel.Controls.Add(filterButton);

        Controls.Add(_missingImei);
        Controls.Add(_locationCount);
        Controls.Add(_totals);
        Controls.Add(topPanel);

        Shown += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var totals = await _store.CountByTypeAsync();
        _totals.Items.Clear();
        foreach (var item in totals)
        {
            var key = item.Key ?? "Unknown";
            var value = item.Value.ToString();
            _totals.Items.Add(new ListViewItem(new[] { key, value }));
        }

        var missing = await _store.DevicesMissingImeiAsync();
        _missingImei.Items.Clear();
        foreach (var device in missing)
        {
            string id = device.Id?.ToString() ?? "";
            string serialNumber = device.SerialNumber?.ToString() ?? "";
            _missingImei.Items.Add(new ListViewItem(new[] { id, serialNumber }));
        }
    }

    private async Task LoadLocationAsync()
    {
        var devices = await _store.DevicesByLocationAsync();
        var filtered = devices.Where(d => d.Value > 0).ToList();
        
        if (!string.IsNullOrWhiteSpace(_locationFilter.Text))
        {
            var term = _locationFilter.Text.ToLowerInvariant();
            filtered = filtered.Where(d => d.Key.ToLowerInvariant().Contains(term)).ToList();
        }
        
        if (filtered.Count == 0)
        {
            _locationCount.Text = "Nenhum local encontrado.";
            return;
        }

        var total = filtered.Sum(d => d.Value);
        _locationCount.Text = $"Total no(s) local(is) selecionado(s): {total}";
    }
}
