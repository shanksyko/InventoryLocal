using System.Text.RegularExpressions;

namespace InventarioSistem.Core.Utilities;

public static class PasswordValidator
{
    private const int MIN_LENGTH = 8;
    private const int MAX_LENGTH = 128;

    public static (bool isValid, string? errorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "A senha não pode ser vazia.");
        }

        if (password.Length < MIN_LENGTH)
        {
            return (false, $"A senha deve ter no mínimo {MIN_LENGTH} caracteres.");
        }

        if (password.Length > MAX_LENGTH)
        {
            return (false, $"A senha não pode ter mais de {MAX_LENGTH} caracteres.");
        }

        // Verificar letra maiúscula
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return (false, "A senha deve conter pelo menos uma letra maiúscula.");
        }

        // Verificar letra minúscula
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return (false, "A senha deve conter pelo menos uma letra minúscula.");
        }

        // Verificar número
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return (false, "A senha deve conter pelo menos um número.");
        }

        // Verificar caractere especial
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
        {
            return (false, "A senha deve conter pelo menos um caractere especial (!@#$%^&*()_+-=[]{}|etc).");
        }

        return (true, null);
    }

    public static string GetPasswordRequirements()
    {
        return $"A senha deve ter:\n" +
               $"• Mínimo de {MIN_LENGTH} caracteres\n" +
               $"• Pelo menos uma letra maiúscula\n" +
               $"• Pelo menos uma letra minúscula\n" +
               $"• Pelo menos um número\n" +
               $"• Pelo menos um caractere especial (!@#$%^&*etc)";
    }
}
