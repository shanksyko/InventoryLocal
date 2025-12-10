namespace InventarioSistem.Core.Devices;

public class ColetorAndroid
{
    public int Id { get; set; }

    public string Host { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public override string ToString()
        => $"[Coletor] Id={Id}, Host={Host}, NS={SerialNumber}, MAC={MacAddress}, IP={IpAddress}, Local={Local}";
}
