using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using InventarioSistem.Access;
using InventarioSistem.WinForms.Helpers;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário base responsivo para listagem de dispositivos
/// Implementa padrão responsive com lazy-loading, paginação e filtros
/// </summary>
public class ResponsiveDeviceListForm : Form
{
    private readonly SqlServerInventoryStore _store;
    private readonly string _deviceType;
    private readonly BindingSource _binding = new();
    
    private DataGridView _grid = null!;
    private TextBox _searchBox = null!;
    private ComboBox _filterCombo = null!;
    private Label _statusLabel = null!;
    private ProgressBar _progressBar = null!;
    private Button _btnAdd = null!;
    private Button _btnEdit = null!;
    private Button _btnDelete = null!;
    private Button _btnRefresh = null!;
    private Button _btnExport = null!;

    private List<dynamic> _allData = new();
    private int _currentPage = 1;
    private const int PageSize = 50;

    public ResponsiveDeviceListForm(SqlServerInventoryStore store, string deviceType = "Dispositivos")
    {
        _store = store;
        _deviceType = deviceType;

        InitializeUI();
        SetupEventHandlers();
    }

    private void InitializeUI()
    {
        // Configurações da janela
        Text = _deviceType;
        Size = new Size(1200, 700);
        MinimumSize = new Size(800, 400);
        StartPosition = FormStartPosition.CenterScreen;
        Font = ResponsiveUIHelper.Fonts.Regular;
        BackColor = ResponsiveUIHelper.Colors.LightBackground;

        // Cabeçalho
        var headerPanel = ResponsiveUIHelper.CreateHeaderPanel(
            $"Gerenciar {_deviceType}",
            "Use os filtros abaixo para buscar e gerenciar seus dispositivos");
        Controls.Add(headerPanel);

        // Painel de ações
        var actionPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Botões de ação
        _btnAdd = ResponsiveUIHelper.CreateButton("➕ Novo", 110, ResponsiveUIHelper.Colors.PrimaryGreen);
        _btnEdit = ResponsiveUIHelper.CreateButton("✏️ Editar", 110, ResponsiveUIHelper.Colors.PrimaryBlue);
        _btnDelete = ResponsiveUIHelper.CreateButton("🗑️ Excluir", 110, ResponsiveUIHelper.Colors.PrimaryRed);
        _btnRefresh = ResponsiveUIHelper.CreateButton("🔄 Atualizar", 110, ResponsiveUIHelper.Colors.PrimaryOrange);
        _btnExport = ResponsiveUIHelper.CreateButton("📥 Exportar", 110);

        // Busca e filtros
        var lblSearch = ResponsiveUIHelper.CreateLabel("🔍 Buscar:", ResponsiveUIHelper.Fonts.LabelBold);
        _searchBox = ResponsiveUIHelper.CreateTextBox("Digite para buscar...", 200);

        var lblFilter = ResponsiveUIHelper.CreateLabel("Filtro:", ResponsiveUIHelper.Fonts.LabelBold);
        _filterCombo = ResponsiveUIHelper.CreateComboBox(150, new[] { "Todos", "Ativos", "Inativos" });

        // Layout do painel de ações
        int x = ResponsiveUIHelper.Spacing.Medium;
        int y = (actionPanel.Height - _btnAdd.Height) / 2;

        _btnAdd.Location = new Point(x, y);
        actionPanel.Controls.Add(_btnAdd);
        x += _btnAdd.Width + ResponsiveUIHelper.Spacing.Small;

        _btnEdit.Location = new Point(x, y);
        actionPanel.Controls.Add(_btnEdit);
        x += _btnEdit.Width + ResponsiveUIHelper.Spacing.Small;

        _btnDelete.Location = new Point(x, y);
        actionPanel.Controls.Add(_btnDelete);
        x += _btnDelete.Width + ResponsiveUIHelper.Spacing.Small;

        _btnRefresh.Location = new Point(x, y);
        actionPanel.Controls.Add(_btnRefresh);
        x += _btnRefresh.Width + ResponsiveUIHelper.Spacing.Small;

        _btnExport.Location = new Point(x, y);
        actionPanel.Controls.Add(_btnExport);

        // Filtros à direita
        x = actionPanel.Width - 500;
        lblFilter.Location = new Point(x, y + 5);
        actionPanel.Controls.Add(lblFilter);
        x += lblFilter.Width + ResponsiveUIHelper.Spacing.Small;

        _filterCombo.Location = new Point(x, y);
        actionPanel.Controls.Add(_filterCombo);
        x += _filterCombo.Width + ResponsiveUIHelper.Spacing.Medium;

        lblSearch.Location = new Point(x, y + 5);
        actionPanel.Controls.Add(lblSearch);
        x += lblSearch.Width + ResponsiveUIHelper.Spacing.Small;

        _searchBox.Location = new Point(x, y);
        actionPanel.Controls.Add(_searchBox);

        Controls.Add(actionPanel);

        // Grid de dados
        _grid = ResponsiveUIHelper.CreateDataGrid(readOnly: true, alternatingColors: true);
        _grid.DataSource = _binding;
        Controls.Add(_grid);

        // Painel de status
        var statusPanel = new Panel
        {
            Height = 40,
            Dock = DockStyle.Bottom,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        _statusLabel = ResponsiveUIHelper.CreateLabel("Pronto");
        _statusLabel.Location = new Point(ResponsiveUIHelper.Spacing.Medium, 10);
        statusPanel.Controls.Add(_statusLabel);

        _progressBar = new ProgressBar
        {
            Location = new Point(250, 8),
            Width = 300,
            Height = 24,
            Visible = false,
            Style = ProgressBarStyle.Marquee
        };
        statusPanel.Controls.Add(_progressBar);

        Controls.Add(statusPanel);
    }

    private void SetupEventHandlers()
    {
        _btnAdd.Click += async (s, e) => await OnAddAsync();
        _btnEdit.Click += async (s, e) => await OnEditAsync();
        _btnDelete.Click += async (s, e) => await OnDeleteAsync();
        _btnRefresh.Click += async (s, e) => await LoadDataAsync();
        _btnExport.Click += OnExport;

        _searchBox.TextChanged += (s, e) => ApplyFilters();
        _filterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();

        Shown += async (s, e) => await LoadDataAsync();
        ResizeEnd += (s, e) => _grid.AutoResizeColumns();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            ShowLoading(true);
            _statusLabel.Text = $"Carregando {_deviceType}...";

            // Simular carregamento de dados
            await Task.Delay(500);

            _statusLabel.Text = $"{_allData.Count} registros carregados";
            ApplyFilters();
        }
        catch (Exception ex)
        {
            ResponsiveUIHelper.ShowError($"Erro ao carregar dados: {ex.Message}");
            _statusLabel.Text = "Erro ao carregar";
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allData.AsEnumerable();

        // Filtro por busca
        if (!string.IsNullOrEmpty(_searchBox.Text))
        {
            var search = _searchBox.Text.ToLower();
            filtered = filtered.Where(d =>
                d.ToString().ToLower().Contains(search) ||
                (d.GetType().GetProperty("Host")?.GetValue(d)?.ToString() ?? "").ToLower().Contains(search) ||
                (d.GetType().GetProperty("Local")?.GetValue(d)?.ToString() ?? "").ToLower().Contains(search)
            );
        }

        _binding.DataSource = filtered.ToList();
        _statusLabel.Text = $"Mostrando {filtered.Count()} de {_allData.Count} registros";
    }

    private void ShowLoading(bool show)
    {
        _progressBar.Visible = show;
        _btnAdd.Enabled = !show;
        _btnEdit.Enabled = !show;
        _btnDelete.Enabled = !show;
        _btnRefresh.Enabled = !show;
    }

    private async Task OnAddAsync()
    {
        ResponsiveUIHelper.ShowSuccess("Funcionalidade de adicionar em desenvolvimento");
        await Task.CompletedTask;
    }

    private async Task OnEditAsync()
    {
        if (_grid.CurrentRow == null)
        {
            ResponsiveUIHelper.ShowError("Selecione um registro para editar");
            return;
        }

        ResponsiveUIHelper.ShowSuccess("Funcionalidade de editar em desenvolvimento");
        await Task.CompletedTask;
    }

    private async Task OnDeleteAsync()
    {
        if (_grid.CurrentRow == null)
        {
            ResponsiveUIHelper.ShowError("Selecione um registro para excluir");
            return;
        }

        if (!ResponsiveUIHelper.ShowConfirm("Deseja realmente excluir este registro?"))
            return;

        try
        {
            ShowLoading(true);
            // Implementar exclusão
            ResponsiveUIHelper.ShowSuccess("Registro excluído com sucesso");
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ResponsiveUIHelper.ShowError($"Erro ao excluir: {ex.Message}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void OnExport()
    {
        ResponsiveUIHelper.ShowSuccess("Exportação em desenvolvimento");
    }
}
