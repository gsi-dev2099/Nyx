using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SalesOrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SalesOrder>> GetByFiltersAsync(
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new StringBuilder("SELECT * FROM sales_service.sales_order WHERE 1=1");
        var parameters = new DynamicParameters();

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
            sql.Append(" AND sales_date >= @DateFrom AND register >= @DateFrom");
            parameters.Add("DateFrom", dateFrom.Value);
        }
        if (dateTo.HasValue)
        {
            sql.Append(" AND sales_date <= @DateTo AND register <= @DateTo");
            parameters.Add("DateTo", dateTo.Value);
        }

        sql.Append(" ORDER BY sales_date DESC;");

        return await connection.QueryAsync<SalesOrder>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );
    }

    public async Task<SalesOrder?> GetByIdAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.sales_order WHERE id_order = @IdOrder;";

        return await connection.QueryFirstOrDefaultAsync<SalesOrder>(
            new CommandDefinition(sql, new { IdOrder = idOrder }, cancellationToken: ct)
        );
    }

    public async Task<long> CreateAsync(SalesOrder order, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.sales_order (
                id_lead, id_cmpg, id_user, owner_user_id, custody_user_id,
                id_status, id_substatus, currency_code, commission_currency,
                status, sales_date, total_products, total_value, is_alternate,
                register, last_update
            )
            VALUES (
                @IdLead, @IdCmpg, @IdUser, @OwnerUserId, @CustodyUserId,
                @IdStatus, @IdSubstatus, @CurrencyCode, @CommissionCurrency,
                @Status, @SalesDate, @TotalProducts, @TotalValue, @IsAlternate,
                @Register, @LastUpdate
            )
            RETURNING id_order;";

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, order, cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateStatusAsync(
        long idOrder,
        long toStatusId,
        long? toSubstatusId,
        string? comment,
        long actorId,
        bool isBulk,
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
            await connection.ExecuteAsync(
                "SELECT set_config('app.current_user_id', @ActorId, true);",
                new { ActorId = actorId.ToString() },
                transaction: transaction
            );

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

            const string updateSql = @"
                UPDATE sales_service.sales_order 
                SET id_status = @ToStatusId, id_substatus = @ToSubstatusId, last_update = NOW()
                WHERE id_order = @IdOrder;";

            await connection.ExecuteAsync(
                new CommandDefinition(updateSql, new { ToStatusId = toStatusId, ToSubstatusId = toSubstatusId, IdOrder = idOrder }, transaction: transaction, cancellationToken: ct)
            );

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

                if (historyId.HasValue)
                {
                    const string updateHistorySql = @"
                        UPDATE sales_service.sales_order_status_history 
                        SET comment = @Comment, is_bulk = @IsBulk
                        WHERE id_history = @HistoryId;";

                    await connection.ExecuteAsync(
                        new CommandDefinition(updateHistorySql, new { Comment = comment, IsBulk = isBulk, HistoryId = historyId.Value }, transaction: transaction, cancellationToken: ct)
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

    public async Task<IEnumerable<SalesOrderHistoryEventRaw>> GetOrderHistoryTimelineAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                register AS timestamp,
                'STATUS_CHANGE' AS event_type,
                changed_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Cambio de estado del pedido de ', COALESCE(from_status_id::text, 'Inicial'), ' a ', to_status_id::text) AS description,
                json_build_object(
                    'from_status_id', from_status_id,
                    'to_status_id', to_status_id,
                    'comment', comment,
                    'is_bulk', is_bulk
                )::text AS details_json
            FROM sales_service.sales_order_status_history h
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = h.changed_by
            WHERE h.id_order = @IdOrder

            UNION ALL

            SELECT 
                register AS timestamp,
                'CUSTODY_TRANSFER' AS event_type,
                from_user_id AS actor_id,
                TRIM(CONCAT(col_from.name, ' ', col_from.paternal_surname, ' ', col_from.maternal_surname)) AS actor_name,
                CONCAT('Transferencia de custodia de ', from_role, ' (', TRIM(CONCAT(col_from.name, ' ', col_from.paternal_surname)), ') a ', to_role, ' (', TRIM(CONCAT(col_to.name, ' ', col_to.paternal_surname)), ')') AS description,
                json_build_object(
                    'from_role', from_role,
                    'to_role', to_role,
                    'transfer_type', transfer_type,
                    'to_user_id', to_user_id,
                    'comment', comment
                )::text AS details_json
            FROM sales_service.sales_order_custody_log c
            LEFT JOIN ext_ecosystem.collaborators col_from ON col_from.id_user = c.from_user_id
            LEFT JOIN ext_ecosystem.collaborators col_to ON col_to.id_user = c.to_user_id
            WHERE c.id_order = @IdOrder

            UNION ALL

            SELECT 
                register AS timestamp,
                'INCIDENT_DETECTED' AS event_type,
                detected_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Incidente registrado: ', custom_name) AS description,
                json_build_object(
                    'custom_name', custom_name,
                    'custom_description', custom_description,
                    'incident_status', incident_status
                )::text AS details_json
            FROM sales_service.order_incident i
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = i.detected_by
            WHERE i.id_order = @IdOrder

            UNION ALL

            SELECT 
                resolved_at AS timestamp,
                'INCIDENT_RESOLVED' AS event_type,
                resolved_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Incidente resuelto: ', custom_name) AS description,
                json_build_object(
                    'custom_name', custom_name,
                    'resolution_notes', resolution_notes
                )::text AS details_json
            FROM sales_service.order_incident i
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = i.resolved_by
            WHERE i.id_order = @IdOrder AND i.resolved_at IS NOT NULL

            UNION ALL

            SELECT 
                r.register AS timestamp,
                'INCIDENT_RESPONSE' AS event_type,
                r.responded_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Respuesta en incidencia #', r.id_order_incident, ' (', r.response_type, '): ', r.response_text) AS description,
                json_build_object(
                    'id_order_incident', r.id_order_incident,
                    'response_type', r.response_type,
                    'response_text', r.response_text
                )::text AS details_json
            FROM sales_service.incident_response r
            INNER JOIN sales_service.order_incident i ON i.id_order_incident = r.id_order_incident
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = r.responded_by
            WHERE i.id_order = @IdOrder

            UNION ALL

            SELECT 
                uploaded_at AS timestamp,
                'DOCUMENT_UPLOADED' AS event_type,
                uploaded_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Documento subido: ', file_name, ' (', document_type, ')') AS description,
                json_build_object(
                    'file_name', file_name,
                    'document_type', document_type,
                    'file_size_kb', file_size_kb,
                    'mime_type', mime_type
                )::text AS details_json
            FROM sales_service.order_document d
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = d.uploaded_by
            WHERE d.id_order = @IdOrder AND d.is_active = true

            UNION ALL

            SELECT 
                verified_at AS timestamp,
                'DOCUMENT_VERIFIED' AS event_type,
                verified_by AS actor_id,
                TRIM(CONCAT(col.name, ' ', col.paternal_surname, ' ', col.maternal_surname)) AS actor_name,
                CONCAT('Documento verificado: ', file_name, ' (', document_type, ') como ', verification_status) AS description,
                json_build_object(
                    'file_name', file_name,
                    'document_type', document_type,
                    'verification_status', verification_status,
                    'verification_notes', verification_notes
                )::text AS details_json
            FROM sales_service.order_document d
            LEFT JOIN ext_ecosystem.collaborators col ON col.id_user = d.verified_by
            WHERE d.id_order = @IdOrder AND d.verified_at IS NOT NULL AND d.is_active = true

            ORDER BY timestamp ASC;";

        return await connection.QueryAsync<SalesOrderHistoryEventRaw>(
            new CommandDefinition(sql, new { IdOrder = idOrder }, cancellationToken: ct)
        );
     }
     
     public async Task<SalesOrder?> GetByLeadIdAsync(long idLead, CancellationToken ct = default)
     {
         using var connection = _connectionFactory.CreateConnection();
         const string sql = "SELECT * FROM sales_service.sales_order WHERE id_lead = @IdLead LIMIT 1;";
         return await connection.QueryFirstOrDefaultAsync<SalesOrder>(
             new CommandDefinition(sql, new { IdLead = idLead }, cancellationToken: ct)
         );
     }
 }
