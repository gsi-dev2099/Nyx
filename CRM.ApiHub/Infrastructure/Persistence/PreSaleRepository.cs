using Dapper;
using System.Data;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class PreSaleRepository : IPreSaleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PreSaleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LeadPreSale>> GetByUserAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM lead_service.lead_pre_sale WHERE current_user_id = @UserId;";
        
        return await connection.QueryAsync<LeadPreSale>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(LeadPreSale preSale)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO lead_service.lead_pre_sale (
                id_cmpg, phone, operator, first_name, last_name, address, province, 
                coverage_status, id_status, owner_user_id, current_user_id, notes, register
            )
            VALUES (
                @IdCmpg, @Phone, @Operator, @FirstName, @LastName, @Address, @Province, 
                @CoverageStatus, @IdStatus, @OwnerUserId, @CurrentUserId, @Notes, @Register
            )
            RETURNING id_presale;";

        return await connection.ExecuteScalarAsync<int>(sql, preSale);
    }

    public async Task<bool> AddCallLogAsync(int idPresale, string callLog)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO lead_service.lead_call_log (id_presale, notes, register)
            VALUES (@PreSaleId, @LogDetails, @CreatedAt);";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            PreSaleId = idPresale, 
            LogDetails = callLog,
            CreatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> AssignAsync(int idPresale, int toUserId, string context)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET current_user_id = @ToUserId, notes = @Context, last_activity_at = @UpdatedAt
            WHERE id_presale = @IdPresale;";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            ToUserId = toUserId, 
            Context = context, 
            IdPresale = idPresale,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> ConvertAsync(int idPresale, dynamic paramsData)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET id_status = 3, converted_at = @UpdatedAt
            WHERE id_presale = @IdPresale;";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            IdPresale = idPresale,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }
}