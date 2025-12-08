using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.WinForms.Forms;

public class DeviceEditForm : Form
{
    private readonly AccessInventoryStore _store;
    private Device _device;

    private readonly ComboBox _typeCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _patrimonio = new();
    private readonly TextBox _marca = new();
    private readonly TextBox _modelo = new();
    private readonly TextBox _serie = new();
    private readonly TextBox _imei = new();
    private readonly TextBox _responsavel = new();
    private readonly TextBox _localizacao = new();
    private readonly TextBox _observacoes = new() { Multiline = true, Height = 60 };

    private readonly TextBox _sistema = new();
    private readonly TextBox _processador = new();
    private readonly NumericUpDown _ram = new() { Minimum = 0, Maximum = 512 };
    private readonly NumericUpDown _armazenamento = new() { Minimum = 0, Maximum = 4096 };

    private readonly TextBox _versaoAndroid = new();
    private readonly TextBox _linha = new();
    private readonly CheckBox _teclado = new() { Text = "Possui teclado" };

    private readonly TextBox _scanner = new();
    private readonly CheckBox _base = new() { Text = "Base/Carregador" };

    private readonly CheckBox _corporativo = new() { Text = "Linha corporativa" };

    public DeviceEditForm(AccessInventoryStore store, Device? device)
    {
        _store = store;
        _device = device ?? new Computer();

        Text = device == null ? "Novo dispositivo" : "Editar dispositivo";
        Width = 520;
        Height = 650;
        StartPosition = FormStartPosition.CenterParent;

        _typeCombo.DataSource = Enum.GetValues(typeof(DeviceType));
        _typeCombo.SelectedItem = _device.Type;
        _typeCombo.SelectedValueChanged += (_, _) => OnTypeChanged();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 0,
            AutoSize = true,
            Padding = new Padding(10),
            AutoScroll = true
        };

        void AddRow(string label, Control control)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 6, 0, 0) });
            layout.Controls.Add(control);
        }

        AddRow("Tipo", _typeCombo);
        AddRow("Patrimônio", _patrimonio);
        AddRow("Marca", _marca);
        AddRow("Modelo", _modelo);
        AddRow("Número de Série", _serie);
        AddRow("IMEI", _imei);
        AddRow("Responsável", _responsavel);
        AddRow("Localização", _localizacao);
        AddRow("Observações", _observacoes);

        AddRow("Sistema Operacional", _sistema);
        AddRow("Processador", _processador);
        AddRow("Memória RAM (GB)", _ram);
        AddRow("Armazenamento (GB)", _armazenamento);

        AddRow("Android", _versaoAndroid);
        AddRow("Linha telefônica", _linha);
        AddRow("Teclado (Tablet)", _teclado);

        AddRow("Scanner (Coletor)", _scanner);
        AddRow("Base (Coletor)", _base);

        AddRow("Corporativo (Celular)", _corporativo);

        var saveButton = new Button { Text = "Salvar", Dock = DockStyle.Bottom, Height = 35 };
        saveButton.Click += async (_, _) => await SaveAsync();

        Controls.Add(saveButton);
        Controls.Add(layout);

        BindFields();
        OnTypeChanged();
    }

    private void BindFields()
    {
        _patrimonio.Text = _device.Patrimonio;
        _marca.Text = _device.Marca;
        _modelo.Text = _device.Modelo;
        _serie.Text = _device.NumeroSerie;
        _imei.Text = _device.Imei ?? string.Empty;
        _responsavel.Text = _device.Responsavel;
        _localizacao.Text = _device.Localizacao;
        _observacoes.Text = _device.Observacoes ?? string.Empty;

        if (_device is Computer computer)
        {
            _sistema.Text = computer.SistemaOperacional;
            _processador.Text = computer.Processador;
            _ram.Value = computer.MemoriaRamGb;
            _armazenamento.Value = computer.ArmazenamentoGb;
        }

        if (_device is Tablet tablet)
        {
            _versaoAndroid.Text = tablet.VersaoAndroid;
            _linha.Text = tablet.LinhaTelefonica;
            _teclado.Checked = tablet.PossuiTeclado;
        }

        if (_device is ColetorAndroid coletor)
        {
            _versaoAndroid.Text = coletor.VersaoAndroid;
            _scanner.Text = coletor.FabricanteScanner;
            _base.Checked = coletor.PossuiCarregadorBase;
        }

        if (_device is Celular celular)
        {
            _versaoAndroid.Text = celular.VersaoAndroid;
            _linha.Text = celular.LinhaTelefonica;
            _corporativo.Checked = celular.Corporativo;
        }
    }

    private void OnTypeChanged()
    {
        var selected = (DeviceType)_typeCombo.SelectedItem!;
        if (_device.Type != selected && _device.Id == 0)
        {
            _device = selected switch
            {
                DeviceType.Tablet => new Tablet(),
                DeviceType.ColetorAndroid => new ColetorAndroid(),
                DeviceType.Celular => new Celular(),
                _ => new Computer()
            };
        }

        _sistema.Enabled = selected == DeviceType.Computer;
        _processador.Enabled = selected == DeviceType.Computer;
        _ram.Enabled = selected == DeviceType.Computer;
        _armazenamento.Enabled = selected == DeviceType.Computer;

        _versaoAndroid.Enabled = selected != DeviceType.Computer;
        _linha.Enabled = selected is DeviceType.Tablet or DeviceType.Celular;
        _teclado.Enabled = selected == DeviceType.Tablet;

        _scanner.Enabled = selected == DeviceType.ColetorAndroid;
        _base.Enabled = selected == DeviceType.ColetorAndroid;

        _corporativo.Enabled = selected == DeviceType.Celular;
    }

    private async Task SaveAsync()
    {
        try
        {
            _device.Patrimonio = _patrimonio.Text;
            _device.Marca = _marca.Text;
            _device.Modelo = _modelo.Text;
            _device.NumeroSerie = _serie.Text;
            _device.Responsavel = _responsavel.Text;
            _device.Localizacao = _localizacao.Text;
            _device.Observacoes = _observacoes.Text;
            _device.AtualizadoEm = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(_imei.Text))
            {
                if (!ImeiUtility.IsValid(_imei.Text))
                {
                    MessageBox.Show(this, "IMEI inválido", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _device.Imei = ImeiUtility.Normalize(_imei.Text);
            }
            else
            {
                _device.Imei = null;
            }

            switch (_device)
            {
                case Computer computer:
                    computer.SistemaOperacional = _sistema.Text;
                    computer.Processador = _processador.Text;
                    computer.MemoriaRamGb = (int)_ram.Value;
                    computer.ArmazenamentoGb = (int)_armazenamento.Value;
                    break;
                case Tablet tablet:
                    tablet.VersaoAndroid = _versaoAndroid.Text;
                    tablet.LinhaTelefonica = _linha.Text;
                    tablet.PossuiTeclado = _teclado.Checked;
                    break;
                case ColetorAndroid coletor:
                    coletor.VersaoAndroid = _versaoAndroid.Text;
                    coletor.FabricanteScanner = _scanner.Text;
                    coletor.PossuiCarregadorBase = _base.Checked;
                    break;
                case Celular celular:
                    celular.VersaoAndroid = _versaoAndroid.Text;
                    celular.LinhaTelefonica = _linha.Text;
                    celular.Corporativo = _corporativo.Checked;
                    break;
            }

            if (_device.Id == 0)
            {
                await _store.AddAsync(_device);
            }
            else
            {
                await _store.UpdateAsync(_device);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
