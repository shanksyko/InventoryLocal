using System;
using System.Security.Cryptography;
using System.Text;

namespace InventarioSistem.Core.Entities;

/// <summary>
/// Representa um usuário do sistema (local ou AD)
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } // Nulo se autenticação via AD
    public UserRole Role { get; set; } = UserRole.Visualizador;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public string? Provider { get; set; } = "Local"; // "Local", "ActiveDirectory", etc.

    /// <summary>
    /// Hash uma senha usando SHA256
    /// </summary>
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash
    /// </summary>
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(PasswordHash))
            return false;

        var hash = HashPassword(password);
        return hash == PasswordHash;
    }
}
