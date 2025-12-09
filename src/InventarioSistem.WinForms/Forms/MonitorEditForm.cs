using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class MonitorEditForm : Form
    {
        private TextBox _txtModelo = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtLocal = null!;
        private TextBox _txtResponsavel = null!;
        private TextBox _txtComputador = null!;
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public Monitor Monitor { get; private set; }

        public MonitorEditForm(Monitor? existing = null)
        {
            Text = existing == null ? "Novo Monitor" : "Editar Monitor";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(430, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Monitor = existing == null
                ? new Monitor()
                : new Monitor
                {
                    Id = existing.Id,
                    Modelo = existing.Modelo,
                    SerialNumber = existing.SerialNumber,
                    Local = existing.Local,
                    Responsavel = existing.Responsavel,
                    ComputadorVinculado = existing.ComputadorVinculado
                };

            InitializeLayout();

            if (existing != null)
            {
                _txtModelo.Text = existing.Modelo;
                _txtSerial.Text = existing.SerialNumber;
                _txtLocal.Text = existing.Local;
                _txtResponsavel.Text = existing.Responsavel;
                _txtComputador.Text = existing.ComputadorVinculado;
            }
        }

        private void InitializeLayout()
        {
            var lblModelo = new Label { Text = "Modelo:", AutoSize = true, Location = new Point(10, 15) };
            _txtModelo = new TextBox { Location = new Point(150, 12), Width = 250 };

            var lblSerial = new Label { Text = "SerialNumber:", AutoSize = true, Location = new Point(10, 45) };
            _txtSerial = new TextBox { Location = new Point(150, 42), Width = 250 };

            var lblLocal = new Label { Text = "Local:", AutoSize = true, Location = new Point(10, 75) };
            _txtLocal = new TextBox { Location = new Point(150, 72), Width = 250 };

            var lblResponsavel = new Label { Text = "Responsável:", AutoSize = true, Location = new Point(10, 105) };
            _txtResponsavel = new TextBox { Location = new Point(150, 102), Width = 250 };

            var lblComputador = new Label { Text = "Computador vinculado:", AutoSize = true, Location = new Point(10, 135) };
            _txtComputador = new TextBox { Location = new Point(150, 132), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(230, 200), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(315, 200), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Monitor.Modelo = _txtModelo.Text.Trim();
                Monitor.SerialNumber = _txtSerial.Text.Trim();
                Monitor.Local = _txtLocal.Text.Trim();
                Monitor.Responsavel = _txtResponsavel.Text.Trim();
                Monitor.ComputadorVinculado = _txtComputador.Text.Trim();
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
                lblModelo, _txtModelo,
                lblSerial, _txtSerial,
                lblLocal, _txtLocal,
                lblResponsavel, _txtResponsavel,
                lblComputador, _txtComputador,
                _btnOk, _btnCancelar
            });
        }
    }
}
