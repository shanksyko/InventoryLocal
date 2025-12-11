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

        _panelSqlServer = ResponsiveUIHelper.CreateCard(650, 150);
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

        var txtSqlServer = ResponsiveUIHelper.CreateTextBox("localhost\\SQLEXPRESS", 300);
        txtSqlServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium + 25);
        pnlSqlControls.Controls.Add(txtSqlServer);
        pnlSqlControls.Tag = txtSqlServer;

        var btnTestSql = ResponsiveUIHelper.CreateButton("🔗 Testar", 100, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnTestSql.Location = new Point(320, ResponsiveUIHelper.Spacing.Medium + 25);
        btnTestSql.Click += (s, e) => TestSqlConnection(txtSqlServer.Text);
        pnlSqlControls.Controls.Add(btnTestSql);

        _panelSqlServer.Controls.Add(pnlSqlControls);
        mainPanel.Controls.Add(_panelSqlServer);

        y += 170;

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
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnContinue = ResponsiveUIHelper.CreateButton("➡️  Continuar", 150, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnContinue.Click += OnContinue;

        _btnCancel = ResponsiveUIHelper.CreateButton("❌ Cancelar", 100, ResponsiveUIHelper.Colors.PrimaryRed);
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        int x = buttonPanel.Width - 270;
        int btnY = (buttonPanel.Height - _btnContinue.Height) / 2;

        _btnContinue.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnContinue);

        x += _btnContinue.Width + ResponsiveUIHelper.Spacing.Medium;
        _btnCancel.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnCancel);

        _lblStatus = ResponsiveUIHelper.CreateLabel("Pronto");
        _lblStatus.Location = new Point(ResponsiveUIHelper.Spacing.Medium, btnY + 5);
        buttonPanel.Controls.Add(_lblStatus);

        _progressBar = new ProgressBar
        {
            Location = new Point(250, btnY),
            Size = new Size(300, 20),
            Visible = false,
            Style = ProgressBarStyle.Marquee
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

    private void TestSqlConnection(string server)
    {
        try
        {
            AddLog($"🔗 Testando conexão para {server}...");
            var connString = $"Server={server};Integrated Security=true;TrustServerCertificate=true;Connection Timeout=5;";
            
            using var conn = new SqlConnection(connString);
            conn.Open();
            
            _connectionString = connString;
            AddLog($"✅ Conexão estabelecida com sucesso!");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro na conexão: {ex.Message}", Color.Red);
        }
    }

    private void BrowseMdfFile(TextBox txtPath)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "SQL Database Files (*.mdf)|*.mdf|All Files (*.*)|*.*",
            Title = "Selecione o arquivo .mdf"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtPath.Text = dialog.FileName;
            _connectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={dialog.FileName};Integrated Security=true;";
            AddLog($"📁 Arquivo selecionado: {Path.GetFileName(dialog.FileName)}");
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
                if (string.IsNullOrEmpty(_connectionString))
                {
                    AddLog("❌ Teste a conexão do SQL Server primeiro", Color.Red);
                    _btnContinue.Enabled = true;
                    _progressBar.Visible = false;
                    return;
                }
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
        _rtbLog.Invoke(() =>
        {
            _rtbLog.SelectionColor = color ?? ResponsiveUIHelper.Colors.TextDark;
            _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            _rtbLog.ScrollToCaret();
        });
    }

    public string GetConnectionString() => _connectionString;
    public string GetMode() => _selectedMode;
}
