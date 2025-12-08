using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms.Forms;

public class MainForm : Form
{
    private readonly AccessInventoryStore _store;

    public MainForm(AccessInventoryStore store)
    {
        _store = store;
        Text = "Inventário de Dispositivos";
        Width = 420;
        Height = 240;
        StartPosition = FormStartPosition.CenterScreen;

        var listButton = new Button { Text = "Listar/Cadastrar", Dock = DockStyle.Top, Height = 40 };
        listButton.Click += (_, _) => new DeviceListForm(_store).ShowDialog(this);

        var reportsButton = new Button { Text = "Relatórios", Dock = DockStyle.Top, Height = 40 };
        reportsButton.Click += (_, _) => new ReportsForm(_store).ShowDialog(this);

        var exportButton = new Button { Text = "Exportar CSV", Dock = DockStyle.Top, Height = 40 };
        exportButton.Click += (_, _) => CsvExporter.ExportWithDialog(_store, this);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            WrapContents = false
        };

        panel.Controls.Add(listButton);
        panel.Controls.Add(reportsButton);
        panel.Controls.Add(exportButton);

        Controls.Add(panel);
    }
}
