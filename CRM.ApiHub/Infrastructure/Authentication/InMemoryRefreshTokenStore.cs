using System;
using System.Collections.Concurrent;
using System.Linq;
using CRM.ApiHub.Application.Interfaces;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, (long UserId, string FamilyId, bool IsUsed, DateTime Expiry, string IpAddress, string UserAgent)> _tokens = new();

    public void SaveToken(string token, long userId, string familyId, bool isUsed, DateTime expiry, string ipAddress, string userAgent)
    {
        _tokens[token] = (userId, familyId, isUsed, expiry, ipAddress, userAgent);
    }

    public bool TryGetTokenInfo(string token, string ipAddress, string userAgent, out long userId, out string familyId, out bool isUsed)
    {
        userId = 0;
        familyId = string.Empty;
        isUsed = false;
        
        if (_tokens.TryGetValue(token, out var val))
        {
            if (val.Expiry > DateTime.UtcNow)
            {
                userId = val.UserId;
                familyId = val.FamilyId;
                isUsed = val.IsUsed;
                return true;
            }
            _tokens.TryRemove(token, out _);
        }
        return false;
    }

    public void RevokeToken(string token)
    {
        _tokens.TryRemove(token, out _);
    }

    public void MarkAsUsed(string token)
    {
        if (_tokens.TryGetValue(token, out var val))
        {
            _tokens[token] = (val.UserId, val.FamilyId, true, val.Expiry, val.IpAddress, val.UserAgent);
        }
    }

    public void RevokeFamily(string familyId)
    {
        var tokensToRemove = _tokens.Where(t => t.Value.FamilyId == familyId).Select(t => t.Key).ToList();
        foreach (var t in tokensToRemove)
        {
            _tokens.TryRemove(t, out _);
        }
    }
}
