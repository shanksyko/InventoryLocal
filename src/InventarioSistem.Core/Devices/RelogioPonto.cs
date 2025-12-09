namespace InventarioSistem.Core.Devices;

public class RelogioPonto
{
    public int Id { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[RelogioPonto] Id={Id}, Host={Hostname}, Modelo={Modelo}, NS={NumeroSerie}, IP={IpAddress}, Local={Local}, Resp={Responsavel}";
}
