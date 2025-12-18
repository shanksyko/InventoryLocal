using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Data.SqlClient;
using InventarioSistem.Core.Logging;

namespace InventarioSistem.Access;

public static class MdfCacheManager
{
    private const string LoggerSource = "MdfCache";

    private static string CacheRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "InventoryLocal",
        "MdfCache"
    );

    public static bool IsNetworkPath(string path)
    {
        if (path.StartsWith("\\\\", StringComparison.Ordinal))
            return true;

        try
        {
            var root = Path.GetPathRoot(path);
            if (!string.IsNullOrWhiteSpace(root) && OperatingSystem.IsWindows())
            {
                var di = new DriveInfo(root);
                return di.DriveType == DriveType.Network;
            }
        }
        catch
        {
            // best-effort
        }

        return false;
    }

    public static string GetCachedMdfPath(string originalMdfPath)
    {
        Directory.CreateDirectory(CacheRoot);

        var originalFull = Path.GetFullPath(originalMdfPath);
        var key = ComputeSha1(originalFull);
        var name = Path.GetFileNameWithoutExtension(originalMdfPath);

        // mantém identificável e estável por caminho
        var safeName = SanitizeFileName(name);
        return Path.Combine(CacheRoot, $"{safeName}_{key}.mdf");
    }

    public static string EnsureCacheReady(string originalMdfPath, Action<string>? log = null)
    {
        void Log(string msg)
        {
            log?.Invoke(msg);
            InventoryLogger.Info(LoggerSource, msg);
        }

        var cachedMdfPath = GetCachedMdfPath(originalMdfPath);
        var cachedLdfPath = Path.ChangeExtension(cachedMdfPath, ".ldf");

        var originalLdfPath = Path.ChangeExtension(originalMdfPath, ".ldf");

        // Se não existir cache, copia.
        // Se existir, atualiza apenas se o original estiver mais novo.
        DateTime? originalWrite = SafeGetLastWriteUtc(originalMdfPath);
        DateTime? cachedWrite = SafeGetLastWriteUtc(cachedMdfPath);

        var mustCopy = !File.Exists(cachedMdfPath) || (originalWrite.HasValue && cachedWrite.HasValue && originalWrite.Value > cachedWrite.Value);

        if (mustCopy)
        {
            Log("📦 Preparando cache local do MDF...");
            CopyWithRetries(originalMdfPath, cachedMdfPath, Log);

            if (File.Exists(originalLdfPath))
            {
                CopyWithRetries(originalLdfPath, cachedLdfPath, Log);
            }
            else
            {
                Log("ℹ️  LDF não encontrado no original (ok). O SQL poderá gerar um novo localmente.");
            }

            Log($"✅ Cache pronto: {cachedMdfPath}");
        }
        else
        {
            Log("✅ Cache local já está atualizado.");
        }

        return cachedMdfPath;
    }

    public static void TrySyncBack(string originalMdfPath, string cachedMdfPath, Action<string>? log = null)
    {
        void Log(string msg)
        {
            log?.Invoke(msg);
            InventoryLogger.Info(LoggerSource, msg);
        }

        try
        {
            // solta pools para reduzir chance de lock na cópia
            SqlConnection.ClearAllPools();
        }
        catch
        {
            // best-effort
        }

        try
        {
            if (!File.Exists(cachedMdfPath))
            {
                Log("⚠️  Cache MDF não encontrado para sincronizar.");
                return;
            }

            var cachedLdfPath = Path.ChangeExtension(cachedMdfPath, ".ldf");
            var originalLdfPath = Path.ChangeExtension(originalMdfPath, ".ldf");

            Directory.CreateDirectory(Path.GetDirectoryName(originalMdfPath) ?? throw new InvalidOperationException("Diretório do MDF inválido"));

            Log("🔄 Sincronizando cache local → destino (rede/local)...");
            CopyWithRetries(cachedMdfPath, originalMdfPath, Log);

            if (File.Exists(cachedLdfPath))
            {
                CopyWithRetries(cachedLdfPath, originalLdfPath, Log);
            }

            Log("✅ Sincronização concluída.");
        }
        catch (Exception ex)
        {
            Log($"⚠️  Falha ao sincronizar de volta: {ex.Message}");
        }
    }

    private static void CopyWithRetries(string source, string destination, Action<string> log)
    {
        EnsureNotReadOnly(destination, log);
        EnsureNotReadOnly(source, log);

        var destDir = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(destDir))
            Directory.CreateDirectory(destDir);

        const int attempts = 5;
        for (int i = 1; i <= attempts; i++)
        {
            try
            {
                File.Copy(source, destination, overwrite: true);
                return;
            }
            catch (IOException ex) when (i < attempts)
            {
                log($"⚠️  Cópia falhou (tentativa {i}/{attempts}): {ex.Message}");
                Thread.Sleep(300 * i);
            }
            catch (UnauthorizedAccessException ex) when (i < attempts)
            {
                log($"⚠️  Sem acesso ao copiar (tentativa {i}/{attempts}): {ex.Message}");
                Thread.Sleep(300 * i);
            }
        }

        // última tentativa sem capturar
        File.Copy(source, destination, overwrite: true);
    }

    private static void EnsureNotReadOnly(string path, Action<string> log)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var attr = File.GetAttributes(path);
            if ((attr & FileAttributes.ReadOnly) == 0)
                return;

            File.SetAttributes(path, attr & ~FileAttributes.ReadOnly);
            log($"ℹ️  Removido atributo somente leitura: {path}");
        }
        catch
        {
            // best-effort
        }
    }

    private static DateTime? SafeGetLastWriteUtc(string path)
    {
        try
        {
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : null;
        }
        catch
        {
            return null;
        }
    }

    private static string ComputeSha1(string input)
    {
        using var sha1 = SHA1.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha1.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "Inventory" : name;
    }
}
