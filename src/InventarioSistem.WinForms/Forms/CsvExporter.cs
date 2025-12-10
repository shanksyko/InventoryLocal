using System.Text;
using System.Windows.Forms;
using InventarioSistem.Access;

namespace InventarioSistem.WinForms.Forms;

public static class CsvExporter
{
    public static void ExportWithDialog(SqlServerInventoryStore store, IWin32Window owner)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"inventario-{DateTime.Now:yyyyMMdd}.csv"
        };

        if (dialog.ShowDialog(owner) == DialogResult.OK)
        {
            ExportToFile(store, dialog.FileName);
            MessageBox.Show(owner, "Exportação concluída", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static void ExportToFile(SqlServerInventoryStore store, string path)
    {
        var devices = store.ListAsync().GetAwaiter().GetResult();
        var csv = BuildCsv(devices);
        File.WriteAllText(path, csv, Encoding.UTF8);
    }

    private static string BuildCsv(IEnumerable<dynamic> devices)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Id;Tipo;SerialNumber;CreatedAt");
        foreach (var device in devices)
        {
            builder.AppendLine(string.Join(';', new[]
            {
                device.Id.ToString(),
                device.Type.ToString(),
                (device.SerialNumber ?? "").ToString(),
                (device.CreatedAt ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm")
            }));
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
