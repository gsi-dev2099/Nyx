using System;
using System.Collections.Concurrent;
using CRM.ApiHub.Application.Interfaces;

namespace CRM.ApiHub.Infrastructure.Authentication;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, (long UserId, DateTime Expiry, string IpAddress, string UserAgent)> _tokens = new();

    public void SaveToken(string token, long userId, DateTime expiry, string ipAddress, string userAgent)
    {
        _tokens[token] = (userId, expiry, ipAddress, userAgent);
    }

    public bool TryGetUserId(string token, string ipAddress, string userAgent, out long userId)
    {
        userId = 0;
        if (_tokens.TryGetValue(token, out var val))
        {
            if (val.Expiry > DateTime.UtcNow)
            {
                // Verify that the request's IP and User-Agent match the ones stored during login
                bool isIpMatch = string.IsNullOrEmpty(val.IpAddress) || val.IpAddress == ipAddress;
                bool isUserAgentMatch = string.IsNullOrEmpty(val.UserAgent) || val.UserAgent == userAgent;

                if (isIpMatch && isUserAgentMatch)
                {
                    userId = val.UserId;
                    return true;
                }
            }
            _tokens.TryRemove(token, out _);
        }
        return false;
    }

    public void RevokeToken(string token)
    {
        _tokens.TryRemove(token, out _);
    }
}
