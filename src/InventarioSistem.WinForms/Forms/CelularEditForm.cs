using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class CelularEditForm : Form
    {
        private TextBox _txtModelo = null!;
        private TextBox _txtNumero = null!;
        private TextBox _txtProprietario = null!;
        private TextBox _txtImeis = null!;
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
                _txtModelo.Text = existing.Modelo ?? string.Empty;
                _txtNumero.Text = existing.Numero ?? string.Empty;
                _txtProprietario.Text = existing.Proprietario ?? string.Empty;
                _txtImeis.Text = existing.Imeis != null ? string.Join(";", existing.Imeis) : string.Empty;

                Result = new Celular
                {
                    Id = existing.Id,
                    Modelo = existing.Modelo ?? string.Empty,
                    Numero = existing.Numero ?? string.Empty,
                    Proprietario = existing.Proprietario ?? string.Empty,
                    Imeis = existing.Imeis?.ToList() ?? new()
                };
            }
            else
            {
                Result = new Celular
                {
                    Modelo = string.Empty,
                    Numero = string.Empty,
                    Proprietario = string.Empty,
                    Imeis = new()
                };
            }
        }

        private void InitializeLayout()
        {
            var lblModelo = new Label { Text = "Modelo:", AutoSize = true, Location = new Point(10, 15) };
            _txtModelo = new TextBox { Location = new Point(130, 12), Width = 250 };

            var lblNumero = new Label { Text = "Número:", AutoSize = true, Location = new Point(10, 45) };
            _txtNumero = new TextBox { Location = new Point(130, 42), Width = 250 };

            var lblProprietario = new Label { Text = "Proprietário:", AutoSize = true, Location = new Point(10, 75) };
            _txtProprietario = new TextBox { Location = new Point(130, 72), Width = 250 };

            var lblImeis = new Label { Text = "IMEIs (; separados):", AutoSize = true, Location = new Point(10, 105) };
            _txtImeis = new TextBox { Location = new Point(130, 102), Width = 250 };

            _btnOk = new Button { Text = "OK", Location = new Point(220, 170), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(305, 170), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Modelo = _txtModelo.Text.Trim();
                Result.Numero = _txtNumero.Text.Trim();
                Result.Proprietario = _txtProprietario.Text.Trim();
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
                lblModelo, _txtModelo,
                lblNumero, _txtNumero,
                lblProprietario, _txtProprietario,
                lblImeis, _txtImeis,
                _btnOk, _btnCancelar
            });
        }
    }
}
