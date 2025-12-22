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
        private void InitializeNobreaksTab(TabPage page)
        {
            _btnAtualizarNobreaks = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarNobreaks.Click += (_, _) => LoadNobreaks();

            _btnNovoNobreak = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoNobreak.Click += (_, _) => NovoNobreak();

            _btnEditarNobreak = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarNobreak.Click += (_, _) => EditarNobreak();

            _btnExcluirNobreak = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirNobreak.Click += (_, _) => ExcluirNobreak();

            var _btnExportNobreaks = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportNobreaks.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Nobreak, this);

            var _btnDashboardNobreaks = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardNobreaks.Click += (_, _) => MostrarDashboardTotal();

            var _btnHelpNobreaks = new Button
            {
                Text = "?",
                AutoSize = true,
                Location = new Point(570, 10),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnHelpNobreaks.Click += (_, _) => MostrarOpcoesAbaComSubcategorias("Nobreaks", new Dictionary<string, string[]>
            {
                ["Ações de Gerenciamento"] = new[]
                {
                    "• Novo - Abre formulário para cadastrar um novo nobreak",
                    "• Editar selecionado - Edita o nobreak selecionado na lista",
                    "• Excluir - Remove o nobreak selecionado do banco de dados",
                    "• Atualizar - Recarrega a lista de nobreaks do banco de dados"
                },
                ["Exportação e Relatórios"] = new[]
                {
                    "• Exportar XLSX - Exporta a lista de nobreaks para arquivo Excel",
                    "• Gráfico - Mostra dashboard com estatísticas dos nobreaks"
                },
                ["Filtros e Pesquisa"] = new[]
                {
                    "• Filtro - Filtra nobreaks por Hostname, Local, IP, Modelo, Status ou Serial",
                    "• Limpar filtro - Remove os filtros aplicados"
                },
                ["Interações"] = new[]
                {
                    "• Duplo clique - Clique duas vezes em um registro para editá-lo rapidamente"
                },
                ["Informações Exibidas"] = new[]
                {
                    "• Campos: Hostname, Local, IP, Modelo, Status, SerialNumber, Cadastrado em"
                }
            });

            var lblFiltro = new Label
            {
                Text = "Filtro (Hostname/Local/IP/Modelo/Status/Serial):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtNobreaksFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtNobreaksFilter.TextChanged += (_, _) => ApplyNobreaksFilter();

            var btnClear = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtNobreaksFilter.Right + 10, 68)
            };
            btnClear.Click += (_, _) => _txtNobreaksFilter.Text = string.Empty;

            _gridNobreaks = CreateGenericGrid(page, 105);
            _gridNobreaks.AutoGenerateColumns = false;
            _gridNobreaks.Columns.Clear();
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hostname", DataPropertyName = "Hostname", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IP", DataPropertyName = "IpAddress", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modelo", DataPropertyName = "Modelo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = "SerialNumber", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridNobreaks.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridNobreaks.CellDoubleClick += (_, _) => EditarNobreak();

            page.Controls.Add(_btnAtualizarNobreaks);
            page.Controls.Add(_btnNovoNobreak);
            page.Controls.Add(_btnEditarNobreak);
            page.Controls.Add(_btnExcluirNobreak);
            page.Controls.Add(_btnExportNobreaks);
            page.Controls.Add(_btnDashboardNobreaks);
            page.Controls.Add(_btnHelpNobreaks);
            page.Controls.Add(lblFiltro);
            page.Controls.Add(_txtNobreaksFilter);
            page.Controls.Add(btnClear);
            page.Controls.Add(_gridNobreaks);
        }

        private async void LoadNobreaks()
        {
            if (_store == null) return;

            try
            {
                var list = await Task.Run(() => _store.GetAllNobreaks());
                _nobreaksCache = list.ToList();
                ApplyNobreaksFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar nobreaks:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void NovoNobreak()
        {
            if (_store == null) return;

            using var form = new NobreakEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddNobreak(form.Nobreak);
                LoadNobreaks();
                InventoryLogger.Info("WinForms", $"Nobreak cadastrado via UI: {form.Nobreak}");
            }
        }

        private void EditarNobreak()
        {
            if (_store == null) return;
            if (_gridNobreaks.CurrentRow?.DataBoundItem is not LegacyDevices.Nobreak selected)
            {
                MessageBox.Show(this, "Selecione um nobreak para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new NobreakEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Nobreak;
                updated.Id = selected.Id;
                _store.UpdateNobreak(updated);
                LoadNobreaks();
                InventoryLogger.Info("WinForms", $"Nobreak atualizado via UI (Id={updated.Id}).");
            }
        }

        private void ExcluirNobreak()
        {
            if (_store == null) return;
            if (_gridNobreaks.CurrentRow?.DataBoundItem is not LegacyDevices.Nobreak selected)
            {
                MessageBox.Show(this, "Selecione um nobreak para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o nobreak selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteNobreak(selected.Id);
            LoadNobreaks();
            InventoryLogger.Info("WinForms", $"Nobreak excluído via UI (Id={selected.Id}).");
        }
    }
}
