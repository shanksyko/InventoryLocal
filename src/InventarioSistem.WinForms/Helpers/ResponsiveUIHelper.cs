using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace InventarioSistem.WinForms.Helpers;

/// <summary>
/// Helper para criar componentes UI responsivos e bonitos
/// </summary>
public static class ResponsiveUIHelper
{
    // Cores modernas e profissionais
    public static class Colors
    {
        public static readonly Color PrimaryBlue = Color.FromArgb(13, 110, 253);
        public static readonly Color PrimaryGreen = Color.FromArgb(25, 135, 84);
        public static readonly Color PrimaryRed = Color.FromArgb(220, 53, 69);
        public static readonly Color PrimaryOrange = Color.FromArgb(253, 126, 20);
        
        public static readonly Color LightBackground = Color.FromArgb(245, 247, 250);
        public static readonly Color DarkBackground = Color.FromArgb(33, 37, 41);
        public static readonly Color CardBackground = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(220, 221, 225);
        
        public static readonly Color TextDark = Color.FromArgb(33, 37, 41);
        public static readonly Color TextLight = Color.FromArgb(108, 117, 125);
        public static readonly Color TextLighter = Color.FromArgb(173, 181, 189);
        
        public static readonly Color HoverColor = Color.FromArgb(230, 235, 242);
        public static readonly Color SelectedColor = Color.FromArgb(200, 225, 255);
    }

    // Fontes responsivas
    public static class Fonts
    {
        public static readonly Font TitleBold = new("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font Subtitle = new("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font Regular = new("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font Small = new("Segoe UI", 8F, FontStyle.Regular);
        public static readonly Font ButtonFont = new("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font LabelBold = new("Segoe UI", 9F, FontStyle.Bold);
    }

    // Padding/Margin padrões
    public static class Spacing
    {
        public const int XSmall = 4;
        public const int Small = 8;
        public const int Medium = 16;
        public const int Large = 24;
        public const int XLarge = 32;
    }

    /// <summary>
    /// Cria um botão estilizado e responsivo
    /// </summary>
    public static Button CreateButton(string text, int width = 100, Color? backColor = null, EventHandler? onClick = null)
    {
        var btn = new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor ?? Colors.PrimaryBlue,
            ForeColor = Color.White,
            Font = Fonts.ButtonFont,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };

        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = AdjustBrightness(btn.BackColor, 0.8);
        btn.FlatAppearance.MouseDownBackColor = AdjustBrightness(btn.BackColor, 0.6);

        if (onClick != null)
            btn.Click += onClick;

        return btn;
    }

    /// <summary>
    /// Cria um label estilizado
    /// </summary>
    public static Label CreateLabel(string text, Font? font = null, Color? foreColor = null, bool autoSize = true)
    {
        return new Label
        {
            Text = text,
            Font = font ?? Fonts.Regular,
            ForeColor = foreColor ?? Colors.TextDark,
            AutoSize = autoSize,
            BackColor = Color.Transparent
        };
    }

    /// <summary>
    /// Cria um TextBox responsivo
    /// </summary>
    public static TextBox CreateTextBox(string placeholderText = "", int width = 200, bool multiline = false)
    {
        var txt = new TextBox
        {
            Width = width,
            Font = Fonts.Regular,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Colors.CardBackground,
            ForeColor = Colors.TextDark,
            PlaceholderText = placeholderText,
            Multiline = multiline,
            Height = multiline ? 100 : 36
        };

        return txt;
    }

    /// <summary>
    /// Cria um ComboBox responsivo
    /// </summary>
    public static ComboBox CreateComboBox(int width = 200, string[] items = null)
    {
        var combo = new ComboBox
        {
            Width = width,
            Height = 36,
            Font = Fonts.Regular,
            BackColor = Colors.CardBackground,
            ForeColor = Colors.TextDark,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };

        if (items != null)
            combo.Items.AddRange(items);

        return combo;
    }

    /// <summary>
    /// Cria um DataGridView otimizado e responsivo
    /// </summary>
    public static DataGridView CreateDataGrid(
        bool readOnly = true,
        bool allowUserResize = true,
        bool alternatingColors = true)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true,
            ReadOnly = readOnly,
            AllowUserToResizeRows = allowUserResize,
            AllowUserToResizeColumns = allowUserResize,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = Fonts.Regular,
            BackgroundColor = Colors.LightBackground,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Colors.BorderColor
        };

        // Estilo das colunas
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(100, 149, 237),
            ForeColor = Color.White,
            Font = new Font(Fonts.Regular.FontFamily, 9, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(Spacing.Small)
        };

        // Estilo das células
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Colors.CardBackground,
            ForeColor = Colors.TextDark,
            SelectionBackColor = Colors.SelectedColor,
            SelectionForeColor = Colors.TextDark,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(Spacing.XSmall)
        };

        // Cores alternadas nas linhas
        if (alternatingColors)
        {
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Colors.LightBackground,
                ForeColor = Colors.TextDark
            };
        }

        grid.RowTemplate.Height = 36;
        grid.ColumnHeadersHeight = 40;

        return grid;
    }

    /// <summary>
    /// Cria um Panel responsivo com estilo de card
    /// </summary>
    public static Panel CreateCard(int width = 0, int height = 0, Color? backColor = null)
    {
        var panel = new Panel
        {
            BackColor = backColor ?? Colors.CardBackground,
            AutoScroll = true
        };

        if (width > 0) panel.Width = width;
        if (height > 0) panel.Height = height;

        // Adicionar borda suave
        panel.Paint += (s, e) =>
        {
            using (var pen = new Pen(Colors.BorderColor, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            }
        };

        return panel;
    }

    /// <summary>
    /// Cria um panel de cabeçalho com gradiente
    /// </summary>
    public static Panel CreateHeaderPanel(string title, string subtitle = "")
    {
        var panel = new Panel
        {
            Height = string.IsNullOrEmpty(subtitle) ? 60 : 80,
            Dock = DockStyle.Top,
            BackColor = Colors.PrimaryBlue
        };

        // Gradiente
        panel.Paint += (s, e) =>
        {
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                panel.ClientRectangle,
                Colors.PrimaryBlue,
                AdjustBrightness(Colors.PrimaryBlue, 0.85),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = Fonts.TitleBold,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(Spacing.Medium, Spacing.Small)
        };

        panel.Controls.Add(lblTitle);

        if (!string.IsNullOrEmpty(subtitle))
        {
            var lblSubtitle = new Label
            {
                Text = subtitle,
                Font = Fonts.Small,
                ForeColor = Color.FromArgb(200, 210, 240),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(Spacing.Medium, Spacing.Medium + 20)
            };

            panel.Controls.Add(lblSubtitle);
        }

        return panel;
    }

    /// <summary>
    /// Cria um panel de ação com múltiplos botões
    /// </summary>
    public static Panel CreateActionPanel(params (string text, EventHandler onClick, Color? color)[] actions)
    {
        var panel = new Panel
        {
            Height = 50,
            Dock = DockStyle.Top,
            BackColor = Colors.LightBackground,
            BorderStyle = BorderStyle.FixedSingle
        };

        int x = Spacing.Medium;
        foreach (var (text, onClick, color) in actions)
        {
            var btn = CreateButton(text, 90, color, onClick);
            btn.Location = new Point(x, (panel.Height - btn.Height) / 2);
            panel.Controls.Add(btn);
            x += btn.Width + Spacing.Small;
        }

        return panel;
    }

    /// <summary>
    /// Cria um TabControl responsivo
    /// </summary>
    public static TabControl CreateTabControl()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = Fonts.Regular,
            ItemSize = new Size(120, 35)
        };

        return tabs;
    }

    /// <summary>
    /// Cria uma TabPage responsiva
    /// </summary>
    public static TabPage CreateTabPage(string text)
    {
        return new TabPage
        {
            Text = text,
            Font = Fonts.Regular,
            BackColor = Colors.LightBackground,
            Padding = new Padding(Spacing.Medium)
        };
    }

    /// <summary>
    /// Cria um CheckBox estilizado
    /// </summary>
    public static CheckBox CreateCheckBox(string text, EventHandler? onCheckedChanged = null)
    {
        var chk = new CheckBox
        {
            Text = text,
            Font = Fonts.Regular,
            ForeColor = Colors.TextDark,
            AutoSize = true,
            Cursor = Cursors.Hand
        };

        if (onCheckedChanged != null)
            chk.CheckedChanged += onCheckedChanged;

        return chk;
    }

    /// <summary>
    /// Ajusta o brilho de uma cor
    /// </summary>
    public static Color AdjustBrightness(Color color, double factor)
    {
        return Color.FromArgb(
            color.A,
            (int)Math.Min(255, color.R * factor),
            (int)Math.Min(255, color.G * factor),
            (int)Math.Min(255, color.B * factor)
        );
    }

    /// <summary>
    /// Centraliza um formulário na tela
    /// </summary>
    public static void CenterForm(Form form)
    {
        form.StartPosition = FormStartPosition.CenterScreen;
    }

    /// <summary>
    /// Aplica theme escuro a um formulário
    /// </summary>
    public static void ApplyDarkTheme(Form form)
    {
        form.BackColor = Colors.DarkBackground;
        form.ForeColor = Color.White;
        
        foreach (Control ctrl in GetAllControls(form))
        {
            if (ctrl is DataGridView) continue;
            
            ctrl.BackColor = Colors.DarkBackground;
            ctrl.ForeColor = Color.White;
        }
    }

    /// <summary>
    /// Aplica theme claro a um formulário
    /// </summary>
    public static void ApplyLightTheme(Form form)
    {
        form.BackColor = Colors.LightBackground;
        form.ForeColor = Colors.TextDark;
        
        foreach (Control ctrl in GetAllControls(form))
        {
            if (ctrl is DataGridView) continue;
            
            ctrl.BackColor = Colors.CardBackground;
            ctrl.ForeColor = Colors.TextDark;
        }
    }

    /// <summary>
    /// Obtém todos os controles de um formulário recursivamente
    /// </summary>
    private static IEnumerable<Control> GetAllControls(Control container)
    {
        foreach (Control control in container.Controls)
        {
            yield return control;
            foreach (var child in GetAllControls(control))
                yield return child;
        }
    }

    /// <summary>
    /// Mostra uma mensagem de sucesso
    /// </summary>
    public static void ShowSuccess(string message, string title = "Sucesso")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Mostra uma mensagem de erro
    /// </summary>
    public static void ShowError(string message, string title = "Erro")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Mostra uma mensagem de confirmação
    /// </summary>
    public static bool ShowConfirm(string message, string title = "Confirmação")
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            == DialogResult.Yes;
    }
}
