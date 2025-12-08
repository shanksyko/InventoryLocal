using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class ComputerEditForm : Form
    {
        private TextBox _txtHost = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtProprietario = null!;
        private TextBox _txtDepartamento = null!;
        private TextBox _txtMatricula = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Computer Result { get; private set; }

        public ComputerEditForm(Computer? existing = null)
        {
            Text = existing == null ? "Novo Computador" : "Editar Computador";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(400, 280);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                _txtHost.Text = existing.Host ?? string.Empty;
                _txtSerial.Text = existing.SerialNumber ?? string.Empty;
                _txtProprietario.Text = existing.Proprietario ?? string.Empty;
                _txtDepartamento.Text = existing.Departamento ?? string.Empty;
                _txtMatricula.Text = existing.Matricula ?? string.Empty;

                Result = new Computer
                {
                    Id = existing.Id,
                    Host = existing.Host,
                    SerialNumber = existing.SerialNumber,
                    Proprietario = existing.Proprietario,
                    Departamento = existing.Departamento,
                    Matricula = existing.Matricula
                };
            }
            else
            {
                Result = new Computer();
            }
        }

        private void InitializeLayout()
        {
            var lblHost = new Label { Text = "Host:", AutoSize = true, Location = new Point(10, 15) };
            _txtHost = new TextBox { Location = new Point(120, 12), Width = 240 };

            var lblSerial = new Label { Text = "N/S:", AutoSize = true, Location = new Point(10, 45) };
            _txtSerial = new TextBox { Location = new Point(120, 42), Width = 240 };

            var lblProprietario = new Label { Text = "Proprietário:", AutoSize = true, Location = new Point(10, 75) };
            _txtProprietario = new TextBox { Location = new Point(120, 72), Width = 240 };

            var lblDepartamento = new Label { Text = "Departamento:", AutoSize = true, Location = new Point(10, 105) };
            _txtDepartamento = new TextBox { Location = new Point(120, 102), Width = 240 };

            var lblMatricula = new Label { Text = "Matrícula:", AutoSize = true, Location = new Point(10, 135) };
            _txtMatricula = new TextBox { Location = new Point(120, 132), Width = 240 };

            _btnOk = new Button { Text = "OK", Location = new Point(200, 180), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(285, 180), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Host = _txtHost.Text.Trim();
                Result.SerialNumber = _txtSerial.Text.Trim();
                Result.Proprietario = _txtProprietario.Text.Trim();
                Result.Departamento = _txtDepartamento.Text.Trim();
                Result.Matricula = _txtMatricula.Text.Trim();
                DialogResult = DialogResult.OK;
                Close();
            };

            _btnCancelar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                lblHost, _txtHost,
                lblSerial, _txtSerial,
                lblProprietario, _txtProprietario,
                lblDepartamento, _txtDepartamento,
                lblMatricula, _txtMatricula,
                _btnOk, _btnCancelar
            });
        }
    }
}
