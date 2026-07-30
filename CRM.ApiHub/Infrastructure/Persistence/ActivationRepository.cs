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
            p.id_tracking AS IdTracking,
            p.id_order AS IdOrder,
            p.id_order_item AS IdOrderItem,
            p.product_name AS ProductName,
            p.id_provider AS IdProvider,
            COALESCE(prov.name, 'Proveedor Desconocido') AS ProviderName,
            COALESCE(l.first_name || ' ' || l.last_name, 'Cliente Desconocido') AS CustomerName,
            p.provider_ref AS ProviderRef,
            p.order_loaded_at::timestamp AS OrderLoadedAt,
            p.expected_activation_date::timestamp AS ExpectedActivationDate,
            p.actual_activation_date::timestamp AS ActualActivationDate,
            p.activation_status AS ActivationStatus,
            CASE 
                WHEN p.actual_activation_date IS NOT NULL THEN 
                    GREATEST(0, (p.actual_activation_date - p.expected_activation_date)::integer)
                WHEN p.expected_activation_date < CURRENT_DATE THEN 
                    (CURRENT_DATE - p.expected_activation_date)::integer
                ELSE 
                    0 
            END AS DelayDays,
            p.delay_reason AS DelayReason,
            p.alert_sent_at::timestamp AS AlertSentAt,
            p.last_checked_at::timestamp AS LastCheckedAt,
            p.notes AS Notes,
            p.created_at::timestamp AS CreatedAt,
            p.updated_at::timestamp AS UpdatedAt
        FROM sales_service.product_activation_tracking p
        LEFT JOIN sales_service.sales_order o ON p.id_order = o.id_order
        LEFT JOIN lead_service.lead l ON o.id_lead = l.id_lead
        LEFT JOIN sales_service.provider_catalog prov ON p.id_provider = prov.id_provider";

    public async Task<IEnumerable<ProductActivationTracking>> GetPendingActivationsAsync(long idProvider, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        string sql;
        if (idProvider > 0)
        {
            sql = $"{SelectColumnsSql} WHERE p.id_provider = @IdProvider ORDER BY p.expected_activation_date ASC;";
        }
        else
        {
            sql = $"{SelectColumnsSql} ORDER BY p.expected_activation_date ASC;";
        }

        return await connection.QueryAsync<ProductActivationTracking>(
            new CommandDefinition(sql, new { IdProvider = idProvider }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<ProductActivationTracking>> GetByOrderAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectColumnsSql} WHERE p.id_order = @IdOrder ORDER BY p.id_tracking ASC;";

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
            WHERE p.activation_status NOT IN ('ACTIVATED', 'COMPLETED', 'CANCELLED') 
              AND p.expected_activation_date < CURRENT_DATE 
            ORDER BY p.expected_activation_date ASC;";

        return await connection.QueryAsync<ProductActivationTracking>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<ProductActivationTracking?> GetByIdAsync(long idTracking, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectColumnsSql} WHERE p.id_tracking = @IdTracking;";

        return await connection.QueryFirstOrDefaultAsync<ProductActivationTracking>(
            new CommandDefinition(sql, new { IdTracking = idTracking }, cancellationToken: ct)
        );
    }
}
