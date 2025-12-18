using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
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
    private ComboBox _cmbSqlDatabase = null!;
    private TextBox _txtSqlUser = null!;
    private TextBox _txtSqlPassword = null!;
    private CheckBox _chkSqlIntegratedSecurity = null!;

    private Button _btnListDbs = null!;
    private Button _btnCreateDb = null!;
    
    private Panel _panelSqlServer = null!;
    private Panel _panelFileMdf = null!;
    
    private Label _lblStatus = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _rtbLog = null!;
    private Button _btnContinue = null!;
    private Button _btnCancel = null!;

    private string _selectedMode = "sqlserver";
    private string _connectionString = string.Empty;

    private bool _useMdfCache;
    private string? _originalMdfPath;

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

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 0,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // ===== MODO 1: SQL Server =====
        _rbSqlServer = new RadioButton
        {
            Text = "🖥️  SQL Server (Servidor/Rede)",
            AutoSize = true,
            Checked = true,
            Font = ResponsiveUIHelper.Fonts.LabelBold,
            Margin = new Padding(0, 0, 0, 6)
        };
        _rbSqlServer.CheckedChanged += (s, e) => { if (_rbSqlServer.Checked) ShowSqlServerPanel(); };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(_rbSqlServer);

        // Altura fixa para garantir que o card sempre apareça abaixo do RadioButton
        _panelSqlServer = ResponsiveUIHelper.CreateCard(600, 195);
        _panelSqlServer.Dock = DockStyle.Top;
        _panelSqlServer.Margin = new Padding(20, 0, 0, 14);
        _panelSqlServer.Visible = true;

        var pnlSqlControls = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10) };

        // Layout responsivo: elimina sobreposição e mantém alinhamento perfeito
        var sqlLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 4,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));   // rótulos
        sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // inputs
        sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); // ações

        // Row 0: Servidor + Testar
        var lblSqlServer = new Label { Text = "Servidor:", AutoSize = true, Anchor = AnchorStyles.Left, Font = ResponsiveUIHelper.Fonts.Regular };
        _txtSqlServer = ResponsiveUIHelper.CreateTextBox("localhost\\SQLEXPRESS", 10);
        _txtSqlServer.Dock = DockStyle.Fill;
        var btnTestSql = ResponsiveUIHelper.CreateButton("🔗 Testar", 110, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnTestSql.Anchor = AnchorStyles.Right;
        btnTestSql.Click += (s, e) => TestSqlConnection();

        sqlLayout.Controls.Add(lblSqlServer, 0, 0);
        sqlLayout.Controls.Add(_txtSqlServer, 1, 0);
        sqlLayout.Controls.Add(btnTestSql, 2, 0);

        // Row 1: Database + botões
        var lblDatabase = new Label { Text = "Database:", AutoSize = true, Anchor = AnchorStyles.Left, Font = ResponsiveUIHelper.Fonts.Regular };
        _cmbSqlDatabase = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            Font = ResponsiveUIHelper.Fonts.Regular
        };
        _cmbSqlDatabase.Text = "InventoryLocal";

        var dbButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };

        _btnListDbs = ResponsiveUIHelper.CreateButton("📚 DBs", 80, ResponsiveUIHelper.Colors.PrimaryBlue);
        _btnListDbs.Click += (_, _) => LoadDatabasesIntoCombo();

        _btnCreateDb = ResponsiveUIHelper.CreateButton("➕ Criar", 80, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnCreateDb.Click += (_, _) => CreateDatabaseFromInput();

        dbButtons.Controls.Add(_btnListDbs);
        dbButtons.Controls.Add(_btnCreateDb);

        sqlLayout.Controls.Add(lblDatabase, 0, 1);
        sqlLayout.Controls.Add(_cmbSqlDatabase, 1, 1);
        sqlLayout.Controls.Add(dbButtons, 2, 1);

        // Row 2: Segurança integrada
        _chkSqlIntegratedSecurity = new CheckBox
        {
            Text = "☑ Segurança Windows",
            AutoSize = true,
            Checked = true,
            Anchor = AnchorStyles.Left,
            Font = ResponsiveUIHelper.Fonts.Regular,
            Margin = new Padding(0, 2, 0, 0)
        };
        _chkSqlIntegratedSecurity.CheckedChanged += (_, _) => ToggleSqlAuthFields();
        sqlLayout.Controls.Add(_chkSqlIntegratedSecurity, 1, 2);
        sqlLayout.SetColumnSpan(_chkSqlIntegratedSecurity, 2);

        // Row 3: Usuário + Senha (2 colunas internas)
        var credentialsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 4, 0, 0)
        };
        credentialsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        credentialsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        var lblSqlUser = new Label { Text = "Usuário SQL:", AutoSize = true, Anchor = AnchorStyles.Left, Font = ResponsiveUIHelper.Fonts.Regular };
        var lblSqlPassword = new Label { Text = "Senha:", AutoSize = true, Anchor = AnchorStyles.Left, Font = ResponsiveUIHelper.Fonts.Regular };

        _txtSqlUser = ResponsiveUIHelper.CreateTextBox(string.Empty, 10);
        _txtSqlUser.Dock = DockStyle.Fill;

        _txtSqlPassword = ResponsiveUIHelper.CreateTextBox(string.Empty, 10);
        _txtSqlPassword.Dock = DockStyle.Fill;
        _txtSqlPassword.PasswordChar = '*';

        credentialsLayout.Controls.Add(lblSqlUser, 0, 0);
        credentialsLayout.Controls.Add(lblSqlPassword, 1, 0);
        credentialsLayout.Controls.Add(_txtSqlUser, 0, 1);
        credentialsLayout.Controls.Add(_txtSqlPassword, 1, 1);

        sqlLayout.Controls.Add(credentialsLayout, 0, 3);
        sqlLayout.SetColumnSpan(credentialsLayout, 3);

        // Espaçamento entre linhas
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sqlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        pnlSqlControls.Controls.Add(sqlLayout);

        ToggleSqlAuthFields();

        _panelSqlServer.Controls.Add(pnlSqlControls);
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(_panelSqlServer);

        // ===== MODO 2: Arquivo .mdf =====
        _rbFileMdf = new RadioButton
        {
            Text = "📁 Arquivo .mdf (Rede/Local)",
            AutoSize = true,
            Font = ResponsiveUIHelper.Fonts.LabelBold,
            Margin = new Padding(0, 0, 0, 6)
        };
        _rbFileMdf.CheckedChanged += (s, e) => { if (_rbFileMdf.Checked) ShowFileMdfPanel(); };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(_rbFileMdf);

        // Altura fixa para garantir consistência visual e evitar colapso
        _panelFileMdf = ResponsiveUIHelper.CreateCard(600, 80);
        _panelFileMdf.Dock = DockStyle.Top;
        _panelFileMdf.Margin = new Padding(20, 0, 0, 14);
        _panelFileMdf.Visible = false;

        var pnlFileControls = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10) };

        var fileLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        fileLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        var lblFilePath = new Label { Text = "Arquivo .mdf:", AutoSize = true, Anchor = AnchorStyles.Left, Font = ResponsiveUIHelper.Fonts.Regular };
        var txtFilePath = ResponsiveUIHelper.CreateTextBox("", 10);
        txtFilePath.Dock = DockStyle.Fill;
        txtFilePath.ReadOnly = true;
        txtFilePath.BackColor = Color.White;
        txtFilePath.ForeColor = ResponsiveUIHelper.Colors.TextDark;
        txtFilePath.PlaceholderText = "Clique em Procurar para selecionar ou criar...";

        var btnBrowse = ResponsiveUIHelper.CreateButton("📂 Procurar", 110, ResponsiveUIHelper.Colors.PrimaryBlue);
        btnBrowse.Anchor = AnchorStyles.Right;
        btnBrowse.Click += (s, e) => BrowseMdfFile(txtFilePath);

        var lblInfo = new Label
        {
            Text = "💡 Criar novo ou selecionar existente",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font(ResponsiveUIHelper.Fonts.Regular.FontFamily, 8F, FontStyle.Italic),
            ForeColor = ResponsiveUIHelper.Colors.TextLight,
            Margin = new Padding(0, 6, 0, 0)
        };

        fileLayout.Controls.Add(lblFilePath, 0, 0);
        fileLayout.Controls.Add(txtFilePath, 1, 0);
        fileLayout.Controls.Add(btnBrowse, 2, 0);
        fileLayout.Controls.Add(lblInfo, 1, 1);
        fileLayout.SetColumnSpan(lblInfo, 2);

        pnlFileControls.Controls.Add(fileLayout);
        pnlFileControls.Tag = txtFilePath;

        _panelFileMdf.Controls.Add(pnlFileControls);
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(_panelFileMdf);

        // ===== LOG =====
        var lblLog = ResponsiveUIHelper.CreateLabel("Log:", ResponsiveUIHelper.Fonts.LabelBold);
        lblLog.Margin = new Padding(0, 0, 0, 6);
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(lblLog);

        _rtbLog = new RichTextBox
        {
            Height = 120,
            Dock = DockStyle.Top,
            ReadOnly = true,
            BackColor = Color.White,
            ForeColor = ResponsiveUIHelper.Colors.TextDark,
            Margin = new Padding(0)
        };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.Controls.Add(_rtbLog);

        mainPanel.Controls.Add(contentLayout);

        Controls.Add(mainPanel);

        // Rodapé (botões + status + progresso) com alinhamento perfeito
        var bottomBar = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Medium),
            ColumnCount = 5,
            RowCount = 1
        };
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));

        _btnContinue = ResponsiveUIHelper.CreateButton("➡️  Continuar", 150, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnContinue.Dock = DockStyle.Fill;
        _btnContinue.Click += OnContinue;

        _btnCancel = ResponsiveUIHelper.CreateButton("❌ Cancelar", 100, ResponsiveUIHelper.Colors.PrimaryRed);
        _btnCancel.Dock = DockStyle.Fill;
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        _lblStatus = ResponsiveUIHelper.CreateLabel("Pronto");
        _lblStatus.Anchor = AnchorStyles.Left;

        _progressBar = new ProgressBar
        {
            Visible = false,
            Style = ProgressBarStyle.Marquee,
            Dock = DockStyle.Fill
        };

        bottomBar.Controls.Add(_btnContinue, 0, 0);
        bottomBar.Controls.Add(_btnCancel, 1, 0);
        bottomBar.Controls.Add(_lblStatus, 3, 0);
        bottomBar.Controls.Add(_progressBar, 4, 0);

        Controls.Add(bottomBar);

        AddLog("✅ Bem-vindo ao Inventory System!");
        AddLog("Escolha como deseja armazenar os dados.");
    }

    private void ShowSqlServerPanel()
    {
        _panelSqlServer.Visible = true;
        _panelFileMdf.Visible = false;
        _selectedMode = "sqlserver";
        _useMdfCache = false;
        _originalMdfPath = null;
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
        var database = _cmbSqlDatabase.Text.Trim();

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

            // O app precisa criar/alterar tabelas e inserir usuário admin na primeira execução.
            // Validar permissões mínimas aqui evita falhas posteriores menos claras.
            if (!ValidateSqlServerPermissions(conn, out error))
            {
                return false;
            }

            error = null;
            return true;
        }
        catch (SqlException ex)
        {
            error = ExplainSqlException(ex);
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static string ExplainSqlException(SqlException ex)
    {
        // Mensagens mais amigáveis para os erros mais comuns do dia a dia
        // Alguns erros de attach/arquivo físico aparecem como 5120 e/ou citam "Operating system error"
        if (ex.Number == 5120 || ex.Message.Contains("Cannot open the physical file", StringComparison.OrdinalIgnoreCase))
        {
            if (ex.Message.Contains("Operating system error 5", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Access is denied", StringComparison.OrdinalIgnoreCase))
            {
                return "Sem permissão para acessar o MDF/LDF (Access denied). Confirme permissões de leitura+escrita na pasta/compartilhamento e se o arquivo não está em uso.";
            }

            if (ex.Message.Contains("Operating system error 3", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("The system cannot find the path specified", StringComparison.OrdinalIgnoreCase))
            {
                return "Caminho do MDF/LDF não encontrado pelo SQL/LocalDB. Confirme se o caminho existe e se o compartilhamento está disponível.";
            }

            if (ex.Message.Contains("Operating system error 53", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("network path was not found", StringComparison.OrdinalIgnoreCase))
            {
                return "Caminho de rede não encontrado/indisponível para o SQL/LocalDB. Verifique conectividade e permissões do compartilhamento.";
            }

            return "Falha ao anexar/abrir o MDF/LDF. Em caminhos de rede isso pode falhar se o mecanismo SQL/LocalDB não conseguir acessar o compartilhamento com escrita.";
        }

        return ex.Number switch
        {
            18456 => "Falha de login no SQL Server (usuário/senha inválidos ou sem permissão).",
            4060 => "Não foi possível abrir o banco informado (Database). Ele pode não existir ou seu usuário não tem acesso.",
            18452 => "Login sem permissão (Autenticação Windows falhou / sem confiança).",
            53 => "Servidor SQL não encontrado ou inacessível (verifique hostname/instância e rede).",
            40 => "Não foi possível conectar ao SQL Server (porta bloqueada, serviço parado ou rede).",
            229 => "Permissão negada no SQL Server para a operação solicitada.",
            _ => ex.Message
        };
    }

    private static bool ValidateSqlServerPermissions(SqlConnection conn, out string? error)
    {
        error = null;
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 5;
            cmd.CommandText = @"
                SELECT
                    IS_SRVROLEMEMBER('sysadmin') AS IsSysAdmin,
                    IS_MEMBER('db_owner') AS IsDbOwner,
                    HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'CREATE TABLE') AS CanCreateTable";

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return true; // não deve acontecer, mas não vamos bloquear

            var isSysAdmin = TryGetInt(reader, 0) == 1;
            var isDbOwner = TryGetInt(reader, 1) == 1;
            var canCreateTable = TryGetInt(reader, 2) == 1;

            if (isSysAdmin || isDbOwner || canCreateTable)
                return true;

            error =
                "Conexão OK, mas o usuário não tem permissão para criar tabelas nesse banco. " +
                "Conceda 'db_owner' para o usuário nesse database ou permissões equivalentes (ex.: CREATE TABLE).";
            return false;
        }
        catch (SqlException ex)
        {
            // Se não der para checar permissões, melhor mostrar o motivo (sem mascarar)
            error = ExplainSqlException(ex);
            return false;
        }
    }

    private static int? TryGetInt(SqlDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;
        try
        {
            return Convert.ToInt32(reader.GetValue(ordinal));
        }
        catch
        {
            return null;
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

            // Se conectou, aproveita para carregar lista de DBs (melhora UX)
            try { LoadDatabasesIntoCombo(); } catch { /* best-effort */ }
        }
        else
        {
            AddLog($"❌ Erro na conexão: {error}", Color.Red);
        }
    }

    private bool DatabaseExistsOnServer(string masterConnectionString, string databaseName, out string? error)
    {
        error = null;
        try
        {
            using var conn = new SqlConnection(masterConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 5;
            cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", databaseName);
            var count = (int?)cmd.ExecuteScalar() ?? 0;
            return count > 0;
        }
        catch (SqlException ex)
        {
            error = ExplainSqlException(ex);
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool TryCreateDatabase(string masterConnectionString, string databaseName, out string? error)
    {
        error = null;
        try
        {
            using var conn = new SqlConnection(masterConnectionString);
            conn.Open();

            using var create = conn.CreateCommand();
            create.CommandTimeout = 30;
            create.CommandText = $"CREATE DATABASE [{databaseName}]";
            create.ExecuteNonQuery();
            return true;
        }
        catch (SqlException ex)
        {
            error = ExplainSqlException(ex);
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private string? BuildMasterConnectionString(out string? error)
    {
        error = null;

        var server = _txtSqlServer.Text.Trim();
        if (string.IsNullOrWhiteSpace(server))
        {
            error = "Informe o servidor do SQL Server.";
            return null;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = "master",
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
                    error = "Informe usuário e senha ou marque Segurança Integrada.";
                    return null;
                }

                builder.IntegratedSecurity = false;
                builder.UserID = user;
                builder.Password = password;
            }

            return builder.ToString();
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    private void LoadDatabasesIntoCombo()
    {
        var masterConnStr = BuildMasterConnectionString(out var err);
        if (masterConnStr == null)
        {
            AddLog($"❌ {err}", Color.Red);
            return;
        }

        AddLog("📚 Carregando lista de bancos (databases)...");

        try
        {
            using var conn = new SqlConnection(masterConnStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 5;
            cmd.CommandText = @"
                SELECT name
                FROM sys.databases
                WHERE name NOT IN ('master','tempdb','model','msdb')
                ORDER BY name";

            var names = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }

            var current = _cmbSqlDatabase.Text;
            _cmbSqlDatabase.BeginUpdate();
            _cmbSqlDatabase.Items.Clear();
            foreach (var n in names)
                _cmbSqlDatabase.Items.Add(n);
            _cmbSqlDatabase.EndUpdate();

            if (!string.IsNullOrWhiteSpace(current))
                _cmbSqlDatabase.Text = current;

            if (names.Count == 0)
            {
                AddLog("⚠️  Nenhum database listado (pode ser permissão limitada).", Color.DarkOrange);
                AddLog("💡 Digite um nome e clique em 'Criar' para criar um novo DB.", Color.DarkOrange);
            }
            else
            {
                AddLog($"✅ {names.Count} databases carregados.");
            }
        }
        catch (SqlException ex)
        {
            AddLog($"❌ Falha ao listar databases: {ExplainSqlException(ex)}", Color.Red);
            AddLog("💡 Você ainda pode digitar o nome do DB manualmente ou clicar em 'Criar'.", Color.DarkOrange);
        }
        catch (Exception ex)
        {
            AddLog($"❌ Falha ao listar databases: {ex.Message}", Color.Red);
        }
    }

    private void CreateDatabaseFromInput()
    {
        var dbName = _cmbSqlDatabase.Text.Trim();
        if (string.IsNullOrWhiteSpace(dbName))
        {
            AddLog("❌ Informe o nome do database para criar.", Color.Red);
            return;
        }

        var masterConnStr = BuildMasterConnectionString(out var err);
        if (masterConnStr == null)
        {
            AddLog($"❌ {err}", Color.Red);
            return;
        }

        AddLog($"➕ Criando database '{dbName}'...", Color.DarkOrange);
        try
        {
            // Cria banco e retorna connection string para ele (não persiste config aqui)
            var builder = new SqlConnectionStringBuilder(masterConnStr)
            {
                InitialCatalog = "master"
            };

            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

                using var check = conn.CreateCommand();
                check.CommandTimeout = 10;
                check.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
                check.Parameters.AddWithValue("@name", dbName);
                var exists = ((int?)check.ExecuteScalar() ?? 0) > 0;
                if (exists)
                {
                    AddLog("ℹ️  Database já existe. Selecionando...", Color.DarkOrange);
                }
                else
                {
                    using var create = conn.CreateCommand();
                    create.CommandTimeout = 30;
                    create.CommandText = $"CREATE DATABASE [{dbName}]";
                    create.ExecuteNonQuery();
                    AddLog("✅ Database criado com sucesso!", Color.DarkOrange);
                }
            }

            // Recarrega lista e seleciona
            LoadDatabasesIntoCombo();
            _cmbSqlDatabase.Text = dbName;

            // Testa conexão já no DB criado/selecionado
            var openErr = "Connection string inválida.";
            if (TryBuildSqlServerConnectionString(out var dbConn))
            {
                if (TryOpenConnection(dbConn, out var openError))
                {
                    _connectionString = dbConn;
                    AddLog("✅ Conexão com o database validada.");
                }
                else
                {
                    openErr = openError ?? "Erro desconhecido.";
                    AddLog($"⚠️  Database criado, mas não foi possível validar conexão: {openErr}", Color.DarkOrange);
                }
            }
            else
            {
                AddLog($"⚠️  Database criado, mas não foi possível validar conexão: {openErr}", Color.DarkOrange);
            }
        }
        catch (SqlException ex)
        {
            AddLog($"❌ Falha ao criar database: {ExplainSqlException(ex)}", Color.Red);
            AddLog("💡 Seu usuário pode não ter permissão de CREATE DATABASE. Peça para conceder 'dbcreator' ou 'sysadmin'.", Color.DarkOrange);
        }
        catch (Exception ex)
        {
            AddLog($"❌ Falha ao criar database: {ex.Message}", Color.Red);
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

                if (IsNetworkPath(mdfPath))
                {
                    AddLog("⚠️  MDF em caminho de rede detectado.", Color.DarkOrange);
                    AddLog("ℹ️  Isso só funcionará se o mecanismo SQL/LocalDB conseguir acessar o compartilhamento com permissão de escrita.", Color.DarkOrange);
                }
                
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
                        AddLog("⚠️  Escolha uma pasta com permissão de escrita (ex.: Documentos / AppData).", Color.DarkOrange);
                        return;
                    }
                }
                
                // Testar permissão de escrita
                AddLog($"🔍 Testando permissões de escrita em: {directory}");
                try
                {
                    var testFile = Path.Combine(directory, $".write_test_{Guid.NewGuid():N}");
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
                _useMdfCache = MdfCacheManager.IsNetworkPath(mdfPath);
                _originalMdfPath = _useMdfCache ? mdfPath : null;
                _connectionString = $"CREATE:{mdfPath}"; // Marcador especial

                if (_useMdfCache)
                {
                    AddLog("🛡️  Modo garantido (rede): será usado cache local e sincronização.", Color.DarkOrange);
                }
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

                if (IsNetworkPath(mdfPath))
                {
                    AddLog("⚠️  MDF em caminho de rede detectado.", Color.DarkOrange);
                    AddLog("ℹ️  O SQL/LocalDB precisa de leitura+escrita na pasta para anexar/criar o .ldf.", Color.DarkOrange);
                }
                
                // Validar se arquivo existe e é acessível
                try
                {
                    if (!File.Exists(mdfPath))
                    {
                        AddLog($"❌ Arquivo não encontrado: {mdfPath}", Color.Red);
                        return;
                    }

                    // Se estiver marcado como somente leitura, o attach tende a falhar ou ficar "read-only".
                    // Tentar remover o atributo (sem alterar ACL/compartilhamento).
                    if (!TryEnsureNotReadOnly(mdfPath, msg => AddLog(msg, Color.DarkOrange)))
                    {
                        AddLog("❌ O arquivo MDF está como somente leitura e não foi possível remover o atributo.", Color.Red);
                        AddLog("💡 Remova o atributo 'Somente leitura' nas propriedades do arquivo ou copie para uma pasta local com permissão.", Color.DarkOrange);
                        return;
                    }

                    // Validar que a pasta permite escrita (LocalDB/SQL precisa criar/atualizar o .ldf ao anexar)
                    var directory = Path.GetDirectoryName(mdfPath);
                    if (string.IsNullOrEmpty(directory))
                    {
                        AddLog($"❌ Caminho inválido: {mdfPath}", Color.Red);
                        return;
                    }

                    AddLog($"🔍 Testando escrita na pasta do MDF: {directory}");
                    var testFile = Path.Combine(directory, $".write_test_{Guid.NewGuid():N}");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);

                    // Testar leitura/escrita (LocalDB normalmente precisa escrever no MDF/LDF)
                    using (var fs = new FileStream(mdfPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        // no-op
                    }

                    // Se existir LDF, também precisa estar acessível
                    var ldfPath = Path.ChangeExtension(mdfPath, ".ldf");
                    if (File.Exists(ldfPath))
                    {
                        if (!TryEnsureNotReadOnly(ldfPath, msg => AddLog(msg, Color.DarkOrange)))
                        {
                            AddLog("❌ O arquivo LDF está como somente leitura e não foi possível remover o atributo.", Color.Red);
                            return;
                        }

                        using (var ldf = new FileStream(ldfPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            // no-op
                        }
                    }
                    else
                    {
                        AddLog("ℹ️  Arquivo .ldf não encontrado — o SQL/LocalDB deve criar um ao anexar (precisa de escrita na pasta).", Color.DarkOrange);
                    }

                    AddLog($"✅ Arquivo é acessível (leitura/escrita)");
                }
                catch (Exception ex)
                {
                    AddLog($"❌ Erro ao acessar arquivo: {ex.Message}", Color.Red);
                    AddLog($"⚠️  Verifique se o arquivo está em uso ou sem permissão", Color.Red);
                    return;
                }
                
                txtPath.Text = mdfPath;
                _useMdfCache = MdfCacheManager.IsNetworkPath(mdfPath);
                _originalMdfPath = _useMdfCache ? mdfPath : null;

                if (_useMdfCache)
                {
                    AddLog("🛡️  Modo garantido (rede): copiando para cache local...", Color.DarkOrange);
                    var cachedMdf = MdfCacheManager.EnsureCacheReady(mdfPath, msg => AddLog(msg));
                    AddLog($"📌 Usando cache local: {cachedMdf}");
                    _connectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={cachedMdf};Integrated Security=true;TrustServerCertificate=true;";
                }
                else
                {
                    _connectionString = $"Data Source=(LocalDB)\\mssqllocaldb;AttachDbFileName={mdfPath};Integrated Security=true;TrustServerCertificate=true;";
                }
                AddLog($"📁 Arquivo existente selecionado: {Path.GetFileName(mdfPath)}");
                AddLog($"✅ Caminho validado com sucesso");
            }
        }
    }

    private static bool TryEnsureNotReadOnly(string path, Action<string> log)
    {
        try
        {
            var attr = File.GetAttributes(path);
            if ((attr & FileAttributes.ReadOnly) == 0)
                return true;

            log($"⚠️  Arquivo marcado como somente leitura: {path}");

            var newAttr = attr & ~FileAttributes.ReadOnly;
            File.SetAttributes(path, newAttr);
            log("✅ Atributo 'Somente leitura' removido.");
            return true;
        }
        catch (Exception ex)
        {
            log($"⚠️  Não foi possível remover atributo 'Somente leitura': {ex.Message}");
            return false;
        }
    }

    private static bool IsNetworkPath(string path)
    {
        // UNC: \\server\share\file.mdf
        if (path.StartsWith("\\\\", StringComparison.Ordinal))
            return true;

        try
        {
            // Unidade mapeada em rede (Windows): Z:\...
            var root = Path.GetPathRoot(path);
            if (!string.IsNullOrWhiteSpace(root) && OperatingSystem.IsWindows())
            {
                var di = new DriveInfo(root);
                return di.DriveType == DriveType.Network;
            }
        }
        catch
        {
            // Se não deu para detectar, não bloqueia aqui
        }

        return false;
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
                        // Se o erro for porque o DB não existe, oferecer: criar DB OU mudar para MDF.
                        var dbName = _cmbSqlDatabase.Text.Trim();
                        var masterConnStr = BuildMasterConnectionString(out var masterErr);

                        if (!string.IsNullOrWhiteSpace(dbName) && masterConnStr != null)
                        {
                            var exists = DatabaseExistsOnServer(masterConnStr, dbName, out var existsErr);
                            if (!exists && existsErr == null)
                            {
                                DialogResult userChoice = DialogResult.Cancel;
                                this.Invoke(() =>
                                {
                                    userChoice = MessageBox.Show(
                                        $"O database '{dbName}' não foi encontrado no servidor.\n\n" +
                                        "Deseja criar agora?\n\n" +
                                        "Sim = Criar database\n" +
                                        "Não = Usar Arquivo .mdf (local/rede)\n" +
                                        "Cancelar = Voltar",
                                        "Database não encontrado",
                                        MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Question);
                                });

                                if (userChoice == DialogResult.Yes)
                                {
                                    AddLog($"➕ Criando database '{dbName}'...", Color.DarkOrange);
                                    if (!TryCreateDatabase(masterConnStr, dbName, out var createErr))
                                    {
                                        AddLog($"❌ Falha ao criar database: {createErr}", Color.Red);
                                        AddLog("💡 Seu usuário pode não ter permissão de CREATE DATABASE. Peça 'dbcreator' ou 'sysadmin'.", Color.DarkOrange);
                                        this.Invoke(() =>
                                        {
                                            _btnContinue.Enabled = true;
                                            _progressBar.Visible = false;
                                        });
                                        return;
                                    }

                                    // Revalidar conexão no DB recém-criado
                                    if (!TryBuildSqlServerConnectionString(out connString) || !TryOpenConnection(connString, out error))
                                    {
                                        AddLog($"❌ Database criado, mas falha ao conectar: {error}", Color.Red);
                                        this.Invoke(() =>
                                        {
                                            _btnContinue.Enabled = true;
                                            _progressBar.Visible = false;
                                        });
                                        return;
                                    }

                                    AddLog("✅ Database criado e conexão validada!", Color.DarkOrange);
                                }
                                else if (userChoice == DialogResult.No)
                                {
                                    // Trocar para modo MDF e deixar o usuário escolher
                                    this.Invoke(() =>
                                    {
                                        _rbFileMdf.Checked = true;
                                        _btnContinue.Enabled = true;
                                        _progressBar.Visible = false;
                                    });
                                    AddLog("📁 Trocado para modo Arquivo .mdf — selecione/crie o arquivo.", Color.DarkOrange);
                                    return;
                                }
                                else
                                {
                                    // Cancelar volta para o form
                                    this.Invoke(() =>
                                    {
                                        _btnContinue.Enabled = true;
                                        _progressBar.Visible = false;
                                    });
                                    return;
                                }
                            }
                            else if (!exists && existsErr != null)
                            {
                                AddLog($"⚠️  Não foi possível verificar existência do DB: {existsErr}", Color.DarkOrange);
                            }
                        }

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
                            string effectiveMdfPath = mdfPath;

                            // Modo garantido em rede: cria em cache local e sincroniza para o destino.
                            if (MdfCacheManager.IsNetworkPath(mdfPath))
                            {
                                _useMdfCache = true;
                                _originalMdfPath = mdfPath;
                                AddLog("🛡️  Modo garantido (rede): criando no cache local...", Color.DarkOrange);
                                effectiveMdfPath = MdfCacheManager.GetCachedMdfPath(mdfPath);
                            }

                            _connectionString = LocalDbManager.CreateMdfDatabase(effectiveMdfPath, (msg) => AddLog(msg));

                            // Se a criação foi para cache, publicar imediatamente para o destino
                            if (_useMdfCache && !string.IsNullOrEmpty(_originalMdfPath))
                            {
                                AddLog("🔄 Publicando banco para o destino (rede)...", Color.DarkOrange);
                                MdfCacheManager.TrySyncBack(_originalMdfPath, effectiveMdfPath, msg => AddLog(msg));
                                AddLog("✅ Publicação concluída (ou tentada). Continuando com cache local.", Color.DarkOrange);
                            }

                            AddLog($"✅ Banco de dados criado com sucesso! (DB: {Path.GetFileNameWithoutExtension(effectiveMdfPath)})");
                            try
                            {
                                var mdfOk = File.Exists(effectiveMdfPath);
                                var ldfOk = File.Exists(Path.ChangeExtension(effectiveMdfPath, ".ldf"));
                                if (mdfOk && ldfOk)
                                {
                                    AddLog($"📂 Arquivos físicos confirmados:\n   MDF: {effectiveMdfPath}\n   LDF: {Path.ChangeExtension(effectiveMdfPath, ".ldf")}");
                                }
                                else
                                    AddLog("⚠️  Aviso: não foi possível confirmar ambos os arquivos físicos", System.Drawing.Color.DarkOrange);
                            }
                            catch { /* não bloquear UI por erro de verificação */ }
                            AddLog($"🔗 ConnectionString final: {_connectionString}");
                            AddLog("👤 Usuário admin criado: admin / L9l337643k#$");

                            if (!TryOpenConnection(_connectionString, out var openErr))
                            {
                                AddLog($"❌ Não foi possível abrir o banco recém-criado: {openErr}", Color.Red);
                                AddLog("💡 Se o MDF estiver em rede, tente um caminho local ou use o modo SQL Server (Servidor/Rede).", Color.DarkOrange);
                                this.Invoke(() =>
                                {
                                    _btnContinue.Enabled = true;
                                    _progressBar.Visible = false;
                                });
                                return;
                            }
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
                    else
                    {
                        // MDF existente selecionado: validar que o attach realmente funciona agora.
                        if (!TryOpenConnection(_connectionString, out var error))
                        {
                            AddLog($"❌ Erro ao validar MDF: {error}", Color.Red);
                            AddLog("💡 Se o MDF estiver em rede, confirme permissão de escrita no compartilhamento e na pasta; se persistir, use o modo SQL Server (Servidor/Rede).", Color.DarkOrange);
                            this.Invoke(() =>
                            {
                                _btnContinue.Enabled = true;
                                _progressBar.Visible = false;
                            });
                            return;
                        }

                        AddLog("✅ MDF validado com sucesso!");
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

    public bool GetUseMdfCache() => _useMdfCache;
    public string? GetOriginalMdfPath() => _originalMdfPath;
    
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
