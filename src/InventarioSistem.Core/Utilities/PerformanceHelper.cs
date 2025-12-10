using System;
using System.Diagnostics;
using System.Linq;

namespace InventarioSistem.Core.Utilities;

/// <summary>
/// Utilitários para otimização de performance
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Retorna resultado paginado ou em cache
    /// </summary>
    public static T CachedOrExecute<T>(
        string cacheKey,
        Func<T> operation,
        TimeSpan? cacheDuration = null)
    {
        var cache = CacheManager.Instance;
        var cached = cache.Get<T>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = operation();
        if (cacheDuration.HasValue)
        {
            cache.Set(cacheKey, result, cacheDuration.Value);
        }
        else
        {
            cache.Set(cacheKey, result);
        }

        return result;
    }

    /// <summary>
    /// Versão async
    /// </summary>
    public static async Task<T> CachedOrExecuteAsync<T>(
        string cacheKey,
        Func<Task<T>> operation,
        TimeSpan? cacheDuration = null)
    {
        var cache = CacheManager.Instance;
        var cached = cache.Get<T>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var result = await operation();
        if (cacheDuration.HasValue)
        {
            cache.Set(cacheKey, result, cacheDuration.Value);
        }
        else
        {
            cache.Set(cacheKey, result);
        }

        return result;
    }

    /// <summary>
    /// Batching de operações para reduzir overhead
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(
        IEnumerable<T> source,
        int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be > 0", nameof(batchSize));

        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return InternalBatch(enumerator, batchSize);
        }
    }

    private static IEnumerable<T> InternalBatch<T>(IEnumerator<T> source, int batchSize)
    {
        do
        {
            yield return source.Current;
        } while (--batchSize > 0 && source.MoveNext());
    }

    /// <summary>
    /// Mede tempo de execução com logging
    /// </summary>
    public static T MeasureExecution<T>(
        string operationName,
        Func<T> operation,
        Action<long>? onDurationMs = null)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return operation();
        }
        finally
        {
            sw.Stop();
            onDurationMs?.Invoke(sw.ElapsedMilliseconds);
            Debug.WriteLine($"[{operationName}] executado em {sw.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Versão async
    /// </summary>
    public static async Task<T> MeasureExecutionAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        Action<long>? onDurationMs = null)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await operation();
        }
        finally
        {
            sw.Stop();
            onDurationMs?.Invoke(sw.ElapsedMilliseconds);
            Debug.WriteLine($"[{operationName}] executado em {sw.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// Debounce para operações frequentes (busca, filtro, etc)
    /// </summary>
    public class Debouncer
    {
        private System.Timers.Timer? _timer;
        private readonly int _delayMs;
        private readonly object _lock = new();

        public Debouncer(int delayMs = 300)
        {
            _delayMs = delayMs;
        }

        public void Debounce(Action action)
        {
            lock (_lock)
            {
                _timer?.Stop();
                _timer?.Dispose();

                _timer = new System.Timers.Timer(_delayMs);
                _timer.AutoReset = false;
                _timer.Elapsed += (_, _) =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch { /* Ignora exceções */ }
                };
                _timer.Start();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
            }
        }
    }
}
