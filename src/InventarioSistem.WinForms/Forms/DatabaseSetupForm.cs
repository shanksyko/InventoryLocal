using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.WinForms.Helpers;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário para selecionar e configurar SQL Server e banco de dados
/// Permite escolher servidor, banco, e executar scripts SQL
/// </summary>
public class DatabaseSetupForm : Form
{
    private ComboBox _cmbServers = null!;
    private TextBox _txtServer = null!;
    private TextBox _txtUser = null!;
    private TextBox _txtPassword = null!;
    private ComboBox _cmbDatabases = null!;
    private TextBox _txtDatabaseName = null!;
    private Button _btnTestConnection = null!;
    private Button _btnRefreshDatabases = null!;
    private Button _btnSelectSqlScript = null!;
    private TextBox _txtSqlScript = null!;
    private Button _btnExecuteScript = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    private Label _lblStatus = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _rtbLog = null!;

    private string? _selectedSqlScript;
    private string _connectionString = string.Empty;

    public DatabaseSetupForm()
    {
        Text = "Configuração do Banco de Dados SQL Server - OBRIGATÓRIO";
        Size = new Size(800, 700);
        MinimumSize = new Size(600, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ResponsiveUIHelper.Colors.LightBackground;
        Font = ResponsiveUIHelper.Fonts.Regular;
        FormBorderStyle = FormBorderStyle.Sizable;
        ControlBox = false; // Desabilita fechar via X
        MaximizeBox = false;

        InitializeUI();
        LoadLocalServers();
    }

    private void InitializeUI()
    {
        // Header com aviso
        var headerPanel = ResponsiveUIHelper.CreateHeaderPanel(
            "⚙️ Configuração Inicial - OBRIGATÓRIA",
            "Configure o SQL Server para continuar. Este processo é necessário apenas na primeira execução."
        );
        headerPanel.BackColor = ResponsiveUIHelper.Colors.PrimaryOrange;
        Controls.Add(headerPanel);

        // Painel principal com scroll
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ResponsiveUIHelper.Colors.LightBackground,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Large)
        };

        // Painel de instruções
        var instructionsPanel = ResponsiveUIHelper.CreateCard(650, 100);
        instructionsPanel.Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium);
        
        var lblInstructions = new Label
        {
            Text = "📋 Instruções:\n" +
                   "1. Selecione ou digite o servidor SQL Server (ex: localhost\\SQLEXPRESS)\n" +
                   "2. Clique em 'Testar Conexão' para validar\n" +
                   "3. Selecione um banco de dados ou crie um novo\n" +
                   "4. Selecione o arquivo create-schema.sql\n" +
                   "5. Clique em 'Executar Script' para criar as tabelas\n" +
                   "6. Clique em 'Salvar Configuração' para finalizar",
            Font = ResponsiveUIHelper.Fonts.Small,
            ForeColor = ResponsiveUIHelper.Colors.TextDark,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Medium)
        };
        instructionsPanel.Controls.Add(lblInstructions);
        mainPanel.Controls.Add(instructionsPanel);

        int y = ResponsiveUIHelper.Spacing.Medium + 120;

        // ===== SEÇÃO 1: SELEÇÃO DE SERVIDOR =====
        var lblServer = ResponsiveUIHelper.CreateLabel(
            "1. Selecionar Servidor SQL Server",
            ResponsiveUIHelper.Fonts.LabelBold
        );
        lblServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblServer);

        y += 30;

        // Servidor
        var lblServerName = ResponsiveUIHelper.CreateLabel("Servidor (localhost\\SQLEXPRESS):");
        lblServerName.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblServerName);

        y += 25;

        _cmbServers = ResponsiveUIHelper.CreateComboBox(300);
        _cmbServers.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        _cmbServers.DropDownStyle = ComboBoxStyle.Simple;
        mainPanel.Controls.Add(_cmbServers);

        var lblOuDigite = ResponsiveUIHelper.CreateLabel("ou digite manualmente:");
        lblOuDigite.Location = new Point(320, y);
        mainPanel.Controls.Add(lblOuDigite);

        y += 25;

        _txtServer = ResponsiveUIHelper.CreateTextBox("localhost\\SQLEXPRESS", 300);
        _txtServer.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(_txtServer);

        // Usuário e Senha
        y += 35;

        var lblUser = ResponsiveUIHelper.CreateLabel("Usuário SQL:");
        lblUser.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblUser);

        _txtUser = ResponsiveUIHelper.CreateTextBox("sa", 200);
        _txtUser.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y + 25);
        mainPanel.Controls.Add(_txtUser);

        var lblPassword = ResponsiveUIHelper.CreateLabel("Senha:");
        lblPassword.Location = new Point(320, y);
        mainPanel.Controls.Add(lblPassword);

        _txtPassword = ResponsiveUIHelper.CreateTextBox("", 200);
        _txtPassword.Location = new Point(320, y + 25);
        _txtPassword.UseSystemPasswordChar = true;
        mainPanel.Controls.Add(_txtPassword);

        y += 60;

        // Botão testar conexão
        _btnTestConnection = ResponsiveUIHelper.CreateButton(
            "🔗 Testar Conexão",
            150,
            ResponsiveUIHelper.Colors.PrimaryBlue
        );
        _btnTestConnection.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        _btnTestConnection.Click += OnTestConnection;
        mainPanel.Controls.Add(_btnTestConnection);

        y += 50;

        // ===== SEÇÃO 2: SELEÇÃO DE BANCO =====
        var lblDatabase = ResponsiveUIHelper.CreateLabel(
            "2. Selecionar ou Criar Banco de Dados",
            ResponsiveUIHelper.Fonts.LabelBold
        );
        lblDatabase.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblDatabase);

        y += 30;

        var lblSelectDb = ResponsiveUIHelper.CreateLabel("Bancos existentes:");
        lblSelectDb.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblSelectDb);

        y += 25;

        _cmbDatabases = ResponsiveUIHelper.CreateComboBox(300);
        _cmbDatabases.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(_cmbDatabases);

        _btnRefreshDatabases = ResponsiveUIHelper.CreateButton(
            "🔄 Atualizar",
            100,
            ResponsiveUIHelper.Colors.PrimaryOrange
        );
        _btnRefreshDatabases.Location = new Point(320, y);
        _btnRefreshDatabases.Click += OnRefreshDatabases;
        mainPanel.Controls.Add(_btnRefreshDatabases);

        y += 40;

        var lblNewDb = ResponsiveUIHelper.CreateLabel("ou criar novo banco:");
        lblNewDb.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblNewDb);

        y += 25;

        _txtDatabaseName = ResponsiveUIHelper.CreateTextBox("InventoryLocal", 300);
        _txtDatabaseName.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(_txtDatabaseName);

        y += 50;

        // ===== SEÇÃO 3: SCRIPT SQL =====
        var lblScript = ResponsiveUIHelper.CreateLabel(
            "3. Selecionar Script SQL",
            ResponsiveUIHelper.Fonts.LabelBold
        );
        lblScript.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblScript);

        y += 30;

        var lblScriptPath = ResponsiveUIHelper.CreateLabel("Arquivo .SQL:");
        lblScriptPath.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblScriptPath);

        y += 25;

        _txtSqlScript = ResponsiveUIHelper.CreateTextBox("", 500);
        _txtSqlScript.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        _txtSqlScript.ReadOnly = true;
        mainPanel.Controls.Add(_txtSqlScript);

        _btnSelectSqlScript = ResponsiveUIHelper.CreateButton(
            "📂 Procurar",
            100,
            ResponsiveUIHelper.Colors.PrimaryBlue
        );
        _btnSelectSqlScript.Location = new Point(520, y);
        _btnSelectSqlScript.Click += OnSelectSqlScript;
        mainPanel.Controls.Add(_btnSelectSqlScript);

        y += 40;

        // Botão executar script
        _btnExecuteScript = ResponsiveUIHelper.CreateButton(
            "▶️ Executar Script",
            150,
            ResponsiveUIHelper.Colors.PrimaryGreen
        );
        _btnExecuteScript.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        _btnExecuteScript.Click += OnExecuteScript;
        mainPanel.Controls.Add(_btnExecuteScript);

        y += 50;

        // ===== LOG =====
        var lblLog = ResponsiveUIHelper.CreateLabel(
            "Log de Execução:",
            ResponsiveUIHelper.Fonts.LabelBold
        );
        lblLog.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblLog);

        y += 30;

        _rtbLog = new RichTextBox
        {
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Size = new Size(650, 150),
            ReadOnly = true,
            BackColor = Color.White,
            ForeColor = ResponsiveUIHelper.Colors.TextDark
        };
        mainPanel.Controls.Add(_rtbLog);

        y += 160;

        // Status bar
        _lblStatus = ResponsiveUIHelper.CreateLabel("Pronto");
        _lblStatus.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(_lblStatus);

        _progressBar = new ProgressBar
        {
            Location = new Point(250, y),
            Size = new Size(400, 20),
            Visible = false,
            Style = ProgressBarStyle.Marquee
        };
        mainPanel.Controls.Add(_progressBar);

        Controls.Add(mainPanel);

        // Painel de botões
        var buttonPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnSave = ResponsiveUIHelper.CreateButton(
            "💾 Salvar Configuração",
            150,
            ResponsiveUIHelper.Colors.PrimaryGreen
        );
        _btnSave.Click += (s, e) => DialogResult = DialogResult.OK;

        _btnCancel = ResponsiveUIHelper.CreateButton(
            "❌ Cancelar",
            100,
            ResponsiveUIHelper.Colors.PrimaryRed
        );
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        int x = buttonPanel.Width - 270;
        int btnY = (buttonPanel.Height - _btnSave.Height) / 2;

        _btnSave.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnSave);

        x += _btnSave.Width + ResponsiveUIHelper.Spacing.Medium;
        _btnCancel.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnCancel);

        Controls.Add(buttonPanel);
    }

    private void LoadLocalServers()
    {
        try
        {
            AddLog("🔍 Procurando servidores SQL Server locais...");

            var servers = new List<string>
            {
                "localhost\\SQLEXPRESS",
                "(local)\\SQLEXPRESS",
                ".\\SQLEXPRESS",
                "localhost",
                Environment.MachineName + "\\SQLEXPRESS"
            };

            _cmbServers.Items.Clear();
            foreach (var server in servers)
            {
                _cmbServers.Items.Add(server);
            }

            _cmbServers.SelectedIndex = 0;
            AddLog("✅ Servidores carregados");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro ao carregar servidores: {ex.Message}", Color.Red);
        }
    }

    private void OnTestConnection(object? sender, EventArgs e)
    {
        try
        {
            ShowLoading(true);
            AddLog("🔗 Testando conexão...");

            var server = _txtServer.Text.Trim();
            var user = _txtUser.Text.Trim();
            var password = _txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(server))
            {
                AddLog("❌ Digite o servidor SQL", Color.Red);
                return;
            }

            var connString = $"Server={server};User Id={user};Password={password};Connection Timeout=5;";
            using var conn = new SqlConnection(connString);
            conn.Open();

            _connectionString = connString;
            _lblStatus.Text = "✅ Conexão estabelecida com sucesso!";
            _lblStatus.ForeColor = ResponsiveUIHelper.Colors.PrimaryGreen;
            AddLog($"✅ Conectado ao servidor: {server}");

            // Carregar bancos de dados
            OnRefreshDatabases(sender, e);
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "❌ Erro na conexão";
            _lblStatus.ForeColor = ResponsiveUIHelper.Colors.PrimaryRed;
            AddLog($"❌ Erro: {ex.Message}", Color.Red);
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void OnRefreshDatabases(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                AddLog("⚠️ Conecte ao servidor primeiro", Color.Orange);
                return;
            }

            AddLog("📦 Carregando bancos de dados...");

            _cmbDatabases.Items.Clear();

            using var conn = new SqlConnection(_connectionString + "Database=master;");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var dbName = reader["name"]?.ToString();
                if (!string.IsNullOrEmpty(dbName))
                {
                    _cmbDatabases.Items.Add(dbName);
                }
            }

            AddLog($"✅ {_cmbDatabases.Items.Count} bancos de dados encontrados");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro ao carregar bancos: {ex.Message}", Color.Red);
        }
    }

    private void OnSelectSqlScript(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts"),
                Title = "Selecione o arquivo SQL"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _selectedSqlScript = dialog.FileName;
                _txtSqlScript.Text = _selectedSqlScript;
                AddLog($"📄 Script selecionado: {Path.GetFileName(_selectedSqlScript)}");
            }
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro ao selecionar arquivo: {ex.Message}", Color.Red);
        }
    }

    private void OnExecuteScript(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                ResponsiveUIHelper.ShowError("Conecte ao servidor primeiro");
                return;
            }

            if (string.IsNullOrEmpty(_selectedSqlScript) || !File.Exists(_selectedSqlScript))
            {
                ResponsiveUIHelper.ShowError("Selecione um arquivo SQL válido");
                return;
            }

            var database = _cmbDatabases.SelectedItem?.ToString() ?? _txtDatabaseName.Text.Trim();

            if (string.IsNullOrEmpty(database))
            {
                ResponsiveUIHelper.ShowError("Selecione ou digite um banco de dados");
                return;
            }

            ShowLoading(true);
            AddLog($"⏳ Executando script em '{database}'...");

            ExecuteSqlScript(_selectedSqlScript, database);

            AddLog("✅ Script executado com sucesso!");
            ResponsiveUIHelper.ShowSuccess("Script executado com sucesso!");
        }
        catch (Exception ex)
        {
            AddLog($"❌ Erro ao executar script: {ex.Message}", Color.Red);
            ResponsiveUIHelper.ShowError($"Erro: {ex.Message}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void ExecuteSqlScript(string scriptPath, string database)
    {
        var connStringWithDb = _connectionString + $"Database={database};";

        using var conn = new SqlConnection(connStringWithDb);
        conn.Open();

        var scriptContent = File.ReadAllText(scriptPath);
        var batches = scriptContent.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = batch.Trim();
            cmd.CommandTimeout = 300; // 5 minutos

            cmd.ExecuteNonQuery();
        }
    }

    private void ShowLoading(bool show)
    {
        _progressBar.Visible = show;
        _btnTestConnection.Enabled = !show;
        _btnSelectSqlScript.Enabled = !show;
        _btnExecuteScript.Enabled = !show;
        _btnSave.Enabled = !show;
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

    public string? GetConnectionString() => _connectionString;
}
