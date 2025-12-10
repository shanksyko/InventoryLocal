using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.Access;

/// <summary>
/// Gerencia usuários no banco Access
/// </summary>
public class UserStore
{
    private readonly AccessConnectionFactory _factory;

    public UserStore(AccessConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Garante que a tabela Users existe
    /// </summary>
    public async Task EnsureUsersTableAsync()
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        bool tableCreated = false;
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE 1=0";
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // Tabela não existe, criar
            using var create = conn.CreateCommand();
            create.CommandText = @"
                CREATE TABLE Users (
                    Id AUTOINCREMENT PRIMARY KEY,
                    Username TEXT(100) NOT NULL UNIQUE,
                    PasswordHash TEXT(500),
                    Role TEXT(50) NOT NULL,
                    Email TEXT(255),
                    FullName TEXT(255),
                    IsActive YESNO NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    LastLogin DATETIME,
                    Provider TEXT(50)
                )
            ";
            create.ExecuteNonQuery();
            tableCreated = true;
        }

        // Se tabela foi criada agora, adicionar admin padrão
        if (tableCreated)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Users";
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            
            if (count == 0)
            {
                using var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, Role, Email, FullName, IsActive, CreatedAt, Provider)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                ";
                insertCmd.Parameters.AddWithValue("@username", "admin");
                insertCmd.Parameters.AddWithValue("@passwordHash", User.HashPassword("admin123"));
                insertCmd.Parameters.AddWithValue("@role", "Admin");
                insertCmd.Parameters.AddWithValue("@email", "admin@inventory.local");
                insertCmd.Parameters.AddWithValue("@fullName", "Administrador");
                insertCmd.Parameters.AddWithValue("@isActive", true);
                insertCmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                insertCmd.Parameters.AddWithValue("@provider", "Local");
                
                insertCmd.ExecuteNonQuery();
            }
        }

        conn.Close();
    }

    /// <summary>
    /// Obtém um usuário por username
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash, Role, Email, FullName, IsActive, CreatedAt, LastLogin, Provider FROM Users WHERE Username = ?";
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }

        return null;
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash, Role, Email, FullName, IsActive, CreatedAt, LastLogin, Provider FROM Users WHERE Id = ?";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }

        return null;
    }

    /// <summary>
    /// Lista todos os usuários
    /// </summary>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash, Role, Email, FullName, IsActive, CreatedAt, LastLogin, Provider FROM Users ORDER BY CreatedAt DESC";

        var users = new List<User>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    /// <summary>
    /// Adiciona um novo usuário
    /// </summary>
    public async Task<User> AddUserAsync(User user)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (Username, PasswordHash, Role, Email, FullName, IsActive, CreatedAt, LastLogin, Provider)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        ";
        cmd.Parameters.AddWithValue("@username", user.Username ?? string.Empty);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.Parameters.AddWithValue("@email", user.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@fullName", user.FullName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@isActive", user.IsActive);
        cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);
        cmd.Parameters.AddWithValue("@lastLogin", user.LastLogin ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@provider", user.Provider ?? "Local");

        cmd.ExecuteNonQuery();

        // Retorna o usuário com ID atualizado
        if (string.IsNullOrWhiteSpace(user.Username))
            return user;
        
        return (await GetUserByUsernameAsync(user.Username))!;
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Users SET
                Email = ?,
                FullName = ?,
                Role = ?,
                IsActive = ?,
                LastLogin = ?,
                Provider = ?
            WHERE Id = ?
        ";
        cmd.Parameters.AddWithValue("@email", user.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@fullName", user.FullName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.Parameters.AddWithValue("@isActive", user.IsActive);
        cmd.Parameters.AddWithValue("@lastLogin", user.LastLogin ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@provider", user.Provider ?? "Local");
        cmd.Parameters.AddWithValue("@id", user.Id);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Atualiza a senha de um usuário
    /// </summary>
    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET PasswordHash = ? WHERE Id = ?";
        cmd.Parameters.AddWithValue("@passwordHash", newPasswordHash);
        cmd.Parameters.AddWithValue("@id", userId);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Atualiza o último login
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET LastLogin = ? WHERE Id = ?";
        cmd.Parameters.AddWithValue("@lastLogin", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@id", userId);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Deleta um usuário
    /// </summary>
    public async Task DeleteUserAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await Task.Run(() => conn.Open());

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id = ?";
        cmd.Parameters.AddWithValue("@id", userId);

        cmd.ExecuteNonQuery();
    }

    private static User MapUser(IDataReader reader)
    {
        return new User
        {
            Id = Convert.ToInt32(reader["Id"]),
            Username = reader["Username"]?.ToString() ?? string.Empty,
            PasswordHash = reader["PasswordHash"] is DBNull ? null : reader["PasswordHash"]?.ToString(),
            Role = Enum.Parse<UserRole>(reader["Role"]?.ToString() ?? "Visualizador"),
            Email = reader["Email"] is DBNull ? null : reader["Email"]?.ToString(),
            FullName = reader["FullName"] is DBNull ? null : reader["FullName"]?.ToString(),
            IsActive = Convert.ToBoolean(reader["IsActive"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            LastLogin = reader["LastLogin"] is DBNull ? null : Convert.ToDateTime(reader["LastLogin"]),
            Provider = reader["Provider"]?.ToString() ?? "Local"
        };
    }
}
