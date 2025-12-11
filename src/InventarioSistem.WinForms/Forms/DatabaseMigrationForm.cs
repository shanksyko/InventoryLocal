using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access.DataMigration;
using InventarioSistem.WinForms.Helpers;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário para migração de dados entre bancos de dados
/// </summary>
public class DatabaseMigrationForm : Form
{
    private Label _lblStatus = null!;
    private ProgressBar _progressBar = null!;
    private RichTextBox _rtbLog = null!;
    private Button _btnStartMigration = null!;
    private Button _btnClose = null!;

    private string _sourceConnectionString = string.Empty;
    private string _targetConnectionString = string.Empty;
    private bool _isMigrating = false;

    public DatabaseMigrationForm(string sourceConnStr, string targetConnStr)
    {
        _sourceConnectionString = sourceConnStr;
        _targetConnectionString = targetConnStr;

        Text = "🔄 Migração de Dados";
        Size = new Size(700, 600);
        MinimumSize = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ResponsiveUIHelper.Colors.LightBackground;
        Font = ResponsiveUIHelper.Fonts.Regular;
        FormBorderStyle = FormBorderStyle.Sizable;

        InitializeUI();
    }

    private void InitializeUI()
    {
        // Header
        var headerPanel = ResponsiveUIHelper.CreateHeaderPanel(
            "🔄 Migração de Dados",
            "Migre dados de um banco para outro com segurança"
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

        // Info box
        var infoBox = ResponsiveUIHelper.CreateCard(600, 80);
        infoBox.Location = new Point(ResponsiveUIHelper.Spacing.Medium, ResponsiveUIHelper.Spacing.Medium);

        var lblInfo = new Label
        {
            Text = "⚠️  ATENÇÃO: Esta operação irá:\n" +
                   "1. Verificar integridade dos dados\n" +
                   "2. Migrar todas as tabelas\n" +
                   "3. Validar dados no destino",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Medium),
            Font = ResponsiveUIHelper.Fonts.Small,
            ForeColor = ResponsiveUIHelper.Colors.PrimaryOrange
        };
        infoBox.Controls.Add(lblInfo);
        mainPanel.Controls.Add(infoBox);

        int y = 110;

        // Progress bar
        _progressBar = new ProgressBar
        {
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Size = new Size(600, 25),
            Style = ProgressBarStyle.Continuous,
            Visible = false
        };
        mainPanel.Controls.Add(_progressBar);

        y += 40;

        // Status label
        _lblStatus = ResponsiveUIHelper.CreateLabel("Pronto para iniciar...", ResponsiveUIHelper.Fonts.Regular);
        _lblStatus.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(_lblStatus);

        y += 30;

        // Log box
        var lblLog = ResponsiveUIHelper.CreateLabel("Log de Operação:", ResponsiveUIHelper.Fonts.LabelBold);
        lblLog.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y);
        mainPanel.Controls.Add(lblLog);

        y += 25;

        _rtbLog = new RichTextBox
        {
            Location = new Point(ResponsiveUIHelper.Spacing.Medium, y),
            Size = new Size(600, 300),
            ReadOnly = true,
            BackColor = Color.White,
            ForeColor = ResponsiveUIHelper.Colors.TextDark,
            Font = new Font("Courier New", 9)
        };
        mainPanel.Controls.Add(_rtbLog);

        Controls.Add(mainPanel);

        // Footer com botões
        var buttonPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnStartMigration = ResponsiveUIHelper.CreateButton("▶️  Iniciar Migração", 150, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnStartMigration.Click += OnStartMigration;

        _btnClose = ResponsiveUIHelper.CreateButton("✖️  Fechar", 100, ResponsiveUIHelper.Colors.PrimaryRed);
        _btnClose.Click += (s, e) => Close();

        int x = buttonPanel.Width - 270;
        int btnY = (buttonPanel.Height - _btnStartMigration.Height) / 2;

        _btnStartMigration.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnStartMigration);

        x += _btnStartMigration.Width + ResponsiveUIHelper.Spacing.Medium;
        _btnClose.Location = new Point(x, btnY);
        buttonPanel.Controls.Add(_btnClose);

        Controls.Add(buttonPanel);

        AddLog("✅ Formulário de migração carregado");
        AddLog("Clique em 'Iniciar Migração' para começar");
    }

    private async void OnStartMigration(object? sender, EventArgs e)
    {
        if (_isMigrating)
            return;

        _isMigrating = true;
        _btnStartMigration.Enabled = false;
        _progressBar.Visible = true;
        _progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            AddLog("\n═══════════════════════════════════════════════════════════");
            AddLog("🔄 INICIANDO MIGRAÇÃO DE DADOS");
            AddLog("═══════════════════════════════════════════════════════════\n");

            var progress = new Progress<string>(message =>
            {
                AddLog(message);
                this.Invoke(() => _lblStatus.Text = message);
            });

            var result = await DatabaseMigrator.MigrateAsync(
                _sourceConnectionString,
                _targetConnectionString,
                progress);

            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.Value = 100;

            AddLog("\n═══════════════════════════════════════════════════════════");
            if (result.Success)
            {
                AddLog($"✅ SUCESSO!", Color.Green);
                AddLog($"Tabelas: {result.TablesCount}");
                AddLog($"Linhas migradas: {result.RowsMigrated}");
                AddLog($"Tempo: {result.Duration.TotalSeconds:F2}s");
                AddLog("═══════════════════════════════════════════════════════════\n");

                if (result.Warnings.Count > 0)
                {
                    AddLog("⚠️  AVISO:", Color.Orange);
                    foreach (var warning in result.Warnings)
                    {
                        AddLog(warning, Color.Orange);
                    }
                }

                MessageBox.Show(
                    $"✅ Migração concluída com sucesso!\n\n" +
                    $"Tabelas: {result.TablesCount}\n" +
                    $"Linhas migradas: {result.RowsMigrated}\n" +
                    $"Tempo: {result.Duration.TotalSeconds:F2}s",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                AddLog($"❌ ERRO: {result.Message}", Color.Red);
                AddLog("═══════════════════════════════════════════════════════════\n", Color.Red);

                MessageBox.Show(
                    $"❌ Erro na migração:\n\n{result.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"\n❌ ERRO NÃO TRATADO: {ex.Message}", Color.Red);
            AddLog(ex.StackTrace ?? "", Color.Red);

            MessageBox.Show(
                $"❌ Erro inesperado:\n\n{ex.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _isMigrating = false;
            _btnStartMigration.Enabled = true;
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
            _rtbLog.AppendText($"{message}\n");
            _rtbLog.ScrollToCaret();
        });
    }
}
