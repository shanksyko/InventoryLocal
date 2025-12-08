using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioSistem.Core.Utils;

public static class DeviceStringUtils
{
    /// <summary>
    /// Normaliza uma string para uso em entidades de domínio:
    /// - Converte null para string.Empty
    /// - Aplica Trim()
    /// </summary>
    public static string NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim();
    }

    /// <summary>
    /// Converte uma lista de IMEIs em uma única string, usando ';' como separador.
    /// - Ignora itens nulos ou vazios.
    /// - Aplica Trim() em cada item.
    /// - Retorna string.Empty se não houver IMEIs válidos.
    /// </summary>
    public static string ImeisToString(IEnumerable<string>? imeis)
    {
        if (imeis is null)
            return string.Empty;

        var normalized = imeis
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToArray();

        if (normalized.Length == 0)
            return string.Empty;

        return string.Join(';', normalized);
    }

    /// <summary>
    /// Converte uma string com IMEIs separados por ';' em uma lista.
    /// - Trata null/empty como lista vazia.
    /// - Ignora itens vazios.
    /// - Aplica Trim() em cada item.
    /// </summary>
    public static List<string> ImeisFromString(string? value)
    {
        var result = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part))
                result.Add(part.Trim());
        }

        return result;
    }
}
