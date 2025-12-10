using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public class ReportsForm : Form
{
    private readonly AccessInventoryStore _store;
    private readonly ListView _totals = new() { Dock = DockStyle.Top, Height = 150, View = View.Details, FullRowSelect = true };
    private readonly ListBox _missingImei = new() { Dock = DockStyle.Fill };
    private readonly TextBox _locationFilter = new() { PlaceholderText = "Localização" };
    private readonly Label _locationCount = new() { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft };

    public ReportsForm(AccessInventoryStore store)
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
            _totals.Items.Add(new ListViewItem(new[] { item.Key.ToString(), item.Value.ToString() }));
        }

        var missing = await _store.DevicesMissingImeiAsync();
        _missingImei.Items.Clear();
        foreach (var device in missing)
        {
            _missingImei.Items.Add(device.ToString());
        }
    }

    private async Task LoadLocationAsync()
    {
        if (string.IsNullOrWhiteSpace(_locationFilter.Text))
        {
            _locationCount.Text = "Informe um local para filtrar.";
            return;
        }

        var devices = await _store.DevicesByLocationAsync(_locationFilter.Text);
        _locationCount.Text = $"Total no local: {devices.Count}";
    }
}
