using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;

    public UserRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"
            SELECT id_user, username, password_hash, date_created, is_logged_in, last_activity, fingerprint, state 
            FROM users 
            WHERE username = @Username;";

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Username = username }, cancellationToken: ct)
        );
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"
            SELECT id_user, username, password_hash, date_created, is_logged_in, last_activity, fingerprint, state 
            FROM users 
            WHERE id_user = @Id;";

        return await conn.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct)
        );
    }

    public async Task<UserDetail?> GetUserDetailByIdAsync(long userId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"
            SELECT 
                u.id_user,
                COALESCE(NULLIF(CONCAT_WS(' ', col.name, col.paternal_surname, col.maternal_surname), ''), u.username) AS username,
                r.name AS role_name,
                c.name AS campaign_name
            FROM users u
            LEFT JOIN collaborators col ON u.id_user = col.id_user
            LEFT JOIN user_role ur ON u.id_user = ur.id_user AND ur.is_active = true
            LEFT JOIN role r ON ur.id_role = r.id_role AND r.is_active = true
            LEFT JOIN user_campaign uc ON u.id_user = uc.id_user AND uc.is_active = true
            LEFT JOIN campaign c ON uc.id_cmpg = c.id_cmpg AND c.is_active = true
            WHERE u.id_user = @UserId;";

        var result = await conn.QueryFirstOrDefaultAsync<UserDetail>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct)
        );

        if (result != null)
        {
            result.Username = Domain.Utils.StringSanitizer.Sanitize(result.Username) ?? "";
            result.RoleName = Domain.Utils.StringSanitizer.Sanitize(result.RoleName);
            result.CampaignName = Domain.Utils.StringSanitizer.Sanitize(result.CampaignName);
        }

        return result;
    }
}
