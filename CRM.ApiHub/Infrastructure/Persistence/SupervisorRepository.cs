using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class SupervisorRepository : ISupervisorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private const int PENDING_BACKOFFICE_STATUS_ID = 3;

    public SupervisorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private async Task<List<long>> GetEligibleAdvisorIdsAsync(IDbConnection connection, long supervisorId, CancellationToken ct = default)
    {
        const string sql = @"
            WITH supervisor_campaigns AS (
                SELECT id_cmpg FROM user_service.user_campaign WHERE id_user = @SupervisorId AND is_active = true
            ),
            supervisor_portfolios AS (
                SELECT id_ptflo FROM user_service.user_portfolio WHERE id_user = @SupervisorId AND is_active = true
            ),
            team_members AS (
                SELECT DISTINCT uc.id_user 
                FROM user_service.user_campaign uc
                JOIN access_control.user_role ur ON uc.id_user = ur.id_user AND ur.id_role = 1 -- ASESOR
                WHERE uc.id_cmpg IN (SELECT id_cmpg FROM supervisor_campaigns) AND uc.is_active = true
                
                UNION
                
                SELECT DISTINCT up.id_user 
                FROM user_service.user_portfolio up
                JOIN access_control.user_role ur ON up.id_user = ur.id_user AND ur.id_role = 1 -- ASESOR
                WHERE up.id_ptflo IN (SELECT id_ptflo FROM supervisor_portfolios) AND up.is_active = true
            )
            SELECT id_user FROM team_members;";

        var result = await connection.QueryAsync<long>(
            new CommandDefinition(sql, new { SupervisorId = supervisorId }, cancellationToken: ct)
        );
        return result.ToList();
    }

    public async Task<IEnumerable<SalesOrder>> GetTeamOrdersAsync(
        long supervisorId,
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var advisorIds = await GetEligibleAdvisorIdsAsync(connection, supervisorId, ct);
        if (advisorIds.Count == 0)
        {
            return Array.Empty<SalesOrder>();
        }

        var sql = new StringBuilder(@"
            SELECT o.* 
            FROM sales_service.sales_order o
            WHERE o.id_user = ANY(@AdvisorIds)");

        var parameters = new DynamicParameters();
        parameters.Add("AdvisorIds", advisorIds.ToArray());

        if (userId.HasValue)
        {
            sql.Append(" AND o.id_user = @UserId");
            parameters.Add("UserId", userId.Value);
        }
        if (statusId.HasValue)
        {
            sql.Append(" AND o.id_status = @StatusId");
            parameters.Add("StatusId", statusId.Value);
        }
        if (campaignId.HasValue)
        {
            sql.Append(" AND o.id_cmpg = @CampaignId");
            parameters.Add("CampaignId", campaignId.Value);
        }
        if (dateFrom.HasValue)
        {
            sql.Append(" AND o.sales_date >= @DateFrom AND o.register >= @DateFrom");
            parameters.Add("DateFrom", dateFrom.Value);
        }
        if (dateTo.HasValue)
        {
            sql.Append(" AND o.sales_date <= @DateTo AND o.register <= @DateTo");
            parameters.Add("DateTo", dateTo.Value);
        }

        sql.Append(" ORDER BY o.sales_date DESC;");

        return await connection.QueryAsync<SalesOrder>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );
    }

    public async Task<SupervisorStatsDto> GetTeamStatsAsync(
        long supervisorId,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var advisorIds = await GetEligibleAdvisorIdsAsync(connection, supervisorId, ct);
        if (advisorIds.Count == 0)
        {
            return new SupervisorStatsDto();
        }

        const string summarySql = @"
            SELECT 
                COUNT(*) AS total_orders,
                COALESCE(SUM(total_value), 0) AS total_value,
                COALESCE(SUM(total_products), 0) AS total_products
            FROM sales_service.sales_order
            WHERE id_user = ANY(@AdvisorIds)
              AND sales_date >= @DateFrom AND sales_date <= @DateTo;";

        const string statusSql = @"
            SELECT 
                COALESCE(s.name, 'Desconocido') AS status_name,
                COUNT(o.id_order) AS count
            FROM sales_service.sales_order o
            LEFT JOIN sales_service.order_status s ON o.id_status = s.id_status
            WHERE o.id_user = ANY(@AdvisorIds)
              AND o.sales_date >= @DateFrom AND o.sales_date <= @DateTo
            GROUP BY s.name;";

        var summaryParams = new { AdvisorIds = advisorIds.ToArray(), DateFrom = dateFrom, DateTo = dateTo };

        var summary = await connection.QueryFirstOrDefaultAsync<dynamic>(
            new CommandDefinition(summarySql, summaryParams, cancellationToken: ct)
        );

        var stats = new SupervisorStatsDto();
        if (summary != null)
        {
            stats.TotalOrders = Convert.ToInt32(summary.total_orders);
            stats.TotalValue = Convert.ToDecimal(summary.total_value);
            stats.TotalProducts = Convert.ToInt32(summary.total_products);
        }

        var statusCounts = await connection.QueryAsync<dynamic>(
            new CommandDefinition(statusSql, summaryParams, cancellationToken: ct)
        );

        foreach (var row in statusCounts)
        {
            string statusName = row.status_name;
            int count = Convert.ToInt32(row.count);
            stats.OrdersByStatus[statusName] = count;
        }

        return stats;
    }

    public async Task<CRM.ApiHub.Domain.DTOs.BulkTransferResultDto> BulkTransferToBackofficeAsync(
        long[] orderIds,
        long supervisorId,
        long backofficeUserId,
        string? comment,
        CancellationToken ct = default)
    {
        var result = new CRM.ApiHub.Domain.DTOs.BulkTransferResultDto();
        if (orderIds == null || orderIds.Length == 0)
        {
            return result;
        }

        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var batchId = Guid.NewGuid();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Establecer el ID de usuario actor en la sesión
            await connection.ExecuteAsync(
                "SELECT set_config('app.current_user_id', @SupervisorIdStr, true);",
                new { SupervisorIdStr = supervisorId.ToString() },
                transaction: transaction
            );

            // 2. Ejecutar la transferencia masiva en un solo query optimizado (CTE)
            const string bulkSql = @"
                WITH target_orders AS (
                    SELECT id_order, id_status
                    FROM sales_service.sales_order
                    WHERE id_order = ANY(@OrderIds)
                    FOR UPDATE
                ),
                allowed_orders AS (
                    SELECT id_order, id_status
                    FROM target_orders
                    WHERE access_control.can_user_action(@SupervisorId, 'sales.order.bulk_transfer', id_status)
                ),
                updated_orders AS (
                    UPDATE sales_service.sales_order o
                    SET custody_user_id = @BackofficeUserId,
                        id_status = 3,
                        last_update = NOW()
                    FROM allowed_orders a
                    WHERE o.id_order = a.id_order
                    RETURNING o.id_order, a.id_status
                ),
                inserted_logs AS (
                    INSERT INTO sales_service.sales_order_custody_log (
                        id_order, log_date, from_user_id, to_user_id, 
                        from_role, to_role, transfer_type, id_status_at, 
                        comment, is_bulk, batch_id, register
                    )
                    SELECT 
                        u.id_order, NOW(), @SupervisorId, @BackofficeUserId,
                        'SUPERVISOR', 'BACKOFFICE', 'BULK_TO_BACKOFFICE', 3,
                        @Comment, true, @BatchId, NOW()
                    FROM updated_orders u
                )
                SELECT id_order FROM updated_orders;";

            var successfulList = (await connection.QueryAsync<long>(
                new CommandDefinition(
                    bulkSql, 
                    new { 
                        OrderIds = orderIds, 
                        SupervisorId = supervisorId, 
                        BackofficeUserId = backofficeUserId, 
                        Comment = comment ?? "Envío masivo al BAC", 
                        BatchId = batchId 
                    }, 
                    transaction: transaction, 
                    cancellationToken: ct
                )
            )).ToList();

            transaction.Commit();

            var successfulSet = new HashSet<long>(successfulList);
            foreach (var orderId in orderIds)
            {
                if (successfulSet.Contains(orderId))
                {
                    result.SuccessfulCount++;
                }
                else
                {
                    result.FailedCount++;
                    result.FailedOrderIds.Add(orderId);
                }
            }
        }
        catch (Exception)
        {
            transaction.Rollback();
            // Si toda la transacción falla, todas las órdenes se marcan como fallidas
            foreach (var orderId in orderIds)
            {
                result.FailedCount++;
                result.FailedOrderIds.Add(orderId);
            }
        }

        return result;
    }
}
