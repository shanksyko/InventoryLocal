using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using InventarioSistem.Access;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.WinForms.Forms;

public static class XlsxExporter
{
    public static void ExportWithDialog(AccessInventoryStore store, DeviceType type, IWin32Window owner)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"inventario-{type.ToString().ToLowerInvariant()}-{DateTime.Now:yyyyMMdd}.xlsx"
        };

        if (dialog.ShowDialog(owner) == DialogResult.OK)
        {
            ExportToFile(store, type, dialog.FileName);
            MessageBox.Show(owner, "Exportação concluída", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static void ExportToFile(AccessInventoryStore store, DeviceType type, string path)
    {
        var devices = store.ListAsync(type).GetAwaiter().GetResult();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Itens");

        // Headers
        var headers = new[] { "Id", "Tipo", "Patrimonio", "Marca", "Modelo", "NumeroSerie", "Imei", "Localizacao", "Responsavel", "Observacoes", "AtualizadoEm" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var d in devices.OrderBy(x => x.Id))
        {
            ws.Cell(row, 1).Value = d.Id;
            ws.Cell(row, 2).Value = d.Type.ToString();
            ws.Cell(row, 3).Value = d.Patrimonio;
            ws.Cell(row, 4).Value = d.Marca;
            ws.Cell(row, 5).Value = d.Modelo;
            ws.Cell(row, 6).Value = d.NumeroSerie;
            ws.Cell(row, 7).Value = d.Imei ?? string.Empty;
            ws.Cell(row, 8).Value = d.Localizacao ?? string.Empty;
            ws.Cell(row, 9).Value = d.Responsavel ?? string.Empty;
            ws.Cell(row, 10).Value = d.Observacoes ?? string.Empty;
            ws.Cell(row, 11).Value = d.AtualizadoEm.ToString("s");
            row++;
        }

        ws.Columns().AdjustToContents();

        // Ensure directory exists
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        wb.SaveAs(path);
    }
}
