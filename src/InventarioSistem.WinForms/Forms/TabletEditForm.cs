using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class TabletEditForm : Form
    {
        private TextBox _txtHost = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtLocal = null!;
        private TextBox _txtResponsavel = null!;
        private TextBox _txtImeis = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Tablet Result { get; private set; }

        public TabletEditForm(Tablet? existing = null)
        {
            Text = existing == null ? "Novo Tablet" : "Editar Tablet";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                _txtHost.Text = existing.Host ?? string.Empty;
                _txtSerial.Text = existing.SerialNumber ?? string.Empty;
                _txtLocal.Text = existing.Local ?? string.Empty;
                _txtResponsavel.Text = existing.Responsavel ?? string.Empty;
                _txtImeis.Text = existing.Imeis != null ? string.Join(";", existing.Imeis) : string.Empty;

                Result = new Tablet
                {
                    Id = existing.Id,
                    Host = existing.Host,
                    SerialNumber = existing.SerialNumber,
                    Local = existing.Local,
                    Responsavel = existing.Responsavel,
                    Imeis = existing.Imeis?.ToList() ?? new()
                };
            }
            else
            {
                Result = new Tablet { Imeis = new() };
            }
        }

        private void InitializeLayout()
        {
            var lblHost = new Label { Text = "Host:", AutoSize = true, Location = new Point(10, 15) };
            _txtHost = new TextBox { Location = new Point(130, 12), Width = 250 };

            var lblSerial = new Label { Text = "N/S:", AutoSize = true, Location = new Point(10, 45) };
            _txtSerial = new TextBox { Location = new Point(130, 42), Width = 250 };

            var lblLocal = new Label { Text = "Local:", AutoSize = true, Location = new Point(10, 75) };
            _txtLocal = new TextBox { Location = new Point(130, 72), Width = 250 };

            var lblResponsavel = new Label { Text = "Responsável:", AutoSize = true, Location = new Point(10, 105) };
            _txtResponsavel = new TextBox { Location = new Point(130, 102), Width = 250 };

            var lblImeis = new Label { Text = "IMEIs (; separados):", AutoSize = true, Location = new Point(10, 135) };
            _txtImeis = new TextBox { Location = new Point(130, 132), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(220, 200), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(305, 200), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Host = _txtHost.Text.Trim();
                Result.SerialNumber = _txtSerial.Text.Trim();
                Result.Local = _txtLocal.Text.Trim();
                Result.Responsavel = _txtResponsavel.Text.Trim();
                var imeis = (_txtImeis.Text ?? string.Empty)
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                Result.Imeis = imeis;
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
                lblLocal, _txtLocal,
                lblResponsavel, _txtResponsavel,
                lblImeis, _txtImeis,
                _btnOk, _btnCancelar
            });
        }
    }
}
