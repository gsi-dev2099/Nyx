using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string? roleName);
}
