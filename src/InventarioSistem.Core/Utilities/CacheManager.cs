using System;
using System.Collections.Generic;
using System.Linq;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.Core.Utilities;

/// <summary>
/// Gerenciador de cache em memória para reduzir consultas ao banco de dados
/// </summary>
public class CacheManager
{
    private static readonly Lazy<CacheManager> _instance = new(() => new CacheManager());
    public static CacheManager Instance => _instance.Value;

    private class CacheEntry<T>
    {
        public T? Data { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    private readonly Dictionary<string, object?> _cache = new();
    private readonly object _lockObj = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Obtém valor do cache, ou null se expirado
    /// </summary>
    public T? Get<T>(string key)
    {
        lock (_lockObj)
        {
            if (_cache.TryGetValue(key, out var entry) && entry is CacheEntry<T> cacheEntry)
            {
                if (!cacheEntry.IsExpired)
                {
                    return cacheEntry.Data;
                }

                _cache.Remove(key);
            }
        }

        return default;
    }

    /// <summary>
    /// Define valor no cache com expiração padrão (5 minutos)
    /// </summary>
    public void Set<T>(string key, T value)
    {
        Set(key, value, _defaultExpiration);
    }

    /// <summary>
    /// Define valor no cache com expiração customizada
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        lock (_lockObj)
        {
            _cache[key] = new CacheEntry<T>
            {
                Data = value,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };
        }
    }

    /// <summary>
    /// Remove valor do cache
    /// </summary>
    public void Remove(string key)
    {
        lock (_lockObj)
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Limpa todo o cache
    /// </summary>
    public void Clear()
    {
        lock (_lockObj)
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// Remove todas as chaves com prefixo específico (útil para invalidar grupos)
    /// </summary>
    public void RemoveByPrefix(string prefix)
    {
        lock (_lockObj)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// Obtém valor ou executa factory se não existir/expirado
    /// </summary>
    public T GetOrAdd<T>(string key, Func<T> factory)
    {
        var cached = Get<T>(key);
        if (cached != null)
        {
            return cached;
        }

        var value = factory();
        Set(key, value);
        return value;
    }

    /// <summary>
    /// Versão async de GetOrAdd
    /// </summary>
    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory)
    {
        var cached = Get<T>(key);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        Set(key, value);
        return value;
    }
}
