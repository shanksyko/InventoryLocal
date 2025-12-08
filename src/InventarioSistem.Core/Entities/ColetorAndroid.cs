using System.Text;

namespace InventarioSistem.Core.Entities;

public class ColetorAndroid : Device
{
    public override DeviceType Type => DeviceType.ColetorAndroid;
    public string VersaoAndroid { get; set; } = string.Empty;
    public string FabricanteScanner { get; set; } = string.Empty;
    public bool PossuiCarregadorBase { get; set; }

    public override string ToMultilineString()
    {
        var builder = new StringBuilder(base.ToMultilineString());
        builder.AppendLine($"Android: {VersaoAndroid}");
        builder.AppendLine($"Scanner: {FabricanteScanner}");
        builder.AppendLine($"Base de carga: {(PossuiCarregadorBase ? "Sim" : "Não")}");
        return builder.ToString();
    }
}
