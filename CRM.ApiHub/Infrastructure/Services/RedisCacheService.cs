using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CRM.ApiHub.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IConnectionMultiplexer? redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private IDatabase? GetDatabase()
    {
        if (_redis == null || !_redis.IsConnected) return null;
        try
        {
            return _redis.GetDatabase();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo acceder a la base de datos Redis.");
            return null;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return default;

        try
        {
            var value = await db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al leer clave '{Key}' desde Redis.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null || value == null) return;

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var ttl = expiry ?? TimeSpan.FromMinutes(30);
            await db.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al escribir clave '{Key}' en Redis.", key);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached != null && !cached.Equals(default(T)))
        {
            return cached;
        }

        var result = await factory();
        if (result != null)
        {
            await SetAsync(key, result, expiry, ct);
        }
        return result;
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var db = GetDatabase();
        if (db == null) return;

        try
        {
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al eliminar clave '{Key}' de Redis.", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        if (_redis == null || !_redis.IsConnected) return;

        try
        {
            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: $"{prefix}*");
                var db = _redis.GetDatabase();
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al eliminar claves por prefijo '{Prefix}' en Redis.", prefix);
        }
    }
}
