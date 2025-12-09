namespace InventarioSistem.Core.Devices;

public class Monitor
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;

    /// <summary>
    /// Hostname do computador ao qual este monitor está vinculado.
    /// </summary>
    public string ComputadorVinculado { get; set; } = string.Empty;

    public override string ToString()
        => $"[Monitor] Id={Id}, Modelo={Modelo}, Serial={SerialNumber}, Local={Local}, Resp={Responsavel}, PC={ComputadorVinculado}";
}
