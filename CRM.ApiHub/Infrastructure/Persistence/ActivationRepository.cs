using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class ActivationRepository : IActivationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ActivationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string SelectColumnsSql = @"
        SELECT 
            id_tracking AS IdTracking,
            id_order AS IdOrder,
            id_order_item AS IdOrderItem,
            product_name AS ProductName,
            id_provider AS IdProvider,
            provider_ref AS ProviderRef,
            order_loaded_at::timestamp AS OrderLoadedAt,
            expected_activation_date::timestamp AS ExpectedActivationDate,
            actual_activation_date::timestamp AS ActualActivationDate,
            activation_status AS ActivationStatus,
            CASE 
                WHEN actual_activation_date IS NOT NULL THEN 
                    GREATEST(0, (actual_activation_date - expected_activation_date)::integer)
                WHEN expected_activation_date < CURRENT_DATE THEN 
                    (CURRENT_DATE - expected_activation_date)::integer
                ELSE 
                    0 
            END AS DelayDays,
            delay_reason AS DelayReason,
            alert_sent_at::timestamp AS AlertSentAt,
            last_checked_at::timestamp AS LastCheckedAt,
            notes AS Notes,
            created_at::timestamp AS CreatedAt,
            updated_at::timestamp AS UpdatedAt
        FROM sales_service.product_activation_tracking";

    public async Task<IEnumerable<ProductActivationTracking>> GetPendingActivationsAsync(long idProvider, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql;
        if (idProvider > 0)
        {
            sql = $"{SelectColumnsSql} WHERE id_provider = @IdProvider ORDER BY expected_activation_date ASC;";
        }
        else
        {
            sql = $"{SelectColumnsSql} ORDER BY expected_activation_date ASC;";
        }

        return await connection.QueryAsync<ProductActivationTracking>(
            new CommandDefinition(sql, new { IdProvider = idProvider }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<ProductActivationTracking>> GetByOrderAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectColumnsSql} WHERE id_order = @IdOrder ORDER BY id_tracking ASC;";

        return await connection.QueryAsync<ProductActivationTracking>(
            new CommandDefinition(sql, new { IdOrder = idOrder }, cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateActivationAsync(
        long idTracking, 
        string status, 
        DateTime? actualDate, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.product_activation_tracking 
            SET activation_status = @Status, 
                actual_activation_date = @ActualDate, 
                last_checked_at = NOW(), 
                updated_at = NOW() 
            WHERE id_tracking = @IdTracking;";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new 
            { 
                IdTracking = idTracking, 
                Status = status, 
                ActualDate = actualDate 
            }, cancellationToken: ct)
        );

        return rows > 0;
    }

    public async Task<IEnumerable<ProductActivationTracking>> GetDelayedAsync(CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Return activations where the status is PENDING/DELAYED and expected_activation_date has already passed CURRENT_DATE
        var sql = @$"{SelectColumnsSql} 
            WHERE activation_status NOT IN ('ACTIVATED', 'COMPLETED', 'CANCELLED') 
              AND expected_activation_date < CURRENT_DATE 
            ORDER BY expected_activation_date ASC;";

        return await connection.QueryAsync<ProductActivationTracking>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<ProductActivationTracking?> GetByIdAsync(long idTracking, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectColumnsSql} WHERE id_tracking = @IdTracking;";

        return await connection.QueryFirstOrDefaultAsync<ProductActivationTracking>(
            new CommandDefinition(sql, new { IdTracking = idTracking }, cancellationToken: ct)
        );
    }
}
