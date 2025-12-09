using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class CelularEditForm : Form
    {
        private TextBox _txtHostname = null!;
        private TextBox _txtModelo = null!;
        private TextBox _txtNumero = null!;
        private TextBox _txtProprietario = null!;
        private TextBox _txtImei1 = null!;
        private TextBox _txtImei2 = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Celular Result { get; private set; }

        public CelularEditForm(Celular? existing = null)
        {
            Text = existing == null ? "Novo Celular" : "Editar Celular";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                _txtHostname.Text = existing.Hostname ?? string.Empty;
                _txtModelo.Text = existing.Modelo ?? string.Empty;
                _txtNumero.Text = existing.Numero ?? string.Empty;
                _txtProprietario.Text = existing.Proprietario ?? string.Empty;
                _txtImei1.Text = existing.Imei1 ?? string.Empty;
                _txtImei2.Text = existing.Imei2 ?? string.Empty;

                Result = new Celular
                {
                    Id = existing.Id,
                    Hostname = existing.Hostname ?? string.Empty,
                    Modelo = existing.Modelo ?? string.Empty,
                    Numero = existing.Numero ?? string.Empty,
                    Proprietario = existing.Proprietario ?? string.Empty,
                    Imei1 = existing.Imei1 ?? string.Empty,
                    Imei2 = existing.Imei2 ?? string.Empty
                };
            }
            else
            {
                Result = new Celular
                {
                    Hostname = string.Empty,
                    Modelo = string.Empty,
                    Numero = string.Empty,
                    Proprietario = string.Empty,
                    Imei1 = string.Empty,
                    Imei2 = string.Empty
                };
            }
        }

        private void InitializeLayout()
        {
            var lblHostname = new Label { Text = "Hostname:", AutoSize = true, Location = new Point(10, 15) };
            _txtHostname = new TextBox { Location = new Point(130, 12), Width = 250 };

            var lblModelo = new Label { Text = "Modelo:", AutoSize = true, Location = new Point(10, 45) };
            _txtModelo = new TextBox { Location = new Point(130, 42), Width = 250 };

            var lblNumero = new Label { Text = "Número:", AutoSize = true, Location = new Point(10, 75) };
            _txtNumero = new TextBox { Location = new Point(130, 72), Width = 250 };

            var lblProprietario = new Label { Text = "Proprietário:", AutoSize = true, Location = new Point(10, 105) };
            _txtProprietario = new TextBox { Location = new Point(130, 102), Width = 250 };

            var lblImei1 = new Label { Text = "IMEI 1:", AutoSize = true, Location = new Point(10, 135) };
            _txtImei1 = new TextBox { Location = new Point(130, 132), Width = 250 };

            var lblImei2 = new Label { Text = "IMEI 2:", AutoSize = true, Location = new Point(10, 165) };
            _txtImei2 = new TextBox { Location = new Point(130, 162), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(220, 210), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(305, 210), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Hostname = _txtHostname.Text.Trim();
                Result.Modelo = _txtModelo.Text.Trim();
                Result.Numero = _txtNumero.Text.Trim();
                Result.Proprietario = _txtProprietario.Text.Trim();
                Result.Imei1 = _txtImei1.Text.Trim();
                Result.Imei2 = _txtImei2.Text.Trim();
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
                lblHostname, _txtHostname,
                lblModelo, _txtModelo,
                lblNumero, _txtNumero,
                lblProprietario, _txtProprietario,
                lblImei1, _txtImei1,
                lblImei2, _txtImei2,
                _btnOk, _btnCancelar
            });
        }
    }
}
