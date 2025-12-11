using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly SqlServerInventoryStore _store;
    private Chart _chart = null!;
    private DataGridView _gridTotals = null!;

    public TotalDashboardForm(SqlServerInventoryStore store)
    {
        _store = store;
        InitializeUI();
        LoadDataAsync();
    }

    private void InitializeUI()
    {
        Text = "Dashboard Total de Itens";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(245, 247, 250);

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
            Label = "#VALX\n#PERCENT{P0}",  // Nome na primeira linha, percentual na segunda
            LegendText = "#VALX (#VAL)",
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            LabelForeColor = Color.Black
        };
        _chart.Series.Add(series);

        // Remover legenda para deixar mais espaço ao gráfico
        _chart.Legends.Clear();

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
            new DataGridViewTextBoxColumn { HeaderText = "Quantidade", DataPropertyName = "Count", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
            new DataGridViewTextBoxColumn { HeaderText = "Percentual", DataPropertyName = "Percentage", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells }
        });

        splitContainer.Panel2.Controls.Add(_gridTotals);

        Controls.Add(splitContainer);
        Controls.Add(lblTitle);
    }

    private async void LoadDataAsync()
    {
        try
        {
            // Mostrar indicador de carregamento
            var loadingLabel = new Label
            {
                Text = "⏳ Carregando dados...",
                Font = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkBlue
            };
            _chart.Visible = false;
            _gridTotals.Visible = false;

            // Carregar dados de forma assíncrona
            var totals = await Task.Run(async () =>
            {
                var result = new Dictionary<string, int>();
                var counts = await _store.CountByTypeAsync();
                
                // Mapear os nomes dos tipos
                var typeMapping = new Dictionary<string, string>
                {
                    ["Computadores"] = "Computadores",
                    ["Tablets"] = "Tablets",
                    ["ColetoresAndroid"] = "Coletores",
                    ["Celulares"] = "Celulares",
                    ["Impressoras"] = "Impressoras",
                    ["Dects"] = "DECTs",
                    ["TelefonesCisco"] = "Telefones Cisco",
                    ["Televisores"] = "Televisores",
                    ["RelogiosPonto"] = "Relógios Ponto",
                    ["Monitores"] = "Monitores",
                    ["Nobreaks"] = "Nobreaks"
                };
                
                foreach (var kvp in counts)
                {
                    var displayName = typeMapping.ContainsKey(kvp.Key) 
                        ? typeMapping[kvp.Key] 
                        : kvp.Key;
                    
                    if (kvp.Value > 0)
                        result[displayName] = kvp.Value;
                }
                
                return result;
            });

            loadingLabel.Dispose();
            _chart.Visible = true;
            _gridTotals.Visible = true;

            // Preencher gráfico
            var series = _chart.Series["Itens"];
            series.Points.Clear();

            foreach (var item in totals)
            {
                series.Points.AddXY(item.Key, item.Value);
            }

            // Preencher grid com dados e total
            var totalCount = totals.Sum(x => x.Value);
            var dataSource = totals.Select(x => new 
            { 
                Type = x.Key, 
                Count = x.Value,
                Percentage = $"{(x.Value * 100.0 / totalCount):F1}%"
            }).ToList();
            
            // Adicionar linha de total
            if (dataSource.Count > 0)
            {
                dataSource.Add(new 
                { 
                    Type = "TOTAL", 
                    Count = totalCount,
                    Percentage = "100.0%"
                });
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
