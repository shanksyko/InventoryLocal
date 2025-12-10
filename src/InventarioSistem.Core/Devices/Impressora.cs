namespace InventarioSistem.Core.Devices;

public class Impressora
{
    public int Id { get; set; }

    // Campos principais
    public string Nome { get; set; } = string.Empty;
    public string TipoModelo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string LocalAtual { get; set; } = string.Empty;
    public string LocalAnterior { get; set; } = string.Empty;

    // Aliases para compatibilidade com código legado
    public string Modelo
    {
        get => TipoModelo;
        set => TipoModelo = value;
    }

    public string NumeroSerie
    {
        get => SerialNumber;
        set => SerialNumber = value;
    }

    public string Local
    {
        get => LocalAtual;
        set => LocalAtual = value;
    }

    public string Hostname
    {
        get => Nome;
        set => Nome = value;
    }

    public string Responsavel { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }

    public override string ToString()
        => $"[Impressora] Id={Id}, Nome={Nome}, Modelo={TipoModelo}, NS={SerialNumber}, LocalAtual={LocalAtual}, LocalAnterior={LocalAnterior}";
}
