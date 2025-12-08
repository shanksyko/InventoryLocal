using System.Text;
using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public static class CsvExporter
{
    public static void ExportWithDialog(AccessInventoryStore store, IWin32Window owner)
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

    public static void ExportToFile(AccessInventoryStore store, string path)
    {
        var devices = store.ListAsync().GetAwaiter().GetResult();
        var csv = BuildCsv(devices);
        File.WriteAllText(path, csv, Encoding.UTF8);
    }

    private static string BuildCsv(IEnumerable<Device> devices)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Id;Tipo;Patrimonio;Marca;Modelo;NumeroSerie;Imei;Responsavel;Localizacao;Observacoes");
        foreach (var device in devices)
        {
            builder.AppendLine(string.Join(';', new[]
            {
                device.Id.ToString(),
                device.Type.ToString(),
                Escape(device.Patrimonio),
                Escape(device.Marca),
                Escape(device.Modelo),
                Escape(device.NumeroSerie),
                Escape(device.Imei ?? string.Empty),
                Escape(device.Responsavel),
                Escape(device.Localizacao),
                Escape(device.Observacoes ?? string.Empty)
            }));
        }

        return builder.ToString();
    }

    private static string Escape(string value) => value.Replace(";", ",");
}
