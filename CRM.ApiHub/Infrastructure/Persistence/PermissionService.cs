using Dapper;
using System.Data;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class PermissionService : IPermissionService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PermissionService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CanUserActionAsync(int userId, string permissionKey, int statusId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT access_control.can_user_action(@UserId, @PermissionKey, @StatusId);";
        
        return await connection.ExecuteScalarAsync<bool>(sql, new 
        { 
            UserId = userId, 
            PermissionKey = permissionKey, 
            StatusId = statusId 
        });
    }
}