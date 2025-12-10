using System.Linq;

namespace InventarioSistem.Core.Devices;

public class Tablet
{
    public int Id { get; set; }

    public string Host { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    public List<string> Imeis { get; set; } = new();

    public string ImeisJoined => string.Join(";", Imeis ?? Enumerable.Empty<string>());

    public DateTime? CreatedAt { get; set; }

    public override string ToString()
        => $"[Tablet] Id={Id}, Host={Host}, NS={SerialNumber}, Local={Local}, Resp={Responsavel}, IMEIs={string.Join(';', Imeis)}";
}
