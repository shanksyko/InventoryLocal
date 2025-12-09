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

    public override string ToString()
        => $"[RelogioPonto] Id={Id}, IdRelogio={IdRelogio}, IP={Ip}, MAC={Mac}, NS={NumeroSerie}, FirmwareMRP={VersaoFirmwareMrp}, Firmware={VersaoFirmware}, IdSwitch={IdSwitch}, Bateria={Bateria}, Nobreak={Nobreak}, ProximaAvaliacao={ProximaAvaliacao}";
}
