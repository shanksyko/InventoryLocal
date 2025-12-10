namespace InventarioSistem.Core.Devices;

public class Nobreak
{
    public int Id { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public override string ToString()
        => $"[Nobreak] Id={Id}, Hostname={Hostname}, Local={Local}, IP={IpAddress}, Modelo={Modelo}, Status={Status}, Serial={SerialNumber}";
}
