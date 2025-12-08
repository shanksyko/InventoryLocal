using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class ColetorEditForm : Form
    {
        private TextBox _txtHost = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtMac = null!;
        private TextBox _txtIp = null!;
        private TextBox _txtLocal = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public ColetorAndroid Result { get; private set; }

        public ColetorEditForm(ColetorAndroid? existing = null)
        {
            Text = existing == null ? "Novo Coletor Android" : "Editar Coletor Android";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                _txtHost.Text = existing.Host ?? string.Empty;
                _txtSerial.Text = existing.SerialNumber ?? string.Empty;
                _txtMac.Text = existing.MacAddress ?? string.Empty;
                _txtIp.Text = existing.IpAddress ?? string.Empty;
                _txtLocal.Text = existing.Local ?? string.Empty;

                Result = new ColetorAndroid
                {
                    Id = existing.Id,
                    Host = existing.Host ?? string.Empty,
                    SerialNumber = existing.SerialNumber ?? string.Empty,
                    MacAddress = existing.MacAddress ?? string.Empty,
                    IpAddress = existing.IpAddress ?? string.Empty,
                    Local = existing.Local ?? string.Empty
                };
            }
            else
            {
                Result = new ColetorAndroid
                {
                    Host = string.Empty,
                    SerialNumber = string.Empty,
                    MacAddress = string.Empty,
                    IpAddress = string.Empty,
                    Local = string.Empty
                };
            }
        }

        private void InitializeLayout()
        {
            var lblHost = new Label { Text = "Host:", AutoSize = true, Location = new Point(10, 15) };
            _txtHost = new TextBox { Location = new Point(130, 12), Width = 250 };

            var lblSerial = new Label { Text = "NS:", AutoSize = true, Location = new Point(10, 45) };
            _txtSerial = new TextBox { Location = new Point(130, 42), Width = 250 };

            var lblMac = new Label { Text = "MAC:", AutoSize = true, Location = new Point(10, 75) };
            _txtMac = new TextBox { Location = new Point(130, 72), Width = 250 };

            var lblIp = new Label { Text = "IP:", AutoSize = true, Location = new Point(10, 105) };
            _txtIp = new TextBox { Location = new Point(130, 102), Width = 250 };

            var lblLocal = new Label { Text = "Local:", AutoSize = true, Location = new Point(10, 135) };
            _txtLocal = new TextBox { Location = new Point(130, 132), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(220, 180), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(305, 180), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Host = _txtHost.Text.Trim();
                Result.SerialNumber = _txtSerial.Text.Trim();
                Result.MacAddress = _txtMac.Text.Trim();
                Result.IpAddress = _txtIp.Text.Trim();
                Result.Local = _txtLocal.Text.Trim();
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
                lblMac, _txtMac,
                lblIp, _txtIp,
                lblLocal, _txtLocal,
                _btnOk, _btnCancelar
            });
        }
    }
}
