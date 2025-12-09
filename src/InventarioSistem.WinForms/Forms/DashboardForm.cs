using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public class DashboardForm : Form
{
    private readonly AccessInventoryStore _store;
    private readonly Chart _chart = new() { Dock = DockStyle.Fill };

    public DashboardForm(AccessInventoryStore store)
    {
        _store = store;
        Text = "Dashboard";
        Width = 720;
        Height = 480;
        StartPosition = FormStartPosition.CenterParent;

        var area = new ChartArea("Main")
        {
            Area3DStyle = { Enable3D = true }
        };

        _chart.ChartAreas.Add(area);
        _chart.Legends.Add(new Legend("Legenda"));
        Controls.Add(_chart);

        Shown += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            await _store.EnsureSchemaAsync();
            var totals = await _store.CountByTypeAsync();

            _chart.Series.Clear();
            var series = new Series("Dispositivos")
            {
                ChartType = SeriesChartType.Pie,
                ChartArea = "Main",
                Legend = "Legenda",
                IsValueShownAsLabel = true,
                LabelFormat = "{0}"
            };

            foreach (var item in totals)
            {
                series.Points.AddXY(item.Key.ToString(), item.Value);
            }

            if (series.Points.Count == 0)
            {
                series.Points.AddXY("Sem dados", 1);
                series.Points[0].Color = Color.LightGray;
            }

            _chart.Series.Add(series);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Erro ao carregar dashboard:\n\n" + ex.Message,
                "Dashboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            _chart.Series.Clear();
            var fallback = new Series("Dispositivos")
            {
                ChartType = SeriesChartType.Pie,
                ChartArea = "Main",
                Legend = "Legenda",
                IsValueShownAsLabel = true
            };

            fallback.Points.AddXY("Erro", 1);
            fallback.Points[0].Color = Color.LightGray;
            _chart.Series.Add(fallback);
        }
    }
}
