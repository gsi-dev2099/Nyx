using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using System.Text;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class CommissionRepository : ICommissionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CommissionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CommissionSettlement>> GetSettlementsAsync(CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_settlement AS IdSettlement,
                id_user AS IdUser,
                period_start::timestamp AS PeriodStart,
                period_end::timestamp AS PeriodEnd,
                settlement_date::timestamp AS SettlementDate,
                total_eur AS TotalEur,
                total_pen AS TotalPen,
                exchange_rate_id AS ExchangeRateId,
                exchange_rate_applied AS ExchangeRateApplied,
                total_orders AS TotalOrders,
                total_products AS TotalProducts,
                status AS Status,
                approved_by AS ApprovedBy,
                approved_at AS ApprovedAt,
                paid_at AS PaidAt,
                notes AS Notes,
                created_at AS CreatedAt
            FROM sales_service.commission_settlement
            ORDER BY created_at DESC;";

        return await connection.QueryAsync<CommissionSettlement>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<CommissionSettlement>> GetSettlementsByUserAsync(long userId, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_settlement AS IdSettlement,
                id_user AS IdUser,
                period_start::timestamp AS PeriodStart,
                period_end::timestamp AS PeriodEnd,
                settlement_date::timestamp AS SettlementDate,
                total_eur AS TotalEur,
                total_pen AS TotalPen,
                exchange_rate_id AS ExchangeRateId,
                exchange_rate_applied AS ExchangeRateApplied,
                total_orders AS TotalOrders,
                total_products AS TotalProducts,
                status AS Status,
                approved_by AS ApprovedBy,
                approved_at AS ApprovedAt,
                paid_at AS PaidAt,
                notes AS Notes,
                created_at AS CreatedAt
            FROM sales_service.commission_settlement
            WHERE id_user = @UserId
            ORDER BY created_at DESC;";

        return await connection.QueryAsync<CommissionSettlement>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct)
        );
    }

    public async Task<CommissionSettlement?> GetSettlementByIdAsync(long idSettlement, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_settlement AS IdSettlement,
                id_user AS IdUser,
                period_start::timestamp AS PeriodStart,
                period_end::timestamp AS PeriodEnd,
                settlement_date::timestamp AS SettlementDate,
                total_eur AS TotalEur,
                total_pen AS TotalPen,
                exchange_rate_id AS ExchangeRateId,
                exchange_rate_applied AS ExchangeRateApplied,
                total_orders AS TotalOrders,
                total_products AS TotalProducts,
                status AS Status,
                approved_by AS ApprovedBy,
                approved_at AS ApprovedAt,
                paid_at AS PaidAt,
                notes AS Notes,
                created_at AS CreatedAt
            FROM sales_service.commission_settlement
            WHERE id_settlement = @IdSettlement;";

        return await connection.QueryFirstOrDefaultAsync<CommissionSettlement>(
            new CommandDefinition(sql, new { IdSettlement = idSettlement }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<CommissionSettlementItem>> GetSettlementItemsAsync(long idSettlement, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_item AS IdItem,
                id_settlement AS IdSettlement,
                id_order AS IdOrder,
                id_order_item AS IdOrderItem,
                commission_eur AS CommissionEur,
                commission_pen AS CommissionPen,
                product_name AS ProductName,
                notes AS Notes
            FROM sales_service.commission_settlement_item
            WHERE id_settlement = @IdSettlement;";

        return await connection.QueryAsync<CommissionSettlementItem>(
            new CommandDefinition(sql, new { IdSettlement = idSettlement }, cancellationToken: ct)
        );
    }

    public async Task<long> CreateSettlementAsync(long userId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.commission_settlement 
                (id_user, period_start, period_end, settlement_date, total_eur, total_pen, total_orders, total_products, status, created_at)
            VALUES 
                (@UserId, @PeriodStart, @PeriodEnd, CURRENT_DATE, 0, 0, 0, 0, 'DRAFT', NOW())
            RETURNING id_settlement;";

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, new { UserId = userId, PeriodStart = periodStart.Date, PeriodEnd = periodEnd.Date }, cancellationToken: ct)
        );
    }

    public async Task<bool> AddSettlementItemsAsync(long idSettlement, long[] orderIds, CancellationToken ct = default)
    {
        if (orderIds == null || orderIds.Length == 0) return false;

        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            // Insertar todos los ítems de las órdenes en una sola consulta de conjuntos
            const string insertSql = @"
                INSERT INTO sales_service.commission_settlement_item 
                    (id_settlement, id_order, id_order_item, commission_eur, commission_pen, product_name)
                SELECT 
                    @IdSettlement, 
                    i.id_order, 
                    i.id_item, 
                    i.commission_eur, 
                    i.commission_pen, 
                    p.name
                FROM sales_service.sales_order_item i
                LEFT JOIN product_service.product p ON i.id_prod = p.id_prod
                WHERE i.id_order = ANY(@OrderIds);

                UPDATE sales_service.commission_settlement
                SET total_eur = (SELECT COALESCE(SUM(commission_eur), 0) FROM sales_service.commission_settlement_item WHERE id_settlement = @IdSettlement),
                    total_pen = (SELECT COALESCE(SUM(commission_pen), 0) FROM sales_service.commission_settlement_item WHERE id_settlement = @IdSettlement),
                    total_orders = (SELECT COUNT(DISTINCT id_order) FROM sales_service.commission_settlement_item WHERE id_settlement = @IdSettlement),
                    total_products = (SELECT COUNT(*) FROM sales_service.commission_settlement_item WHERE id_settlement = @IdSettlement)
                WHERE id_settlement = @IdSettlement;";

            await connection.ExecuteAsync(
                new CommandDefinition(insertSql, new { IdSettlement = idSettlement, OrderIds = orderIds }, transaction: transaction, cancellationToken: ct)
            );

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateSettlementStatusAsync(long idSettlement, string status, long? actorUserId, string? notes, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new StringBuilder("UPDATE sales_service.commission_settlement SET status = @Status");
        var parameters = new DynamicParameters();
        parameters.Add("Status", status.ToUpper());
        parameters.Add("IdSettlement", idSettlement);

        if (status.ToUpper() == "APPROVED")
        {
            sql.Append(", approved_by = @ActorUserId, approved_at = NOW()");
            parameters.Add("ActorUserId", actorUserId);
        }
        else if (status.ToUpper() == "PAID")
        {
            sql.Append(", paid_at = NOW()");
        }

        if (notes != null)
        {
            sql.Append(", notes = @Notes");
            parameters.Add("Notes", notes);
        }

        sql.Append(" WHERE id_settlement = @IdSettlement;");

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );

        return rows > 0;
    }

    public async Task<bool> DeleteSettlementAsync(long idSettlement, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Delete items first
            const string deleteItemsSql = "DELETE FROM sales_service.commission_settlement_item WHERE id_settlement = @IdSettlement;";
            await connection.ExecuteAsync(
                new CommandDefinition(deleteItemsSql, new { IdSettlement = idSettlement }, transaction: transaction, cancellationToken: ct)
            );

            // 2. Delete settlement (only if status is DRAFT)
            const string deleteSettSql = "DELETE FROM sales_service.commission_settlement WHERE id_settlement = @IdSettlement AND status = 'DRAFT';";
            var rows = await connection.ExecuteAsync(
                new CommandDefinition(deleteSettSql, new { IdSettlement = idSettlement }, transaction: transaction, cancellationToken: ct)
            );

            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private class OrderItemWithProduct
    {
        public long IdOrderItem { get; set; }
        public long IdOrder { get; set; }
        public decimal CommissionEur { get; set; }
        public decimal CommissionPen { get; set; }
        public string? ProductName { get; set; }
    }
}
