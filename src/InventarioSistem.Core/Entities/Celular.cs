using System.Text;

namespace InventarioSistem.Core.Entities;

public class Celular : Device
{
    public override DeviceType Type => DeviceType.Celular;
    public string LinhaTelefonica { get; set; } = string.Empty;
    public bool Corporativo { get; set; }
    public string VersaoAndroid { get; set; } = string.Empty;

    public override string ToMultilineString()
    {
        var builder = new StringBuilder(base.ToMultilineString());
        builder.AppendLine($"Linha: {LinhaTelefonica}");
        builder.AppendLine($"Uso corporativo: {(Corporativo ? "Sim" : "Não")}");
        builder.AppendLine($"Android: {VersaoAndroid}");
        return builder.ToString();
    }
}
