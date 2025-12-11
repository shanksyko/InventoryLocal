using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Core.Logging;
using InventarioSistem.WinForms.Forms;
using LegacyDevices = InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public partial class MainForm
    {
        private void InitializeMonitoresTab(TabPage page)
        {
            _btnAtualizarMonitores = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarMonitores.Click += (_, _) => LoadMonitores();

            _btnNovoMonitor = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoMonitor.Click += (_, _) => NovoMonitor();

            _btnEditarMonitor = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarMonitor.Click += (_, _) => EditarMonitor();

            _btnExcluirMonitor = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirMonitor.Click += (_, _) => ExcluirMonitor();

            var _btnExportMonitores = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportMonitores.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Monitor, this);

            var _btnDashboardMonitores = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardMonitores.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltro = new Label
            {
                Text = "Filtro (Modelo/Serial/Local/Responsável/Computador):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtMonitoresFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtMonitoresFilter.TextChanged += (_, _) => ApplyMonitoresFilter();

            var btnClear = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtMonitoresFilter.Right + 10, 68)
            };
            btnClear.Click += (_, _) => _txtMonitoresFilter.Text = string.Empty;

            _gridMonitores = CreateGenericGrid(page, 105);
            _gridMonitores.AutoGenerateColumns = false;
            _gridMonitores.Columns.Clear();
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modelo", DataPropertyName = "Modelo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = "SerialNumber", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Responsável", DataPropertyName = "Responsavel", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Computador", DataPropertyName = "ComputadorVinculado", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridMonitores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridMonitores.CellDoubleClick += (_, _) => EditarMonitor();

            page.Controls.Add(_btnAtualizarMonitores);
            page.Controls.Add(_btnNovoMonitor);
            page.Controls.Add(_btnEditarMonitor);
            page.Controls.Add(_btnExcluirMonitor);
            page.Controls.Add(_btnExportMonitores);
            page.Controls.Add(_btnDashboardMonitores);
            page.Controls.Add(lblFiltro);
            page.Controls.Add(_txtMonitoresFilter);
            page.Controls.Add(btnClear);
            page.Controls.Add(_gridMonitores);
        }

        private async void LoadMonitores()
        {
            if (_store == null) return;

            try
            {
                var list = await Task.Run(() => _store.GetAllMonitores());
                _monitoresCache = list.ToList();
                ApplyMonitoresFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar monitores:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void NovoMonitor()
        {
            if (_store == null) return;

            using var form = new MonitorEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddMonitor(form.Monitor);
                LoadMonitores();
                InventoryLogger.Info("WinForms", $"Monitor cadastrado via UI: {form.Monitor}");
            }
        }

        private void EditarMonitor()
        {
            if (_store == null) return;
            if (_gridMonitores.CurrentRow?.DataBoundItem is not LegacyDevices.Monitor selected)
            {
                MessageBox.Show(this, "Selecione um monitor para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new MonitorEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Monitor;
                updated.Id = selected.Id;
                _store.UpdateMonitor(updated);
                LoadMonitores();
                InventoryLogger.Info("WinForms", $"Monitor atualizado via UI (Id={updated.Id}).");
            }
        }

        private void ExcluirMonitor()
        {
            if (_store == null) return;
            if (_gridMonitores.CurrentRow?.DataBoundItem is not LegacyDevices.Monitor selected)
            {
                MessageBox.Show(this, "Selecione um monitor para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o monitor selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteMonitor(selected.Id);
            LoadMonitores();
            InventoryLogger.Info("WinForms", $"Monitor excluído via UI (Id={selected.Id}).");
        }
    }
}
