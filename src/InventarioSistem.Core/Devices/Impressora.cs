namespace InventarioSistem.Core.Devices;

public class Impressora
{
    public int Id { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[Impressora] Id={Id}, Host={Hostname}, Modelo={Modelo}, NS={NumeroSerie}, Local={Local}, Resp={Responsavel}";
}
