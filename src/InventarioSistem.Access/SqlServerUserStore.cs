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
                SELECT COUNT(*)
                FROM [Users]
                WHERE [Username] = @Username AND [PasswordHash] = @PasswordHash AND [IsActive] = 1";

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", password);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return ((int?)result ?? 0) > 0;
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
    public async Task<(int Id, string Username, string FullName, bool IsActive)?> GetUserAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT [Id], [Username], [FullName], [IsActive]
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
                    reader.GetBoolean(3)
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
    public async Task<int> CreateUserAsync(string username, string password, string fullName, bool isActive = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Username and password are required");

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO [Users]
                ([Username], [PasswordHash], [FullName], [IsActive], [CreatedAt])
                VALUES (@Username, @PasswordHash, @FullName, @IsActive, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", password);
            command.Parameters.AddWithValue("@FullName", fullName ?? "");
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

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [PasswordHash] = @PasswordHash
                WHERE [Username] = @Username";

            command.Parameters.AddWithValue("@PasswordHash", newPassword);
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

    public (int Id, string Username, string FullName, bool IsActive)? GetUser(string username) =>
        GetUserAsync(username).GetAwaiter().GetResult();

    public int CreateUser(string username, string password, string fullName, bool isActive = true) =>
        CreateUserAsync(username, password, fullName, isActive).GetAwaiter().GetResult();

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
    public async Task<List<(int Id, string Username, string FullName, bool IsActive)>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = new List<(int Id, string Username, string FullName, bool IsActive)>();

        try
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT [Id], [Username], [FullName], [IsActive] FROM [Users] ORDER BY [Username]";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                users.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetBoolean(3)
                ));
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
    public async Task AddUserAsync(string username, string password, string fullName, bool isActive = true, CancellationToken cancellationToken = default)
    {
        await CreateUserAsync(username, password, fullName, isActive, cancellationToken);
    }

    /// <summary>
    /// Updates a user's information.
    /// </summary>
    public async Task UpdateUserAsync(string userId, string username, string fullName, bool isActive, CancellationToken cancellationToken = default)
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
                SET [Username] = @Username, [FullName] = @FullName, [IsActive] = @IsActive
                WHERE [Id] = @Id";

            command.Parameters.AddWithValue("@Username", username ?? "");
            command.Parameters.AddWithValue("@FullName", fullName ?? "");
            command.Parameters.AddWithValue("@IsActive", isActive);
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

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE [Users]
                SET [PasswordHash] = @PasswordHash
                WHERE [Id] = @Id";

            command.Parameters.AddWithValue("@PasswordHash", newPassword);
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
