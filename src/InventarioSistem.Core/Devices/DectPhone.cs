namespace InventarioSistem.Core.Devices;

public class DectPhone
{
    public int Id { get; set; }

    public string Responsavel { get; set; } = string.Empty;
    public string Ipei { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;

    public override string ToString()
        => $"[DectPhone] Id={Id}, Numero={Numero}, Resp={Responsavel}, Local={Local}, IPEI={Ipei}, MAC={MacAddress}";
}
