using Dapper;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class OrderDataRepository : IOrderDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderDataRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderData>> GetByOrderAsync(long idOrder)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.sales_order_data WHERE id_order = @IdOrder;";
        return await connection.QueryAsync<OrderData>(sql, new { IdOrder = idOrder });
    }

    public async Task UpdateFieldStatusAsync(long idData, string status, long validatedBy)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.sales_order_data 
            SET field_status = @Status, 
                validated_by = @ValidatedBy, 
                validated_at = NOW() 
            WHERE id_orddata = @IdData;";
            
        await connection.ExecuteAsync(sql, new { Status = status, ValidatedBy = validatedBy, IdData = idData });
    }
}