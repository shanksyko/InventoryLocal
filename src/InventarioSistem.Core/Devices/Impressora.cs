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

    // ==== ALIAS LEGADOS – usados por AccessInventoryStore.Devices.cs ====
    // Hostname -> NomeImpressora
    public string Hostname
    {
        get => NomeImpressora;
        set => NomeImpressora = value;
    }

    // Modelo -> TipoModelo
    public string Modelo
    {
        get => TipoModelo;
        set => TipoModelo = value;
    }

    // Local -> LocalizacaoAtual
    public string Local
    {
        get => LocalizacaoAtual;
        set => LocalizacaoAtual = value;
    }

    // Não existia de fato no schema de Impressora, usamos como campo solto
    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[Impressora] Id={Id}, Servidor={ServidorImpressao}, Nome={NomeImpressora}, Identificador={IdentificadorFisico}, Switch={LocalizacaoSwitch}, Modelo={TipoModelo}, NS={NumeroSerie}, Tipo={TipoDispositivo}, LocalAtual={LocalizacaoAtual}, LocalAnterior={LocalizacaoAnterior}, FormatoPapel={FormatoPapel}";
}
