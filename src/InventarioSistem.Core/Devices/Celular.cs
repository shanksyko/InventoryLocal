namespace InventarioSistem.Core.Devices;

public class Celular
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Proprietario { get; set; } = string.Empty;

    public List<string> Imeis { get; set; } = new();

    public override string ToString()
        => $"[Celular] Id={Id}, Modelo={Modelo}, Numero={Numero}, Prop={Proprietario}, IMEIs={string.Join(';', Imeis)}";
}
