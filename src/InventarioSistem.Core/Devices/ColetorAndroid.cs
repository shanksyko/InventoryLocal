namespace InventarioSistem.Core.Devices;

public class ColetorAndroid
{
    public int Id { get; set; }

    public string Host { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;

    // Aplicativos instalados
    public bool AppGwsFg { get; set; }
    public bool AppGwsRm { get; set; }
    public bool AppInspection { get; set; }
    public bool AppCuringTbr { get; set; }
    public bool AppCuringPcr { get; set; }
    public bool AppInspectionTbr { get; set; }
    public bool AppQuimico { get; set; }
    public bool AppBuildingTbr { get; set; }
    public bool AppBuildingPcr { get; set; }

    // Sistema Operacional
    public bool OsWinCe { get; set; }
    public bool OsAndroid81 { get; set; }
    public bool OsAndroid10 { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Propriedade helper para exibir apps selecionados
    public string AppsInstalados
    {
        get
        {
            var apps = new List<string>();
            if (AppGwsFg) apps.Add("GWS FG");
            if (AppGwsRm) apps.Add("GWS RM");
            if (AppInspection) apps.Add("Inspection");
            if (AppCuringTbr) apps.Add("Curing TBR");
            if (AppCuringPcr) apps.Add("Curing PCR");
            if (AppInspectionTbr) apps.Add("Inspection TBR");
            if (AppQuimico) apps.Add("Quimico");
            if (AppBuildingTbr) apps.Add("Building TBR");
            if (AppBuildingPcr) apps.Add("Building PCR");
            return string.Join(", ", apps);
        }
    }

    // Propriedade helper para exibir SO selecionado
    public string SistemaOperacional
    {
        get
        {
            var os = new List<string>();
            if (OsWinCe) os.Add("WinCE");
            if (OsAndroid81) os.Add("Android 8.1");
            if (OsAndroid10) os.Add("Android 10.0");
            return string.Join(", ", os);
        }
    }

    public override string ToString()
        => $"[Coletor] Id={Id}, Host={Host}, NS={SerialNumber}, MAC={MacAddress}, IP={IpAddress}, Local={Local}";
}
