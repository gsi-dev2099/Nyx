using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class UserPreferencesRepository : IUserPreferencesRepository
{
    private readonly IDbConnectionFactory _factory;
    private readonly ILogger<UserPreferencesRepository> _logger;

    public UserPreferencesRepository(IDbConnectionFactory factory, ILogger<UserPreferencesRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<string?> GetThemeAsync(long userId, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"
            SELECT theme_name 
            FROM user_service.user_preferences 
            WHERE id_user = @UserId;";

        try
        {
            return await conn.QueryFirstOrDefaultAsync<string>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct)
            );
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
        {
            // Table doesn't exist yet — return null gracefully
            _logger.LogWarning("user_service.user_preferences table does not exist yet. Returning default theme.");
            return null;
        }
    }

    public async Task SaveThemeAsync(long userId, string themeName, CancellationToken ct = default)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"
            INSERT INTO user_service.user_preferences (id_user, theme_name, updated_at)
            VALUES (@UserId, @ThemeName, NOW())
            ON CONFLICT (id_user) DO UPDATE 
            SET theme_name = @ThemeName, updated_at = NOW();";

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, ThemeName = themeName }, cancellationToken: ct)
        );
    }
}
