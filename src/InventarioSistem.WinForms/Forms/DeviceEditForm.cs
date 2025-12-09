using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms
{
    /// <summary>
    /// Formulário genérico para edição de um "Device" (InventarioSistem.Core.Entities.Device)
    /// ou qualquer outro dispositivo simples.
    ///
    /// Foi criado para ser compatível com chamadas existentes como:
    ///   new DeviceEditForm(_store)
    ///   new DeviceEditForm(_store, device)
    ///
    /// e evitar erros de compilação (CS0246 / CS1503).
    /// </summary>
    public class DeviceEditForm : Form
    {
        private readonly AccessInventoryStore? _store;
        private readonly Device? _device;

        private readonly Dictionary<string, TextBox> _fields = new();
        private readonly string _title;
        private readonly Dictionary<string, string> _initialValues;

        /// <summary>
        /// Resultado dos campos editados (chave = nome do campo).
        /// </summary>
        public Dictionary<string, string> Result { get; private set; } = new();

        /// <summary>
        /// Construtor mais completo: título + mapa de campos.
        /// Útil se futuramente quisermos usar esse form de forma mais rica.
        /// </summary>
        public DeviceEditForm(string title, Dictionary<string, string> fields)
        {
            _title = string.IsNullOrWhiteSpace(title) ? "Editar dispositivo" : title;
            _initialValues = fields ?? new Dictionary<string, string>();

            InitializeLayout();
        }

        /// <summary>
        /// Construtor usado quando só temos o store.
        /// Compatível com chamadas: new DeviceEditForm(_store)
        /// </summary>
        public DeviceEditForm(AccessInventoryStore store)
            : this("Editar dispositivo", new Dictionary<string, string>())
        {
            _store = store;
        }

        /// <summary>
        /// Construtor usado quando temos o store e um Device.
        /// Compatível com chamadas: new DeviceEditForm(_store, device)
        /// </summary>
        public DeviceEditForm(AccessInventoryStore store, Device? device)
            : this("Editar dispositivo", BuildInitialValues(device))
        {
            _store = store;
            _device = device;
        }

        /// <summary>
        /// Construtor sem parâmetros — apenas para segurança/compat.
        /// </summary>
        public DeviceEditForm()
            : this("Editar dispositivo", new Dictionary<string, string>())
        {
        }

        private static Dictionary<string, string> BuildInitialValues(Device? device)
        {
            // Como não sabemos exatamente quais propriedades existem em Device,
            // preenchemos campos básicos mais comuns em inventário, se existirem
            // (via padrão de nome).
            var dict = new Dictionary<string, string>();

            try
            {
                if (device != null)
                {
                    var type = device.GetType();

                    string GetProp(string name)
                    {
                        var prop = type.GetProperty(name);
                        if (prop == null) return string.Empty;
                        var val = prop.GetValue(device);
                        return val?.ToString() ?? string.Empty;
                    }

                    // Tentativa de mapear propriedades comuns (não quebra se não existirem)
                    dict["Hostname"] = GetProp("Hostname");
                    dict["SerialNumber"] = GetProp("SerialNumber");
                    dict["IP"] = GetProp("IpAddress");
                    dict["Modelo"] = GetProp("Modelo");
                    dict["Numero"] = GetProp("Numero");
                    dict["Responsavel"] = GetProp("Responsavel");
                }
            }
            catch
            {
                // Qualquer problema de reflection: ignora, mantemos só o Info.
            }

            return dict;
        }

        private void InitializeLayout()
        {
            Text = _title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(460, 120 + Math.Max(1, _initialValues.Count) * 32);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;

            if (_initialValues.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "Nenhum campo configurado. (DeviceEditForm genérico)",
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
                        Width = 280,
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
                Location = new Point(260, y + 10),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(345, y + 10),
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
