namespace InventarioSistem.Core.Devices;

public class CiscoPhone
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;

    public override string ToString()
        => $"[CiscoPhone] Id={Id}, Modelo={Modelo}, Numero={Numero}, Resp={Responsavel}, Area={Area}, MAC={MacAddress}, Serial={Serial}";
}
