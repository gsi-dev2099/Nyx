using System;

namespace CRM.ApiHub.Application.Interfaces;

public interface IRefreshTokenStore
{
    void SaveToken(string token, long userId, DateTime expiry, string ipAddress, string userAgent);
    bool TryGetUserId(string token, string ipAddress, string userAgent, out long userId);
    void RevokeToken(string token);
}
