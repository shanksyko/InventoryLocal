using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.WinForms.Helpers;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário melhorado para selecionar modo de banco de dados
/// Permite: Servidor SQL Server ou Arquivo .mdf
/// </summary>
public class DatabaseConfigForm : Form
{
    private RadioButton _rbSqlServer = null!;
    private RadioButton _rbFileMdf = null!;

    private TextBox _txtSqlServer = null!;
    private TextBox _txtSqlDatabase = null!;
    private TextBox _txtSqlUser = null!;
    private TextBox _txtSqlPassword = null!;
    private CheckBox _chkSqlIntegratedSecurity = null!;
    
    private Panel _panelSqlServer = null!;
    private Panel _panelFileMdf = null!;
    
    private Label _lblStatus = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _rtbLog = null!;
    private Button _btnContinue = null!;
    private Button _btnCancel = null!;

    private string _selectedMode = "sqlserver";
    private string _connectionString = string.Empty;

    public DatabaseConfigForm()
    {
        Text = "Configuração do Banco de Dados";
        Size = new Size(1280, 720);
        MinimumSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ResponsiveUIHelper.Colors.LightBackground;
        Font = ResponsiveUIHelper.Fonts.Regular;
        FormBorderStyle = FormBorderStyle.Sizable;
        ControlBox = false;
        MaximizeBox = false;

        InitializeUI();
    }

    private void InitializeUI()
    {
        // Header
        var headerPanel = ResponsiveUIHelper.CreateHeaderPanel(
            "⚙️ Selecione o Modo de Banco de Dados",
            "Escolha como deseja armazenar os dados: Servidor SQL ou Arquivo .mdf"
        );
        headerPanel.BackColor = ResponsiveUIHelper.Colors.PrimaryOrange;
        Controls.Add(headerPanel);

        // Painel principal
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ResponsiveUIHelper.Colors.LightBackground,
            Padding = new Padding(40, 80, 40, 20)
        };

        int y = 100;

        // ===== MODO 1: SQL Server =====
        _rbSqlServer = new RadioButton
        {
            Text = "🖥️  SQL Server (Servidor/Rede)",
            AutoSize = true,
            Checked = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        _rbSqlServer.CheckedChanged += (s, e) => { if (_rbSqlServer.Checked) ShowSqlServerPanel(); };
        mainPanel.Controls.Add(_rbSqlServer);

        y += 30;

        _panelSqlServer = ResponsiveUIHelper.CreateCard(600, 165);
        _panelSqlServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium + 20, y);
        _panelSqlServer.Visible = true;

        var pnlSqlControls = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 8) };
        
        // Linha 1: Servidor e Botão Testar
        var lblSqlServer = new Label
        {
            Text = "Servidor:",
            AutoSize = true,
            Location = new Point(8, 8),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        pnlSqlControls.Controls.Add(lblSqlServer);

        _txtSqlServer = ResponsiveUIHelper.CreateTextBox("localhost\\SQLEXPRESS", 360);
        _txtSqlServer.Location = new Point(8, 28);
        pnlSqlControls.Controls.Add(_txtSqlServer);

        var btnTestSql = ResponsiveUIHelper.CreateButton("🔗 Testar", 100, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnTestSql.Location = new Point(375, 28);
        btnTestSql.Click += (s, e) => TestSqlConnection();
        pnlSqlControls.Controls.Add(btnTestSql);

        // Linha 2: Database e Checkbox lado a lado
        var lblDatabase = new Label
        {
            Text = "Database:",
            AutoSize = true,
            Location = new Point(8, 60),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        pnlSqlControls.Controls.Add(lblDatabase);

        _txtSqlDatabase = ResponsiveUIHelper.CreateTextBox("InventoryLocal", 180);
        _txtSqlDatabase.Location = new Point(8, 80);
        pnlSqlControls.Controls.Add(_txtSqlDatabase);

        _chkSqlIntegratedSecurity = new CheckBox
        {
            Text = "☑ Segurança Windows",
            AutoSize = true,
            Checked = true,
            Location = new Point(200, 82),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        _chkSqlIntegratedSecurity.CheckedChanged += (_, _) => ToggleSqlAuthFields();
        pnlSqlControls.Controls.Add(_chkSqlIntegratedSecurity);

        // Linha 3: Usuário e Senha lado a lado
        var lblSqlUser = new Label
        {
            Text = "Usuário SQL:",
            AutoSize = true,
            Location = new Point(8, 115),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        pnlSqlControls.Controls.Add(lblSqlUser);

        _txtSqlUser = ResponsiveUIHelper.CreateTextBox(string.Empty, 180);
        _txtSqlUser.Location = new Point(8, 135);
        pnlSqlControls.Controls.Add(_txtSqlUser);

        var lblSqlPassword = new Label
        {
            Text = "Senha:",
            AutoSize = true,
            Location = new Point(200, 115),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        pnlSqlControls.Controls.Add(lblSqlPassword);

        _txtSqlPassword = ResponsiveUIHelper.CreateTextBox(string.Empty, 180);
        _txtSqlPassword.Location = new Point(200, 135);
        _txtSqlPassword.PasswordChar = '*';
        pnlSqlControls.Controls.Add(_txtSqlPassword);

        ToggleSqlAuthFields();

        _panelSqlServer.Controls.Add(pnlSqlControls);
        mainPanel.Controls.Add(_panelSqlServer);

        y += 180;

        // ===== MODO 2: Arquivo .mdf =====
        _rbFileMdf = new RadioButton
        {
            Text = "📁 Arquivo .mdf (Rede/Local)",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        _rbFileMdf.CheckedChanged += (s, e) => { if (_rbFileMdf.Checked) ShowFileMdfPanel(); };
        mainPanel.Controls.Add(_rbFileMdf);

        y += 30;

        _panelFileMdf = ResponsiveUIHelper.CreateCard(600, 80);
        _panelFileMdf.Location = new Point(ResponsiveUIHelper.Spacing.Medium + 20, y);
        _panelFileMdf.Visible = false;

        var pnlFileControls = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 8) };

        var lblFilePath = new Label
        {
            Text = "Arquivo .mdf:",
            AutoSize = true,
            Location = new Point(8, 8),
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        pnlFileControls.Controls.Add(lblFilePath);

        var txtFilePath = ResponsiveUIHelper.CreateTextBox("", 460);
        txtFilePath.Location = new Point(8, 28);
        txtFilePath.ReadOnly = true;
        txtFilePath.BackColor = Color.White;
        txtFilePath.ForeColor = ResponsiveUIHelper.Colors.TextDark;
        txtFilePath.PlaceholderText = "Clique em Procurar para selecionar ou criar...";
        pnlFileControls.Controls.Add(txtFilePath);
        pnlFileControls.Tag = txtFilePath;

        var btnBrowse = ResponsiveUIHelper.CreateButton("📂 Procurar", 110, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnBrowse.Location = new Point(475, 28);
        btnBrowse.Click += (s, e) => BrowseMdfFile(txtFilePath);
        pnlFileControls.Controls.Add(btnBrowse);
        
        var lblInfo = new Label
        {
            Text = "💡 Criar novo ou selecionar existente",
            AutoSize = true,
            Location = new Point(8, 54),
            Font = new Font(ResponsiveUIHelper.Fonts.Regular.FontFamily, 8F, FontStyle.Italic),
            ForeColor = ResponsiveUIHelper.Colors.TextLight
        };
        pnlFileControls.Controls.Add(lblInfo);

        _panelFileMdf.Controls.Add(pnlFileControls);
        mainPanel.Controls.Add(_panelFileMdf);

        y += 95;

        // ===== LOG =====
        var lblLog = ResponsiveUIHelper.CreateLabel("Log:", ResponsiveUIHelper.Fonts.LabelBold);
        lblLog.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblLog);

        y += 25;

        _rtbLog = new RichTextBox
        {
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Size = new Size(600, 80),
            ReadOnly = true,
            BackColor = Color.White,
            ForeColor = ResponsiveUIHelper.Colors.TextDark
        };
        mainPanel.Controls.Add(_rtbLog);

        Controls.Add(mainPanel);

        // Painel de botões
        var buttonPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Medium)
        };

        _btnContinue = ResponsiveUIHelper.CreateButton("➡️  Continuar", 150, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnContinue.Click += OnContinue;
        _btnContinue.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

        _btnCancel = ResponsiveUIHelper.CreateButton("❌ Cancelar", 100, ResponsiveUIHelper.Colors.PrimaryRed);
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        _btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;

        int btnY = (buttonPanel.Height - _btnContinue.Height) / 2;
        int x = ResponsiveUIHelper.Spacing.Medium;

        _btnContinue.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnContinue);

        x += _btnContinue.Width + ResponsiveUIHelper.Spacing.Medium;
        _btnCancel.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnCancel);

        _lblStatus = ResponsiveUIHelper.CreateLabel("Pronto");
        _lblStatus.Location = new Point(_btnCancel.Right + ResponsiveUIHelper.Spacing.Large, btnY + 5);
        _lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        buttonPanel.Controls.Add(_lblStatus);

        _progressBar = new ProgressBar
        {
            Location = new Point(_lblStatus.Right + ResponsiveUIHelper.Spacing.Medium, btnY + 2),
            Size = new Size(220, 18),
            Visible = false,
            Style = ProgressBarStyle.Marquee,
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        };
        buttonPanel.Controls.Add(_progressBar);

        Controls.Add(buttonPanel);

        AddLog("✅ Bem-vindo ao Inventory System!");
        AddLog("Escolha como deseja armazenar os dados.");
    }

    private void ShowSqlServerPanel()
    {
        _panelSqlServer.Visible = true;
        _panelFileMdf.Visible = false;
        _selectedMode = "sqlserver";
        AddLog("🖥️  Modo SQL Server selecionado");
    }

    private void ShowFileMdfPanel()
    {
        _panelSqlServer.Visible = false;
        _panelFileMdf.Visible = true;
        _selectedMode = "filemdf";
        AddLog("📁 Modo Arquivo .mdf selecionado");
    }

    private void ToggleSqlAuthFields()
    {
        var useIntegrated = _chkSqlIntegratedSecurity.Checked;
        _txtSqlUser.Enabled = !useIntegrated;
        _txtSqlPassword.Enabled = !useIntegrated;
    }

    private bool TryBuildSqlServerConnectionString(out string connString)
    {
        connString = string.Empty;

        var server = _txtSqlServer.Text.Trim();
        var database = _txtSqlDatabase.Text.Trim();

        if (string.IsNullOrWhiteSpace(server))
        {
            AddLog("❌ Informe o servidor do SQL Server.", Color.Red);
            return false;
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            AddLog("❌ Informe o nome do banco (Initial Catalog).", Color.Red);
            return false;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                Encrypt = false,
                TrustServerCertificate = false,
                PersistSecurityInfo = false,
                Pooling = false,
                MultipleActiveResultSets = false,
                ConnectTimeout = 5
            };

            if (_chkSqlIntegratedSecurity.Checked)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                var user = _txtSqlUser.Text.Trim();
                var password = _txtSqlPassword.Text;

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
                {
                    AddLog("❌ Informe usuário e senha ou marque Segurança Integrada.", Color.Red);
                    return false;
                }

                builder.IntegratedSecurity = false;
                builder.UserID = user;
                builder.Password = password;
            }

            connString = builder.ToString();
            return true;
        }
        catch (Exception ex)
        {
            AddLog($"❌ Connection string inválida: {ex.Message}", Color.Red);
            return false;
        }
    }

    private bool TryOpenConnection(string connString, out string? error)
    {
        try
        {
            using var conn = new SqlConnection(connString);
            conn.Open();
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private void TestSqlConnection()
    {
        if (!TryBuildSqlServerConnectionString(out var connString))
            return;

        AddLog($"🔗 Testando conexão para {_txtSqlServer.Text.Trim()}...");
        if (TryOpenConnection(connString, out var error))
        {
            _connectionString = connString;
            AddLog("✅ Conexão estabelecida com sucesso!");
        }
        else
        {
            AddLog($"❌ Erro na conexão: {error}", Color.Red);
        }
    }

    private void BrowseMdfFile(TextBox txtPath)
    {
        var choice = MessageBox.Show(
            "Deseja criar um NOVO arquivo .mdf ou selecionar um EXISTENTE?\n\n" +
            "Sim = Criar novo\n" +
            "Não = Selecionar existente",
            "Arquivo .mdf",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (choice == DialogResult.Cancel)
            return;

        if (choice == DialogResult.Yes)
        {
            // Criar novo arquivo - pré-preencher com caminho padrão em AppData
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InventoryLocal",
                "InventoryDB.mdf"
            );
            var defaultDir = Path.GetDirectoryName(defaultPath);

            using var saveDialog = new SaveFileDialog
            {
                Filter = "SQL Database Files (*.mdf)|*.mdf",
                Title = "Criar novo arquivo de banco de dados",
                FileName = "InventoryDB.mdf",
                DefaultExt = "mdf",
                InitialDirectory = defaultDir ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var mdfPath = saveDialog.FileName;
                AddLog($"📍 Caminho selecionado: {mdfPath}");
                
                // Validar permissões de escrita
                var directory = Path.GetDirectoryName(mdfPath);
                if (string.IsNullOrEmpty(directory))
                {
                    AddLog($"❌ Caminho inválido: {mdfPath}", Color.Red);
                    return;
                }

                if (!Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                        AddLog($"✅ Diretório criado: {directory}");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"❌ Erro ao criar diretório: {ex.Message}", Color.Red);
                        AddLog($"⚠️  Tentando com permissões elevadas (Admin)...", Color.DarkOrange);
                        try
                        {
                            var parentDir = Path.GetDirectoryName(directory);
                            if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
                            {
                                Directory.CreateDirectory(directory);
                                AddLog($"✅ Diretório criado com elevação: {directory}");
                            }
                            else
                            {
                                AddLog($"❌ Caminho pai inválido: {parentDir}", Color.Red);
                                AddLog($"⚠️  Escolha outra pasta com permissões válidas", Color.Red);
                                return;
                            }
                        }
                        catch (Exception retryEx)
                        {
                            AddLog($"❌ Falha ao criar diretório mesmo com elevação: {retryEx.Message}", Color.Red);
                            AddLog($"⚠️  Escolha outra pasta ou execute como Administrador", Color.Red);
                            return;
                        }
                    }
                }
                
                // Testar permissão de escrita
                AddLog($"🔍 Testando permissões de escrita em: {directory}");
                try
                {
                    var testFile = Path.Combine(directory, ".write_test");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    AddLog($"✅ Pasta tem permissão de escrita - pode prosseguir");
                }
                catch (Exception ex)
                {
                    AddLog($"❌ Sem permissão de escrita: {ex.Message}", Color.Red);
                    AddLog($"⚠️  Diretório: {directory}", Color.Red);
                    AddLog($"💡 Soluções:", Color.DarkOrange);
                    AddLog($"   1. Execute o programa como Administrador", Color.DarkOrange);
                    AddLog($"   2. Escolha uma pasta com permissão (Documentos, Desktop)", Color.DarkOrange);
                    AddLog($"   3. Verifique se a pasta está bloqueada por antivírus", Color.DarkOrange);
                    AddLog($"⚠️  Escolha outra pasta ou execute como Administrador", Color.Red);
                    return;
                }
                
                txtPath.Text = mdfPath;
                AddLog($"📁 Novo arquivo será criado em: {mdfPath}");
                AddLog($"✅ Caminho validado com sucesso");
                _connectionString = $"CREATE:{mdfPath}"; // Marcador especial
            }
        }
        else
        {
            // Selecionar existente
            using var openDialog = new OpenFileDialog
            {
                Filter = "SQL Database Files (*.mdf)|*.mdf|All Files (*.*)|*.*",
                Title = "Selecione o arquivo .mdf"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                var mdfPath = openDialog.FileName;
                
                // Validar se arquivo existe e é acessível
                try
                {
                    if (!File.Exists(mdfPath))
                    {
                        AddLog($"❌ Arquivo não encontrado: {mdfPath}", Color.Red);
                        return;
                    }
                    
                    // Testar leitura
                    using var fs = File.OpenRead(mdfPath);
                    AddLog($"✅ Arquivo é acessível");
                }
                catch (Exception ex)
                {
                    AddLog($"❌ Erro ao acessar arquivo: {ex.Message}", Color.Red);
                    AddLog($"⚠️  Verifique se o arquivo está em uso ou sem permissão", Color.Red);
                    return;
                }
                
                txtPath.Text = mdfPath;
                _connectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={mdfPath};Integrated Security=true;TrustServerCertificate=true;";
                AddLog($"📁 Arquivo existente selecionado: {Path.GetFileName(mdfPath)}");
                AddLog($"✅ Caminho validado com sucesso");
            }
        }
    }

    private void OnContinue(object? sender, EventArgs e)
    {
        _progressBar.Visible = true;
        _btnContinue.Enabled = false;

        // Executar em thread background para não bloquear a UI
        // Com timeout de 5 minutos
        var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(5));
        
        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                if (_selectedMode == "sqlserver")
                {
                    if (!TryBuildSqlServerConnectionString(out var connString))
                    {
                        this.Invoke(() =>
                        {
                            _btnContinue.Enabled = true;
                            _progressBar.Visible = false;
                        });
                        return;
                    }

                    if (!TryOpenConnection(connString, out var error))
                    {
                        AddLog($"❌ Erro ao validar SQL Server: {error}", Color.Red);
                        this.Invoke(() =>
                        {
                            _btnContinue.Enabled = true;
                            _progressBar.Visible = false;
                        });
                        return;
                    }

                    _connectionString = connString;
                    AddLog("✅ Conexão SQL Server validada com sucesso!");
                }
                else if (_selectedMode == "filemdf")
                {
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        AddLog("❌ Selecione um arquivo .mdf primeiro", Color.Red);
                        this.Invoke(() =>
                        {
                            _btnContinue.Enabled = true;
                            _progressBar.Visible = false;
                        });
                        return;
                    }

                    // Se for criar novo arquivo
                    if (_connectionString.StartsWith("CREATE:"))
                    {
                        var mdfPath = _connectionString.Substring(7); // Remove "CREATE:"
                        AddLog($"📦 Criando novo banco de dados em {Path.GetFileName(mdfPath)}...");
                        AddLog($"⏱️  Isso pode levar alguns segundos...");
                        
                        try
                        {
                            _connectionString = LocalDbManager.CreateMdfDatabase(mdfPath, (msg) => AddLog(msg));
                            AddLog($"✅ Banco de dados criado com sucesso! (DB: {Path.GetFileNameWithoutExtension(mdfPath)})");
                            try
                            {
                                var mdfOk = File.Exists(mdfPath);
                                var ldfOk = File.Exists(Path.ChangeExtension(mdfPath, ".ldf"));
                                if (mdfOk && ldfOk)
                                {
                                    AddLog($"📂 Arquivos físicos confirmados:\n   MDF: {mdfPath}\n   LDF: {Path.ChangeExtension(mdfPath, ".ldf")}");
                                }
                                else
                                    AddLog("⚠️  Aviso: não foi possível confirmar ambos os arquivos físicos", System.Drawing.Color.DarkOrange);
                            }
                            catch { /* não bloquear UI por erro de verificação */ }
                            AddLog($"🔗 ConnectionString final: {_connectionString}");
                            AddLog("👤 Usuário admin criado: admin / L9l337643k#$");
                        }
                        catch (OperationCanceledException)
                        {
                            AddLog($"❌ Timeout: Operação demorou muito (>5 min)", Color.Red);
                            AddLog($"⚠️  Verifique se o caminho é válido e acessível", Color.Red);
                            this.Invoke(() =>
                            {
                                _btnContinue.Enabled = true;
                                _progressBar.Visible = false;
                            });
                            return;
                        }
                        catch (Exception ex)
                        {
                            AddLog($"❌ Erro ao criar banco: {ex.Message}", Color.Red);
                            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
                            {
                                AddLog($"   Detalhes: {ex.InnerException.Message}", Color.Red);
                            }
                            this.Invoke(() =>
                            {
                                _btnContinue.Enabled = true;
                                _progressBar.Visible = false;
                            });
                            return;
                        }
                    }
                }

                AddLog("✅ Configuração validada com sucesso!");
                this.Invoke(() =>
                {
                    DialogResult = DialogResult.OK;
                    _progressBar.Visible = false;
                });
            }
            catch (Exception ex)
            {
                AddLog($"❌ Erro: {ex.Message}", Color.Red);
                this.Invoke(() =>
                {
                    _btnContinue.Enabled = true;
                    _progressBar.Visible = false;
                });
            }
        }, cts.Token);
    }

    private void AddLog(string message, Color? color = null)
    {
        if (!IsHandleCreated)
        {
            return; // Ignore se o formulário ainda não foi criado
        }

        this.Invoke(() =>
        {
            _rtbLog.SelectionColor = color ?? ResponsiveUIHelper.Colors.TextDark;
            _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            _rtbLog.ScrollToCaret();
        });
    }

    public string GetConnectionString() => _connectionString;
    public string GetMode() => _selectedMode;
    
    /// <summary>
    /// Informa se há dados no banco atual (para oferecer migração)
    /// </summary>
    public static bool HasExistingData(string connectionString)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'";
            
            var tableCount = (int?)cmd.ExecuteScalar() ?? 0;
            return tableCount > 0;
        }
        catch
        {
            return false;
        }
    }
}
