using Dapper;
using System.Data;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class CatalogRepository : ICatalogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CatalogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderStatus>> GetOrderStatusesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM order_status WHERE is_active = true;";
        return await connection.QueryAsync<OrderStatus>(sql);
    }

    public async Task<IEnumerable<OrderSubstatus>> GetOrderSubstatusesAsync(int idStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM order_substatus WHERE order_status_id = @IdStatus AND is_active = true;";
        return await connection.QueryAsync<OrderSubstatus>(sql, new { IdStatus = idStatus });
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(int idCmpg)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.* FROM product p
            INNER JOIN campaign_service.campaign_product cp ON p.id = cp.product_id
            WHERE cp.campaign_id = @IdCampaign AND p.is_active = true;";
            
        return await connection.QueryAsync<Product>(sql, new { IdCampaign = idCmpg });
    }

   public async Task<IEnumerable<Currency>> GetCurrenciesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.currency WHERE is_active = true;";
        
        return await connection.QueryAsync<Currency>(sql);
    }
}