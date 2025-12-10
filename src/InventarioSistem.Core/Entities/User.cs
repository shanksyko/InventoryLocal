using System;

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
    public bool IsFirstLogin { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public string? Provider { get; set; } = "Local"; // "Local", "ActiveDirectory", etc.

    /// <summary>
    /// Hash uma senha usando BCrypt (seguro com salt automático)
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash
    /// </summary>
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(PasswordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }
        catch
        {
            return false;
        }
    }
}
