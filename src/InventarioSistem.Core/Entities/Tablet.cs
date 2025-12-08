using System.Text;

namespace InventarioSistem.Core.Entities;

public class Tablet : Device
{
    public override DeviceType Type => DeviceType.Tablet;
    public string VersaoAndroid { get; set; } = string.Empty;
    public bool PossuiTeclado { get; set; }
    public string LinhaTelefonica { get; set; } = string.Empty;

    public override string ToMultilineString()
    {
        var builder = new StringBuilder(base.ToMultilineString());
        builder.AppendLine($"Android: {VersaoAndroid}");
        builder.AppendLine($"Linha: {LinhaTelefonica}");
        builder.AppendLine($"Teclado destacado: {(PossuiTeclado ? "Sim" : "Não")}");
        return builder.ToString();
    }
}
