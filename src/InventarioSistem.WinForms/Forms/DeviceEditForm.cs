using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms
{
    /// <summary>
    /// Formulário genérico para edição de "dispositivo".
    /// É pensado para ser usado de forma simples, por exemplo:
    ///
    ///   var form = new DeviceEditForm(
    ///       "Impressora",
    ///       new Dictionary<string, string>
    ///       {
    ///           ["Hostname"] = impressora.Hostname,
    ///           ["Serial Number"] = impressora.SerialNumber,
    ///           ["IP"] = impressora.IpAddress,
    ///           ["Modelo"] = impressora.Modelo
    ///       });
    ///
    ///   if (form.ShowDialog(this) == DialogResult.OK)
    ///   {
    ///       var result = form.Result;
    ///       impressora.Hostname     = result["Hostname"];
    ///       impressora.SerialNumber = result["Serial Number"];
    ///       ...
    ///   }
    ///
    /// Para chamadas antigas que apenas instanciam DeviceEditForm
    /// com outro tipo de assinatura, há construtores de conveniência
    /// que não quebram o build.
    /// </summary>
    public class DeviceEditForm : Form
    {
        private readonly Dictionary<string, TextBox> _fields = new();
        private readonly string _title;
        private readonly Dictionary<string, string> _initialValues;

        public Dictionary<string, string> Result { get; private set; } = new();

        /// <summary>
        /// Construtor mais completo: título + mapa de campos.
        /// </summary>
        public DeviceEditForm(string title, Dictionary<string, string> fields)
        {
            _title = string.IsNullOrWhiteSpace(title) ? "Editar dispositivo" : title;
            _initialValues = fields ?? new Dictionary<string, string>();

            InitializeLayout();
        }

        /// <summary>
        /// Construtor de conveniência para casos em que o código antigo chama apenas DeviceEditForm().
        /// Mostra mensagem mínima para não quebrar o fluxo.
        /// </summary>
        public DeviceEditForm()
            : this("Editar dispositivo", new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Construtor de conveniência quando se passa um objeto qualquer (por compatibilidade).
        /// Não faz introspecção; apenas usa ToString() do objeto como "Info".
        /// </summary>
        public DeviceEditForm(object existing)
            : this("Editar dispositivo", new Dictionary<string, string>
            {
                ["Info"] = existing?.ToString() ?? string.Empty
            })
        {
        }

        /// <summary>
        /// Construtor de conveniência com título + objeto (por compatibilidade).
        /// </summary>
        public DeviceEditForm(string title, object existing)
            : this(title, new Dictionary<string, string>
            {
                ["Info"] = existing?.ToString() ?? string.Empty
            })
        {
        }

        /// <summary>
        /// Construtor de compatibilidade para chamadas existentes que usam o repositório e uma entidade Device.
        /// Ele pré-popula alguns campos básicos com valores do dispositivo.
        /// </summary>
        public DeviceEditForm(AccessInventoryStore store, Device? existing)
            : this(existing?.Type.ToString() ?? "Editar dispositivo", BuildFieldMap(existing))
        {
            _ = store; // Compat apenas para manter a assinatura antiga sem warnings.
        }

        private static Dictionary<string, string> BuildFieldMap(Device? device)
        {
            if (device == null)
            {
                return new Dictionary<string, string>();
            }

            return new Dictionary<string, string>
            {
                ["Tipo"] = device.Type.ToString(),
                ["Patrimônio"] = device.Patrimonio ?? string.Empty,
                ["Marca"] = device.Marca ?? string.Empty,
                ["Modelo"] = device.Modelo ?? string.Empty,
                ["Número de Série"] = device.NumeroSerie ?? string.Empty,
                ["Responsável"] = device.Responsavel ?? string.Empty,
                ["Localização"] = device.Localizacao ?? string.Empty,
                ["Observações"] = device.Observacoes ?? string.Empty
            };
        }

        private void InitializeLayout()
        {
            Text = _title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 80 + Math.Max(1, _initialValues.Count) * 30);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            int y = 15;

            if (_initialValues.Count == 0)
            {
                var lbl = new Label
                {
                    Text = "Nenhum campo configurado. (Form genérico - ajustar uso no MainForm)",
                    AutoSize = true,
                    Location = new Point(10, y)
                };
                Controls.Add(lbl);
                y += 30;
            }
            else
            {
                foreach (var kvp in _initialValues)
                {
                    var lbl = new Label
                    {
                        Text = kvp.Key + ":",
                        AutoSize = true,
                        Location = new Point(10, y + 3)
                    };

                    var txt = new TextBox
                    {
                        Location = new Point(140, y),
                        Width = 240,
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
                Location = new Point(210, y + 10),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(295, y + 10),
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
