using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class BackofficeRepository : IBackofficeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<BackofficeRepository> _logger;

    public BackofficeRepository(IDbConnectionFactory connectionFactory, ILogger<BackofficeRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private class SalesOrderWithCount : SalesOrder
    {
        public int TotalCount { get; set; }
    }

    public async Task<CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>> GetAssignedOrdersAsync(
        long backofficeId,
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = new StringBuilder("SELECT *, COUNT(*) OVER() AS TotalCount FROM sales_service.sales_order WHERE (custody_user_id = @BackofficeId OR id_status = 3)");
            var parameters = new DynamicParameters();
            parameters.Add("BackofficeId", backofficeId);

            if (userId.HasValue)
            {
                sql.Append(" AND id_user = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            if (statusId.HasValue)
            {
                sql.Append(" AND id_status = @StatusId");
                parameters.Add("StatusId", statusId.Value);
            }
            if (campaignId.HasValue)
            {
                sql.Append(" AND id_cmpg = @CampaignId");
                parameters.Add("CampaignId", campaignId.Value);
            }
            if (dateFrom.HasValue)
            {
                sql.Append(" AND sales_date >= @DateFrom");
                parameters.Add("DateFrom", dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                sql.Append(" AND sales_date <= @DateTo");
                parameters.Add("DateTo", dateTo.Value);
            }

            sql.Append(" ORDER BY sales_date DESC");
            
            var offset = (page - 1) * pageSize;
            sql.Append(" LIMIT @Limit OFFSET @Offset;");
            parameters.Add("Limit", pageSize);
            parameters.Add("Offset", offset);

            var results = await connection.QueryAsync<SalesOrderWithCount>(
                new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
            );

            var items = new List<SalesOrder>();
            int totalCount = 0;
            foreach (var r in results)
            {
                items.Add(r);
                totalCount = r.TotalCount;
            }

            return new CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en {Method} — backofficeId={BackofficeId}, userId={UserId}, statusId={StatusId}, campaignId={CampaignId}, page={Page}, pageSize={PageSize}", 
                nameof(GetAssignedOrdersAsync), backofficeId, userId, statusId, campaignId, page, pageSize);
            throw;
        }
    }

    public async Task<IEnumerable<OrderDocument>> GetPendingVerificationAsync(long backofficeId, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT d.*, 
                   l.full_name AS CustomerName,
                   COALESCE(c.name, 'Media') AS Priority
            FROM sales_service.order_document d
            INNER JOIN sales_service.sales_order o ON d.id_order = o.id_order
            INNER JOIN lead_service.lead l ON o.id_lead = l.id_lead
            LEFT JOIN campaign_service.campaign c ON o.id_cmpg = c.id_cmpg
            WHERE o.custody_user_id = @BackofficeId 
              AND d.verification_status = 'PENDING' 
              AND d.is_active = true
            ORDER BY d.uploaded_at ASC;";

        return await connection.QueryAsync<OrderDocument>(
            new CommandDefinition(sql, new { BackofficeId = backofficeId }, cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateOrderStatusAsync(
        long idOrder,
        long toStatusId,
        long? toSubstatusId,
        string? comment,
        long actorId,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Establecer el ID de usuario actor en la sesión de PostgreSQL para que el trigger fn_log_order_status_change no falle
            await connection.ExecuteAsync(
                "SELECT set_config('app.current_user_id', @ActorIdStr, true);",
                new { ActorIdStr = actorId.ToString() },
                transaction: transaction
            );

            // 2. Obtener estado y subestado actuales con bloqueo FOR UPDATE
            const string selectSql = "SELECT id_status, id_substatus FROM sales_service.sales_order WHERE id_order = @IdOrder FOR UPDATE;";
            var current = await connection.QueryFirstOrDefaultAsync<dynamic>(
                new CommandDefinition(selectSql, new { IdOrder = idOrder }, transaction: transaction, cancellationToken: ct)
            );

            if (current == null)
            {
                transaction.Rollback();
                return false;
            }

            long? fromStatusId = current.id_status != null ? (long?)current.id_status : null;
            long? fromSubstatusId = current.id_substatus != null ? (long?)current.id_substatus : null;

            // 3. Validar si el usuario analista cuenta con el permiso sales.order.edit.backoffice para el estado actual
            const string checkPermissionSql = "SELECT access_control.can_user_action(@ActorId, 'sales.order.edit.backoffice', @StatusId);";
            var hasPermission = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(checkPermissionSql, new { ActorId = actorId, StatusId = fromStatusId }, transaction: transaction, cancellationToken: ct)
            );

            if (!hasPermission)
            {
                transaction.Rollback();
                return false;
            }

            // 4. Actualizar el estado y subestado en la orden (disparará el trigger fn_log_order_status_change)
            const string updateSql = @"
                UPDATE sales_service.sales_order 
                SET id_status = @ToStatusId, 
                    id_substatus = COALESCE(@ToSubstatusId, CASE WHEN @ToStatusId IN (11, 12) THEN 23 ELSE id_substatus END),
                    custody_user_id = CASE WHEN @ToStatusId IN (2, 11, 12) THEN id_user ELSE custody_user_id END,
                    last_update = NOW()
                WHERE id_order = @IdOrder;";

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(updateSql, new { ToStatusId = toStatusId, ToSubstatusId = toSubstatusId, IdOrder = idOrder }, transaction: transaction, cancellationToken: ct)
            );

            if (rowsAffected == 0)
            {
                transaction.Rollback();
                return false;
            }

            // 4. Si el estado o subestado cambiaron, el trigger ya habrá insertado una fila de historial.
            // La buscamos y actualizamos con comentarios.
            if (fromStatusId != toStatusId || fromSubstatusId != toSubstatusId)
            {
                const string getHistoryIdSql = @"
                    SELECT id_history 
                    FROM sales_service.sales_order_status_history 
                    WHERE id_order = @IdOrder 
                    ORDER BY id_history DESC 
                    LIMIT 1;";

                var historyId = await connection.ExecuteScalarAsync<long?>(
                    new CommandDefinition(getHistoryIdSql, new { IdOrder = idOrder }, transaction: transaction, cancellationToken: ct)
                );

                if (historyId.HasValue && !string.IsNullOrEmpty(comment))
                {
                    const string updateHistorySql = @"
                        UPDATE sales_service.sales_order_status_history 
                        SET comment = @Comment
                        WHERE id_history = @HistoryId;";

                    await connection.ExecuteAsync(
                        new CommandDefinition(updateHistorySql, new { Comment = comment, HistoryId = historyId.Value }, transaction: transaction, cancellationToken: ct)
                    );
                }
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> VerifyDocumentAsync(
        long idDoc,
        string status,
        string? notes,
        long verifiedBy,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_document 
            SET verification_status = @Status, 
                verification_notes = @Notes, 
                verified_by = @VerifiedBy, 
                verified_at = NOW()
            WHERE id_document = @IdDoc;";

        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Status = status, Notes = notes, VerifiedBy = verifiedBy, IdDoc = idDoc }, cancellationToken: ct)
        );

        return rowsAffected > 0;
    }
}
