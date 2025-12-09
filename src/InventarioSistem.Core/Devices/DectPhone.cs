namespace InventarioSistem.Core.Devices;

public class DectPhone
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Ipei { get; set; } = string.Empty;

    public override string ToString()
        => $"[DectPhone] Id={Id}, Modelo={Modelo}, Numero={Numero}, Resp={Responsavel}, Area={Area}, IPEI={Ipei}";
}
