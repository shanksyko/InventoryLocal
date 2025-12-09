namespace InventarioSistem.Core.Devices;

public class RelogioPonto
{
    public int Id { get; set; }

    public string IdRelogio { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string VersaoFirmwareMrp { get; set; } = string.Empty;
    public string VersaoFirmware { get; set; } = string.Empty;
    public string IdSwitch { get; set; } = string.Empty;
    public string Bateria { get; set; } = string.Empty;
    public string Nobreak { get; set; } = string.Empty;
    public string ProximaAvaliacao { get; set; } = string.Empty;

    // ==== ALIAS LEGADOS ====
    // Hostname -> IdRelogio
    public string Hostname
    {
        get => IdRelogio;
        set => IdRelogio = value;
    }

    // Modelo -> NumeroSerie
    public string Modelo
    {
        get => NumeroSerie;
        set => NumeroSerie = value;
    }

    // IpAddress -> Ip
    public string IpAddress
    {
        get => Ip;
        set => Ip = value;
    }

    // Local -> IdSwitch
    public string Local
    {
        get => IdSwitch;
        set => IdSwitch = value;
    }

    public string Responsavel { get; set; } = string.Empty;

    public override string ToString()
        => $"[RelogioPonto] Id={Id}, IdRelogio={IdRelogio}, IP={Ip}, MAC={Mac}, NS={NumeroSerie}, FirmwareMRP={VersaoFirmwareMrp}, Firmware={VersaoFirmware}, IdSwitch={IdSwitch}, Bateria={Bateria}, Nobreak={Nobreak}, ProximaAvaliacao={ProximaAvaliacao}";
}
