using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.WinForms.Helpers;

namespace InventarioSistem.WinForms.Forms;

/// <summary>
/// Formulário base responsivo para edição de itens
/// Implementa padrão responsivo com validação e layout adaptativo
/// </summary>
public class ResponsiveEditForm : Form
{
    protected Panel FormPanel = null!;
    protected FlowLayoutPanel FieldsPanel = null!;
    protected Button BtnSave = null!;
    protected Button BtnCancel = null!;
    protected Label StatusLabel = null!;

    public ResponsiveEditForm(string title, string subtitle = "")
    {
        Text = title;
        Size = new Size(600, 500);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        Font = ResponsiveUIHelper.Fonts.Regular;
        BackColor = ResponsiveUIHelper.Colors.LightBackground;
        FormBorderStyle = FormBorderStyle.Sizable;

        InitializeLayout(title, subtitle);
    }

    private void InitializeLayout(string title, string subtitle)
    {
        // Cabeçalho
        var headerPanel = ResponsiveUIHelper.CreateHeaderPanel(title, subtitle);
        Controls.Add(headerPanel);

        // Painel principal com scroll
        FormPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ResponsiveUIHelper.Colors.LightBackground
        };

        // Painel de campos
        FieldsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(ResponsiveUIHelper.Spacing.Large),
            BackColor = ResponsiveUIHelper.Colors.LightBackground
        };

        FormPanel.Controls.Add(FieldsPanel);
        Controls.Add(FormPanel);

        // Painel de botões
        var buttonPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Bottom,
            BackColor = ResponsiveUIHelper.Colors.CardBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        BtnSave = ResponsiveUIHelper.CreateButton("💾 Salvar", 120, ResponsiveUIHelper.Colors.PrimaryGreen);
        BtnCancel = ResponsiveUIHelper.CreateButton("❌ Cancelar", 120, ResponsiveUIHelper.Colors.PrimaryRed);

        int x = buttonPanel.Width - 260;
        int y = (buttonPanel.Height - BtnSave.Height) / 2;

        BtnSave.Location = new Point(x, y);
        buttonPanel.Controls.Add(BtnSave);
        x += BtnSave.Width + ResponsiveUIHelper.Spacing.Medium;

        BtnCancel.Location = new Point(x, y);
        buttonPanel.Controls.Add(BtnCancel);

        // Label de status
        StatusLabel = ResponsiveUIHelper.CreateLabel("", ResponsiveUIHelper.Fonts.Small);
        StatusLabel.Location = new Point(ResponsiveUIHelper.Spacing.Medium, y + 5);
        StatusLabel.ForeColor = ResponsiveUIHelper.Colors.TextLight;
        buttonPanel.Controls.Add(StatusLabel);

        Controls.Add(buttonPanel);

        BtnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    /// Adiciona um campo de texto responsivo
    /// </summary>
    protected TextBox AddTextField(string label, string placeholder = "", bool required = false, int width = 500)
    {
        var container = CreateFieldContainer(label, required);
        
        var textBox = ResponsiveUIHelper.CreateTextBox(placeholder, width, multiline: false);
        container.Controls.Add(textBox);
        
        FieldsPanel.Controls.Add(container);
        
        return textBox;
    }

    /// <summary>
    /// Adiciona um campo de múltiplas linhas
    /// </summary>
    protected TextBox AddTextAreaField(string label, string placeholder = "", bool required = false, int width = 500, int height = 100)
    {
        var container = CreateFieldContainer(label, required);
        
        var textBox = ResponsiveUIHelper.CreateTextBox(placeholder, width, multiline: true);
        textBox.Height = height;
        container.Controls.Add(textBox);
        
        FieldsPanel.Controls.Add(container);
        
        return textBox;
    }

    /// <summary>
    /// Adiciona um campo de seleção (ComboBox)
    /// </summary>
    protected ComboBox AddComboField(string label, string[] items, bool required = false, int width = 500)
    {
        var container = CreateFieldContainer(label, required);
        
        var comboBox = ResponsiveUIHelper.CreateComboBox(width, items);
        container.Controls.Add(comboBox);
        
        FieldsPanel.Controls.Add(container);
        
        return comboBox;
    }

    /// <summary>
    /// Adiciona um checkbox
    /// </summary>
    protected CheckBox AddCheckField(string label)
    {
        var checkBox = ResponsiveUIHelper.CreateCheckBox(label);
        FieldsPanel.Controls.Add(checkBox);
        return checkBox;
    }

    /// <summary>
    /// Adiciona um campo de data
    /// </summary>
    protected DateTimePicker AddDateField(string label, bool required = false)
    {
        var container = CreateFieldContainer(label, required);
        
        var datePicker = new DateTimePicker
        {
            Width = 500,
            Font = ResponsiveUIHelper.Fonts.Regular,
            Format = DateTimePickerFormat.Short
        };
        container.Controls.Add(datePicker);
        
        FieldsPanel.Controls.Add(container);
        
        return datePicker;
    }

    /// <summary>
    /// Adiciona um campo numérico
    /// </summary>
    protected NumericUpDown AddNumericField(string label, decimal min = 0, decimal max = 100, decimal value = 0, bool required = false)
    {
        var container = CreateFieldContainer(label, required);
        
        var numeric = new NumericUpDown
        {
            Width = 500,
            Font = ResponsiveUIHelper.Fonts.Regular,
            Minimum = min,
            Maximum = max,
            Value = value
        };
        container.Controls.Add(numeric);
        
        FieldsPanel.Controls.Add(container);
        
        return numeric;
    }

    /// <summary>
    /// Cria um container para um campo com label
    /// </summary>
    private Panel CreateFieldContainer(string label, bool required = false)
    {
        var container = new Panel
        {
            Height = 70,
            Width = FieldsPanel.Width - (ResponsiveUIHelper.Spacing.Large * 2),
            BackColor = ResponsiveUIHelper.Colors.LightBackground,
            Margin = new Padding(0, ResponsiveUIHelper.Spacing.Small, 0, ResponsiveUIHelper.Spacing.Small)
        };

        var labelText = required ? $"{label} *" : label;
        var lblField = ResponsiveUIHelper.CreateLabel(labelText, ResponsiveUIHelper.Fonts.LabelBold);
        lblField.Location = new Point(0, 0);
        container.Controls.Add(lblField);

        return container;
    }

    /// <summary>
    /// Mostra mensagem de status
    /// </summary>
    protected void SetStatus(string message, Color? color = null)
    {
        StatusLabel.Text = message;
        StatusLabel.ForeColor = color ?? ResponsiveUIHelper.Colors.TextLight;
    }

    /// <summary>
    /// Limpa todos os campos
    /// </summary>
    protected void ClearFields()
    {
        foreach (Control ctrl in FieldsPanel.Controls)
        {
            if (ctrl is TextBox txt) txt.Clear();
            if (ctrl is ComboBox combo) combo.SelectedIndex = -1;
            if (ctrl is CheckBox chk) chk.Checked = false;
            if (ctrl is DateTimePicker dtp) dtp.Value = DateTime.Now;
            if (ctrl is NumericUpDown num) num.Value = num.Minimum;
        }
    }

    /// <summary>
    /// Valida se todos os campos obrigatórios estão preenchidos
    /// </summary>
    protected bool ValidateRequired()
    {
        var emptyFields = new System.Collections.Generic.List<string>();

        foreach (Control ctrl in FieldsPanel.Controls)
        {
            if (ctrl is Panel container && container.Controls.Count > 0)
            {
                var label = container.Controls[0] as Label;
                if (label?.Text.EndsWith("*") == true)
                {
                    var field = container.Controls.Count > 1 ? container.Controls[1] : null;
                    if (field is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
                        emptyFields.Add(label.Text.Replace(" *", ""));
                    else if (field is ComboBox combo && combo.SelectedIndex < 0)
                        emptyFields.Add(label.Text.Replace(" *", ""));
                }
            }
        }

        if (emptyFields.Count > 0)
        {
            ResponsiveUIHelper.ShowError($"Campos obrigatórios não preenchidos:\n\n{string.Join("\n", emptyFields)}");
            return false;
        }

        return true;
    }
}

/// <summary>
/// Exemplo de formulário de edição responsivo para Computer
/// </summary>
public class ResponsiveComputerEditForm : ResponsiveEditForm
{
    private TextBox _txtHost = null!;
    private TextBox _txtSerialNumber = null!;
    private TextBox _txtProprietario = null!;
    private TextBox _txtDepartamento = null!;
    private TextBox _txtMatricula = null!;

    public ResponsiveComputerEditForm() : base("Editar Computador", "Gerenciar informações do computador")
    {
        InitializeFields();
        SetupSaveButton();
    }

    private void InitializeFields()
    {
        _txtHost = AddTextField("Host", "Digite o hostname", required: true);
        _txtSerialNumber = AddTextField("Número Serial", "Digite o número serial", required: true);
        _txtProprietario = AddTextField("Proprietário", "Nome do proprietário", required: true);
        _txtDepartamento = AddTextField("Departamento", "Digite o departamento", required: true);
        _txtMatricula = AddTextField("Matrícula", "Digite a matrícula", required: false);
    }

    private void SetupSaveButton()
    {
        BtnSave.Click += async (s, e) =>
        {
            if (!ValidateRequired())
                return;

            try
            {
                SetStatus("Salvando...", ResponsiveUIHelper.Colors.PrimaryOrange);
                BtnSave.Enabled = false;

                // Aqui você implementaria a lógica de salvamento
                await System.Threading.Tasks.Task.Delay(500);

                SetStatus("Salvo com sucesso!", ResponsiveUIHelper.Colors.PrimaryGreen);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                SetStatus($"Erro: {ex.Message}", ResponsiveUIHelper.Colors.PrimaryRed);
            }
            finally
            {
                BtnSave.Enabled = true;
            }
        };
    }

    public string Host => _txtHost.Text;
    public string SerialNumber => _txtSerialNumber.Text;
    public string Proprietario => _txtProprietario.Text;
    public string Departamento => _txtDepartamento.Text;
    public string Matricula => _txtMatricula.Text;
}
