namespace InventarioSistem.Core.Devices;

public class Impressora
{
    public int Id { get; set; }

    public string ServidorImpressao { get; set; } = string.Empty;
    public string NomeImpressora { get; set; } = string.Empty;
    public string IdentificadorFisico { get; set; } = string.Empty;
    public string LocalizacaoSwitch { get; set; } = string.Empty;
    public string TipoModelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string TipoDispositivo { get; set; } = string.Empty;
    public string LocalizacaoAtual { get; set; } = string.Empty;
    public string LocalizacaoAnterior { get; set; } = string.Empty;
    public string FormatoPapel { get; set; } = string.Empty;

    public override string ToString()
        => $"[Impressora] Id={Id}, Servidor={ServidorImpressao}, Nome={NomeImpressora}, Identificador={IdentificadorFisico}, Switch={LocalizacaoSwitch}, Modelo={TipoModelo}, NS={NumeroSerie}, Tipo={TipoDispositivo}, LocalAtual={LocalizacaoAtual}, LocalAnterior={LocalizacaoAnterior}, FormatoPapel={FormatoPapel}";
}
