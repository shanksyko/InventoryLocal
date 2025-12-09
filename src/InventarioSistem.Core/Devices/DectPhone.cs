namespace InventarioSistem.Core.Devices;

public class DectPhone
{
    public int Id { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Ipei { get; set; } = string.Empty;

    // ==== ALIAS LEGADOS ====
    // Hostname -> Modelo
    public string Hostname
    {
        get => Modelo;
        set => Modelo = value;
    }

    // NumeroSerie -> Ipei (melhor aproximação)
    public string NumeroSerie
    {
        get => Ipei;
        set => Ipei = value;
    }

    // Ramal -> Numero
    public string Ramal
    {
        get => Numero;
        set => Numero = value;
    }

    public override string ToString()
        => $"[DectPhone] Id={Id}, Modelo={Modelo}, Numero={Numero}, Resp={Responsavel}, Area={Area}, IPEI={Ipei}";
}
