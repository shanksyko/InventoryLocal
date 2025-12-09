using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace InventarioSistem.WinForms.Forms
{
    /// <summary>
    /// Formulário genérico para edição de "dispositivo".
    /// Uso típico:
    ///
    ///   var form = new DeviceEditForm(
    ///       "Impressora",
    ///       new Dictionary<string, string>
    ///       {
    ///           ["Hostname"]      = impressora.Hostname,
    ///           ["Serial Number"] = impressora.SerialNumber,
    ///           ["IP"]           = impressora.IpAddress,
    ///           ["Modelo"]       = impressora.Modelo
    ///       });
    ///
    ///   if (form.ShowDialog(this) == DialogResult.OK)
    ///   {
    ///       var r = form.Result;
    ///       impressora.Hostname      = r["Hostname"];
    ///       impressora.SerialNumber  = r["Serial Number"];
    ///       impressora.IpAddress     = r["IP"];
    ///       impressora.Modelo        = r["Modelo"];
    ///   }
    ///
    /// Também há construtores de conveniência para compatibilidade com
    /// chamadas antigas em MainForm.
    /// </summary>
    public class DeviceEditForm : Form
    {
        private readonly Dictionary<string, TextBox> _fields = new();
        private readonly string _title;
        private readonly Dictionary<string, string> _initialValues;

        /// <summary>
        /// Resultado final: mapa Campo -> Valor.
        /// </summary>
        public Dictionary<string, string> Result { get; private set; } = new();

        /// <summary>
        /// Construtor completo: título + mapa de campos.
        /// </summary>
        public DeviceEditForm(string title, Dictionary<string, string> fields)
        {
            _title = string.IsNullOrWhiteSpace(title) ? "Editar dispositivo" : title;
            _initialValues = fields ?? new Dictionary<string, string>();

            InitializeLayout();
        }

        /// <summary>
        /// Construtor sem parâmetros (compatibilidade).
        /// Mostra um form genérico explicando que precisa ser ajustado.
        /// </summary>
        public DeviceEditForm()
            : this("Editar dispositivo", new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Construtor compatível com chamadas DeviceEditForm(obj).
        /// Usa ToString() do objeto como campo "Info".
        /// </summary>
        public DeviceEditForm(object existing)
            : this("Editar dispositivo", new Dictionary<string, string>
            {
                ["Info"] = existing?.ToString() ?? string.Empty
            })
        {
        }

        /// <summary>
        /// Construtor compatível com chamadas DeviceEditForm(título, obj).
        /// </summary>
        public DeviceEditForm(string title, object existing)
            : this(title, new Dictionary<string, string>
            {
                ["Info"] = existing?.ToString() ?? string.Empty
            })
        {
        }

        private void InitializeLayout()
        {
            Text = _title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(440, 100 + Math.Max(1, _initialValues.Count) * 32);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;

            if (_initialValues.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "Nenhum campo configurado. (DeviceEditForm genérico - ajustar uso no MainForm)",
                    AutoSize = true,
                    Location = new Point(10, y)
                };
                Controls.Add(lbl);
                y += 35;
            }
            else
            {
                foreach (var kvp in _initialValues)
                {
                    var lbl = new Label
                    {
                        Text = kvp.Key + ":",
                        AutoSize = true,
                        Location = new Point(10, y + 4)
                    };

                    var txt = new TextBox
                    {
                        Location = new Point(150, y),
                        Width = 250,
                        Text = kvp.Value ?? string.Empty
                    };

                    _fields[kvp.Key] = txt;

                    Controls.Add(lbl);
                    Controls.Add(txt);

                    y += 30;
                }
            }

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(230, y + 10),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(315, y + 10),
                DialogResult = DialogResult.Cancel
            };

            btnOk.Click += (_, _) =>
            {
                var result = new Dictionary<string, string>();
                foreach (var kvp in _fields)
                {
                    result[kvp.Key] = kvp.Value.Text.Trim();
                }
                Result = result;

                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
    }
}
