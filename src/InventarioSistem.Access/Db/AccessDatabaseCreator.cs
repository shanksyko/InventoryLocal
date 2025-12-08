using System;
using System.IO;
using System.Runtime.InteropServices;
using ADOX;

namespace InventarioSistem.Access.Db;

/// <summary>
/// Responsável por criar um novo arquivo .accdb vazio usando ADOX.
/// </summary>
public static class AccessDatabaseCreator
{
    /// <summary>
    /// Cria um novo banco Access (.accdb) vazio no caminho especificado.
    /// Não sobrescreve arquivos existentes.
    /// </summary>
    /// <param name="path">Caminho completo do arquivo .accdb a ser criado.</param>
    public static void CreateEmptyDatabase(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Caminho do banco não pode ser vazio.", nameof(path));

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(path))
            throw new IOException($"Já existe um arquivo no caminho especificado: '{path}'.");

        Catalog? catalog = null;
        try
        {
            catalog = new Catalog();
            // Engine Type 5 -> Access 2007+ (.accdb)
            var connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Jet OLEDB:Engine Type=5;";
            catalog.Create(connStr);
        }
        finally
        {
            if (catalog is not null)
            {
                try
                {
                    Marshal.FinalReleaseComObject(catalog);
                }
                catch
                {
                    // ignorar erro de liberação de COM
                }
            }
        }
    }
}

