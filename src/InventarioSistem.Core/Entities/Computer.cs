using System.Text;

namespace InventarioSistem.Core.Entities;

public class Computer : Device
{
    public override DeviceType Type => DeviceType.Computer;
    public string SistemaOperacional { get; set; } = string.Empty;
    public string Processador { get; set; } = string.Empty;
    public int MemoriaRamGb { get; set; }
    public int ArmazenamentoGb { get; set; }

    public override string ToMultilineString()
    {
        var builder = new StringBuilder(base.ToMultilineString());
        builder.AppendLine($"Processador: {Processador}");
        builder.AppendLine($"Memória: {MemoriaRamGb} GB");
        builder.AppendLine($"Armazenamento: {ArmazenamentoGb} GB");
        builder.AppendLine($"Sistema Operacional: {SistemaOperacional}");
        return builder.ToString();
    }
}
