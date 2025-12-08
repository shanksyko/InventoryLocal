using System.Text;
using InventarioSistem.Core.Utilities;

namespace InventarioSistem.Core.Entities;

public abstract class Device
{
    public int Id { get; set; }
    public abstract DeviceType Type { get; }
    public string Patrimonio { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string? Imei { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public string Localizacao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public virtual string ToMultilineString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Tipo: {Type}");
        builder.AppendLine($"Patrimônio: {Patrimonio}");
        builder.AppendLine($"Marca/Modelo: {Marca} {Modelo}".Trim());
        builder.AppendLine($"Número de Série: {NumeroSerie}");
        if (!string.IsNullOrWhiteSpace(Imei))
        {
            builder.AppendLine($"IMEI: {Imei}");
        }

        builder.AppendLine($"Responsável: {Responsavel}");
        builder.AppendLine($"Localização: {Localizacao}");

        if (!string.IsNullOrWhiteSpace(Observacoes))
        {
            builder.AppendLine($"Obs.: {StringUtilities.NormalizeWhitespace(Observacoes)}");
        }

        builder.AppendLine($"Atualizado em: {AtualizadoEm:yyyy-MM-dd HH:mm}");
        return builder.ToString();
    }

    public override string ToString()
    {
        var summary = $"[{Type}] {Patrimonio} - {Marca} {Modelo}".Trim();
        if (!string.IsNullOrWhiteSpace(Localizacao))
        {
            summary += $" ({Localizacao})";
        }

        return summary;
    }
}
