namespace InventarioSistem.Core.Devices;

public class CiscoPhone
{
    public int Id { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Ramal { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[CiscoPhone] Id={Id}, Host={Hostname}, MAC={MacAddress}, IP={IpAddress}, Ramal={Ramal}, Resp={Responsavel}";
}
