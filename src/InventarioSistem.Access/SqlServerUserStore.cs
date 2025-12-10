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
                WHERE [Username] = @Username AND [Password] = @Password AND [IsActive] = 1";

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

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
                ([Username], [Password], [FullName], [IsActive], [CreatedAt])
                VALUES (@Username, @Password, @FullName, @IsActive, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
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
                SET [Password] = @Password
                WHERE [Username] = @Username";

            command.Parameters.AddWithValue("@Password", newPassword);
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
}
