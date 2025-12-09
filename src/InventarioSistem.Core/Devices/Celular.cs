namespace InventarioSistem.Core.Devices;

public class Celular
{
    public int Id { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Proprietario { get; set; } = string.Empty;

    public string Imei1 { get; set; } = string.Empty;
    public string Imei2 { get; set; } = string.Empty;

    public override string ToString()
        => $"[Celular] Id={Id}, Host={Hostname}, Modelo={Modelo}, Numero={Numero}, Prop={Proprietario}, IMEI1={Imei1}, IMEI2={Imei2}";
}
