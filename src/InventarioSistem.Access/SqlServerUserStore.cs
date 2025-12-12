using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access;

/// <summary>
/// SQL Server implementation of user authentication and management.
/// Replaces ODBC-based UserStore with native async support.
/// </summary>
public class SqlServerUserStore
{
    private readonly SqlServerConnectionFactory _factory;

    public SqlServerUserStore(SqlServerConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    /// <summary>
    /// Validates user credentials against the database.
    /// </summary>
    public async Task<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT [PasswordHash]
                FROM [Users]
                WHERE [Username] = @Username AND [IsActive] = 1";

            command.Parameters.AddWithValue("@Username", username);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            if (result == null || result == DBNull.Value)
                return false;

            var storedHash = result.ToString();
            
            // Verificar senha usando BCrypt
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao validar usuário: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    public async Task<(int Id, string Username, string FullName, bool IsActive, string Role)?> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT [Id], [Username], [FullName], [IsActive], [Role]
                FROM [Users]
                WHERE [Username] = @Username";

            command.Parameters.AddWithValue("@Username", username);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return (
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetBoolean(3),
                    reader.IsDBNull(4) ? "" : reader.GetString(4)
                );
            }
            return null;
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao obter usuário: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    public async Task<int> CreateUserAsync(string username, string password, string fullName, bool isActive = true, string role = "User", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Username and password are required");

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var passwordHash = Core.Entities.User.HashPassword(password);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO [Users]
                ([Username], [PasswordHash], [FullName], [Role], [IsActive], [CreatedAt])
                VALUES (@Username, @PasswordHash, @FullName, @Role, @IsActive, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@FullName", fullName ?? "");
            command.Parameters.AddWithValue("@Role", role ?? "User");
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var userId = Convert.ToInt32(result);

            InventoryLogger.Info("SqlServerUserStore", $"Usuário criado: {username} (Id={userId})");
            return userId;
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao criar usuário: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(string username, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var passwordHash = Core.Entities.User.HashPassword(newPassword);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [PasswordHash] = @PasswordHash
                WHERE [Username] = @Username";

            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@Username", username);

            await command.ExecuteNonQueryAsync(cancellationToken);
            InventoryLogger.Info("SqlServerUserStore", $"Senha do usuário {username} atualizada");
            return true;
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao atualizar senha: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    public async Task<bool> DeactivateUserAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [IsActive] = 0
                WHERE [Username] = @Username";

            command.Parameters.AddWithValue("@Username", username);

            await command.ExecuteNonQueryAsync(cancellationToken);
            InventoryLogger.Info("SqlServerUserStore", $"Usuário {username} desativado");
            return true;
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao desativar usuário: {ex.Message}");
            return false;
        }
    }

    // Synchronous wrappers for compatibility
    public bool ValidateUser(string username, string password) =>
        ValidateUserAsync(username, password).GetAwaiter().GetResult();

    public (int Id, string Username, string FullName, bool IsActive, string Role)? GetUser(string username) =>
        GetUserAsync(username).GetAwaiter().GetResult();

    public int CreateUser(string username, string password, string fullName, bool isActive = true, string role = "User") =>
        CreateUserAsync(username, password, fullName, isActive, role).GetAwaiter().GetResult();

    public bool UpdatePassword(string username, string newPassword) =>
        UpdatePasswordAsync(username, newPassword).GetAwaiter().GetResult();

    public bool DeactivateUser(string username) =>
        DeactivateUserAsync(username).GetAwaiter().GetResult();

    /// <summary>
    /// Ensures the Users table exists (no-op for SQL Server - table created by SqlServerSchemaManager).
    /// </summary>
    public async Task EnsureUsersTableAsync(CancellationToken cancellationToken = default)
    {
        // Table creation handled by SqlServerSchemaManager.EnsureRequiredTables
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets all users from the database.
    /// </summary>
    public async Task<List<InventarioSistem.Core.Entities.User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = new List<InventarioSistem.Core.Entities.User>();

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"SELECT [Id], [Username], [FullName], [Role], [IsActive], [CreatedAt], [LastLogin]
                                   FROM [Users]
                                   ORDER BY [Username]";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var roleString = reader.IsDBNull(3) ? "Visualizador" : reader.GetString(3);
                var role = roleString.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    ? InventarioSistem.Core.Entities.UserRole.Admin
                    : InventarioSistem.Core.Entities.UserRole.Visualizador;

                users.Add(new InventarioSistem.Core.Entities.User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    FullName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Role = role,
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.IsDBNull(5) ? DateTime.UtcNow : reader.GetDateTime(5),
                    LastLogin = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                });
            }
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao listar usuários: {ex.Message}");
        }

        return users;
    }

    /// <summary>
    /// Adds a new user (alias for CreateUserAsync).
    /// </summary>
    public async Task AddUserAsync(string username, string password, string fullName, bool isActive = true, string role = "User", CancellationToken cancellationToken = default)
    {
        await CreateUserAsync(username, password, fullName, isActive, role, cancellationToken);
    }

    /// <summary>
    /// Updates a user's information.
    /// </summary>
    public async Task UpdateUserAsync(string userId, string username, string fullName, bool isActive, string role, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [Username] = @Username, [FullName] = @FullName, [IsActive] = @IsActive, [Role] = @Role
                WHERE [Id] = @Id";

            command.Parameters.AddWithValue("@Username", username ?? "");
            command.Parameters.AddWithValue("@FullName", fullName ?? "");
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@Role", role ?? "Visualizador");
            command.Parameters.AddWithValue("@Id", int.Parse(userId));

            await command.ExecuteNonQueryAsync(cancellationToken);
            InventoryLogger.Info("SqlServerUserStore", $"Usuário {username} atualizado");
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao atualizar usuário: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates a user's password by user ID.
    /// </summary>
    public async Task UpdateUserPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("User ID and password are required");

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var passwordHash = Core.Entities.User.HashPassword(newPassword);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [PasswordHash] = @PasswordHash
                WHERE [Id] = @Id";

            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@Id", int.Parse(userId));

            await command.ExecuteNonQueryAsync(cancellationToken);
            InventoryLogger.Info("SqlServerUserStore", $"Senha do usuário ID={userId} atualizada");
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao atualizar senha: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes a user by user ID.
    /// </summary>
    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM [Users] WHERE [Id] = @Id";
            command.Parameters.AddWithValue("@Id", int.Parse(userId));

            await command.ExecuteNonQueryAsync(cancellationToken);
            InventoryLogger.Info("SqlServerUserStore", $"Usuário ID={userId} deletado");
        }
        catch (Exception ex)
        {
            InventoryLogger.Error("SqlServerUserStore", $"Erro ao deletar usuário: {ex.Message}");
            throw;
        }
    }
}
