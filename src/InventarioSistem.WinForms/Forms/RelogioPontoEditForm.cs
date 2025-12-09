using System;
using System.Drawing;
using System.Windows.Forms;
using LegacyDevices = InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class RelogioPontoEditForm : Form
    {
        private readonly LegacyDevices.RelogioPonto _model;

        private TextBox _txtModelo = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtLocal = null!;
        private TextBox _txtIp = null!;
        private DateTimePicker _dtpDataBateria = null!;
        private DateTimePicker _dtpDataNobreak = null!;
        private DateTimePicker _dtpProximasVerificacoes = null!;

        public RelogioPontoEditForm(LegacyDevices.RelogioPonto model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            InitializeComponents();
            BindFromModel();
        }

        private void InitializeComponents()
        {
            Text = "Relógio de Ponto";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;

            _txtModelo = CreateTextBox("Modelo", ref y);
            _txtSerial = CreateTextBox("SerialNumber", ref y);
            _txtLocal = CreateTextBox("Local", ref y);
            _txtIp = CreateTextBox("IP", ref y);

            _dtpDataBateria = CreateDatePicker("Data Bateria", ref y);
            _dtpDataNobreak = CreateDatePicker("Data Nobreak", ref y);
            _dtpProximasVerificacoes = CreateDatePicker("Próximas Verificações", ref y);

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(200, y + 10),
                DialogResult = DialogResult.OK
            };
            btnOk.Click += (_, _) =>
            {
                BindToModel();
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(285, y + 10),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += (_, _) => Close();

            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private TextBox CreateTextBox(string label, ref int y)
        {
            var lbl = new Label
            {
                Text = label + ":",
                AutoSize = true,
                Location = new Point(10, y + 4)
            };
            var txt = new TextBox
            {
                Location = new Point(150, y),
                Width = 240
            };

            Controls.Add(lbl);
            Controls.Add(txt);

            y += 32;
            return txt;
        }

        private DateTimePicker CreateDatePicker(string label, ref int y)
        {
            var lbl = new Label
            {
                Text = label + ":",
                AutoSize = true,
                Location = new Point(10, y + 6)
            };

            var picker = new DateTimePicker
            {
                Location = new Point(150, y),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true
            };

            Controls.Add(lbl);
            Controls.Add(picker);

            y += 32;
            return picker;
        }

        private void BindFromModel()
        {
            _txtModelo.Text = _model.Modelo;
            _txtSerial.Text = _model.SerialNumber;
            _txtLocal.Text = _model.Local;
            _txtIp.Text = _model.Ip;

            BindDate(_dtpDataBateria, _model.DataBateria);
            BindDate(_dtpDataNobreak, _model.DataNobreak);
            BindDate(_dtpProximasVerificacoes, _model.ProximasVerificacoes);
        }

        private static void BindDate(DateTimePicker picker, DateTime? value)
        {
            picker.Value = value ?? DateTime.Today;
            picker.Checked = value.HasValue;
        }

        private void BindToModel()
        {
            _model.Modelo = _txtModelo.Text.Trim();
            _model.SerialNumber = _txtSerial.Text.Trim();
            _model.Local = _txtLocal.Text.Trim();
            _model.Ip = _txtIp.Text.Trim();

            _model.DataBateria = _dtpDataBateria.Checked ? _dtpDataBateria.Value.Date : null;
            _model.DataNobreak = _dtpDataNobreak.Checked ? _dtpDataNobreak.Value.Date : null;
            _model.ProximasVerificacoes = _dtpProximasVerificacoes.Checked ? _dtpProximasVerificacoes.Value.Date : null;
        }
    }
}
