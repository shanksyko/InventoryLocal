using System;
using System.Collections.Generic;
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
            try
            {
                ExportToFile(store, type, dialog.FileName);
                MessageBox.Show(owner, $"Exportação concluída!\n\nArquivo: {dialog.FileName}", "Inventário", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, $"Erro ao exportar: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static void ExportToFile(AccessInventoryStore store, DeviceType type, string path)
    {
        // Criar workbook
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Itens");

        // Usar os métodos específicos e exportar de acordo com o tipo
        switch (type)
        {
            case DeviceType.Computer:
                {
                    var items = store.GetAllComputers();
                    ExportComputers(ws, items);
                    break;
                }
            case DeviceType.Tablet:
                {
                    var items = store.GetAllTablets();
                    ExportTablets(ws, items);
                    break;
                }
            case DeviceType.ColetorAndroid:
                {
                    var items = store.GetAllColetores();
                    ExportColetores(ws, items);
                    break;
                }
            case DeviceType.Celular:
                {
                    var items = store.GetAllCelulares();
                    ExportCelulares(ws, items);
                    break;
                }
            case DeviceType.Impressora:
                {
                    var items = store.GetAllImpressoras();
                    ExportImpressoras(ws, items);
                    break;
                }
            case DeviceType.Dect:
                {
                    var items = store.GetAllDects();
                    ExportDects(ws, items);
                    break;
                }
            case DeviceType.TelefoneCisco:
                {
                    var items = store.GetAllTelefonesCisco();
                    ExportTelefonesCisco(ws, items);
                    break;
                }
            case DeviceType.Televisor:
                {
                    var items = store.GetAllTelevisores();
                    ExportTelevisores(ws, items);
                    break;
                }
            case DeviceType.RelogioPonto:
                {
                    var items = store.GetAllRelogiosPonto();
                    ExportRelogiosPonto(ws, items);
                    break;
                }
            default:
                throw new Exception($"Tipo {type} não suportado para exportação");
        }

        ws.Columns().AdjustToContents();

        // Ensure directory exists
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        wb.SaveAs(path);
    }

    private static void ExportComputers(IXLWorksheet ws, List<Core.Devices.Computer> items)
    {
        // Headers
        ws.Cell(1, 1).Value = "Host";
        ws.Cell(1, 2).Value = "SerialNumber";
        ws.Cell(1, 3).Value = "Proprietário";
        ws.Cell(1, 4).Value = "Departamento";
        ws.Cell(1, 5).Value = "Matrícula";
        ws.Cell(1, 6).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Host))
        {
            ws.Cell(row, 1).Value = item.Host;
            ws.Cell(row, 2).Value = item.SerialNumber;
            ws.Cell(row, 3).Value = item.Proprietario;
            ws.Cell(row, 4).Value = item.Departamento;
            ws.Cell(row, 5).Value = item.Matricula;
            ws.Cell(row, 6).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportTablets(IXLWorksheet ws, List<Core.Devices.Tablet> items)
    {
        ws.Cell(1, 1).Value = "Host";
        ws.Cell(1, 2).Value = "SerialNumber";
        ws.Cell(1, 3).Value = "Local";
        ws.Cell(1, 4).Value = "Responsável";
        ws.Cell(1, 5).Value = "IMEIs";
        ws.Cell(1, 6).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Host))
        {
            ws.Cell(row, 1).Value = item.Host;
            ws.Cell(row, 2).Value = item.SerialNumber;
            ws.Cell(row, 3).Value = item.Local;
            ws.Cell(row, 4).Value = item.Responsavel;
            ws.Cell(row, 5).Value = item.ImeisJoined;
            ws.Cell(row, 6).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportColetores(IXLWorksheet ws, List<Core.Devices.ColetorAndroid> items)
    {
        ws.Cell(1, 1).Value = "Host";
        ws.Cell(1, 2).Value = "SerialNumber";
        ws.Cell(1, 3).Value = "MAC Address";
        ws.Cell(1, 4).Value = "IP Address";
        ws.Cell(1, 5).Value = "Local";
        ws.Cell(1, 6).Value = "Aplicativos";
        ws.Cell(1, 7).Value = "Sistema Operacional";
        ws.Cell(1, 8).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Host))
        {
            ws.Cell(row, 1).Value = item.Host;
            ws.Cell(row, 2).Value = item.SerialNumber;
            ws.Cell(row, 3).Value = item.MacAddress;
            ws.Cell(row, 4).Value = item.IpAddress;
            ws.Cell(row, 5).Value = item.Local;
            ws.Cell(row, 6).Value = item.AppsInstalados;
            ws.Cell(row, 7).Value = item.SistemaOperacional;
            ws.Cell(row, 8).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportCelulares(IXLWorksheet ws, List<Core.Devices.Celular> items)
    {
        ws.Cell(1, 1).Value = "CellName";
        ws.Cell(1, 2).Value = "IMEI1";
        ws.Cell(1, 3).Value = "IMEI2";
        ws.Cell(1, 4).Value = "Modelo";
        ws.Cell(1, 5).Value = "Número";
        ws.Cell(1, 6).Value = "Usuário";
        ws.Cell(1, 7).Value = "Matrícula";
        ws.Cell(1, 8).Value = "Cargo";
        ws.Cell(1, 9).Value = "Setor";
        ws.Cell(1, 10).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.CellName))
        {
            ws.Cell(row, 1).Value = item.CellName;
            ws.Cell(row, 2).Value = item.Imei1;
            ws.Cell(row, 3).Value = item.Imei2;
            ws.Cell(row, 4).Value = item.Modelo;
            ws.Cell(row, 5).Value = item.Numero;
            ws.Cell(row, 6).Value = item.Usuario;
            ws.Cell(row, 7).Value = item.Matricula;
            ws.Cell(row, 8).Value = item.Cargo;
            ws.Cell(row, 9).Value = item.Setor;
            ws.Cell(row, 10).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportImpressoras(IXLWorksheet ws, List<Core.Devices.Impressora> items)
    {
        ws.Cell(1, 1).Value = "Nome";
        ws.Cell(1, 2).Value = "Tipo/Modelo";
        ws.Cell(1, 3).Value = "SerialNumber";
        ws.Cell(1, 4).Value = "Local Atual";
        ws.Cell(1, 5).Value = "Local Anterior";
        ws.Cell(1, 6).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Nome))
        {
            ws.Cell(row, 1).Value = item.Nome;
            ws.Cell(row, 2).Value = item.TipoModelo;
            ws.Cell(row, 3).Value = item.SerialNumber;
            ws.Cell(row, 4).Value = item.LocalAtual;
            ws.Cell(row, 5).Value = item.LocalAnterior;
            ws.Cell(row, 6).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportDects(IXLWorksheet ws, List<Core.Devices.DectPhone> items)
    {
        ws.Cell(1, 1).Value = "Número";
        ws.Cell(1, 2).Value = "Responsável";
        ws.Cell(1, 3).Value = "Local";
        ws.Cell(1, 4).Value = "IPEI";
        ws.Cell(1, 5).Value = "MAC Address";
        ws.Cell(1, 6).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Numero))
        {
            ws.Cell(row, 1).Value = item.Numero;
            ws.Cell(row, 2).Value = item.Responsavel;
            ws.Cell(row, 3).Value = item.Local;
            ws.Cell(row, 4).Value = item.Ipei;
            ws.Cell(row, 5).Value = item.MacAddress;
            ws.Cell(row, 6).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportTelefonesCisco(IXLWorksheet ws, List<Core.Devices.CiscoPhone> items)
    {
        ws.Cell(1, 1).Value = "Responsável";
        ws.Cell(1, 2).Value = "MAC Address";
        ws.Cell(1, 3).Value = "Número";
        ws.Cell(1, 4).Value = "Local";
        ws.Cell(1, 5).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Numero))
        {
            ws.Cell(row, 1).Value = item.Responsavel;
            ws.Cell(row, 2).Value = item.MacAddress;
            ws.Cell(row, 3).Value = item.Numero;
            ws.Cell(row, 4).Value = item.Local;
            ws.Cell(row, 5).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportTelevisores(IXLWorksheet ws, List<Core.Devices.Televisor> items)
    {
        ws.Cell(1, 1).Value = "Local";
        ws.Cell(1, 2).Value = "Modelo";
        ws.Cell(1, 3).Value = "SerialNumber";
        ws.Cell(1, 4).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Local))
        {
            ws.Cell(row, 1).Value = item.Local;
            ws.Cell(row, 2).Value = item.Modelo;
            ws.Cell(row, 3).Value = item.SerialNumber;
            ws.Cell(row, 4).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportRelogiosPonto(IXLWorksheet ws, List<Core.Devices.RelogioPonto> items)
    {
        ws.Cell(1, 1).Value = "Local";
        ws.Cell(1, 2).Value = "IP";
        ws.Cell(1, 3).Value = "Modelo";
        ws.Cell(1, 4).Value = "SerialNumber";
        ws.Cell(1, 5).Value = "Data Bateria";
        ws.Cell(1, 6).Value = "Data Nobreak";
        ws.Cell(1, 7).Value = "Próximas Verificações";
        ws.Cell(1, 8).Value = "Cadastrado em";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var item in items.OrderBy(x => x.Local))
        {
            ws.Cell(row, 1).Value = item.Local;
            ws.Cell(row, 2).Value = item.Ip;
            ws.Cell(row, 3).Value = item.Modelo;
            ws.Cell(row, 4).Value = item.SerialNumber;
            ws.Cell(row, 5).Value = item.DataBateria?.ToString("g") ?? "";
            ws.Cell(row, 6).Value = item.DataNobreak?.ToString("g") ?? "";
            ws.Cell(row, 7).Value = item.ProximasVerificacoes?.ToString("g") ?? "";
            ws.Cell(row, 8).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }
}
