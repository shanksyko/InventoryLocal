using System;

namespace InventarioSistem.Core.Devices;

public class RelogioPonto
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public DateTime? DataBateria { get; set; }
    public DateTime? DataNobreak { get; set; }
    public DateTime? ProximasVerificacoes { get; set; }

    public override string ToString()
        => $"[RelogioPonto] Id={Id}, Modelo={Modelo}, NS={NumeroSerie}, Local={Local}, IP={Ip}, DataBateria={DataBateria:yyyy-MM-dd}, DataNobreak={DataNobreak:yyyy-MM-dd}, ProximaVerificacao={ProximasVerificacoes:yyyy-MM-dd}";
}
