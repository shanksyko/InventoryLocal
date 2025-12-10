# Security & SQL Best Practices - Implementation Guide

## Overview
This guide addresses the security issues and best practices identified in the SQL validation.

---

## Issue 1: Plaintext Password Storage ?? CRITICAL

### Problem
Passwords are currently stored and compared in plaintext. This is a major security violation.

### Solution: Implement Password Hashing

**Recommended**: Use bcrypt or PBKDF2

#### Option 1: Using System.Security.Cryptography (Built-in)

```csharp
using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bits
    private const int Iterations = 10000;

    /// <summary>
    /// Hashes a password using PBKDF2
    /// </summary>
    public static string HashPassword(string password)
    {
        using (var salt = RandomNumberGenerator.GetBytes(SaltSize))
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[SaltSize + hash.Length];
                
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, hash.Length);
                
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        byte[] hashBytes = Convert.FromBase64String(passwordHash);
        byte[] salt = new byte[SaltSize];
        
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);
        
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(20);
            
            for (int i = 0; i < hash.Length; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
        }
        
        return true;
    }
}
```

#### Option 2: Using BCrypt.Net-Next (Recommended)

```bash
# Install NuGet package
dotnet add package BCrypt.Net-Next
```

```csharp
using BCrypt.Net;

public static class PasswordHelper
{
    /// <summary>
    /// Hashes a password using bcrypt with work factor 12
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }
}
```

#### Implementation in SqlServerUserStore

Update the `ValidateUserAsync` method:

```csharp
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

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            string storedHash = reader.GetString(0);
            return PasswordHelper.VerifyPassword(password, storedHash);
        }

        return false;
    }
    catch (Exception ex)
    {
        InventoryLogger.Error("SqlServerUserStore", $"Erro ao validar usuário: {ex.Message}");
        return false;
    }
}
```

Update `CreateUserAsync`:

```csharp
public async Task<int> CreateUserAsync(string username, string password, string fullName, bool isActive = true, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        throw new ArgumentException("Username and password are required");

    try
    {
        string hashedPassword = PasswordHelper.HashPassword(password);

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [Users]
            ([Username], [PasswordHash], [FullName], [IsActive], [CreatedAt])
            VALUES (@Username, @PasswordHash, @FullName, @IsActive, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
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
```

Update `UpdatePasswordAsync`:

```csharp
public async Task<bool> UpdatePasswordAsync(string username, string newPassword, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
        return false;

    try
    {
        string hashedPassword = PasswordHelper.HashPassword(newPassword);

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE [Users]
            SET [PasswordHash] = @PasswordHash, [LastPasswordChange] = @LastPasswordChange
            WHERE [Username] = @Username";

        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
        command.Parameters.AddWithValue("@LastPasswordChange", DateTime.Now);
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
```

---

## Issue 2: Add Transaction Support ??

### Problem
No explicit transaction management for multi-step operations.

### Solution

Create a transaction helper:

```csharp
public async Task<bool> AddComputerWithTransactionAsync(Computer computer, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(computer);

    await using var connection = _factory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var transaction = connection.BeginTransaction();
    try
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO [Computadores]
            ([Host], [SerialNumber], [Proprietario], [Departamento], [Matricula], [CreatedAt])
            VALUES (@Host, @SerialNumber, @Proprietario, @Departamento, @Matricula, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        command.Parameters.AddWithValue("@Host", computer.Host ?? "");
        command.Parameters.AddWithValue("@SerialNumber", computer.SerialNumber ?? "");
        command.Parameters.AddWithValue("@Proprietario", computer.Proprietario ?? "");
        command.Parameters.AddWithValue("@Departamento", computer.Departamento ?? "");
        command.Parameters.AddWithValue("@Matricula", computer.Matricula ?? "");
        command.Parameters.AddWithValue("@CreatedAt", computer.CreatedAt ?? DateTime.Now);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        computer.Id = Convert.ToInt32(result);

        await transaction.CommitAsync(cancellationToken);
        InvalidateCache();
        InventoryLogger.Info("SqlServerInventoryStore", $"Computador inserido com transação: Host='{computer.Host}'");
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        InventoryLogger.Error("SqlServerInventoryStore", $"Erro em transação: {ex.Message}", ex);
        throw;
    }
}
```

---

## Issue 3: Add Database Indexes for Performance

### High-Priority Indexes

```sql
-- Users table (authentication queries)
CREATE INDEX IX_Users_Username ON [Users]([Username]);
CREATE INDEX IX_Users_IsActive ON [Users]([IsActive]);

-- Device lookup tables
CREATE INDEX IX_Computadores_Host ON [Computadores]([Host]);
CREATE INDEX IX_Tablets_SerialNumber ON [Tablets]([SerialNumber]);
CREATE INDEX IX_Celulares_Numero ON [Celulares]([Numero]);
CREATE INDEX IX_Impressoras_SerialNumber ON [Impressoras]([SerialNumber]);
CREATE INDEX IX_Dects_Numero ON [Dects]([Numero]);
CREATE INDEX IX_TelefonesCisco_Numero ON [TelefonesCisco]([Numero]);
CREATE INDEX IX_Nobreaks_IpAddress ON [Nobreaks]([IpAddress]);
```

### Implementation in Schema Manager

Add to `SqlServerSchemaManager.EnsureRequiredTables()`:

```csharp
void EnsureIndex(string tableName, string indexName, string columnName)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = $@"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = '{indexName}')
        BEGIN
            CREATE INDEX [{indexName}] ON [{tableName}]([{columnName}])
        END";
    cmd.ExecuteNonQuery();
}

// After creating tables
EnsureIndex("Users", "IX_Users_Username", "Username");
EnsureIndex("Computadores", "IX_Computadores_Host", "Host");
EnsureIndex("Celulares", "IX_Celulares_Numero", "Numero");
```

---

## Issue 4: Add Command Timeout Configuration

### For Long-Running Operations

```csharp
public async Task<List<Computer>> GetAllComputersAsync(int? commandTimeoutSeconds = null, CancellationToken cancellationToken = default)
{
    var computers = new List<Computer>();
    await using var connection = _factory.CreateConnection();
    await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    
    // Set timeout if provided
    if (commandTimeoutSeconds.HasValue)
        command.CommandTimeout = commandTimeoutSeconds.Value;
    
    command.CommandText = "SELECT [Id], [Host], [SerialNumber], [Proprietario], [Departamento], [Matricula], [CreatedAt] FROM [Computadores] ORDER BY [Id]";

    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
        computers.Add(new Computer
        {
            Id = reader.GetInt32(0),
            Host = GetStringSafe(reader, 1),
            // ... other columns
        });
    }
    return computers;
}
```

---

## Issue 5: Add Audit Logging

### Audit Trail for Sensitive Operations

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string Operation { get; set; } // CREATE, UPDATE, DELETE
    public string TableName { get; set; }
    public int? RecordId { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}

public async Task LogAuditAsync(string operation, string tableName, int recordId, string? changedBy = null, string? oldValues = null, string? newValues = null)
{
    try
    {
        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [AuditLog]
            ([Operation], [TableName], [RecordId], [ChangedBy], [Timestamp], [OldValues], [NewValues])
            VALUES (@Operation, @TableName, @RecordId, @ChangedBy, @Timestamp, @OldValues, @NewValues)";

        command.Parameters.AddWithValue("@Operation", operation);
        command.Parameters.AddWithValue("@TableName", tableName);
        command.Parameters.AddWithValue("@RecordId", recordId);
        command.Parameters.AddWithValue("@ChangedBy", changedBy ?? "SYSTEM");
        command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);
        command.Parameters.AddWithValue("@OldValues", oldValues ?? "");
        command.Parameters.AddWithValue("@NewValues", newValues ?? "");

        await command.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        InventoryLogger.Error("AuditLog", $"Falha ao registrar auditoria: {ex.Message}");
    }
}
```

---

## Implementation Checklist

- [ ] **CRITICAL**: Fix Password column references (? Already Done)
- [ ] **CRITICAL**: Implement password hashing (Use bcrypt)
- [ ] Create `PasswordHelper` utility class
- [ ] Update `SqlServerUserStore` to use hashing
- [ ] Create migration script for existing passwords
- [ ] Add transaction support to multi-step operations
- [ ] Add database indexes for performance
- [ ] Add command timeout configuration
- [ ] Implement audit logging for sensitive operations
- [ ] Add unit tests for password validation
- [ ] Add integration tests for authentication flow
- [ ] Update documentation with security requirements

---

## Testing Guidelines

### Unit Tests for Password Hashing

```csharp
[TestFixture]
public class PasswordHelperTests
{
    [Test]
    public void HashPassword_CreatesConsistentHash()
    {
        string password = "TestPassword123!@#";
        string hash1 = PasswordHelper.HashPassword(password);
        string hash2 = PasswordHelper.HashPassword(password);
        
        // Hashes should be different (due to random salt)
        Assert.AreNotEqual(hash1, hash2);
        
        // Both should verify correctly
        Assert.IsTrue(PasswordHelper.VerifyPassword(password, hash1));
        Assert.IsTrue(PasswordHelper.VerifyPassword(password, hash2));
    }

    [Test]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        string password = "TestPassword123!@#";
        string hash = PasswordHelper.HashPassword(password);
        
        Assert.IsFalse(PasswordHelper.VerifyPassword("WrongPassword", hash));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class SqlServerUserStoreTests
{
    [Test]
    public async Task ValidateUser_WithCorrectPassword_ReturnsTrue()
    {
        var store = new SqlServerUserStore(_factory);
        
        // Create user
        var userId = await store.CreateUserAsync("testuser", "TestPassword123!@#", "Test User");
        
        // Validate with correct password
        bool isValid = await store.ValidateUserAsync("testuser", "TestPassword123!@#");
        Assert.IsTrue(isValid);
    }

    [Test]
    public async Task ValidateUser_WithIncorrectPassword_ReturnsFalse()
    {
        var store = new SqlServerUserStore(_factory);
        
        // Create user
        await store.CreateUserAsync("testuser", "TestPassword123!@#", "Test User");
        
        // Validate with wrong password
        bool isValid = await store.ValidateUserAsync("testuser", "WrongPassword");
        Assert.IsFalse(isValid);
    }
}
```

---

## Compliance

- ? OWASP Top 10 - A02:2021 – Cryptographic Failures
- ? OWASP Top 10 - A01:2021 – Broken Access Control
- ? CWE-256: Unprotected Storage of Credentials
- ? CWE-521: Weak Password Requirements

---

**Next Steps**: Implement password hashing immediately, then add additional security enhancements in order of priority.

