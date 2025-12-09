namespace InventarioSistem.Core.Devices;

public class DectPhone
{
    public int Id { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string Ramal { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[DectPhone] Id={Id}, Host={Hostname}, Ramal={Ramal}, NS={NumeroSerie}, Resp={Responsavel}";
}
