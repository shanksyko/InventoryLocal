using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms.Forms;

public class MainForm : Form
{
    private AccessInventoryStore _store;

    public MainForm(AccessInventoryStore store)
    {
        _store = store;
        Text = "Inventário de Dispositivos";
        Width = 420;
        Height = 240;
        StartPosition = FormStartPosition.CenterScreen;

        var listButton = new Button { Text = "Listar/Cadastrar", Dock = DockStyle.Top, Height = 40 };
        listButton.Click += (_, _) => new DeviceListForm(_store).ShowDialog(this);

        var reportsButton = new Button { Text = "Relatórios", Dock = DockStyle.Top, Height = 40 };
        reportsButton.Click += (_, _) => new ReportsForm(_store).ShowDialog(this);

        var exportButton = new Button { Text = "Exportar CSV", Dock = DockStyle.Top, Height = 40 };
        exportButton.Click += (_, _) => CsvExporter.ExportWithDialog(_store, this);

        var selectDbButton = new Button { Text = "Banco Access...", Dock = DockStyle.Top, Height = 40 };
        selectDbButton.Click += (_, _) =>
        {
            using var menu = new ContextMenuStrip();
            menu.Items.Add("Selecionar existente", null, (_, _) => SelecionarBancoExistenteWinForms());
            menu.Items.Add("Criar novo a partir do modelo", null, (_, _) => CriarNovoBancoWinForms());
            menu.Show(selectDbButton, new Point(0, selectDbButton.Height));
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            WrapContents = false
        };

        panel.Controls.Add(selectDbButton);
        panel.Controls.Add(listButton);
        panel.Controls.Add(reportsButton);
        panel.Controls.Add(exportButton);

        Controls.Add(panel);
    }

    private void SelecionarBancoExistenteWinForms()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Access DB (*.accdb)|*.accdb",
            Title = "Selecione um banco Access existente"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                RefreshStore(dialog.FileName);

                var result = MessageBox.Show(
                    "Banco selecionado com sucesso.\n\nDeseja exibir um resumo deste banco agora?",
                    "Banco Access",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var summary = AccessDatabaseManager.GetDatabaseSummary(dialog.FileName);
                    MessageBox.Show(summary, "Resumo do banco", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao selecionar banco: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void CriarNovoBancoWinForms()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Access DB (*.accdb)|*.accdb",
            Title = "Escolha onde salvar o novo banco Access"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var createdPath = AccessDatabaseManager.CreateNewDatabase(dialog.FileName);
                RefreshStore(createdPath);

                var result = MessageBox.Show(
                    "Novo banco criado com sucesso.\n\nDeseja exibir um resumo deste banco agora?",
                    "Banco Access",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var summary = AccessDatabaseManager.GetDatabaseSummary(createdPath);
                    MessageBox.Show(summary, "Resumo do banco", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao criar banco: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void RefreshStore(string databasePath)
    {
        AccessDatabaseManager.SetActiveDatabasePath(databasePath);
        var factory = new AccessConnectionFactory(databasePath);
        _store = new AccessInventoryStore(factory);
        _store.EnsureSchemaAsync().GetAwaiter().GetResult();
    }
}
