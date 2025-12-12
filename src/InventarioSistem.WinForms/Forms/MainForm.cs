using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Logging;
using InventarioSistem.WinForms.Forms;
using LegacyDevices = InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public partial class MainForm : Form
    {
        private SqlServerInventoryStore? _store;
        private SqlServerUserStore? _userStore;
        private SqlServerConnectionFactory? _sqlFactory;
        private System.Threading.Timer? _backupTimer;
        private bool _backupInProgress;
        private User? _currentUser;

        // Cabeçalho visual
        private Panel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;
        private CheckBox _chkUserMode = null!;
        private Label _lblMode = null!;
        private Label _lblUserInfo = null!;
        private Button _btnTotalDashboard = null!;

        private TabControl _tabs = null!;

        private DataGridView _gridComputadores = null!;
        private Button _btnAtualizarComputadores = null!;
        private Button _btnNovoComputador = null!;
        private Button _btnEditarComputador = null!;
        private Button _btnExcluirComputador = null!;

        private DataGridView _gridTablets = null!;
        private Button _btnAtualizarTablets = null!;
        private Button _btnNovoTablet = null!;
        private Button _btnEditarTablet = null!;
        private Button _btnExcluirTablet = null!;

        private DataGridView _gridColetores = null!;
        private Button _btnAtualizarColetores = null!;
        private Button _btnNovoColetor = null!;
        private Button _btnEditarColetor = null!;
        private Button _btnExcluirColetor = null!;

        private DataGridView _gridCelulares = null!;
        private Button _btnAtualizarCelulares = null!;
        private Button _btnNovoCelular = null!;
        private Button _btnEditarCelular = null!;
        private Button _btnExcluirCelular = null!;

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

        private DataGridView _gridRelogiosPonto = null!;
        private Button _btnAtualizarRelogiosPonto = null!;
        private Button _btnNovoRelogioPonto = null!;
        private Button _btnEditarRelogioPonto = null!;
        private Button _btnExcluirRelogioPonto = null!;

        private DataGridView _gridMonitores = null!;
        private Button _btnAtualizarMonitores = null!;
        private Button _btnNovoMonitor = null!;
        private Button _btnEditarMonitor = null!;
        private Button _btnExcluirMonitor = null!;

        private DataGridView _gridNobreaks = null!;
        private Button _btnAtualizarNobreaks = null!;
        private Button _btnNovoNobreak = null!;
        private Button _btnEditarNobreak = null!;
        private Button _btnExcluirNobreak = null!;

        // Aba Avançado (config do banco)
        private Label _lblDbPath = null!;
        private Button _btnSelecionarDb = null!;
        private Button _btnResumoDb = null!;
        private Button _btnGerenciarUsuariosAvancado = null!;
        private Button _btnBackupEnviarSql = null!;

        // Aba Log
        private TextBox _txtLog = null!;

        public MainForm(SqlServerConnectionFactory? sqlFactory = null, SqlServerInventoryStore? store = null, SqlServerUserStore? SqlServerUserStore = null, User? currentUser = null)
        {
            Text = "Inventory System";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 650);
            MinimumSize = new Size(900, 550);

            // Deixar o app com uma cara um pouco mais moderna
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
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

            // Initialize with SQL Server infrastructure
            var config = new SqlServerConfig();
            _sqlFactory = sqlFactory ?? new SqlServerConnectionFactory(config.ConnectionString);
            _store = store ?? new SqlServerInventoryStore(_sqlFactory);
            _userStore = SqlServerUserStore;
            _currentUser = currentUser ?? new User { Username = "admin", FullName = "Administrador", Role = UserRole.Admin };

            StartAutoBackupTimer();

            InitializeLayout();

            try
            {
                // Ensure schema is created
                SqlServerSchemaManager.EnsureRequiredTables(_sqlFactory);

                _lblDbPath.Text = "Banco: SQL Server (configurado)";
                EnableDataTabs(true);
                // Lazy-loading: não carrega todas as abas automaticamente
                // Dados serão carregados quando o usuário selecionar cada aba
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
                Text = "Inventory System",
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

            _chkUserMode = new CheckBox
            {
                Text = "Modo Usuário",
                Appearance = Appearance.Button,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 350, 20),
                BackColor = Color.Transparent
            };
            _chkUserMode.CheckedChanged += (_, _) => ApplyUserMode(_chkUserMode.Checked);

            _lblMode = new Label
            {
                AutoSize = true,
                Text = string.Empty,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 440, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point)
            };

            _lblUserInfo = new Label
            {
                AutoSize = true,
                Text = _currentUser != null ?
                    $"Conectado como: {_currentUser.FullName ?? _currentUser.Username} ({_currentUser.Role})" :
                    "",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 750, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(64, 64, 64)
            };

            _btnTotalDashboard = new Button
            {
                Text = "Dashboard Total",
                Size = new System.Drawing.Size(110, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 220, 18),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnTotalDashboard.Click += (_, _) => MostrarDashboardTotal();

            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_lblUserInfo);
            _headerPanel.Controls.Add(_lblMode);
            _headerPanel.Controls.Add(_chkUserMode);
            _headerPanel.Controls.Add(_btnTotalDashboard);

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var tabComputadores = new TabPage("Computadores");
            var tabTablets = new TabPage("Tablets");
            var tabColetores = new TabPage("Coletores Android");
            var tabCelulares = new TabPage("Celulares/Tablets");
            var tabImpressoras = new TabPage("Impressoras");
            var tabDects = new TabPage("DECTs");
            var tabTelefonesCisco = new TabPage("Telefones Cisco");
            var tabTelevisores = new TabPage("Televisores");
            var tabRelogiosPonto = new TabPage("Relógios Ponto");
            var tabMonitores = new TabPage("Monitores");
            var tabNobreaks = new TabPage("Nobreaks");
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
            InitializeRelogiosPontoTab(tabRelogiosPonto);
            InitializeMonitoresTab(tabMonitores);
            InitializeNobreaksTab(tabNobreaks);
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
            _tabs.TabPages.Add(tabRelogiosPonto);
            _tabs.TabPages.Add(tabMonitores);
            _tabs.TabPages.Add(tabNobreaks);
            _tabs.TabPages.Add(tabAvancado);
            _tabs.TabPages.Add(tabLog);

            // Implementar lazy-loading: carregar dados apenas quando a aba for selecionada
            _tabs.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            Controls.Add(_tabs);
            Controls.Add(_headerPanel);

            // Set default user mode off
            ApplyUserMode(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _backupTimer?.Dispose();
        }

        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Lazy-loading: carrega dados apenas quando a aba é selecionada
            if (_store == null) return;

            switch (_tabs.SelectedIndex)
            {
                case 0: // Computadores
                    if (_gridComputadores.DataSource == null)
                        LoadComputadores();
                    break;
                case 1: // Tablets
                    if (_gridTablets.DataSource == null)
                        LoadTablets();
                    break;
                case 2: // Coletores
                    if (_gridColetores.DataSource == null)
                        LoadColetores();
                    break;
                case 3: // Celulares
                    if (_gridCelulares.DataSource == null)
                        LoadCelulares();
                    break;
                case 4: // Impressoras
                    if (_gridImpressoras.DataSource == null)
                        LoadImpressoras();
                    break;
                case 5: // DECTs
                    if (_gridDects.DataSource == null)
                        LoadDects();
                    break;
                case 6: // Telefones Cisco
                    if (_gridTelefonesCisco.DataSource == null)
                        LoadTelefonesCisco();
                    break;
                case 7: // Televisores
                    if (_gridTelevisores.DataSource == null)
                        LoadTelevisores();
                    break;
                case 8: // Relógios Ponto
                    if (_gridRelogiosPonto.DataSource == null)
                        LoadRelogiosPonto();
                    break;
                case 9: // Monitores
                    if (_gridMonitores.DataSource == null)
                        LoadMonitores();
                    break;
                case 10: // Nobreaks
                    if (_gridNobreaks.DataSource == null)
                        LoadNobreaks();
                    break;
            }
        }

        private void ApplyUserMode(bool isUserMode)
        {
            var isVisualizer = _currentUser?.Role == UserRole.Visualizador;
            var effectiveUserMode = isUserMode || isVisualizer;

            // Update header visuals
            if (isVisualizer)
            {
                _headerPanel.BackColor = Color.FromArgb(245, 245, 235);
                _lblMode.Text = "Modo: Visualizador (somente leitura)";
                _chkUserMode.Checked = true;
                _chkUserMode.Enabled = false;
            }
            else if (effectiveUserMode)
            {
                _headerPanel.BackColor = Color.FromArgb(245, 250, 245);
                _lblMode.Text = "Modo: Usuário";
                _chkUserMode.Checked = true;
                _chkUserMode.Enabled = true;
            }
            else
            {
                _headerPanel.BackColor = Color.FromArgb(230, 236, 245);
                _lblMode.Text = string.Empty;
                _chkUserMode.Checked = false;
                _chkUserMode.Enabled = true;
            }

            // Disable editing controls when in user mode OR when user is Visualizador
            var enableEdit = !effectiveUserMode;
            var enableRefresh = _store != null; // visualizador pode atualizar listas

            // Refresh buttons (sempre liberados se banco configurado)
            _btnAtualizarComputadores.Enabled = enableRefresh;
            _btnAtualizarTablets.Enabled = enableRefresh;
            _btnAtualizarColetores.Enabled = enableRefresh;
            _btnAtualizarCelulares.Enabled = enableRefresh;
            _btnAtualizarImpressoras.Enabled = enableRefresh;
            _btnAtualizarDects.Enabled = enableRefresh;
            _btnAtualizarTelefonesCisco.Enabled = enableRefresh;
            _btnAtualizarTelevisores.Enabled = enableRefresh;
            _btnAtualizarRelogiosPonto.Enabled = enableRefresh;
            _btnAtualizarMonitores.Enabled = enableRefresh;
            _btnAtualizarNobreaks.Enabled = enableRefresh;

            // Edit/Create/Delete buttons (bloqueados para visualizador ou modo usuário)
            _btnNovoComputador.Enabled = enableEdit;
            _btnEditarComputador.Enabled = enableEdit;
            _btnExcluirComputador.Enabled = enableEdit;

            _btnNovoTablet.Enabled = enableEdit;
            _btnEditarTablet.Enabled = enableEdit;
            _btnExcluirTablet.Enabled = enableEdit;

            _btnNovoColetor.Enabled = enableEdit;
            _btnEditarColetor.Enabled = enableEdit;
            _btnExcluirColetor.Enabled = enableEdit;

            _btnNovoCelular.Enabled = enableEdit;
            _btnEditarCelular.Enabled = enableEdit;
            _btnExcluirCelular.Enabled = enableEdit;

            _btnNovaImpressora.Enabled = enableEdit;
            _btnEditarImpressora.Enabled = enableEdit;
            _btnExcluirImpressora.Enabled = enableEdit;

            _btnNovoDect.Enabled = enableEdit;
            _btnEditarDect.Enabled = enableEdit;
            _btnExcluirDect.Enabled = enableEdit;

            _btnNovoTelefoneCisco.Enabled = enableEdit;
            _btnEditarTelefoneCisco.Enabled = enableEdit;
            _btnExcluirTelefoneCisco.Enabled = enableEdit;

            _btnNovoTelevisor.Enabled = enableEdit;
            _btnEditarTelevisor.Enabled = enableEdit;
            _btnExcluirTelevisor.Enabled = enableEdit;

            _btnNovoRelogioPonto.Enabled = enableEdit;
            _btnEditarRelogioPonto.Enabled = enableEdit;
            _btnExcluirRelogioPonto.Enabled = enableEdit;

            _btnNovoMonitor.Enabled = enableEdit;
            _btnEditarMonitor.Enabled = enableEdit;
            _btnExcluirMonitor.Enabled = enableEdit;

            _btnNovoNobreak.Enabled = enableEdit;
            _btnEditarNobreak.Enabled = enableEdit;
            _btnExcluirNobreak.Enabled = enableEdit;

            // Advanced controls continuam bloqueados para visualizador
            _btnSelecionarDb.Enabled = enableEdit;
            _btnResumoDb.Enabled = enableEdit;
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

            _btnExcluirComputador = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirComputador.Click += (_, _) => ExcluirComputador();

            var _btnExportComputadores = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportComputadores.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Computer, this);

            var _btnDashboardComputadores = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardComputadores.Click += (_, _) => MostrarDashboardTotal();

            var lblInfo = new Label
            {
                Text = "ℹ️ Exibindo até 1000 registros mais recentes para melhor performance",
                AutoSize = true,
                Location = new Point(10, 50),
                ForeColor = Color.DarkBlue
            };

            var lblFiltro = new Label
            {
                Text = "Filtro (Host/N/S/Proprietário/Departamento/Matrícula):",
                AutoSize = true,
                Location = new Point(10, 75)
            };

            _txtComputersFilter = new TextBox
            {
                Location = new Point(10, 95),
                Width = 260
            };
            _txtComputersFilter.TextChanged += (_, _) => ApplyComputersFilter();

            var btnClearFilter = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtComputersFilter.Right + 10, 93)
            };
            btnClearFilter.Click += (_, _) => _txtComputersFilter.Text = string.Empty;

            _gridComputadores = new DataGridView
            {
                Location = new Point(10, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - 140),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            _gridComputadores.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Host",
                    DataPropertyName = nameof(LegacyDevices.Computer.Host),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "SerialNumber",
                    DataPropertyName = nameof(LegacyDevices.Computer.SerialNumber),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Proprietário",
                    DataPropertyName = nameof(LegacyDevices.Computer.Proprietario),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Departamento",
                    DataPropertyName = nameof(LegacyDevices.Computer.Departamento),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Matrícula",
                    DataPropertyName = nameof(LegacyDevices.Computer.Matricula),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Cadastrado em",
                    DataPropertyName = nameof(LegacyDevices.Computer.CreatedAt),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
                }
            });
            _gridComputadores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridComputadores.CellDoubleClick += (_, _) => EditarComputador();
            _gridComputadores.Resize += (_, _) =>
            {
                _gridComputadores.Size = new Size(
                    page.ClientSize.Width - 20,
                    page.ClientSize.Height - _gridComputadores.Top - 10);
            };

            page.Controls.Add(_btnAtualizarComputadores);
            page.Controls.Add(_btnNovoComputador);
            page.Controls.Add(_btnEditarComputador);
            page.Controls.Add(_btnExcluirComputador);
            page.Controls.Add(_btnExportComputadores);
            page.Controls.Add(_btnDashboardComputadores);
            page.Controls.Add(lblInfo);
            page.Controls.Add(lblFiltro);
            page.Controls.Add(_txtComputersFilter);
            page.Controls.Add(btnClearFilter);
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

            _btnExcluirTablet = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirTablet.Click += (_, _) => ExcluirTablet();

            var _btnExportTablets = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportTablets.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Tablet, this);

            var _btnDashboardTablets = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardTablets.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroTablets = new Label
            {
                Text = "Filtro (Host/Serial/Local/Responsável/IMEI):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtTabletsFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtTabletsFilter.TextChanged += (_, _) => ApplyTabletsFilter();

            var btnClearFilterTablets = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtTabletsFilter.Right + 10, 68)
            };
            btnClearFilterTablets.Click += (_, _) => _txtTabletsFilter.Text = string.Empty;

            _gridTablets = CreateGenericGrid(page, 105);
            _gridTablets.AutoGenerateColumns = false;
            _gridTablets.Columns.Clear();
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Host", DataPropertyName = nameof(LegacyDevices.Tablet.Host), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = nameof(LegacyDevices.Tablet.SerialNumber), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = nameof(LegacyDevices.Tablet.Local), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Responsável", DataPropertyName = nameof(LegacyDevices.Tablet.Responsavel), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IMEIs", DataPropertyName = nameof(LegacyDevices.Tablet.ImeisJoined), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTablets.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = nameof(LegacyDevices.Tablet.CreatedAt), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridTablets.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridTablets.CellDoubleClick += (_, _) => EditarTablet();

            page.Controls.Add(_btnAtualizarTablets);
            page.Controls.Add(_btnNovoTablet);
            page.Controls.Add(_btnEditarTablet);
            page.Controls.Add(_btnExcluirTablet);
            page.Controls.Add(_btnExportTablets);
            page.Controls.Add(_btnDashboardTablets);
            page.Controls.Add(lblFiltroTablets);
            page.Controls.Add(_txtTabletsFilter);
            page.Controls.Add(btnClearFilterTablets);
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

            _btnExcluirColetor = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirColetor.Click += (_, _) => ExcluirColetor();

            var _btnExportColetores = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportColetores.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.ColetorAndroid, this);

            var _btnDashboardColetores = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardColetores.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroColetores = new Label
            {
                Text = "Filtro (Host/Serial/MAC/IP/Local):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtColetoresFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtColetoresFilter.TextChanged += (_, _) => ApplyColetoresFilter();

            var btnClearFilterColetores = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtColetoresFilter.Right + 10, 68)
            };
            btnClearFilterColetores.Click += (_, _) => _txtColetoresFilter.Text = string.Empty;

            _gridColetores = CreateGenericGrid(page, 105);
            _gridColetores.AutoGenerateColumns = false;
            _gridColetores.Columns.Clear();
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Host", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.Host), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.SerialNumber), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MAC Address", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.MacAddress), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IP", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.IpAddress), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.Local), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Aplicativos", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.AppsInstalados), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sistema Operacional", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.SistemaOperacional), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridColetores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = nameof(LegacyDevices.ColetorAndroid.CreatedAt), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridColetores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridColetores.CellDoubleClick += (_, _) => EditarColetor();

            page.Controls.Add(_btnAtualizarColetores);
            page.Controls.Add(_btnNovoColetor);
            page.Controls.Add(_btnEditarColetor);
            page.Controls.Add(_btnExcluirColetor);
            page.Controls.Add(_btnExportColetores);
            page.Controls.Add(_btnDashboardColetores);
            page.Controls.Add(lblFiltroColetores);
            page.Controls.Add(_txtColetoresFilter);
            page.Controls.Add(btnClearFilterColetores);
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

            _btnExcluirCelular = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirCelular.Click += (_, _) => ExcluirCelular();

            var _btnExportCelulares = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportCelulares.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Celular, this);

            var _btnDashboardCelulares = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardCelulares.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroCelulares = new Label
            {
                Text = "Filtro (CellName / IMEI / Modelo / Número / Usuário / Setor):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtCelularesFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 320
            };
            _txtCelularesFilter.TextChanged += (_, _) => ApplyCelularesFilter();

            var btnClearFilterCelulares = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtCelularesFilter.Right + 10, 68)
            };
            btnClearFilterCelulares.Click += (_, _) => _txtCelularesFilter.Text = string.Empty;

            _gridCelulares = CreateGenericGrid(page, 105);
            _gridCelulares.AutoGenerateColumns = false;
            _gridCelulares.Columns.Clear();
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CellName", DataPropertyName = nameof(LegacyDevices.Celular.CellName), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IMEI1", DataPropertyName = nameof(LegacyDevices.Celular.Imei1), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IMEI2", DataPropertyName = nameof(LegacyDevices.Celular.Imei2), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modelo", DataPropertyName = nameof(LegacyDevices.Celular.Modelo), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Número", DataPropertyName = nameof(LegacyDevices.Celular.Numero), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Roaming", DataPropertyName = nameof(LegacyDevices.Celular.Roaming), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Usuário", DataPropertyName = nameof(LegacyDevices.Celular.Usuario), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Matrícula", DataPropertyName = nameof(LegacyDevices.Celular.Matricula), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cargo", DataPropertyName = nameof(LegacyDevices.Celular.Cargo), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Setor", DataPropertyName = nameof(LegacyDevices.Celular.Setor), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "E-mail", DataPropertyName = nameof(LegacyDevices.Celular.Email), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Senha", DataPropertyName = nameof(LegacyDevices.Celular.Senha), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridCelulares.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = nameof(LegacyDevices.Celular.CreatedAt), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridCelulares.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridCelulares.CellDoubleClick += (_, _) => EditarCelular();

            page.Controls.Add(_btnAtualizarCelulares);
            page.Controls.Add(_btnNovoCelular);
            page.Controls.Add(_btnEditarCelular);
            page.Controls.Add(_btnExcluirCelular);
            page.Controls.Add(_btnExportCelulares);
            page.Controls.Add(_btnDashboardCelulares);
            page.Controls.Add(lblFiltroCelulares);
            page.Controls.Add(_txtCelularesFilter);
            page.Controls.Add(btnClearFilterCelulares);
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

            var _btnExportImpressoras = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportImpressoras.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Impressora, this);

            var _btnDashboardImpressoras = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardImpressoras.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroImpressoras = new Label
            {
                Text = "Filtro (Nome/Modelo/Serial/Local/Responsável):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtImpressorasFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtImpressorasFilter.TextChanged += (_, _) => ApplyImpressorasFilter();

            var btnClearFilterImpressoras = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtImpressorasFilter.Right + 10, 68)
            };
            btnClearFilterImpressoras.Click += (_, _) => _txtImpressorasFilter.Text = string.Empty;

            _gridImpressoras = CreateGenericGrid(page, 105);
            _gridImpressoras.AutoGenerateColumns = false;
            _gridImpressoras.Columns.Clear();
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Nome",
                DataPropertyName = "Nome",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tipo/Modelo",
                DataPropertyName = "TipoModelo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "SerialNumber",
                DataPropertyName = "SerialNumber",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Local Atual",
                DataPropertyName = "LocalAtual",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Local Anterior",
                DataPropertyName = "LocalAnterior",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            _gridImpressoras.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cadastrado em",
                DataPropertyName = "CreatedAt",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
            });
            _gridImpressoras.CellDoubleClick += (_, _) => EditarImpressora();

            page.Controls.Add(_btnAtualizarImpressoras);
            page.Controls.Add(_btnNovaImpressora);
            page.Controls.Add(_btnEditarImpressora);
            page.Controls.Add(_btnExcluirImpressora);
            page.Controls.Add(_btnExportImpressoras);
            page.Controls.Add(_btnDashboardImpressoras);
            page.Controls.Add(lblFiltroImpressoras);
            page.Controls.Add(_txtImpressorasFilter);
            page.Controls.Add(btnClearFilterImpressoras);
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

            var _btnExportDects = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportDects.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Dect, this);

            var _btnDashboardDects = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardDects.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroDects = new Label
            {
                Text = "Filtro (Responsável/IPEI/MAC/Número/Local/Modelo):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtDectsFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtDectsFilter.TextChanged += (_, _) => ApplyDectsFilter();

            var btnClearFilterDects = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtDectsFilter.Right + 10, 68)
            };
            btnClearFilterDects.Click += (_, _) => _txtDectsFilter.Text = string.Empty;

            _gridDects = CreateGenericGrid(page, 105);
            _gridDects.AutoGenerateColumns = false;
            _gridDects.Columns.Clear();
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Responsável", DataPropertyName = "Responsavel", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IPEI", DataPropertyName = "Ipei", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MAC Address", DataPropertyName = "MacAddress", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Número", DataPropertyName = "Numero", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridDects.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridDects.CellDoubleClick += (_, _) => EditarDect();

            page.Controls.Add(_btnAtualizarDects);
            page.Controls.Add(_btnNovoDect);
            page.Controls.Add(_btnEditarDect);
            page.Controls.Add(_btnExcluirDect);
            page.Controls.Add(_btnExportDects);
            page.Controls.Add(_btnDashboardDects);
            page.Controls.Add(lblFiltroDects);
            page.Controls.Add(_txtDectsFilter);
            page.Controls.Add(btnClearFilterDects);
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

            var _btnExportTelefonesCisco = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportTelefonesCisco.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.TelefoneCisco, this);

            var _btnDashboardCisco = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardCisco.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroCisco = new Label
            {
                Text = "Filtro (Responsável/MAC/Número/Local/IP/Serial):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtCiscoFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtCiscoFilter.TextChanged += (_, _) => ApplyCiscoFilter();

            var btnClearFilterCisco = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtCiscoFilter.Right + 10, 68)
            };
            btnClearFilterCisco.Click += (_, _) => _txtCiscoFilter.Text = string.Empty;

            _gridTelefonesCisco = CreateGenericGrid(page, 105);
            _gridTelefonesCisco.AutoGenerateColumns = false;
            _gridTelefonesCisco.Columns.Clear();
            _gridTelefonesCisco.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Responsável", DataPropertyName = "Responsavel", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTelefonesCisco.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MAC Address", DataPropertyName = "MacAddress", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTelefonesCisco.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Número", DataPropertyName = "Numero", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTelefonesCisco.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTelefonesCisco.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridTelefonesCisco.CellDoubleClick += (_, _) => EditarTelefoneCisco();

            page.Controls.Add(_btnAtualizarTelefonesCisco);
            page.Controls.Add(_btnNovoTelefoneCisco);
            page.Controls.Add(_btnEditarTelefoneCisco);
            page.Controls.Add(_btnExcluirTelefoneCisco);
            page.Controls.Add(_btnExportTelefonesCisco);
            page.Controls.Add(_btnDashboardCisco);
            page.Controls.Add(lblFiltroCisco);
            page.Controls.Add(_txtCiscoFilter);
            page.Controls.Add(btnClearFilterCisco);
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

            var _btnExportTelevisores = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportTelevisores.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.Televisor, this);

            var _btnDashboardTelevisores = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardTelevisores.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroTvs = new Label
            {
                Text = "Filtro (Modelo/Serial/Local):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtTvsFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtTvsFilter.TextChanged += (_, _) => ApplyTvsFilter();

            var btnClearFilterTvs = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtTvsFilter.Right + 10, 68)
            };
            btnClearFilterTvs.Click += (_, _) => _txtTvsFilter.Text = string.Empty;

            _gridTelevisores = CreateGenericGrid(page, 105);
            _gridTelevisores.AutoGenerateColumns = false;
            _gridTelevisores.Columns.Clear();
            _gridTelevisores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modelo", DataPropertyName = "Modelo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTelevisores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = "SerialNumber", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridTelevisores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridTelevisores.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridTelevisores.CellDoubleClick += (_, _) => EditarTelevisor();

            page.Controls.Add(_btnAtualizarTelevisores);
            page.Controls.Add(_btnNovoTelevisor);
            page.Controls.Add(_btnEditarTelevisor);
            page.Controls.Add(_btnExcluirTelevisor);
            page.Controls.Add(_btnExportTelevisores);
            page.Controls.Add(_btnDashboardTelevisores);
            page.Controls.Add(lblFiltroTvs);
            page.Controls.Add(_txtTvsFilter);
            page.Controls.Add(btnClearFilterTvs);
            page.Controls.Add(_gridTelevisores);
        }

        private void InitializeRelogiosPontoTab(TabPage page)
        {
            _btnAtualizarRelogiosPonto = new Button
            {
                Text = "Atualizar",
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _btnAtualizarRelogiosPonto.Click += (_, _) => LoadRelogiosPonto();

            _btnNovoRelogioPonto = new Button
            {
                Text = "Novo",
                AutoSize = true,
                Location = new Point(100, 10)
            };
            _btnNovoRelogioPonto.Click += (_, _) => NovoRelogioPonto();

            _btnEditarRelogioPonto = new Button
            {
                Text = "Editar selecionado",
                AutoSize = true,
                Location = new Point(170, 10)
            };
            _btnEditarRelogioPonto.Click += (_, _) => EditarRelogioPonto();

            _btnExcluirRelogioPonto = new Button
            {
                Text = "Excluir",
                AutoSize = true,
                Location = new Point(310, 10)
            };
            _btnExcluirRelogioPonto.Click += (_, _) => ExcluirRelogioPonto();

            var _btnExportRelogiosPonto = new Button
            {
                Text = "Exportar XLSX",
                AutoSize = true,
                Location = new Point(400, 10)
            };
            _btnExportRelogiosPonto.Click += (_, _) => XlsxExporter.ExportWithDialog(_store!, InventarioSistem.Core.Entities.DeviceType.RelogioPonto, this);

            var _btnDashboardRelogios = new Button
            {
                Text = "Gráfico",
                AutoSize = true,
                Location = new Point(480, 10),
                BackColor = Color.FromArgb(50, 160, 120),
                ForeColor = Color.White
            };
            _btnDashboardRelogios.Click += (_, _) => MostrarDashboardTotal();

            var lblFiltroRelogios = new Label
            {
                Text = "Filtro (Modelo/Serial/Local/IP):",
                AutoSize = true,
                Location = new Point(10, 50)
            };

            _txtRelogiosFilter = new TextBox
            {
                Location = new Point(10, 70),
                Width = 260
            };
            _txtRelogiosFilter.TextChanged += (_, _) => ApplyRelogiosFilter();

            var btnClearFilterRelogios = new Button
            {
                Text = "Limpar filtro",
                AutoSize = true,
                Location = new Point(_txtRelogiosFilter.Right + 10, 68)
            };
            btnClearFilterRelogios.Click += (_, _) => _txtRelogiosFilter.Text = string.Empty;

            _gridRelogiosPonto = CreateGenericGrid(page, 105);
            _gridRelogiosPonto.AutoGenerateColumns = false;
            _gridRelogiosPonto.Columns.Clear();
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modelo", DataPropertyName = "Modelo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SerialNumber", DataPropertyName = "SerialNumber", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Local", DataPropertyName = "Local", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IP", DataPropertyName = "Ip", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Data Bateria", DataPropertyName = "DataBateria", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "d" } });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Data Nobreak", DataPropertyName = "DataNobreak", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "d" } });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Próximas Verificações", DataPropertyName = "ProximasVerificacoes", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "d" } });
            _gridRelogiosPonto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cadastrado em", DataPropertyName = "CreatedAt", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
            _gridRelogiosPonto.CellDoubleClick += (_, _) => EditarRelogioPonto();

            page.Controls.Add(_btnAtualizarRelogiosPonto);
            page.Controls.Add(_btnNovoRelogioPonto);
            page.Controls.Add(_btnEditarRelogioPonto);
            page.Controls.Add(_btnExcluirRelogioPonto);
            page.Controls.Add(_btnExportRelogiosPonto);
            page.Controls.Add(_btnDashboardRelogios);
            page.Controls.Add(lblFiltroRelogios);
            page.Controls.Add(_txtRelogiosFilter);
            page.Controls.Add(btnClearFilterRelogios);
            page.Controls.Add(_gridRelogiosPonto);
        }

        private DataGridView CreateGenericGrid(TabPage page, int topMargin = 45)
        {
            var grid = new DataGridView
            {
                Location = new Point(10, topMargin),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - (topMargin + 10)),
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
                grid.Size = new Size(page.ClientSize.Width - 20, page.ClientSize.Height - (topMargin + 10));
            };

            return grid;
        }

        private void InitializeAvancadoTab(TabPage page)
        {
            var stack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Location = new Point(10, 10),
                Padding = new Padding(0, 0, 0, 0)
            };

            _lblDbPath = new Label
            {
                AutoSize = true,
                Text = "Banco atual: (não configurado)"
            };

            _btnSelecionarDb = new Button
            {
                Text = "Configurar SQL Server...",
                AutoSize = true
            };
            _btnSelecionarDb.Click += (_, _) => SelecionarBanco();

            _btnResumoDb = new Button
            {
                Text = "Resumo do banco",
                AutoSize = true
            };
            _btnResumoDb.Click += (_, _) => MostrarResumoBanco();

            _btnBackupEnviarSql = new Button
            {
                Text = "Backup e enviar para SQL",
                AutoSize = true,
                Visible = _currentUser?.Role == UserRole.Admin
            };
            _btnBackupEnviarSql.Click += (_, _) => AbrirBackupEnvioSql();

            _btnGerenciarUsuariosAvancado = new Button
            {
                Text = "Gerenciar usuários...",
                AutoSize = true,
                Visible = _currentUser?.Role == UserRole.Admin
            };
            _btnGerenciarUsuariosAvancado.Click += (_, _) => AbrirGerenciadorUsuarios();

            var btnLogoff = new Button
            {
                Text = "Sair da Conta (Logoff)",
                AutoSize = true,
                BackColor = Color.FromArgb(220, 60, 60),
                ForeColor = Color.White,
                Padding = new Padding(5)
            };
            btnLogoff.Click += (_, _) => RealizarLogoff();

            var lblHint = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(380, 0),
                Text = "O banco selecionado é salvo nas configurações do usuário.\n" +
                       "Ao abrir o aplicativo novamente, ele reconectará automaticamente\n" +
                       "ao mesmo banco, até que você escolha outro aqui."
            };

            var lblDeveloper = new Label
            {
                AutoSize = true,
                Text = "Desenvolvido por Giancarlo Conrado Romualdo",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            // Espaçamentos
            stack.Controls.Add(_lblDbPath);
            stack.Controls.Add(_btnSelecionarDb);
            stack.Controls.Add(_btnResumoDb);
            stack.Controls.Add(_btnBackupEnviarSql);
            stack.Controls.Add(_btnGerenciarUsuariosAvancado);
            stack.Controls.Add(btnLogoff);
            stack.Controls.Add(lblHint);
            stack.Controls.Add(lblDeveloper);

            stack.SetFlowBreak(lblHint, true);

            page.Controls.Add(stack);
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
            var isVisualizer = _currentUser?.Role == UserRole.Visualizador;
            var enableRefresh = enabled; // pode atualizar se o banco estiver configurado
            var enableEdit = enabled && !isVisualizer; // visualizador não edita

            // Refresh
            _btnAtualizarComputadores.Enabled = enableRefresh;
            _btnAtualizarTablets.Enabled = enableRefresh;
            _btnAtualizarColetores.Enabled = enableRefresh;
            _btnAtualizarCelulares.Enabled = enableRefresh;
            _btnAtualizarImpressoras.Enabled = enableRefresh;
            _btnAtualizarDects.Enabled = enableRefresh;
            _btnAtualizarTelefonesCisco.Enabled = enableRefresh;
            _btnAtualizarTelevisores.Enabled = enableRefresh;
            _btnAtualizarRelogiosPonto.Enabled = enableRefresh;
            _btnAtualizarMonitores.Enabled = enableRefresh;
            _btnAtualizarNobreaks.Enabled = enableRefresh;

            // Edit/Create/Delete
            _btnNovoComputador.Enabled = enableEdit;
            _btnEditarComputador.Enabled = enableEdit;
            _btnExcluirComputador.Enabled = enableEdit;

            _btnNovoTablet.Enabled = enableEdit;
            _btnEditarTablet.Enabled = enableEdit;
            _btnExcluirTablet.Enabled = enableEdit;

            _btnNovoColetor.Enabled = enableEdit;
            _btnEditarColetor.Enabled = enableEdit;
            _btnExcluirColetor.Enabled = enableEdit;

            _btnNovoCelular.Enabled = enableEdit;
            _btnEditarCelular.Enabled = enableEdit;
            _btnExcluirCelular.Enabled = enableEdit;

            _btnNovaImpressora.Enabled = enableEdit;
            _btnEditarImpressora.Enabled = enableEdit;
            _btnExcluirImpressora.Enabled = enableEdit;

            _btnNovoDect.Enabled = enableEdit;
            _btnEditarDect.Enabled = enableEdit;
            _btnExcluirDect.Enabled = enableEdit;

            _btnNovoTelefoneCisco.Enabled = enableEdit;
            _btnEditarTelefoneCisco.Enabled = enableEdit;
            _btnExcluirTelefoneCisco.Enabled = enableEdit;

            _btnNovoTelevisor.Enabled = enableEdit;
            _btnEditarTelevisor.Enabled = enableEdit;
            _btnExcluirTelevisor.Enabled = enableEdit;

            _btnNovoRelogioPonto.Enabled = enableEdit;
            _btnEditarRelogioPonto.Enabled = enableEdit;
            _btnExcluirRelogioPonto.Enabled = enableEdit;

            _btnNovoMonitor.Enabled = enableEdit;
            _btnEditarMonitor.Enabled = enableEdit;
            _btnExcluirMonitor.Enabled = enableEdit;

            _btnNovoNobreak.Enabled = enableEdit;
            _btnEditarNobreak.Enabled = enableEdit;
            _btnExcluirNobreak.Enabled = enableEdit;

            // Grids e filtros
            _gridComputadores.Enabled = enabled;
            _gridTablets.Enabled = enabled;
            _gridColetores.Enabled = enabled;
            _gridCelulares.Enabled = enabled;
            _gridImpressoras.Enabled = enabled;
            _gridDects.Enabled = enabled;
            _gridTelefonesCisco.Enabled = enabled;
            _gridTelevisores.Enabled = enabled;
            _gridRelogiosPonto.Enabled = enabled;

            _txtComputersFilter.Enabled = enabled;
            _txtTabletsFilter.Enabled = enabled;
            _txtColetoresFilter.Enabled = enabled;
            _txtCelularesFilter.Enabled = enabled;
            _txtImpressorasFilter.Enabled = enabled;
            _txtDectsFilter.Enabled = enabled;
            _txtCiscoFilter.Enabled = enabled;
            _txtTvsFilter.Enabled = enabled;
            _txtRelogiosFilter.Enabled = enabled;
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

            // Limita o tamanho do log a 100KB para evitar crescimento ilimitado
            const int maxChars = 102400; // 100 KB
            if (_txtLog.TextLength > maxChars)
            {
                _txtLog.Text = _txtLog.Text.Substring(_txtLog.TextLength - maxChars);
            }

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
            LoadRelogiosPonto();
            LoadMonitores();
            LoadNobreaks();
        }

        private static void HideIdColumn(DataGridView grid)
        {
            if (grid.Columns.Contains("Id"))
            {
                grid.Columns["Id"].Visible = false;
            }
        }

        private async void LoadComputadores()
        {
            if (_store == null) return;

            var originalCursor = Cursor;
            try
            {
                Cursor = Cursors.WaitCursor;
                // Usar async direto - mais rápido que Task.Run com sync
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllComputersAsync(1000, cts.Token);
                _computersCache = ToList(list);
                ApplyComputersFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar computadores:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = originalCursor;
            }
        }

        private async void LoadTablets()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllTabletsAsync(cts.Token);
                _tabletsCache = ToList(list);
                ApplyTabletsFilter();
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

        private async void LoadColetores()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllColetoresAsync(cts.Token);
                _coletoresCache = ToList(list);
                ApplyColetoresFilter();
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

        private async void LoadCelulares()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllCelularesAsync(cts.Token);
                _celularesCache = ToList(list);
                ApplyCelularesFilter();
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

        // Método de validação de permissão para edit/delete (silencioso para visualizador)
        private bool IsUserAuthorizedForEdit()
        {
            // Visualizador ou modo usuário permanecem read-only: apenas retorna false sem popup
            if (_currentUser?.Role == UserRole.Visualizador || _chkUserMode.Checked)
                return false;

            return true;
        }

        private void NovoComputador()
        {
            if (!IsUserAuthorizedForEdit()) return;
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
            if (!IsUserAuthorizedForEdit()) return;
            if (!IsUserAuthorizedForEdit()) return;
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

        private void ExcluirComputador()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridComputadores.CurrentRow?.DataBoundItem is not LegacyDevices.Computer selected)
            {
                MessageBox.Show(this, "Selecione um computador para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                "Deseja realmente excluir o computador selecionado?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _store.DeleteComputer(selected.Id);
            LoadComputadores();
            InventoryLogger.Info("WinForms", $"Computador excluído via UI (Id={selected.Id}).");
        }

        private void NovoTablet()
        {
            if (!IsUserAuthorizedForEdit()) return;
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
            if (!IsUserAuthorizedForEdit()) return;
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

        private void ExcluirTablet()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridTablets.CurrentRow?.DataBoundItem is not LegacyDevices.Tablet selected)
            {
                MessageBox.Show(this, "Selecione um tablet para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                "Deseja realmente excluir o tablet selecionado?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _store.DeleteTablet(selected.Id);
            LoadTablets();
            InventoryLogger.Info("WinForms", $"Tablet excluído via UI (Id={selected.Id}).");
        }

        private void NovoColetor()
        {
            if (!IsUserAuthorizedForEdit()) return;
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
            if (!IsUserAuthorizedForEdit()) return;
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

        private void ExcluirColetor()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridColetores.CurrentRow?.DataBoundItem is not LegacyDevices.ColetorAndroid selected)
            {
                MessageBox.Show(this, "Selecione um coletor para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                "Deseja realmente excluir o coletor selecionado?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _store.DeleteColetor(selected.Id);
            LoadColetores();
            InventoryLogger.Info("WinForms", $"Coletor excluído via UI (Id={selected.Id}).");
        }

        private void NovoCelular()
        {
            if (!IsUserAuthorizedForEdit()) return;
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
            if (!IsUserAuthorizedForEdit()) return;
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

        private void ExcluirCelular()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridCelulares.CurrentRow?.DataBoundItem is not LegacyDevices.Celular selected)
            {
                MessageBox.Show(this, "Selecione um celular para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                "Deseja realmente excluir o celular selecionado?",
                "Confirmação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            _store.DeleteCelular(selected.Id);
            LoadCelulares();
            InventoryLogger.Info("WinForms", $"Celular excluído via UI (Id={selected.Id}).");
        }

        private void LoadDevices(DeviceType type, DataGridView grid)
        {
            if (_store == null) return;

            try
            {
                // Usar BeginInvoke para evitar bloquear a thread UI com GetAwaiter().GetResult()
                var task = _store.ListAsync();
                BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        var allDevices = await task;
                        // Filter by type name
                        var typeName = type.ToString();
                        var filtered = allDevices
                            .Where(d => d.Type?.ToString() == typeName)
                            .ToList();
                        grid.DataSource = filtered;
                        HideIdColumn(grid);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this,
                            $"Erro ao carregar lista de {type}:\n\n" + ex.Message,
                            "Erro",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }));
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

            // Executar exclusão de forma assíncrona sem bloquear a UI
            ExcluirDeviceAsync(selected, reload);
        }

        private async void ExcluirDeviceAsync(Device selected, Action reload)
        {
            if (_store == null) return;

            try
            {
                await _store.DeleteAsync(selected.Id);
                reload();
                InventoryLogger.Info("WinForms", $"{selected.Type} excluído via UI (Id={selected.Id})");
                MessageBox.Show(this, "Item excluído com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao excluir item:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                reload();
            }
        }

        private static string GetResult(Dictionary<string, string> result, string key)
            => result.TryGetValue(key, out var value) ? value?.Trim() ?? string.Empty : string.Empty;

        private async void LoadImpressoras()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllImpressorasAsync(cts.Token);
                _impressorasCache = list.ToList();
                ApplyImpressorasFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar impressoras:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EditarImpressoraModel(LegacyDevices.Impressora model)
        {
            var fields = new Dictionary<string, string>
            {
                ["Nome"] = model.Nome,
                ["Tipo/Modelo"] = model.TipoModelo,
                ["SerialNumber"] = model.SerialNumber,
                ["Local Atual"] = model.LocalAtual,
                ["Local Anterior"] = model.LocalAnterior
            };

            using var form = new DeviceEditForm("Impressora", fields);
            if (form.ShowDialog(this) != DialogResult.OK)
                return false;

            model.Nome = GetResult(form.Result, "Nome");
            model.TipoModelo = GetResult(form.Result, "Tipo/Modelo");
            model.SerialNumber = GetResult(form.Result, "SerialNumber");
            model.LocalAtual = GetResult(form.Result, "Local Atual");
            model.LocalAnterior = GetResult(form.Result, "Local Anterior");
            return true;
        }

        private void NovoImpressora()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;

            var model = new LegacyDevices.Impressora();
            if (!EditarImpressoraModel(model))
                return;

            _store.AddImpressora(model);
            LoadImpressoras();
            InventoryLogger.Info("WinForms", $"Impressora cadastrada via UI: {model}");
        }

        private void EditarImpressora()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridImpressoras.CurrentRow?.DataBoundItem is not LegacyDevices.Impressora selected)
            {
                MessageBox.Show(this, "Selecione uma impressora para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EditarImpressoraModel(selected))
                return;

            _store.UpdateImpressora(selected);
            LoadImpressoras();
            InventoryLogger.Info("WinForms", $"Impressora atualizada via UI (Id={selected.Id}).");
        }

        private void ExcluirImpressora()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridImpressoras.CurrentRow?.DataBoundItem is not LegacyDevices.Impressora selected)
            {
                MessageBox.Show(this, "Selecione uma impressora para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir a impressora selecionada?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteImpressora(selected.Id);
            LoadImpressoras();
            InventoryLogger.Info("WinForms", $"Impressora excluída via UI (Id={selected.Id}).");
        }

        private async void LoadDects()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllDectsAsync(cts.Token);
                _dectsCache = list.ToList();
                ApplyDectsFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar DECTs:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EditarDectModel(LegacyDevices.DectPhone model)
        {
            var fields = new Dictionary<string, string>
            {
                ["Responsável"] = model.Responsavel,
                ["IPEI"] = model.Ipei,
                ["MAC Address"] = model.MacAddress,
                ["Número"] = model.Numero,
                ["Local"] = model.Local
            };

            using var form = new DeviceEditForm("DECT", fields);
            if (form.ShowDialog(this) != DialogResult.OK)
                return false;

            model.Responsavel = GetResult(form.Result, "Responsável");
            model.Ipei = GetResult(form.Result, "IPEI");
            model.MacAddress = GetResult(form.Result, "MAC Address");
            model.Numero = GetResult(form.Result, "Número");
            model.Local = GetResult(form.Result, "Local");
            return true;
        }

        private void NovoDect()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;

            var model = new LegacyDevices.DectPhone();
            if (!EditarDectModel(model))
                return;

            _store.AddDect(model);
            LoadDects();
            InventoryLogger.Info("WinForms", $"DECT cadastrado via UI: {model}");
        }

        private void EditarDect()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridDects.CurrentRow?.DataBoundItem is not LegacyDevices.DectPhone selected)
            {
                MessageBox.Show(this, "Selecione um DECT para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EditarDectModel(selected))
                return;

            _store.UpdateDect(selected);
            LoadDects();
            InventoryLogger.Info("WinForms", $"DECT atualizado via UI (Id={selected.Id}).");
        }

        private void ExcluirDect()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridDects.CurrentRow?.DataBoundItem is not LegacyDevices.DectPhone selected)
            {
                MessageBox.Show(this, "Selecione um DECT para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o DECT selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteDect(selected.Id);
            LoadDects();
            InventoryLogger.Info("WinForms", $"DECT excluído via UI (Id={selected.Id}).");
        }

        private async void LoadTelefonesCisco()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllTelefonesCiscoAsync(cts.Token);
                _ciscoCache = list.ToList();
                ApplyCiscoFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar telefones Cisco:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EditarTelefoneCiscoModel(LegacyDevices.CiscoPhone model)
        {
            var fields = new Dictionary<string, string>
            {
                ["Responsável"] = model.Responsavel,
                ["MAC Address"] = model.MacAddress,
                ["Número"] = model.Numero,
                ["Local"] = model.Local
            };

            using var form = new DeviceEditForm("Telefone Cisco", fields);
            if (form.ShowDialog(this) != DialogResult.OK)
                return false;

            model.Responsavel = GetResult(form.Result, "Responsável");
            model.MacAddress = GetResult(form.Result, "MAC Address");
            model.Numero = GetResult(form.Result, "Número");
            model.Local = GetResult(form.Result, "Local");
            return true;
        }

        private void NovoTelefoneCisco()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;

            var model = new LegacyDevices.CiscoPhone();
            if (!EditarTelefoneCiscoModel(model))
                return;

            _store.AddTelefoneCisco(model);
            LoadTelefonesCisco();
            InventoryLogger.Info("WinForms", $"Telefone Cisco cadastrado via UI: {model}");
        }

        private void EditarTelefoneCisco()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridTelefonesCisco.CurrentRow?.DataBoundItem is not LegacyDevices.CiscoPhone selected)
            {
                MessageBox.Show(this, "Selecione um telefone Cisco para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EditarTelefoneCiscoModel(selected))
                return;

            _store.UpdateTelefoneCisco(selected);
            LoadTelefonesCisco();
            InventoryLogger.Info("WinForms", $"Telefone Cisco atualizado via UI (Id={selected.Id}).");
        }

        private void ExcluirTelefoneCisco()
        {
            if (!IsUserAuthorizedForEdit()) return;
            if (_store == null) return;
            if (_gridTelefonesCisco.CurrentRow?.DataBoundItem is not LegacyDevices.CiscoPhone selected)
            {
                MessageBox.Show(this, "Selecione um telefone Cisco para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o telefone Cisco selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteTelefoneCisco(selected.Id);
            LoadTelefonesCisco();
            InventoryLogger.Info("WinForms", $"Telefone Cisco excluído via UI (Id={selected.Id}).");
        }

        private async void LoadTelevisores()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllTelevisoresAsync(cts.Token);
                _tvsCache = list.ToList();
                ApplyTvsFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar televisores:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EditarTelevisorModel(LegacyDevices.Televisor model)
        {
            var fields = new Dictionary<string, string>
            {
                ["Modelo"] = model.Modelo,
                ["SerialNumber"] = model.SerialNumber,
                ["Local"] = model.Local
            };

            using var form = new DeviceEditForm("Televisor", fields);
            if (form.ShowDialog(this) != DialogResult.OK)
                return false;

            model.Modelo = GetResult(form.Result, "Modelo");
            model.SerialNumber = GetResult(form.Result, "SerialNumber");
            model.Local = GetResult(form.Result, "Local");
            return true;
        }

        private void NovoTelevisor()
        {
            if (_store == null) return;

            var model = new LegacyDevices.Televisor();
            if (!EditarTelevisorModel(model))
                return;

            _store.AddTelevisor(model);
            LoadTelevisores();
            InventoryLogger.Info("WinForms", $"Televisor cadastrado via UI: {model}");
        }

        private void EditarTelevisor()
        {
            if (_store == null) return;
            if (_gridTelevisores.CurrentRow?.DataBoundItem is not LegacyDevices.Televisor selected)
            {
                MessageBox.Show(this, "Selecione um televisor para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EditarTelevisorModel(selected))
                return;

            _store.UpdateTelevisor(selected);
            LoadTelevisores();
            InventoryLogger.Info("WinForms", $"Televisor atualizado via UI (Id={selected.Id}).");
        }

        private void ExcluirTelevisor()
        {
            if (_store == null) return;
            if (_gridTelevisores.CurrentRow?.DataBoundItem is not LegacyDevices.Televisor selected)
            {
                MessageBox.Show(this, "Selecione um televisor para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o televisor selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteTelevisor(selected.Id);
            LoadTelevisores();
            InventoryLogger.Info("WinForms", $"Televisor excluído via UI (Id={selected.Id}).");
        }

        private async void LoadRelogiosPonto()
        {
            if (_store == null) return;

            try
            {
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var list = await _store.GetAllRelogiosPontoAsync(cts.Token);
                _relogiosCache = list.ToList();
                ApplyRelogiosFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Erro ao carregar relógios de ponto:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void NovoRelogioPonto()
        {
            if (_store == null) return;

            var model = new LegacyDevices.RelogioPonto();
            if (!EditarRelogioPontoModel(model))
                return;

            _store.AddRelogioPonto(model);
            LoadRelogiosPonto();
            InventoryLogger.Info("WinForms", $"Relógio de ponto cadastrado via UI: {model}");
        }

        private void EditarRelogioPonto()
        {
            if (_store == null) return;
            if (_gridRelogiosPonto.CurrentRow?.DataBoundItem is not LegacyDevices.RelogioPonto selected)
            {
                MessageBox.Show(this, "Selecione um relógio ponto para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!EditarRelogioPontoModel(selected))
                return;

            _store.UpdateRelogioPonto(selected);
            LoadRelogiosPonto();
            InventoryLogger.Info("WinForms", $"Relógio de ponto atualizado via UI (Id={selected.Id}).");
        }

        private void ExcluirRelogioPonto()
        {
            if (_store == null) return;
            if (_gridRelogiosPonto.CurrentRow?.DataBoundItem is not LegacyDevices.RelogioPonto selected)
            {
                MessageBox.Show(this, "Selecione um relógio ponto para excluir.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, "Deseja realmente excluir o relógio de ponto selecionado?", "Confirmação",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _store.DeleteRelogioPonto(selected.Id);
            LoadRelogiosPonto();
            InventoryLogger.Info("WinForms", $"Relógio de ponto excluído via UI (Id={selected.Id}).");
        }

        private bool EditarRelogioPontoModel(LegacyDevices.RelogioPonto model)
        {
            using var form = new RelogioPontoEditForm(model);
            return form.ShowDialog(this) == DialogResult.OK;
        }

        private static List<T> ToList<T>(IEnumerable<T> source)
        {
            return source is List<T> list ? list : new List<T>(source);
        }

        private async void SelecionarBanco()
        {
            // Carrega config atual para comparar e confirmar troca
            var currentConfig = SqlServerConfig.Load();

            // Reutiliza o configurador completo (LocalDB, SQL Server ou arquivo .mdf)
            using var configForm = new DatabaseConfigForm();
            if (configForm.ShowDialog(this) != DialogResult.OK)
                return;

            var connStr = configForm.GetConnectionString();
            var mode = configForm.GetMode();

            try
            {
                Cursor = Cursors.WaitCursor;

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    MessageBox.Show(this,
                        "Informe uma connection string válida.",
                        "Validação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Valida formato da connection string antes de prosseguir
                try
                {
                    _ = new SqlConnectionStringBuilder(connStr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Connection string inválida:\n\n" + ex.Message,
                        "Validação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Confirmar troca se connection string ou modo mudaram
                var newUseLocalDb = mode == "localdb" || mode == "filemdf";
                var sameConn = string.Equals(currentConfig.ConnectionString, connStr, StringComparison.OrdinalIgnoreCase);
                var sameMode = currentConfig.UseLocalDb == newUseLocalDb;
                if (!sameConn || !sameMode)
                {
                    var confirm = MessageBox.Show(this,
                        "Você está trocando o banco de dados. Isso irá recarregar as tabelas usando a nova conexão.\n\nDeseja continuar?",
                        "Confirmar troca de banco",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirm != DialogResult.Yes)
                        return;
                }

                // Persistir configuração
                var config = SqlServerConfig.Load();
                config.ConnectionString = connStr;
                config.UseLocalDb = newUseLocalDb;
                config.Save();

                // Recriar factory/stores para usar imediatamente
                _sqlFactory = new SqlServerConnectionFactory(connStr);
                _store = new SqlServerInventoryStore(_sqlFactory);
                _userStore ??= new SqlServerUserStore(_sqlFactory);

                StartAutoBackupTimer();

                var modeText = mode switch
                {
                    "localdb" => "LocalDB",
                    "filemdf" => "Arquivo .mdf",
                    _ => "SQL Server"
                };

                _lblDbPath.Text = $"Banco: {modeText} ({connStr.Substring(0, Math.Min(60, connStr.Length))}...)";
                InventoryLogger.Info("WinForms", $"Banco configurado ({modeText})");

                // Validar conexão e garantir schema sem travar a UI
                try
                {
                    await Task.Run(() => SqlServerSchemaManager.EnsureRequiredTables(_sqlFactory));
                    InventoryLogger.Info("WinForms", "EnsureRequiredTables() finalizado com sucesso.");
                }
                catch (Exception ex)
                {
                    InventoryLogger.Error("WinForms", "Erro ao garantir schema SQL Server.", ex);
                    MessageBox.Show(this,
                        "Ocorreu um erro ao criar/verificar as tabelas:\n\n" +
                        ex.Message,
                        "Erro de schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    EnableDataTabs(false);
                    return;
                }
                EnableDataTabs(true);

                try
                {
                    LoadAllGrids();
                }
                catch (Exception ex)
                {
                    InventoryLogger.Error("WinForms", "Erro ao carregar grids após configuração do banco.", ex);
                    MessageBox.Show(this,
                        "O banco foi configurado e as tabelas foram criadas/verificadas,\n" +
                        "mas houve erro ao carregar os dados na interface:\n\n" + ex.Message,
                        "Erro ao carregar dados",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                MessageBox.Show(this,
                    "✔ Banco configurado e salvo nas configurações.\n" +
                    "Na próxima vez que abrir o programa, ele já conectará\n" +
                    "automaticamente nesta conexão.",
                    "Banco de Dados",
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
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MostrarResumoBanco()
        {
            try
            {
                var config = new SqlServerConfig();
                var connStr = config.ConnectionString;

                if (string.IsNullOrWhiteSpace(connStr))
                {
                    MessageBox.Show(this,
                        "Nenhuma conexão SQL Server configurada.\n" +
                        "Use 'Configurar SQL Server' primeiro.",
                        "Resumo do banco",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var factory = new SqlServerConnectionFactory(connStr);
                var totals = _store?.CountByTypeAsync().GetAwaiter().GetResult() ?? new Dictionary<string, int>();

                var summary = "Resumo do banco de dados:\n\n";
                foreach (var item in totals)
                {
                    summary += $"{item.Key}: {item.Value}\n";
                }

                MessageBox.Show(this,
                    summary,
                    "Resumo do banco",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                InventoryLogger.Error("WinForms", "Erro ao gerar resumo do banco.", ex);
                MessageBox.Show(this,
                    "Erro ao obter resumo do banco:\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AbrirGerenciadorUsuarios()
        {
            if (_userStore == null || _currentUser?.Role != UserRole.Admin)
            {
                MessageBox.Show(this,
                    "Apenas administradores podem acessar o gerenciador de usuários.",
                    "Acesso negado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using var form = new UserManagementForm(_userStore, _currentUser);
            form.ShowDialog(this);
        }

        private void StartAutoBackupTimer()
        {
            _backupTimer?.Dispose();

            if (_sqlFactory == null)
                return;

            // Roda a cada 3 minutos; primeira execução após 3 minutos
            _backupTimer = new System.Threading.Timer(async _ => await RunBackupAsync(), null, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            InventoryLogger.Info("WinForms", "Timer de backup automático iniciado (3 min)");
        }

        private async Task RunBackupAsync()
        {
            if (_backupInProgress)
                return;

            if (_sqlFactory == null)
                return;

            try
            {
                _backupInProgress = true;

                var builder = new SqlConnectionStringBuilder(_sqlFactory.ConnectionString);
                var dbName = string.IsNullOrWhiteSpace(builder.InitialCatalog)
                    ? "Inventory"
                    : builder.InitialCatalog;

                var backupFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{dbName}.bak");
                var sanitizedDb = dbName.Replace("]", "]]", StringComparison.Ordinal);
                var escapedPath = backupFile.Replace("'", "''", StringComparison.Ordinal);

                await using var conn = _sqlFactory.CreateConnection();
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"BACKUP DATABASE [{sanitizedDb}] TO DISK = '{escapedPath}' WITH INIT, COMPRESSION";
                await cmd.ExecuteNonQueryAsync();

                InventoryLogger.Info("WinForms", $"Backup automático gerado: {backupFile}");
            }
            catch (Exception ex)
            {
                InventoryLogger.Error("WinForms", "Falha no backup automático", ex);
            }
            finally
            {
                _backupInProgress = false;
            }
        }

        private void AbrirBackupEnvioSql()
        {
            if (_currentUser?.Role != UserRole.Admin)
            {
                MessageBox.Show(this,
                    "Apenas administradores podem executar backup/migração.",
                    "Acesso negado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (_sqlFactory == null)
            {
                MessageBox.Show(this,
                    "Conexão com o banco não está configurada.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var sourceConn = _sqlFactory.ConnectionString;
            var targetConn = PromptForTargetConnectionString();
            if (string.IsNullOrWhiteSpace(targetConn))
                return;

            using var form = new DatabaseMigrationForm(sourceConn, targetConn);
            form.ShowDialog(this);
        }

        private string? PromptForTargetConnectionString()
        {
            var dialog = new Form
            {
                Text = "Destino SQL Server",
                Size = new Size(620, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var lbl = new Label
            {
                Text = "Informe a connection string de destino (SQL Server):",
                AutoSize = true,
                Location = new Point(12, 15)
            };

            var txt = new TextBox
            {
                Location = new Point(12, 40),
                Width = 580
            };

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(400, 80),
                AutoSize = true
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(500, 80),
                AutoSize = true
            };

            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnCancel;

            dialog.Controls.Add(lbl);
            dialog.Controls.Add(txt);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);

            return dialog.ShowDialog(this) == DialogResult.OK
                ? txt.Text.Trim()
                : null;
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

        private void MostrarDashboardTotal()
        {
            if (_store == null)
            {
                MessageBox.Show(this,
                    "Configure o banco antes de abrir o dashboard.",
                    "Dashboard Total",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dashboardTotal = new TotalDashboardForm(_store);
            dashboardTotal.ShowDialog(this);
        }

        private void RealizarLogoff()
        {
            DialogResult result = MessageBox.Show(
                "Deseja realmente sair da conta?",
                "Confirmação de Logoff",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            try
            {
                AuditLog.LogLogoff(_currentUser?.Username ?? "Unknown");
            }
            catch (Exception ex)
            {
                InventoryLogger.Error("MainForm", $"Erro ao registrar logoff: {ex.Message}");
            }

            // Reiniciar a aplicação para voltar ao login
            Application.Restart();
            Environment.Exit(0);
        }
    }
}
