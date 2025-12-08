using System;
using System.Data.Odbc;
using System.IO;
using System.Text;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Schema;

namespace InventarioSistem.Access.Db;

public static class AccessDatabaseManager
{
    private const string DefaultFileName = "InventarioSistem.accdb";
    private const string TemplateFileName = "InventarioTemplate.accdb";

    /// <summary>
    /// Resolve o caminho do banco ativo:
    /// - Primeiro tenta config.json (se existir e for válido).
    /// - Depois tenta o arquivo padrão na pasta do executável.
    /// </summary>
    public static string ResolveActiveDatabasePath()
    {
        var config = AccessConfig.Load();

        if (!string.IsNullOrWhiteSpace(config.DatabasePath)
            && File.Exists(config.DatabasePath))
        {
            return config.DatabasePath;
        }

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var defaultPath = Path.Combine(baseDir, DefaultFileName);

        if (File.Exists(defaultPath))
            return defaultPath;

        throw new FileNotFoundException(
            "Nenhum banco Access válido encontrado. " +
            "Selecione um banco existente ou crie um novo.");
    }

    /// <summary>
    /// Define o caminho do banco ativo e persiste em config.json.
    /// Não valida a estrutura, apenas o caminho.
    /// </summary>
    public static void SetActiveDatabasePath(string databasePath)
    {
        var config = new AccessConfig
        {
            DatabasePath = databasePath
        };
        config.Save();
    }

    /// <summary>
    /// Cria um novo banco Access a partir de um arquivo template vazio
    /// (InventarioTemplate.accdb) localizado na pasta do executável.
    /// Em seguida, define este novo banco como ativo.
    /// Não sobrescreve arquivos existentes.
    /// </summary>
    /// <param name="targetPath">Caminho completo do novo arquivo .accdb.</param>
    /// <returns>Caminho final do arquivo criado.</returns>
    /// <exception cref="FileNotFoundException">
    /// Se o template InventarioTemplate.accdb não for encontrado.
    /// </exception>
    /// <exception cref="IOException">
    /// Se já existir um arquivo no targetPath.
    /// </exception>
    public static string CreateNewDatabaseFromTemplate(string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
            throw new ArgumentException("Caminho do banco não pode ser vazio.", nameof(targetPath));

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var templatePath = Path.Combine(baseDir, TemplateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"Arquivo de template de banco Access não encontrado: '{templatePath}'. " +
                "Crie um arquivo .accdb vazio e salve como 'InventarioTemplate.accdb' na pasta do executável.");
        }

        var dir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(targetPath))
        {
            throw new IOException($"Já existe um arquivo no caminho especificado: '{targetPath}'.");
        }

        File.Copy(templatePath, targetPath, overwrite: false);

        // Define como banco ativo
        SetActiveDatabasePath(targetPath);

        // Garante criação das tabelas padrão
        AccessSchemaManager.EnsureRequiredTables();

        return targetPath;
    }

    /// <summary>
    /// Retorna um resumo textual simples do banco:
    /// - Contagem de registros por tabela principal (se existirem).
    /// Ignora erros de tabela inexistente (marca como "N/A").
    /// </summary>
    public static string GetDatabaseSummary(string databasePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Banco: {databasePath}");
        sb.AppendLine();

        void TryCount(string tableName)
        {
            try
            {
                using var conn = CreateRawConnection(databasePath);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
                var count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                sb.AppendLine($"{tableName}: {count} registro(s)");
            }
            catch
            {
                sb.AppendLine($"{tableName}: N/A (tabela não encontrada ou erro ao consultar)");
            }
        }

        TryCount("Computadores");
        TryCount("Tablets");
        TryCount("ColetoresAndroid");
        TryCount("Celulares");

        return sb.ToString();
    }

    private static OdbcConnection CreateRawConnection(string databasePath)
    {
        var connString =
            $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};Dbq={databasePath};Uid=Admin;";
        var conn = new OdbcConnection(connString);
        conn.Open();
        return conn;
    }
}
