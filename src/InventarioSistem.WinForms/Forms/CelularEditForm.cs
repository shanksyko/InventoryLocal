using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class CelularEditForm : Form
    {
        private TextBox _txtCellName = null!;
        private TextBox _txtImei1 = null!;
        private TextBox _txtImei2 = null!;
        private TextBox _txtModelo = null!;
        private TextBox _txtNumero = null!;
        private CheckBox _chkRoaming = null!;
        private TextBox _txtUsuario = null!;
        private TextBox _txtMatricula = null!;
        private TextBox _txtCargo = null!;
        private TextBox _txtSetor = null!;
        private TextBox _txtEmail = null!;
        private TextBox _txtSenha = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Celular Result { get; private set; }

        public CelularEditForm(Celular? existing = null)
        {
            Text = existing == null ? "Novo Celular" : "Editar Celular";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                BindFromModel(existing);
                Result = new Celular
                {
                    Id = existing.Id,
                    CellName = existing.CellName ?? string.Empty,
                    Imei1 = existing.Imei1 ?? string.Empty,
                    Imei2 = existing.Imei2 ?? string.Empty,
                    Modelo = existing.Modelo ?? string.Empty,
                    Numero = existing.Numero ?? string.Empty,
                    Roaming = existing.Roaming,
                    Usuario = existing.Usuario ?? string.Empty,
                    Matricula = existing.Matricula ?? string.Empty,
                    Cargo = existing.Cargo ?? string.Empty,
                    Setor = existing.Setor ?? string.Empty,
                    Email = existing.Email ?? string.Empty,
                    Senha = existing.Senha ?? string.Empty
                };
            }
            else
            {
                Result = new Celular();
            }
        }

        private void InitializeLayout()
        {
            var layout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 12,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(10, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _txtCellName = AddLabeledTextBox(layout, "CellName:", 0);
            _txtImei1 = AddLabeledTextBox(layout, "IMEI 1:", 1);
            _txtImei2 = AddLabeledTextBox(layout, "IMEI 2:", 2);
            _txtModelo = AddLabeledTextBox(layout, "Modelo:", 3);
            _txtNumero = AddLabeledTextBox(layout, "Número:", 4);
            _chkRoaming = AddLabeledCheckBox(layout, "Roaming:", 5);
            _txtUsuario = AddLabeledTextBox(layout, "Usuário:", 6);
            _txtMatricula = AddLabeledTextBox(layout, "Matrícula:", 7);
            _txtCargo = AddLabeledTextBox(layout, "Cargo:", 8);
            _txtSetor = AddLabeledTextBox(layout, "Setor:", 9);
            _txtEmail = AddLabeledTextBox(layout, "E-mail:", 10);
            _txtSenha = AddLabeledTextBox(layout, "Senha:", 11, usePasswordChar: true);

            _btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            _btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };

            _btnOk.Click += (_, _) =>
            {
                BindToModel(Result);
                DialogResult = DialogResult.OK;
                Close();
            };

            _btnCancelar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10),
                AutoSize = true
            };
            buttonsPanel.Controls.Add(_btnCancelar);
            buttonsPanel.Controls.Add(_btnOk);

            Controls.Add(layout);
            Controls.Add(buttonsPanel);
        }

        private TextBox AddLabeledTextBox(TableLayoutPanel panel, string labelText, int rowIndex, bool usePasswordChar = false)
        {
            var label = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
            var textBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 320 };
            if (usePasswordChar)
            {
                textBox.UseSystemPasswordChar = true;
            }

            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label, 0, rowIndex);
            panel.Controls.Add(textBox, 1, rowIndex);
            return textBox;
        }

        private CheckBox AddLabeledCheckBox(TableLayoutPanel panel, string labelText, int rowIndex)
        {
            var label = new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
            var checkBox = new CheckBox { Anchor = AnchorStyles.Left, AutoSize = true };

            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label, 0, rowIndex);
            panel.Controls.Add(checkBox, 1, rowIndex);
            return checkBox;
        }

        private void BindFromModel(Celular celular)
        {
            _txtCellName.Text = celular.CellName;
            _txtImei1.Text = celular.Imei1;
            _txtImei2.Text = celular.Imei2;
            _txtModelo.Text = celular.Modelo;
            _txtNumero.Text = celular.Numero;
            _chkRoaming.Checked = celular.Roaming;
            _txtUsuario.Text = celular.Usuario;
            _txtMatricula.Text = celular.Matricula;
            _txtCargo.Text = celular.Cargo;
            _txtSetor.Text = celular.Setor;
            _txtEmail.Text = celular.Email;
            _txtSenha.Text = celular.Senha;
        }

        private void BindToModel(Celular celular)
        {
            celular.CellName = _txtCellName.Text.Trim();
            celular.Imei1 = _txtImei1.Text.Trim();
            celular.Imei2 = _txtImei2.Text.Trim();
            celular.Modelo = _txtModelo.Text.Trim();
            celular.Numero = _txtNumero.Text.Trim();
            celular.Roaming = _chkRoaming.Checked;
            celular.Usuario = _txtUsuario.Text.Trim();
            celular.Matricula = _txtMatricula.Text.Trim();
            celular.Cargo = _txtCargo.Text.Trim();
            celular.Setor = _txtSetor.Text.Trim();
            celular.Email = _txtEmail.Text.Trim();
            celular.Senha = _txtSenha.Text.Trim();
        }
    }
}
