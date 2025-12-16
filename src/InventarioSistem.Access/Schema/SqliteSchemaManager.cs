using System;
using Microsoft.Data.Sqlite;
using InventarioSistem.Access.Schema;

namespace InventarioSistem.Access.Schema;

/// <summary>
/// Schema manager para SQLite - adapta estrutura do SQL Server
/// </summary>
public static class SqliteSchemaManager
{
    public static void EnsureRequiredTables(SqliteConnectionFactory factory)
    {
        using var conn = factory.CreateConnection();
        conn.Open();

        // Users
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT,
                    Role TEXT,
                    IsActive INTEGER DEFAULT 1,
                    CreatedAt TEXT,
                    LastPasswordChange TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        // Computers
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Computers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Host TEXT,
                    SerialNumber TEXT,
                    Proprietario TEXT,
                    Departamento TEXT,
                    Matricula TEXT,
                    CreatedAt TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        // Tablets
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tablets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Host TEXT,
                    SerialNumber TEXT,
                    Local TEXT,
                    Responsavel TEXT,
                    Imeis TEXT,
                    CreatedAt TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        // Coletores
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Coletores (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Host TEXT,
                    SerialNumber TEXT,
                    MacAddress TEXT,
                    IpAddress TEXT,
                    Local TEXT,
                    CreatedAt TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        // Celulares
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Celulares (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Hostname TEXT,
                    Modelo TEXT,
                    Numero TEXT,
                    Proprietario TEXT,
                    Imei1 TEXT,
                    Imei2 TEXT,
                    CreatedAt TEXT
                )";
            cmd.ExecuteNonQuery();
        }

        conn.Close();
    }
}
