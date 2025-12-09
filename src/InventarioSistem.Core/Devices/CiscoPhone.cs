namespace InventarioSistem.Core.Devices;

public class CiscoPhone
{
    public int Id { get; set; }

    public string Responsavel { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;

    // Aliases para compatibilidade
    public string Modelo { get; set; } = string.Empty;

    public string Hostname
    {
        get => Responsavel;
        set => Responsavel = value;
    }

    public string IpAddress { get; set; } = string.Empty;

    public string Ramal
    {
        get => Numero;
        set => Numero = value;
    }

    public string Serial { get; set; } = string.Empty;

    public override string ToString()
        => $"[CiscoPhone] Id={Id}, Numero={Numero}, Resp={Responsavel}, Local={Local}, MAC={MacAddress}";
}
