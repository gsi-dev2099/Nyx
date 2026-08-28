using System;
using System.Text.Json;
using System.Linq;
using CRM.ApiHub.Application.Interfaces;
using StackExchange.Redis;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly IConnectionMultiplexer? _redis;
    private const string KeyPrefix = "refresh_token:";
    private const string FamilyPrefix = "token_family:";

    public RedisRefreshTokenStore(IConnectionMultiplexer? redis)
    {
        _redis = redis;
    }

    private bool IsRedisAvailable() => _redis != null && _redis.IsConnected;

    public void SaveToken(string token, long userId, string familyId, bool isUsed, DateTime expiry, string ipAddress, string userAgent)
    {
        if (!IsRedisAvailable()) return;

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        
        var entry = new RefreshTokenEntry
        {
            UserId = userId,
            FamilyId = familyId,
            IsUsed = isUsed,
            Expiry = expiry,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var json = JsonSerializer.Serialize(entry);
        var ttl = expiry - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
        {
            db.StringSet(key, json, ttl);
            var familyKey = FamilyPrefix + familyId;
            db.SetAdd(familyKey, token);
            db.KeyExpire(familyKey, ttl);
        }
    }

    public bool TryGetTokenInfo(string token, string ipAddress, string userAgent, out long userId, out string familyId, out bool isUsed)
    {
        userId = 0;
        familyId = string.Empty;
        isUsed = false;

        if (!IsRedisAvailable()) return false;

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        var value = db.StringGet(key);

        if (value.IsNullOrEmpty) return false;

        try
        {
            var entry = JsonSerializer.Deserialize<RefreshTokenEntry>(value.ToString());
            if (entry != null && entry.Expiry > DateTime.UtcNow)
            {
                userId = entry.UserId;
                familyId = entry.FamilyId ?? string.Empty;
                isUsed = entry.IsUsed;
                return true;
            }
            
            db.KeyDelete(key);
        }
        catch
        {
            db.KeyDelete(key);
        }
        return false;
    }

    public void RevokeToken(string token)
    {
        if (!IsRedisAvailable()) return;
        var db = _redis!.GetDatabase();
        db.KeyDelete(KeyPrefix + token);
    }

    public void MarkAsUsed(string token)
    {
        if (!IsRedisAvailable()) return;

        var db = _redis!.GetDatabase();
        var key = KeyPrefix + token;
        var value = db.StringGet(key);

        if (!value.IsNullOrEmpty)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<RefreshTokenEntry>(value.ToString());
                if (entry != null)
                {
                    entry.IsUsed = true;
                    var ttl = db.KeyTimeToLive(key);
                    db.StringSet(key, JsonSerializer.Serialize(entry), ttl);
                }
            }
            catch { }
        }
    }

    public void RevokeFamily(string familyId)
    {
        if (!IsRedisAvailable()) return;

        var db = _redis!.GetDatabase();
        var familyKey = FamilyPrefix + familyId;
        var tokensInFamily = db.SetMembers(familyKey);

        foreach (var tokenVal in tokensInFamily)
        {
            db.KeyDelete(KeyPrefix + tokenVal.ToString());
        }
        db.KeyDelete(familyKey);
    }

    private class RefreshTokenEntry
    {
        public long UserId { get; set; }
        public string? FamilyId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime Expiry { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
