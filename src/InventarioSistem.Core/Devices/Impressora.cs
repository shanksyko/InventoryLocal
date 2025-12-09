namespace InventarioSistem.Core.Devices;

public class Impressora
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string TipoModelo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string LocalizacaoAtual { get; set; } = string.Empty;
    public string LocalizacaoAnterior { get; set; } = string.Empty;

    public override string ToString()
        => $"[Impressora] Id={Id}, Nome={Nome}, Modelo={TipoModelo}, NS={SerialNumber}, LocalAtual={LocalizacaoAtual}, LocalAnterior={LocalizacaoAnterior}";
}
