using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Logging;
using LegacyDevices = InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class MainForm : Form
    {
        private readonly AccessInventoryStore? _store;

        // Cabeçalho visual
        private Panel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;
        private Button _btnDashboard = null!;

        private TabControl _tabs = null!;

        private DataGridView _gridComputadores = null!;
        private Button _btnAtualizarComputadores = null!;
        private Button _btnNovoComputador = null!;
        private Button _btnEditarComputador = null!;

        private DataGridView _gridTablets = null!;
        private Button _btnAtualizarTablets = null!;
        private Button _btnNovoTablet = null!;
        private Button _btnEditarTablet = null!;

        private DataGridView _gridColetores = null!;
        private Button _btnAtualizarColetores = null!;
        private Button _btnNovoColetor = null!;
        private Button _btnEditarColetor = null!;

        private DataGridView _gridCelulares = null!;
        private Button _btnAtualizarCelulares = null!;
        private Button _btnNovoCelular = null!;
        private Button _btnEditarCelular = null!;

        private DataGridView _gridImpressoras = null!;
        private Button _btnAtualizarImpressoras = null!;
        private Button _btnNovaImpressora = null!;
        private Button _btnEditarImpressora = null!;
        private Button _btnExcluirImpressora = null!;

        private DataGridView _gridDects = null!;
        private Button _btnAtualizarDects = null!;
        private Button _btnNovoDect = null!;
        private Button _btnEditarDect = null!;
        private Button _btnExcluirDect = null!;

        private DataGridView _gridTelefonesCisco = null!;
        private Button _btnAtualizarTelefonesCisco = null!;
        private Button _btnNovoTelefoneCisco = null!;
        private Button _btnEditarTelefoneCisco = null!;
        private Button _btnExcluirTelefoneCisco = null!;

        private DataGridView _gridTelevisores = null!;
        private Button _btnAtualizarTelevisores = null!;
        private Button _btnNovoTelevisor = null!;
        private Button _btnEditarTelevisor = null!;
        private Button _btnExcluirTelevisor = null!;

        // Aba Avançado (config do banco)
        private Label _lblDbPath = null!;
        private Button _btnSelecionarDb = null!;
        private Button _btnResumoDb = null!;

        // Aba Log
        private TextBox _txtLog = null!;

        public MainForm()
        {
            Text = "InventarioSistem";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 650);
            MinimumSize = new Size(900, 550);

            // Deixar o app com uma cara um pouco mais moderna
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(245, 247, 250);

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
                _store.EnsureSchemaAsync().GetAwaiter().GetResult();
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

            // Assina logger para exibir em tempo real
            InventoryLogger.MessageLogged += OnLogMessage;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InventoryLogger.MessageLogged -= OnLogMessage;
            base.OnFormClosed(e);
        }

        private void InitializeLayout()
        {
            // Cabeçalho superior
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(230, 236, 245),
                Padding = new Padding(10, 10, 10, 10)
            };

            _lblTitle = new Label
            {
                AutoSize = true,
                Text = "InventarioSistem",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
                Location = new Point(10, 10)
            };

            _lblSubtitle = new Label
            {
                AutoSize = true,
                Text = "Controle unificado de Computadores, Tablets, Coletores, Celulares e mais",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(12, 40)
            };

            _btnDashboard = new Button
            {
                Text = "Dashboard",
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(950, 20)
            };
            _btnDashboard.Click += (_, _) => MostrarDashboard();

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_btnDashboard);

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var tabComputadores = new TabPage("Computadores");
            var tabTablets = new TabPage("Tablets");
            var tabColetores = new TabPage("Coletores Android");
            var tabCelulares = new TabPage("Celulares");
            var tabImpressoras = new TabPage("Impressoras");
            var tabDects = new TabPage("DECTs");
            var tabTelefonesCisco = new TabPage("Telefones Cisco");
            var tabTelevisores = new TabPage("Televisores");
            var tabAvancado = new TabPage("Avançado");
            var tabLog = new TabPage("Log");

            InitializeComputadoresTab(tabComputadores);
            InitializeTabletsTab(tabTablets);
            InitializeColetoresTab(tabColetores);
            InitializeCelularesTab(tabCelulares);
            InitializeImpressorasTab(tabImpressoras);
            InitializeDectsTab(tabDects);
            InitializeTelefonesCiscoTab(tabTelefonesCisco);
            InitializeTelevisoresTab(tabTelevisores);
            InitializeAvancadoTab(tabAvancado);
            InitializeLogTab(tabLog);

            _tabs.TabPages.Add(tabComputadores);
            _tabs.TabPages.Add(tabTablets);
            _tabs.TabPages.Add(tabColetores);
            _tabs.TabPages.Add(tabCelulares);
            _tabs.TabPages.Add(tabImpressoras);
            _tabs.TabPages.Add(tabDects);
            _tabs.TabPages.Add(tabTelefonesCisco);
            _tabs.TabPages.Add(tabTelevisores);
            _tabs.TabPages.Add(tabAvancado);
            _tabs.TabPages.Add(tabLog);

            Controls.Add(_tabs);
            Controls.Add(_headerPanel);
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

            _btnNovoComputador = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoComputador.Click += (_, _) => NovoComputador();

            _btnEditarComputador = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarComputador.Click += (_, _) => EditarComputador();

            _gridComputadores = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _gridComputadores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridComputadores.CellDoubleClick += (_, _) => EditarComputador();
            _gridComputadores.Resize += (_, _) =>
            {
                _gridComputadores.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarComputadores);
            page.Controls.Add(_btnNovoComputador);
            page.Controls.Add(_btnEditarComputador);
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

            _btnNovoTablet = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoTablet.Click += (_, _) => NovoTablet();

            _btnEditarTablet = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarTablet.Click += (_, _) => EditarTablet();

            _gridTablets = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _gridTablets.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridTablets.CellDoubleClick += (_, _) => EditarTablet();
            _gridTablets.Resize += (_, _) =>
            {
                _gridTablets.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarTablets);
            page.Controls.Add(_btnNovoTablet);
            page.Controls.Add(_btnEditarTablet);
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

            _btnNovoColetor = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoColetor.Click += (_, _) => NovoColetor();

            _btnEditarColetor = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarColetor.Click += (_, _) => EditarColetor();

            _gridColetores = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _gridColetores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridColetores.CellDoubleClick += (_, _) => EditarColetor();
            _gridColetores.Resize += (_, _) =>
            {
                _gridColetores.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarColetores);
            page.Controls.Add(_btnNovoColetor);
            page.Controls.Add(_btnEditarColetor);
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

            _btnNovoCelular = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoCelular.Click += (_, _) => NovoCelular();

            _btnEditarCelular = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarCelular.Click += (_, _) => EditarCelular();

            _gridCelulares = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _gridCelulares.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridCelulares.CellDoubleClick += (_, _) => EditarCelular();
            _gridCelulares.Resize += (_, _) =>
            {
                _gridCelulares.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - 55);
            };

            page.Controls.Add(_btnAtualizarCelulares);
            page.Controls.Add(_btnNovoCelular);
            page.Controls.Add(_btnEditarCelular);
            page.Controls.Add(_gridCelulares);
        }

        private void InitializeImpressorasTab(TabPage page)
        {
            _btnAtualizarImpressoras = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarImpressoras.Click += (_, _) => LoadImpressoras();

            _btnNovaImpressora = new Button
            {
                Text = "Nova",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovaImpressora.Click += (_, _) => NovoImpressora();

            _btnEditarImpressora = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarImpressora.Click += (_, _) => EditarImpressora();

            _btnExcluirImpressora = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirImpressora.Click += (_, _) => ExcluirImpressora();

            _gridImpressoras = CreateGenericGrid(page);
            _gridImpressoras.CellDoubleClick += (_, _) => EditarImpressora();

            page.Controls.Add(_btnAtualizarImpressoras);
            page.Controls.Add(_btnNovaImpressora);
            page.Controls.Add(_btnEditarImpressora);
            page.Controls.Add(_btnExcluirImpressora);
            page.Controls.Add(_gridImpressoras);
        }

        private void InitializeDectsTab(TabPage page)
        {
            _btnAtualizarDects = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarDects.Click += (_, _) => LoadDects();

            _btnNovoDect = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoDect.Click += (_, _) => NovoDect();

            _btnEditarDect = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarDect.Click += (_, _) => EditarDect();

            _btnExcluirDect = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirDect.Click += (_, _) => ExcluirDect();

            _gridDects = CreateGenericGrid(page);
            _gridDects.CellDoubleClick += (_, _) => EditarDect();

            page.Controls.Add(_btnAtualizarDects);
            page.Controls.Add(_btnNovoDect);
            page.Controls.Add(_btnEditarDect);
            page.Controls.Add(_btnExcluirDect);
            page.Controls.Add(_gridDects);
        }

        private void InitializeTelefonesCiscoTab(TabPage page)
        {
            _btnAtualizarTelefonesCisco = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarTelefonesCisco.Click += (_, _) => LoadTelefonesCisco();

            _btnNovoTelefoneCisco = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoTelefoneCisco.Click += (_, _) => NovoTelefoneCisco();

            _btnEditarTelefoneCisco = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarTelefoneCisco.Click += (_, _) => EditarTelefoneCisco();

            _btnExcluirTelefoneCisco = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirTelefoneCisco.Click += (_, _) => ExcluirTelefoneCisco();

            _gridTelefonesCisco = CreateGenericGrid(page);
            _gridTelefonesCisco.CellDoubleClick += (_, _) => EditarTelefoneCisco();

            page.Controls.Add(_btnAtualizarTelefonesCisco);
            page.Controls.Add(_btnNovoTelefoneCisco);
            page.Controls.Add(_btnEditarTelefoneCisco);
            page.Controls.Add(_btnExcluirTelefoneCisco);
            page.Controls.Add(_gridTelefonesCisco);
        }

        private void InitializeTelevisoresTab(TabPage page)
        {
            _btnAtualizarTelevisores = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarTelevisores.Click += (_, _) => LoadTelevisores();

            _btnNovoTelevisor = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoTelevisor.Click += (_, _) => NovoTelevisor();

            _btnEditarTelevisor = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarTelevisor.Click += (_, _) => EditarTelevisor();

            _btnExcluirTelevisor = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirTelevisor.Click += (_, _) => ExcluirTelevisor();

            _gridTelevisores = CreateGenericGrid(page);
            _gridTelevisores.CellDoubleClick += (_, _) => EditarTelevisor();

            page.Controls.Add(_btnAtualizarTelevisores);
            page.Controls.Add(_btnNovoTelevisor);
            page.Controls.Add(_btnEditarTelevisor);
            page.Controls.Add(_btnExcluirTelevisor);
            page.Controls.Add(_gridTelevisores);
        }

        private DataGridView CreateGenericGrid(TabPage page)
        {
            var grid = new DataGridView
            {
                Location = new Point(10, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.Resize += (_, _) =>
            {
                grid.Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 55);
            };

            return grid;
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

        private void InitializeLogTab(TabPage page)
        {
            _txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };

            page.Controls.Add(_txtLog);
        }

        private void EnableDataTabs(bool enabled)
        {
            _btnAtualizarComputadores.Enabled = enabled;
            _btnNovoComputador.Enabled = enabled;
            _btnEditarComputador.Enabled = enabled;
            _btnAtualizarTablets.Enabled = enabled;
            _btnNovoTablet.Enabled = enabled;
            _btnEditarTablet.Enabled = enabled;
            _btnAtualizarColetores.Enabled = enabled;
            _btnNovoColetor.Enabled = enabled;
            _btnEditarColetor.Enabled = enabled;
            _btnAtualizarCelulares.Enabled = enabled;
            _btnNovoCelular.Enabled = enabled;
            _btnEditarCelular.Enabled = enabled;
            _btnAtualizarImpressoras.Enabled = enabled;
            _btnNovaImpressora.Enabled = enabled;
            _btnEditarImpressora.Enabled = enabled;
            _btnExcluirImpressora.Enabled = enabled;
            _btnAtualizarDects.Enabled = enabled;
            _btnNovoDect.Enabled = enabled;
            _btnEditarDect.Enabled = enabled;
            _btnExcluirDect.Enabled = enabled;
            _btnAtualizarTelefonesCisco.Enabled = enabled;
            _btnNovoTelefoneCisco.Enabled = enabled;
            _btnEditarTelefoneCisco.Enabled = enabled;
            _btnExcluirTelefoneCisco.Enabled = enabled;
            _btnAtualizarTelevisores.Enabled = enabled;
            _btnNovoTelevisor.Enabled = enabled;
            _btnEditarTelevisor.Enabled = enabled;
            _btnExcluirTelevisor.Enabled = enabled;

            _gridComputadores.Enabled = enabled;
            _gridTablets.Enabled = enabled;
            _gridColetores.Enabled = enabled;
            _gridCelulares.Enabled = enabled;
            _gridImpressoras.Enabled = enabled;
            _gridDects.Enabled = enabled;
            _gridTelefonesCisco.Enabled = enabled;
            _gridTelevisores.Enabled = enabled;
        }

        private void OnLogMessage(string line)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<string>(OnLogMessage), line);
                }
                catch
                {
                    // Ignorar se a janela já estiver fechando
                }
                return;
            }

            if (_txtLog == null) return;

            _txtLog.AppendText(line + Environment.NewLine);
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.ScrollToCaret();
        }

        private void LoadAllGrids()
        {
            LoadComputadores();
            LoadTablets();
            LoadColetores();
            LoadCelulares();
            LoadImpressoras();
            LoadDects();
            LoadTelefonesCisco();
            LoadTelevisores();
        }

        private void LoadComputadores()
        {
            if (_store == null) return;

            try
            {
                var list = _store.GetAllComputers();
                _gridComputadores.DataSource = new BindingList<LegacyDevices.Computer>(ToList(list));
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
                _gridTablets.DataSource = new BindingList<LegacyDevices.Tablet>(ToList(list));
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
                _gridColetores.DataSource = new BindingList<LegacyDevices.ColetorAndroid>(ToList(list));
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
                _gridCelulares.DataSource = new BindingList<LegacyDevices.Celular>(ToList(list));
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

        private void NovoComputador()
        {
            if (_store == null)
            {
                MessageBox.Show(this, "Banco não configurado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new ComputerEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddComputer(form.Result);
                LoadComputadores();
                InventoryLogger.Info("WinForms", $"Computador cadastrado via UI: Host='{form.Result.Host}', NS='{form.Result.SerialNumber}'");
            }
        }

        private void EditarComputador()
        {
            if (_store == null) return;
            if (_gridComputadores.CurrentRow?.DataBoundItem is not LegacyDevices.Computer selected)
            {
                MessageBox.Show(this, "Selecione um computador para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new ComputerEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Result;
                updated.Id = selected.Id;
                _store.UpdateComputer(updated);
                LoadComputadores();
                InventoryLogger.Info("WinForms", $"Computador editado via UI (Id={updated.Id}): Host='{updated.Host}', NS='{updated.SerialNumber}'");
            }
        }

        private void NovoTablet()
        {
            if (_store == null)
            {
                MessageBox.Show(this, "Banco não configurado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new TabletEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddTablet(form.Result);
                LoadTablets();
                InventoryLogger.Info("WinForms", $"Tablet cadastrado via UI: Host='{form.Result.Host}', NS='{form.Result.SerialNumber}'");
            }
        }

        private void EditarTablet()
        {
            if (_store == null) return;
            if (_gridTablets.CurrentRow?.DataBoundItem is not LegacyDevices.Tablet selected)
            {
                MessageBox.Show(this, "Selecione um tablet para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new TabletEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Result;
                updated.Id = selected.Id;
                _store.UpdateTablet(updated);
                LoadTablets();
                InventoryLogger.Info("WinForms", $"Tablet editado via UI (Id={updated.Id}): Host='{updated.Host}', NS='{updated.SerialNumber}'");
            }
        }

        private void NovoColetor()
        {
            if (_store == null)
            {
                MessageBox.Show(this, "Banco não configurado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new ColetorEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddColetor(form.Result);
                LoadColetores();
                InventoryLogger.Info("WinForms", $"Coletor cadastrado via UI: Host='{form.Result.Host}', NS='{form.Result.SerialNumber}'");
            }
        }

        private void EditarColetor()
        {
            if (_store == null) return;
            if (_gridColetores.CurrentRow?.DataBoundItem is not LegacyDevices.ColetorAndroid selected)
            {
                MessageBox.Show(this, "Selecione um coletor para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new ColetorEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Result;
                updated.Id = selected.Id;
                _store.UpdateColetor(updated);
                LoadColetores();
                InventoryLogger.Info("WinForms", $"Coletor editado via UI (Id={updated.Id}): Host='{updated.Host}', NS='{updated.SerialNumber}'");
            }
        }

        private void NovoCelular()
        {
            if (_store == null)
            {
                MessageBox.Show(this, "Banco não configurado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new CelularEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _store.AddCelular(form.Result);
                LoadCelulares();
                InventoryLogger.Info("WinForms", $"Celular cadastrado via UI: Modelo='{form.Result.Modelo}', Numero='{form.Result.Numero}'");
            }
        }

        private void EditarCelular()
        {
            if (_store == null) return;
            if (_gridCelulares.CurrentRow?.DataBoundItem is not LegacyDevices.Celular selected)
            {
                MessageBox.Show(this, "Selecione um celular para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new CelularEditForm(selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var updated = form.Result;
                updated.Id = selected.Id;
                _store.UpdateCelular(updated);
                LoadCelulares();
                InventoryLogger.Info("WinForms", $"Celular editado via UI (Id={updated.Id}): Modelo='{updated.Modelo}', Numero='{updated.Numero}'");
            }
        }

        private void LoadDevices(DeviceType type, DataGridView grid)
        {
            if (_store == null) return;

            try
            {
                var list = _store.ListAsync(type).GetAwaiter().GetResult();
                grid.DataSource = new BindingList<Device>(list.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Erro ao carregar lista de {type}:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void NovoDevice(Device deviceTemplate, string logLabel, Action reload)
        {
            if (_store == null)
            {
                MessageBox.Show(this, "Banco não configurado.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new DeviceEditForm(_store, deviceTemplate);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                reload();
                InventoryLogger.Info("WinForms", $"{logLabel} cadastrado via UI: {form.Result}");
            }
        }

        private void EditarDevice(DataGridView grid, string selectMessage, Action reload)
        {
            if (_store == null) return;
            if (grid.CurrentRow?.DataBoundItem is not Device selected)
            {
                MessageBox.Show(this, selectMessage, "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new DeviceEditForm(_store, selected);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                reload();
                InventoryLogger.Info("WinForms", $"{selected.Type} editado via UI (Id={selected.Id}): {form.Result}");
            }
        }

        private void ExcluirDevice(DataGridView grid, string selectMessage, Action reload)
        {
            if (_store == null) return;
            if (grid.CurrentRow?.DataBoundItem is not Device selected)
            {
                MessageBox.Show(this, selectMessage, "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                "Deseja realmente excluir o item selecionado?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _store.DeleteAsync(selected.Id).GetAwaiter().GetResult();
            reload();
            InventoryLogger.Info("WinForms", $"{selected.Type} excluído via UI (Id={selected.Id})");
        }

        private void LoadImpressoras() => LoadDevices(DeviceType.Impressora, _gridImpressoras);

        private void NovoImpressora() => NovoDevice(new Impressora(), "Impressora", LoadImpressoras);

        private void EditarImpressora() => EditarDevice(_gridImpressoras, "Selecione uma impressora para editar.", LoadImpressoras);

        private void ExcluirImpressora() => ExcluirDevice(_gridImpressoras, "Selecione uma impressora para excluir.", LoadImpressoras);

        private void LoadDects() => LoadDevices(DeviceType.Dect, _gridDects);

        private void NovoDect() => NovoDevice(new Dect(), "DECT", LoadDects);

        private void EditarDect() => EditarDevice(_gridDects, "Selecione um DECT para editar.", LoadDects);

        private void ExcluirDect() => ExcluirDevice(_gridDects, "Selecione um DECT para excluir.", LoadDects);

        private void LoadTelefonesCisco() => LoadDevices(DeviceType.TelefoneCisco, _gridTelefonesCisco);

        private void NovoTelefoneCisco() => NovoDevice(new TelefoneCisco(), "Telefone Cisco", LoadTelefonesCisco);

        private void EditarTelefoneCisco() => EditarDevice(_gridTelefonesCisco, "Selecione um telefone Cisco para editar.", LoadTelefonesCisco);

        private void ExcluirTelefoneCisco() => ExcluirDevice(_gridTelefonesCisco, "Selecione um telefone Cisco para excluir.", LoadTelefonesCisco);

        private void LoadTelevisores() => LoadDevices(DeviceType.Televisor, _gridTelevisores);

        private void NovoTelevisor() => NovoDevice(new Televisor(), "Televisor", LoadTelevisores);

        private void EditarTelevisor() => EditarDevice(_gridTelevisores, "Selecione um televisor para editar.", LoadTelevisores);

        private void ExcluirTelevisor() => ExcluirDevice(_gridTelevisores, "Selecione um televisor para excluir.", LoadTelevisores);

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
                InventoryLogger.Info("WinForms", $"Banco definido: {path}");

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
                            InventoryLogger.Info("WinForms", "Tabelas padrão criadas/ajustadas no banco selecionado.");
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

                InventoryLogger.Info("WinForms", "Banco configurado com sucesso e grids recarregadas.");
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

        private void MostrarDashboard()
        {
            if (_store == null)
            {
                MessageBox.Show(this,
                    "Configure o banco antes de abrir o dashboard.",
                    "Dashboard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dashboard = new Forms.DashboardForm(_store);
            dashboard.ShowDialog(this);
        }
    }
}
