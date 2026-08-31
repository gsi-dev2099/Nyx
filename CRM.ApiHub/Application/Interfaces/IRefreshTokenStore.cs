using System;

namespace CRM.ApiHub.Application.Interfaces;

public interface IRefreshTokenStore
{
    void SaveToken(string token, long userId, string familyId, bool isUsed, DateTime expiry, string ipAddress, string userAgent);
    bool TryGetTokenInfo(string token, string ipAddress, string userAgent, out long userId, out string familyId, out bool isUsed);
    void RevokeToken(string token);
    void RevokeFamily(string familyId);
    void MarkAsUsed(string token);
}
