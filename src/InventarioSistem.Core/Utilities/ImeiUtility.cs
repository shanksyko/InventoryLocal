using System.Text.RegularExpressions;

namespace InventarioSistem.Core.Utilities;

public static class ImeiUtility
{
    private static readonly Regex DigitsOnly = new("[^0-9]", RegexOptions.Compiled);

    public static string? Normalize(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
        {
            return null;
        }

        return DigitsOnly.Replace(imei, string.Empty).Trim();
    }

    public static bool IsValid(string? imei)
    {
        var normalized = Normalize(imei);
        if (string.IsNullOrEmpty(normalized) || normalized.Length != 15)
        {
            return false;
        }

        return CheckLuhn(normalized);
    }

    private static bool CheckLuhn(string normalized)
    {
        var sum = 0;
        var alternate = false;
        for (var i = normalized.Length - 1; i >= 0; i--)
        {
            var digit = normalized[i] - '0';
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    public static void EnsureValid(string? imei)
    {
        if (!IsValid(imei))
        {
            throw new ArgumentException("IMEI inválido", nameof(imei));
        }
    }
}
