using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace InventarioSistem.Access.DataMigration;

/// <summary>
/// Realiza migrações de dados entre diferentes bancos de dados
/// Suporta: LocalDB ↔ SQL Server ↔ Arquivo .mdf
/// </summary>
public class DatabaseMigrator
{
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TablesCount { get; set; }
        public long RowsMigrated { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Migra dados de um banco origem para um banco destino
    /// </summary>
    public static async System.Threading.Tasks.Task<MigrationResult> MigrateAsync(
        string sourceConnectionString,
        string targetConnectionString,
        IProgress<string>? progress = null)
    {
        var result = new MigrationResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            progress?.Report("🔄 Iniciando migração de dados...");

            // Validar conexões
            progress?.Report("🔌 Validando conexões...");
            if (!await TestConnectionAsync(sourceConnectionString))
            {
                result.Success = false;
                result.Message = "❌ Erro: Não foi possível conectar ao banco de origem";
                return result;
            }

            if (!await TestConnectionAsync(targetConnectionString))
            {
                result.Success = false;
                result.Message = "❌ Erro: Não foi possível conectar ao banco de destino";
                return result;
            }

            // Obter lista de tabelas do banco origem
            progress?.Report("📊 Analisando tabelas...");
            var tables = await GetTablesAsync(sourceConnectionString);
            result.TablesCount = tables.Count;

            if (tables.Count == 0)
            {
                result.Success = true;
                result.Message = "✅ Banco de origem vazio (nenhuma tabela para migrar)";
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Desabilitar constraints no destino
            progress?.Report("🔐 Preparando banco de destino...");
            await DisableForeignKeysAsync(targetConnectionString);

            // Migrar dados de cada tabela
            long totalRows = 0;
            foreach (var table in tables)
            {
                progress?.Report($"📋 Migrando tabela: {table}...");
                var rows = await MigrateTableAsync(sourceConnectionString, targetConnectionString, table);
                totalRows += rows;
                progress?.Report($"✅ {table}: {rows} linhas migradas");
            }

            // Reabilitar constraints
            progress?.Report("🔒 Finalizando banco de destino...");
            await EnableForeignKeysAsync(targetConnectionString);

            // Validar integridade
            progress?.Report("🔍 Validando integridade...");
            var validationErrors = await ValidateDataIntegrityAsync(sourceConnectionString, targetConnectionString, tables);
            
            if (validationErrors.Count > 0)
            {
                result.Warnings.AddRange(validationErrors);
            }

            stopwatch.Stop();
            result.Success = true;
            result.Message = $"✅ Migração concluída com sucesso!";
            result.RowsMigrated = totalRows;
            result.Duration = stopwatch.Elapsed;
            progress?.Report($"🎉 Migração concluída: {totalRows} linhas em {stopwatch.Elapsed.TotalSeconds:F2}s");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Message = $"❌ Erro durante migração: {ex.Message}";
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    /// <summary>
    /// Testa a conexão com um banco de dados
    /// </summary>
    private static async System.Threading.Tasks.Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            conn.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtém lista de tabelas do banco de dados (excluindo tabelas do sistema)
    /// </summary>
    private static async System.Threading.Tasks.Task<List<string>> GetTablesAsync(string connectionString)
    {
        var tables = new List<string>();

        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_SCHEMA = 'dbo'
                ORDER BY TABLE_NAME";

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter lista de tabelas: {ex.Message}", ex);
        }

        return tables;
    }

    /// <summary>
    /// Migra dados de uma tabela específica
    /// </summary>
    private static async System.Threading.Tasks.Task<long> MigrateTableAsync(
        string sourceConnectionString,
        string targetConnectionString,
        string tableName)
    {
        long rowsMigrated = 0;

        try
        {
            // Obter dados da origem
            var data = new DataTable();
            using (var sourceConn = new SqlConnection(sourceConnectionString))
            {
                await sourceConn.OpenAsync();
                using var cmd = sourceConn.CreateCommand();
                cmd.CommandText = $"SELECT * FROM [{tableName}]";
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(data);
            }

            if (data.Rows.Count == 0)
                return 0;

            // Inserir dados no destino
            using (var targetConn = new SqlConnection(targetConnectionString))
            {
                await targetConn.OpenAsync();

                // Limpar tabela destino se existir dados
                using var deleteCmd = targetConn.CreateCommand();
                deleteCmd.CommandText = $"DELETE FROM [{tableName}]";
                await deleteCmd.ExecuteNonQueryAsync();

                // Inserir novos dados
                using var insertCmd = targetConn.CreateCommand();
                foreach (DataRow row in data.Rows)
                {
                    var columns = string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]"));
                    var values = string.Join(", ", data.Columns.Cast<DataColumn>().Select((c, i) => $"@p{i}"));
                    
                    insertCmd.CommandText = $"INSERT INTO [{tableName}] ({columns}) VALUES ({values})";
                    insertCmd.Parameters.Clear();

                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        var value = row[i] ?? DBNull.Value;
                        insertCmd.Parameters.AddWithValue($"@p{i}", value);
                    }

                    await insertCmd.ExecuteNonQueryAsync();
                    rowsMigrated++;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao migrar tabela {tableName}: {ex.Message}", ex);
        }

        return rowsMigrated;
    }

    /// <summary>
    /// Desabilita todas as foreign keys para não bloquear inserção
    /// </summary>
    private static async System.Threading.Tasks.Task DisableForeignKeysAsync(string connectionString)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao desabilitar constraints: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Reabilita todas as foreign keys
    /// </summary>
    private static async System.Threading.Tasks.Task EnableForeignKeysAsync(string connectionString)
    {
        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao reabilitar constraints: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Valida integridade dos dados migrados
    /// </summary>
    private static async System.Threading.Tasks.Task<List<string>> ValidateDataIntegrityAsync(
        string sourceConnectionString,
        string targetConnectionString,
        List<string> tables)
    {
        var warnings = new List<string>();

        try
        {
            using var sourceConn = new SqlConnection(sourceConnectionString);
            using var targetConn = new SqlConnection(targetConnectionString);

            await sourceConn.OpenAsync();
            await targetConn.OpenAsync();

            foreach (var table in tables)
            {
                // Contar linhas em ambos bancos
                long sourceCount = 0;
                long targetCount = 0;

                using var sourceCmd = sourceConn.CreateCommand();
                sourceCmd.CommandText = $"SELECT COUNT(*) FROM [{table}]";
                var sourceResult = await sourceCmd.ExecuteScalarAsync();
                sourceCount = sourceResult != null ? (long)(int)sourceResult : 0;

                using var targetCmd = targetConn.CreateCommand();
                targetCmd.CommandText = $"SELECT COUNT(*) FROM [{table}]";
                var targetResult = await targetCmd.ExecuteScalarAsync();
                targetCount = targetResult != null ? (long)(int)targetResult : 0;

                if (sourceCount != targetCount)
                {
                    warnings.Add($"⚠️  {table}: {sourceCount} linhas na origem, {targetCount} no destino");
                }
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"⚠️  Erro ao validar: {ex.Message}");
        }

        return warnings;
    }

    /// <summary>
    /// Gera script SQL para backup dos dados
    /// </summary>
    public static async System.Threading.Tasks.Task<string> GenerateBackupScriptAsync(string connectionString)
    {
        var script = new StringBuilder();
        script.AppendLine("-- Backup de dados gerado em " + DateTime.Now);
        script.AppendLine("-- =====================================================\n");

        try
        {
            var tables = await GetTablesAsync(connectionString);

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            foreach (var table in tables)
            {
                script.AppendLine($"\n-- Tabela: {table}");
                script.AppendLine($"DELETE FROM [{table}];");

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT * FROM [{table}]";
                using var reader = await cmd.ExecuteReaderAsync();

                // Obter nomes das colunas
                var columns = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => $"[{reader.GetName(i)}]")
                    .ToList();

                var columnsStr = string.Join(", ", columns);

                while (await reader.ReadAsync())
                {
                    var values = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        if (value is DBNull)
                            values.Add("NULL");
                        else if (value is string str)
                            values.Add($"'{str.Replace("'", "''")}'");
                        else if (value is DateTime dt)
                            values.Add($"'{dt:yyyy-MM-dd HH:mm:ss}'");
                        else
                            values.Add(value.ToString() ?? "NULL");
                    }

                    script.AppendLine($"INSERT INTO [{table}] ({columnsStr}) VALUES ({string.Join(", ", values)});");
                }
            }
        }
        catch (Exception ex)
        {
            script.AppendLine($"-- Erro ao gerar backup: {ex.Message}");
        }

        return script.ToString();
    }
}
