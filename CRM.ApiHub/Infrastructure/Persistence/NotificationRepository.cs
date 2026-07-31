using Dapper;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(long userId, string title, string message, string? module, string? actionData)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO notification_service.user_alerts 
            (id_user, title, message, module, is_read, created_at, action_data) 
            VALUES 
            (@IdUser, @Title, @Message, @Module, false, NOW(), @ActionData::jsonb);";

        await connection.ExecuteAsync(sql, new { IdUser = userId, Title = title, Message = message, Module = module, ActionData = actionData });
    }

    public async Task<IEnumerable<UserAlert>> GetRecentAsync(long userId, int limit = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM notification_service.user_alerts 
            WHERE id_user = @UserId 
            ORDER BY created_at DESC 
            LIMIT @Limit;";

        return await connection.QueryAsync<UserAlert>(sql, new { UserId = userId, Limit = limit });
    }

    public async Task MarkReadAsync(int idAlert, long userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE notification_service.user_alerts SET is_read = true, read_at = NOW() WHERE id_alert = @IdAlert AND id_user = @UserId;";
        await connection.ExecuteAsync(sql, new { IdAlert = idAlert, UserId = userId });
    }

    public async Task MarkAllReadAsync(long userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE notification_service.user_alerts SET is_read = true, read_at = NOW() WHERE id_user = @UserId AND is_read = false;";
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}