using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Dashboard que mostra a quantidade total de cada tipo de item em um gráfico de pizza
/// </summary>
public class TotalDashboardForm : Form
{
    private readonly AccessInventoryStore _store;
    private Chart _chart = null!;
    private DataGridView _gridTotals = null!;

    public TotalDashboardForm(AccessInventoryStore store)
    {
        _store = store;
        InitializeUI();
        LoadData();
    }

    private void InitializeUI()
    {
        Text = "Dashboard Total de Itens";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(245, 247, 250);

        // Painel superior com título
        var lblTitle = new Label
        {
            Text = "Quantidades Totais por Tipo de Item",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(230, 236, 245)
        };

        // Painel principal com split container
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 450
        };

        // Gráfico de pizza
        _chart = new Chart
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        var chartArea = new ChartArea("MainArea")
        {
            BackColor = Color.White
        };
        _chart.ChartAreas.Add(chartArea);

        var series = new Series("Itens")
        {
            ChartType = SeriesChartType.Pie,
            IsValueShownAsLabel = true,
            LabelFormat = "#,##0"
        };
        _chart.Series.Add(series);

        splitContainer.Panel1.Controls.Add(_chart);

        // Grid com totais
        _gridTotals = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.FixedSingle
        };

        _gridTotals.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { HeaderText = "Tipo de Item", DataPropertyName = "Type", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
            new DataGridViewTextBoxColumn { HeaderText = "Quantidade", DataPropertyName = "Count", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells }
        });

        splitContainer.Panel2.Controls.Add(_gridTotals);

        Controls.Add(splitContainer);
        Controls.Add(lblTitle);
    }

    private void LoadData()
    {
        try
        {
            var totals = new Dictionary<string, int>();

            // Contar cada tipo de item
            totals["Computadores"] = _store.GetAllComputers().Count;
            totals["Tablets"] = _store.GetAllTablets().Count;
            totals["Coletores"] = _store.GetAllColetores().Count;
            totals["Celulares"] = _store.GetAllCelulares().Count;
            totals["Impressoras"] = _store.GetAllImpressoras().Count;
            totals["DECTs"] = _store.GetAllDects().Count;
            totals["Telefones Cisco"] = _store.GetAllTelefonesCisco().Count;
            totals["Televisores"] = _store.GetAllTelevisores().Count;
            totals["Relógios Ponto"] = _store.GetAllRelogiosPonto().Count;
            totals["Monitores"] = _store.GetAllMonitores().Count;
            totals["Nobreaks"] = _store.GetAllNobreaks().Count;

            // Remover itens com contagem zero
            totals = totals.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);

            // Preencher gráfico
            var series = _chart.Series["Itens"];
            series.Points.Clear();

            foreach (var item in totals)
            {
                series.Points.AddXY(item.Key, item.Value);
            }

            // Preencher grid com dados e total
            var dataSource = totals.Select(x => new { Type = x.Key, Count = x.Value }).ToList();
            
            // Adicionar linha de total
            if (dataSource.Count > 0)
            {
                dataSource.Add(new { Type = "TOTAL", Count = dataSource.Sum(x => x.Count) });
            }
            
            // Usar BindingSource para permitir melhor controle
            var bindingSource = new BindingSource { DataSource = dataSource };
            _gridTotals.DataSource = bindingSource;
            
            // Destacar a linha de total
            if (_gridTotals.Rows.Count > 0)
            {
                var lastRow = _gridTotals.Rows[_gridTotals.Rows.Count - 1];
                lastRow.DefaultCellStyle.BackColor = Color.FromArgb(200, 220, 240);
                lastRow.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Erro ao carregar dados do dashboard: {ex.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
