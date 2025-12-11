using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Entities;
using ComputerDevice = InventarioSistem.Core.Devices.Computer;
using TabletDevice = InventarioSistem.Core.Devices.Tablet;
using ColetorDevice = InventarioSistem.Core.Devices.ColetorAndroid;
using CelularDevice = InventarioSistem.Core.Devices.Celular;
using ImpressoraDevice = InventarioSistem.Core.Devices.Impressora;
using DectDevice = InventarioSistem.Core.Devices.DectPhone;
using CiscoDevice = InventarioSistem.Core.Devices.CiscoPhone;
using TelevisorDevice = InventarioSistem.Core.Devices.Televisor;
using RelogioDevice = InventarioSistem.Core.Devices.RelogioPonto;
using MonitorDevice = InventarioSistem.Core.Devices.Monitor;
using NobreakDevice = InventarioSistem.Core.Devices.Nobreak;

namespace InventarioSistem.Access.Export;

/// <summary>
/// Classe auxiliar para exportação de dados em formato XLSX
/// </summary>
public static class XlsxExporterHelper
{
    public static void ExportToFile(SqlServerInventoryStore store, DeviceType type, string path)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Itens");

        switch (type)
        {
            case DeviceType.Computer:
                ExportComputers(ws, store.GetAllComputers());
                break;
            case DeviceType.Tablet:
                ExportTablets(ws, store.GetAllTablets());
                break;
            case DeviceType.ColetorAndroid:
                ExportColetores(ws, store.GetAllColetores());
                break;
            case DeviceType.Celular:
                ExportCelulares(ws, store.GetAllCelulares());
                break;
            case DeviceType.Impressora:
                ExportImpressoras(ws, store.GetAllImpressoras());
                break;
            case DeviceType.Dect:
                ExportDects(ws, store.GetAllDects());
                break;
            case DeviceType.TelefoneCisco:
                ExportTelefonesCisco(ws, store.GetAllTelefonesCisco());
                break;
            case DeviceType.Televisor:
                ExportTelevisores(ws, store.GetAllTelevisores());
                break;
            case DeviceType.RelogioPonto:
                ExportRelogiosPonto(ws, store.GetAllRelogiosPonto());
                break;
            case DeviceType.Monitor:
                ExportMonitores(ws, store.GetAllMonitores());
                break;
            case DeviceType.Nobreak:
                ExportNobreaks(ws, store.GetAllNobreaks());
                break;
            default:
                throw new Exception($"Tipo {type} não suportado");
        }

        ws.Columns().AdjustToContents();
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        wb.SaveAs(path);
    }

    private static void ExportComputers(IXLWorksheet ws, List<ComputerDevice> items)
    {
        SetHeaders(ws, "Host", "SerialNumber", "Proprietário", "Departamento", "Matrícula", "Cadastrado em");
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

    private static void ExportTablets(IXLWorksheet ws, List<TabletDevice> items)
    {
        SetHeaders(ws, "Host", "SerialNumber", "Local", "Responsável", "IMEIs", "Cadastrado em");
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

    private static void ExportColetores(IXLWorksheet ws, List<ColetorDevice> items)
    {
        SetHeaders(ws, "Host", "SerialNumber", "MAC Address", "IP Address", "Local", "Aplicativos", "Sistema Operacional", "Cadastrado em");
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

    private static void ExportCelulares(IXLWorksheet ws, List<CelularDevice> items)
    {
        SetHeaders(ws, "CellName", "IMEI1", "IMEI2", "Modelo", "Número", "Usuário", "Matrícula", "Cargo", "Setor", "Cadastrado em");
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

    private static void ExportImpressoras(IXLWorksheet ws, List<ImpressoraDevice> items)
    {
        SetHeaders(ws, "Nome", "Tipo/Modelo", "SerialNumber", "Local Atual", "Local Anterior", "Cadastrado em");
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

    private static void ExportDects(IXLWorksheet ws, List<DectDevice> items)
    {
        SetHeaders(ws, "Número", "Responsável", "Local", "IPEI", "MAC Address", "Cadastrado em");
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

    private static void ExportTelefonesCisco(IXLWorksheet ws, List<CiscoDevice> items)
    {
        SetHeaders(ws, "Responsável", "MAC Address", "Número", "Local", "Cadastrado em");
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

    private static void ExportTelevisores(IXLWorksheet ws, List<TelevisorDevice> items)
    {
        SetHeaders(ws, "Local", "Modelo", "SerialNumber", "Cadastrado em");
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

    private static void ExportRelogiosPonto(IXLWorksheet ws, List<RelogioDevice> items)
    {
        SetHeaders(ws, "Local", "IP", "Modelo", "SerialNumber", "Data Bateria", "Data Nobreak", "Próximas Verificações", "Cadastrado em");
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

    private static void ExportMonitores(IXLWorksheet ws, List<MonitorDevice> items)
    {
        SetHeaders(ws, "Modelo", "SerialNumber", "Local", "Responsável", "Computador Vinculado", "Cadastrado em");
        int row = 2;
        foreach (var item in items.OrderBy(x => x.Modelo))
        {
            ws.Cell(row, 1).Value = item.Modelo;
            ws.Cell(row, 2).Value = item.SerialNumber;
            ws.Cell(row, 3).Value = item.Local;
            ws.Cell(row, 4).Value = item.Responsavel;
            ws.Cell(row, 5).Value = item.ComputadorVinculado;
            ws.Cell(row, 6).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void ExportNobreaks(IXLWorksheet ws, List<NobreakDevice> items)
    {
        SetHeaders(ws, "Hostname", "Local", "IP", "Modelo", "Status", "SerialNumber", "Cadastrado em");
        int row = 2;
        foreach (var item in items.OrderBy(x => x.Hostname))
        {
            ws.Cell(row, 1).Value = item.Hostname;
            ws.Cell(row, 2).Value = item.Local;
            ws.Cell(row, 3).Value = item.IpAddress;
            ws.Cell(row, 4).Value = item.Modelo;
            ws.Cell(row, 5).Value = item.Status;
            ws.Cell(row, 6).Value = item.SerialNumber;
            ws.Cell(row, 7).Value = item.CreatedAt?.ToString("g") ?? "";
            row++;
        }
    }

    private static void SetHeaders(IXLWorksheet ws, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }
        ws.Row(1).Style.Font.Bold = true;
    }
}
