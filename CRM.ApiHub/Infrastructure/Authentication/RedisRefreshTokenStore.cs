using System;
using System.Text.Json;
using CRM.ApiHub.Application.Interfaces;
using StackExchange.Redis;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IRefreshTokenStore _fallbackStore;
    private const string KeyPrefix = "refresh_token:";

    public RedisRefreshTokenStore(IConnectionMultiplexer? redis)
    {
        _redis = redis;
        _fallbackStore = new InMemoryRefreshTokenStore();
    }

    private bool IsRedisAvailable()
    {
        return _redis != null && _redis.IsConnected;
    }

    public void SaveToken(string token, long userId, DateTime expiry, string ipAddress, string userAgent)
    {
        if (!IsRedisAvailable())
        {
            _fallbackStore.SaveToken(token, userId, expiry, ipAddress, userAgent);
            return;
        }

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        
        var entry = new RefreshTokenEntry
        {
            UserId = userId,
            Expiry = expiry,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var json = JsonSerializer.Serialize(entry);
        var ttl = expiry - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
        {
            db.StringSet(key, json, ttl);
        }
    }

    public bool TryGetUserId(string token, string ipAddress, string userAgent, out long userId)
    {
        userId = 0;

        if (!IsRedisAvailable())
        {
            return _fallbackStore.TryGetUserId(token, ipAddress, userAgent, out userId);
        }

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        var value = db.StringGet(key);

        if (value.IsNullOrEmpty)
        {
            return false;
        }

        try
        {
            var entry = JsonSerializer.Deserialize<RefreshTokenEntry>(value.ToString());
            if (entry != null)
            {
                if (entry.Expiry > DateTime.UtcNow)
                {
                    bool isIpMatch = string.IsNullOrEmpty(entry.IpAddress) || entry.IpAddress == ipAddress;
                    bool isUserAgentMatch = string.IsNullOrEmpty(entry.UserAgent) || entry.UserAgent == userAgent;

                    if (isIpMatch && isUserAgentMatch)
                    {
                        userId = entry.UserId;
                        return true;
                    }
                }
                
                // If expired or mismatch, delete token
                db.KeyDelete(key);
            }
        }
        catch
        {
            db.KeyDelete(key);
        }

        return false;
    }

    public void RevokeToken(string token)
    {
        if (!IsRedisAvailable())
        {
            _fallbackStore.RevokeToken(token);
            return;
        }

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        db.KeyDelete(key);
    }

    private class RefreshTokenEntry
    {
        public long UserId { get; set; }
        public DateTime Expiry { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
