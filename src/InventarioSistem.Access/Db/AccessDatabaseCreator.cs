using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace InventarioSistem.Access.Db
{
    /// <summary>
    /// Responsável por criar um novo arquivo .accdb vazio usando PowerShell + ADOX.
    /// Não exige COMReference no projeto .NET; apenas depende do provider ACE/Access Engine instalado no Windows.
    /// </summary>
    public static class AccessDatabaseCreator
    {
        /// <summary>
        /// Cria um novo banco Access (.accdb) vazio no caminho especificado.
        /// Não sobrescreve arquivos existentes.
        /// </summary>
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

            // Escapa aspas simples para uso no PowerShell
            var escapedPath = path.Replace("'", "''");

            // Script PowerShell montado por concatenação simples
            var sb = new StringBuilder();
            sb.AppendLine("$path = '" + escapedPath + "'");
            sb.AppendLine("$conn = \"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$path;Jet OLEDB:Engine Type=5;\"");
            sb.AppendLine("$cat  = New-Object -ComObject ADOX.Catalog");
            sb.AppendLine("$cat.Create($conn)");
            sb.AppendLine("[System.Runtime.Interopservices.Marshal]::FinalReleaseComObject($cat) | Out-Null");

            var psScript = sb.ToString();

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -Command -",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var proc = new Process { StartInfo = psi })
            {
                proc.Start();
                proc.StandardInput.Write(psScript);
                proc.StandardInput.Close();

                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Falha ao criar banco Access via PowerShell. Código={proc.ExitCode}. Erro: {stderr}");
                }

                if (!File.Exists(path))
                {
                    throw new InvalidOperationException(
                        $"PowerShell executou sem erro, mas o arquivo não foi encontrado em '{path}'. Saída: {stdout} Erro: {stderr}");
                }
            }
        }
    }
}
