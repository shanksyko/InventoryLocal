using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class MainForm : Form
    {
        private readonly AccessInventoryStore? _store;

        private TabControl _tabs = null!;

        private DataGridView _gridComputadores = null!;
        private Button _btnAtualizarComputadores = null!;

        private DataGridView _gridTablets = null!;
        private Button _btnAtualizarTablets = null!;

        private DataGridView _gridColetores = null!;
        private Button _btnAtualizarColetores = null!;

        private DataGridView _gridCelulares = null!;
        private Button _btnAtualizarCelulares = null!;

        // Aba Avançado (config do banco)
        private Label _lblDbPath = null!;
        private Button _btnSelecionarDb = null!;
        private Button _btnResumoDb = null!;

        public MainForm()
        {
            Text = "InventarioSistem";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 600);
            MinimumSize = new Size(800, 500);

            InitializeLayout();

            try
            {
                // Tenta usar o banco já salvo em config
                var factory = new AccessConnectionFactory();
                _store = new AccessInventoryStore(factory);

                var path = AccessDatabaseManager.ResolveActiveDatabasePath();
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    _lblDbPath.Text = $"Banco atual: {path}";
                    EnableDataTabs(true);
                    LoadAllGrids();
                }
                else
                {
                    _lblDbPath.Text = "Banco atual: (não configurado)";
                    EnableDataTabs(false);
                }
            }
            catch (Exception ex)
            {
                _lblDbPath.Text = "Banco atual: (erro ao inicializar)";
                EnableDataTabs(false);
                MessageBox.Show(
                    this,
                    "Erro ao inicializar conexão com o banco Access:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeLayout()
        {
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var tabComputadores = new TabPage("Computadores");
            var tabTablets = new TabPage("Tablets");
            var tabColetores = new TabPage("Coletores Android");
            var tabCelulares = new TabPage("Celulares");
            var tabAvancado = new TabPage("Avançado");

            InitializeComputadoresTab(tabComputadores);
            InitializeTabletsTab(tabTablets);
            InitializeColetoresTab(tabColetores);
            InitializeCelularesTab(tabCelulares);
            InitializeAvancadoTab(tabAvancado);

            _tabs.TabPages.Add(tabComputadores);
            _tabs.TabPages.Add(tabTablets);
            _tabs.TabPages.Add(tabColetores);
            _tabs.TabPages.Add(tabCelulares);
            _tabs.TabPages.Add(tabAvancado);

            Controls.Add(_tabs);
        }

        private void InitializeComputadoresTab(TabPage page)
        {
            _btnAtualizarComputadores = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarComputadores.Click += (_, _) => LoadComputadores();

            _gridComputadores = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridComputadores.Resize += (_, _) =>
            {
                _gridComputadores.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarComputadores);
            page.Controls.Add(_gridComputadores);
        }

        private void InitializeTabletsTab(TabPage page)
        {
            _btnAtualizarTablets = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarTablets.Click += (_, _) => LoadTablets();

            _gridTablets = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridTablets.Resize += (_, _) =>
            {
                _gridTablets.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarTablets);
            page.Controls.Add(_gridTablets);
        }

        private void InitializeColetoresTab(TabPage page)
        {
            _btnAtualizarColetores = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarColetores.Click += (_, _) => LoadColetores();

            _gridColetores = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridColetores.Resize += (_, _) =>
            {
                _gridColetores.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarColetores);
            page.Controls.Add(_gridColetores);
        }

        private void InitializeCelularesTab(TabPage page)
        {
            _btnAtualizarCelulares = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarCelulares.Click += (_, _) => LoadCelulares();

            _gridCelulares = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _gridCelulares.Resize += (_, _) =>
            {
                _gridCelulares.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarCelulares);
            page.Controls.Add(_gridCelulares);
        }

        private void InitializeAvancadoTab(TabPage page)
        {
            _lblDbPath = new Label
            {
                AutoSize = true,
                Location = new Point(10, 15),
                Text = "Banco atual: (não configurado)"
            };

            _btnSelecionarDb = new Button
            {
                Text = "Selecionar banco (.accdb)...",
                AutoSize = true,
                Location = new Point(10, 45)
            };
            _btnSelecionarDb.Click += (_, _) => SelecionarBanco();

            _btnResumoDb = new Button
            {
                Text = "Resumo do banco",
                AutoSize = true,
                Location = new Point(10, 80)
            };
            _btnResumoDb.Click += (_, _) => MostrarResumoBanco();

            var lblHint = new Label
            {
                AutoSize = true,
                Location = new Point(10, 120),
                Text = "O banco selecionado é salvo nas configurações do usuário.\n" +
                       "Ao abrir o aplicativo novamente, ele reconectará automaticamente\n" +
                       "ao mesmo banco, até que você escolha outro aqui."
            };

            page.Controls.Add(_lblDbPath);
            page.Controls.Add(_btnSelecionarDb);
            page.Controls.Add(_btnResumoDb);
            page.Controls.Add(lblHint);
        }

        private void EnableDataTabs(bool enabled)
        {
            _btnAtualizarComputadores.Enabled = enabled;
            _btnAtualizarTablets.Enabled = enabled;
            _btnAtualizarColetores.Enabled = enabled;
            _btnAtualizarCelulares.Enabled = enabled;

            _gridComputadores.Enabled = enabled;
            _gridTablets.Enabled = enabled;
            _gridColetores.Enabled = enabled;
            _gridCelulares.Enabled = enabled;
        }

        private void LoadAllGrids()
        {
            LoadComputadores();
            LoadTablets();
            LoadColetores();
            LoadCelulares();
        }

        private void LoadComputadores()
        {
            if (_store == null) return;

            try
            {
                var list = _store.GetAllComputers();
                _gridComputadores.DataSource = new BindingList<Computer>(ToList(list));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar computadores:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadTablets()
        {
            if (_store == null) return;

            try
            {
                var list = _store.GetAllTablets();
                _gridTablets.DataSource = new BindingList<Tablet>(ToList(list));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar tablets:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadColetores()
        {
            if (_store == null) return;

            try
            {
                var list = _store.GetAllColetores();
                _gridColetores.DataSource = new BindingList<ColetorAndroid>(ToList(list));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar coletores:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadCelulares()
        {
            if (_store == null) return;

            try
            {
                var list = _store.GetAllCelulares();
                _gridCelulares.DataSource = new BindingList<Celular>(ToList(list));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar celulares:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static List<T> ToList<T>(IEnumerable<T> source)
        {
            return source is List<T> list ? list : new List<T>(source);
        }

        private void SelecionarBanco()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Access DB (*.accdb)|*.accdb",
                Title = "Selecione um banco Access existente"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var path = dialog.FileName;

            try
            {
                AccessDatabaseManager.SetActiveDatabasePath(path);
                _lblDbPath.Text = $"Banco atual: {path}";

                // Garante estrutura mínima
                bool hasAllTables;
                try
                {
                    hasAllTables = AccessSchemaManager.HasAllRequiredTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Erro ao verificar estrutura do banco:\n\n" + ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!hasAllTables)
                {
                    var respCreate = MessageBox.Show(
                        this,
                        "Este banco não possui todas as tabelas padrão do InventarioSistem.\n\n" +
                        "Deseja criá-las agora?",
                        "Criar tabelas",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (respCreate == DialogResult.Yes)
                    {
                        try
                        {
                            AccessSchemaManager.EnsureRequiredTables();
                            MessageBox.Show(this,
                                "Tabelas criadas/ajustadas com sucesso.",
                                "Banco Access",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this,
                                "Erro ao criar tabelas:\n\n" + ex.Message,
                                "Erro",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                EnableDataTabs(true);
                LoadAllGrids();

                MessageBox.Show(this,
                    "✔ Banco configurado e salvo nas configurações.\n" +
                    "Na próxima vez que abrir o programa, ele já conectará\n" +
                    "automaticamente neste banco, até que você escolha outro em 'Avançado'.",
                    "Banco Access",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao configurar banco:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MostrarResumoBanco()
        {
            try
            {
                var path = AccessDatabaseManager.ResolveActiveDatabasePath();
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    MessageBox.Show(this,
                        "Nenhum banco válido está configurado.\n" +
                        "Use 'Selecionar banco (.accdb)...' primeiro.",
                        "Resumo do banco",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var summary = AccessDatabaseManager.GetDatabaseSummary(path);
                MessageBox.Show(this,
                    summary,
                    "Resumo do banco",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao obter resumo do banco:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
