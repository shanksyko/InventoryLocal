using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class NobreakEditForm : Form
    {
        private TextBox _txtHostname = null!;
        private TextBox _txtLocal = null!;
        private TextBox _txtIp = null!;
        private TextBox _txtModelo = null!;
        private TextBox _txtStatus = null!;
        private TextBox _txtSerial = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Nobreak Nobreak { get; private set; }

        public NobreakEditForm(Nobreak? existing = null)
        {
            Text = existing == null ? "Novo Nobreak" : "Editar Nobreak";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(430, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Nobreak = existing == null
                ? new Nobreak()
                : new Nobreak
                {
                    Id = existing.Id,
                    Hostname = existing.Hostname,
                    Local = existing.Local,
                    IpAddress = existing.IpAddress,
                    Modelo = existing.Modelo,
                    Status = existing.Status,
                    SerialNumber = existing.SerialNumber
                };

            InitializeLayout();

            if (existing != null)
            {
                _txtHostname.Text = existing.Hostname;
                _txtLocal.Text = existing.Local;
                _txtIp.Text = existing.IpAddress;
                _txtModelo.Text = existing.Modelo;
                _txtStatus.Text = existing.Status;
                _txtSerial.Text = existing.SerialNumber;
            }
        }

        private void InitializeLayout()
        {
            var lblHostname = new Label { Text = "Hostname:", AutoSize = true, Location = new Point(10, 15) };
            _txtHostname = new TextBox { Location = new Point(150, 12), Width = 250 };

            var lblLocal = new Label { Text = "Local:", AutoSize = true, Location = new Point(10, 45) };
            _txtLocal = new TextBox { Location = new Point(150, 42), Width = 250 };

            var lblIp = new Label { Text = "IP:", AutoSize = true, Location = new Point(10, 75) };
            _txtIp = new TextBox { Location = new Point(150, 72), Width = 250 };

            var lblModelo = new Label { Text = "Modelo:", AutoSize = true, Location = new Point(10, 105) };
            _txtModelo = new TextBox { Location = new Point(150, 102), Width = 250 };

            var lblStatus = new Label { Text = "Status:", AutoSize = true, Location = new Point(10, 135) };
            _txtStatus = new TextBox { Location = new Point(150, 132), Width = 250 };

            var lblSerial = new Label { Text = "SerialNumber:", AutoSize = true, Location = new Point(10, 165) };
            _txtSerial = new TextBox { Location = new Point(150, 162), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(230, 230), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(315, 230), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Nobreak.Hostname = _txtHostname.Text.Trim();
                Nobreak.Local = _txtLocal.Text.Trim();
                Nobreak.IpAddress = _txtIp.Text.Trim();
                Nobreak.Modelo = _txtModelo.Text.Trim();
                Nobreak.Status = _txtStatus.Text.Trim();
                Nobreak.SerialNumber = _txtSerial.Text.Trim();
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
                lblLocal, _txtLocal,
                lblIp, _txtIp,
                lblModelo, _txtModelo,
                lblStatus, _txtStatus,
                lblSerial, _txtSerial,
                _btnOk, _btnCancelar
            });
        }
    }
}
