namespace InventarioSistem.Core.Devices;

public class Televisor
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;

    public string NumeroSerie
    {
        get => SerialNumber;
        set => SerialNumber = value;
    }

    public string Hostname
    {
        get => Modelo;
        set => Modelo = value;
    }

    public DateTime? CreatedAt { get; set; }

    public override string ToString()
        => $"[Televisor] Id={Id}, Modelo={Modelo}, NS={SerialNumber}, Local={Local}";
}
