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
/// Permite: LocalDB (padrão), Servidor SQL Server ou Arquivo .mdf
/// </summary>
public class DatabaseConfigForm : Form
{
    private RadioButton _rbLocalDb = null!;
    private RadioButton _rbSqlServer = null!;
    private RadioButton _rbFileMdf = null!;

    private TextBox _txtSqlServer = null!;
    private TextBox _txtSqlDatabase = null!;
    private TextBox _txtSqlUser = null!;
    private TextBox _txtSqlPassword = null!;
    private CheckBox _chkSqlIntegratedSecurity = null!;
    
    private Panel _panelLocalDb = null!;
    private Panel _panelSqlServer = null!;
    private Panel _panelFileMdf = null!;
    
    private Label _lblStatus = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _rtbLog = null!;
    private Button _btnContinue = null!;
    private Button _btnCancel = null!;

    private string _selectedMode = "localdb";
    private string _connectionString = string.Empty;

    public DatabaseConfigForm()
    {
        Text = "Configuração do Banco de Dados";
        Size = new Size(800, 700);
        MinimumSize = new Size(600, 500);
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
            "Escolha como deseja armazenar os dados: LocalDB, Servidor SQL ou Arquivo de Rede"
        );
        headerPanel.BackColor = ResponsiveUIHelper.Colors.PrimaryOrange;
        Controls.Add(headerPanel);

        // Painel principal
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ResponsiveUIHelper.Colors.LightBackground,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Large)
        };

        int y = ResponsiveUIHelper.Spacing.Medium;

        // ===== MODO 1: LocalDB =====
        _rbLocalDb = new RadioButton
        {
            Text = "✅ LocalDB (Recomendado - Sem Instalação)",
            AutoSize = true,
            Checked = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        _rbLocalDb.CheckedChanged += (s, e) => { if (_rbLocalDb.Checked) ShowLocalDbPanel(); };
        mainPanel.Controls.Add(_rbLocalDb);

        y += 30;

        _panelLocalDb = ResponsiveUIHelper.CreateCard(650, 120);
        _panelLocalDb.Location = new Point(ResponsiveUIHelper.Spacing.Medium + 20, y);
        
        var lblLocalDb = new Label
        {
            Text = "📦 LocalDB é uma versão leve do SQL Server que vem com .NET\n\n" +
                   "✓ Sem instalação necessária\n" +
                   "✓ Zero configuração\n" +
                   "✓ Banco local no computador\n" +
                   "✓ Perfeito para começar agora",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Medium),
            Font = ResponsiveUIHelper.Fonts.Small
        };
        _panelLocalDb.Controls.Add(lblLocalDb);
        mainPanel.Controls.Add(_panelLocalDb);

        y += 140;

        // ===== MODO 2: SQL Server =====
        _rbSqlServer = new RadioButton
        {
            Text = "🖥️  SQL Server (Servidor/Rede)",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        _rbSqlServer.CheckedChanged += (s, e) => { if (_rbSqlServer.Checked) ShowSqlServerPanel(); };
        mainPanel.Controls.Add(_rbSqlServer);

        y += 30;

        _panelSqlServer = ResponsiveUIHelper.CreateCard(650, 220);
        _panelSqlServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium + 20, y);
        _panelSqlServer.Visible = false;

        var pnlSqlControls = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        
        var lblSqlServer = new Label
        {
            Text = "Servidor SQL Server:",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        pnlSqlControls.Controls.Add(lblSqlServer);

        _txtSqlServer = ResponsiveUIHelper.CreateTextBox("GIANPC\\SQLEXPRESS", 320);
        _txtSqlServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium + 22);
        _txtSqlServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlSqlControls.Controls.Add(_txtSqlServer);

        var lblDatabase = new Label
        {
            Text = "Banco de dados (Initial Catalog):",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, _txtSqlServer.Bottom + ResponsiveUIHelper.Spacing.Medium),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        pnlSqlControls.Controls.Add(lblDatabase);

        _txtSqlDatabase = ResponsiveUIHelper.CreateTextBox("InventoryLocal", 220);
        _txtSqlDatabase.Location = new Point(ResponsiveUIHelper.Spacing.Medium, lblDatabase.Bottom + 4);
        pnlSqlControls.Controls.Add(_txtSqlDatabase);

        _chkSqlIntegratedSecurity = new CheckBox
        {
            Text = "Usar Segurança Integrada (Windows)",
            AutoSize = true,
            Checked = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, _txtSqlDatabase.Bottom + ResponsiveUIHelper.Spacing.Small)
        };
        _chkSqlIntegratedSecurity.CheckedChanged += (_, _) => ToggleSqlAuthFields();
        pnlSqlControls.Controls.Add(_chkSqlIntegratedSecurity);

        var lblSqlUser = new Label
        {
            Text = "Usuário (SQL Authentication):",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, _chkSqlIntegratedSecurity.Bottom + ResponsiveUIHelper.Spacing.Small),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        pnlSqlControls.Controls.Add(lblSqlUser);

        _txtSqlUser = ResponsiveUIHelper.CreateTextBox(string.Empty, 200);
        _txtSqlUser.Location = new Point(ResponsiveUIHelper.Spacing.Medium, lblSqlUser.Bottom + 4);
        pnlSqlControls.Controls.Add(_txtSqlUser);

        var lblSqlPassword = new Label
        {
            Text = "Senha:",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, _txtSqlUser.Bottom + ResponsiveUIHelper.Spacing.Small),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        pnlSqlControls.Controls.Add(lblSqlPassword);

        _txtSqlPassword = ResponsiveUIHelper.CreateTextBox(string.Empty, 200);
        _txtSqlPassword.Location = new Point(ResponsiveUIHelper.Spacing.Medium, lblSqlPassword.Bottom + 4);
        _txtSqlPassword.PasswordChar = '*';
        pnlSqlControls.Controls.Add(_txtSqlPassword);

        var btnTestSql = ResponsiveUIHelper.CreateButton("🔗 Testar", 120, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnTestSql.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnTestSql.Location = new Point(_txtSqlServer.Right + ResponsiveUIHelper.Spacing.Small, _txtSqlServer.Top);
        btnTestSql.Click += (s, e) => TestSqlConnection();
        pnlSqlControls.Controls.Add(btnTestSql);

        ToggleSqlAuthFields();

        _panelSqlServer.Controls.Add(pnlSqlControls);
        mainPanel.Controls.Add(_panelSqlServer);

        y += 240;

        // ===== MODO 3: Arquivo .mdf =====
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

        _panelFileMdf = ResponsiveUIHelper.CreateCard(650, 100);
        _panelFileMdf.Location = new Point(ResponsiveUIHelper.Spacing.Medium + 20, y);
        _panelFileMdf.Visible = false;

        var pnlFileControls = new Panel { Dock = DockStyle.Fill };

        var lblFilePath = new Label
        {
            Text = "Caminho do arquivo .mdf:",
            AutoSize = true,
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium),
            Font = ResponsiveUIHelper.Fonts.LabelBold
        };
        pnlFileControls.Controls.Add(lblFilePath);

        var txtFilePath = ResponsiveUIHelper.CreateTextBox("", 400);
        txtFilePath.Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium + 25);
        txtFilePath.ReadOnly = true;
        pnlFileControls.Controls.Add(txtFilePath);
        pnlFileControls.Tag = txtFilePath;

        var btnBrowse = ResponsiveUIHelper.CreateButton("📂 Procurar", 100, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnBrowse.Location = new Point(520, ResponsiveUIHelper.Spacing.Medium + 25);
        btnBrowse.Click += (s, e) => BrowseMdfFile(txtFilePath);
        pnlFileControls.Controls.Add(btnBrowse);

        _panelFileMdf.Controls.Add(pnlFileControls);
        mainPanel.Controls.Add(_panelFileMdf);

        y += 120;

        // ===== LOG =====
        var lblLog = ResponsiveUIHelper.CreateLabel("Log:", ResponsiveUIHelper.Fonts.LabelBold);
        lblLog.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblLog);

        y += 25;

        _rtbLog = new RichTextBox
        {
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Size = new Size(650, 100),
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

    private void ShowLocalDbPanel()
    {
        _panelLocalDb.Visible = true;
        _panelSqlServer.Visible = false;
        _panelFileMdf.Visible = false;
        _selectedMode = "localdb";
        AddLog("📦 Modo LocalDB selecionado");
    }

    private void ShowSqlServerPanel()
    {
        _panelLocalDb.Visible = false;
        _panelSqlServer.Visible = true;
        _panelFileMdf.Visible = false;
        _selectedMode = "sqlserver";
        AddLog("🖥️  Modo SQL Server selecionado");
    }

    private void ShowFileMdfPanel()
    {
        _panelLocalDb.Visible = false;
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
            // Criar novo arquivo
            using var saveDialog = new SaveFileDialog
            {
                Filter = "SQL Database Files (*.mdf)|*.mdf",
                Title = "Criar novo arquivo de banco de dados",
                FileName = "InventoryDB.mdf",
                DefaultExt = "mdf"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var mdfPath = saveDialog.FileName;
                txtPath.Text = mdfPath;
                AddLog($"📁 Novo arquivo será criado em: {Path.GetFileName(mdfPath)}");
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
                txtPath.Text = openDialog.FileName;
                _connectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={openDialog.FileName};Integrated Security=true;TrustServerCertificate=true;";
                AddLog($"📁 Arquivo existente selecionado: {Path.GetFileName(openDialog.FileName)}");
            }
        }
    }

    private void OnContinue(object? sender, EventArgs e)
    {
        try
        {
            _progressBar.Visible = true;
            _btnContinue.Enabled = false;

            if (_selectedMode == "localdb")
            {
                _connectionString = LocalDbManager.GetConnectionString();
                AddLog("✅ Usando LocalDB - Configuração automática");
            }
            else if (_selectedMode == "sqlserver")
            {
                if (!TryBuildSqlServerConnectionString(out var connString))
                {
                    _btnContinue.Enabled = true;
                    _progressBar.Visible = false;
                    return;
                }

                if (!TryOpenConnection(connString, out var error))
                {
                    AddLog($"❌ Erro ao validar SQL Server: {error}", Color.Red);
                    _btnContinue.Enabled = true;
                    _progressBar.Visible = false;
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
                    _btnContinue.Enabled = true;
                    _progressBar.Visible = false;
                    return;
                }

                // Se for criar novo arquivo
                if (_connectionString.StartsWith("CREATE:"))
                {
                    var mdfPath = _connectionString.Substring(7); // Remove "CREATE:"
                    AddLog($"📦 Criando novo banco de dados em {Path.GetFileName(mdfPath)}...");
                    
                    try
                    {
                        _connectionString = LocalDbManager.CreateMdfDatabase(mdfPath, (msg) => AddLog(msg));
                        AddLog("✅ Banco de dados criado com sucesso!");
                        AddLog("👤 Usuário admin criado: admin / L9l337643k#$");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"❌ Erro ao criar banco: {ex.Message}", Color.Red);
                        _btnContinue.Enabled = true;
                        _progressBar.Visible = false;
                        return;
                    }
                }
            }

            AddLog("✅ Configuração validada com sucesso!");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro: {ex.Message}", Color.Red);
            _btnContinue.Enabled = true;
        }
        finally
        {
            _progressBar.Visible = false;
        }
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
