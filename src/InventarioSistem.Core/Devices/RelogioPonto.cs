using System;

namespace InventarioSistem.Core.Devices;

public class RelogioPonto
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public DateTime? DataBateria { get; set; }
    public DateTime? DataNobreak { get; set; }
    public DateTime? ProximasVerificacoes { get; set; }

    public string NumeroSerie
    {
        get => SerialNumber;
        set => SerialNumber = value;
    }

    public string Hostname
    {
        get => Modelo;
        set => Modelo = value;
    }

    public string IpAddress
    {
        get => Ip;
        set => Ip = value;
    }

    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[RelogioPonto] Id={Id}, Modelo={Modelo}, NS={SerialNumber}, Local={Local}, IP={Ip}, DataBateria={DataBateria:yyyy-MM-dd}, DataNobreak={DataNobreak:yyyy-MM-dd}, ProximaVerificacao={ProximasVerificacoes:yyyy-MM-dd}";
}
