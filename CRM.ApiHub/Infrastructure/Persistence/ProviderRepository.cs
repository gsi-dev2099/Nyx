using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class ProviderRepository : IProviderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProviderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProviderCatalog>> GetCatalogAsync(CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_provider AS IdProvider,
                code AS Code,
                name AS Name,
                integration_type AS IntegrationType,
                api_base_url AS ApiBaseUrl,
                api_version AS ApiVersion,
                rpa_config AS RpaConfig,
                is_active AS IsActive,
                notes AS Notes,
                created_at AS CreatedAt
            FROM sales_service.provider_catalog
            WHERE is_active = true;";

        return await connection.QueryAsync<ProviderCatalog>(
            new CommandDefinition(sql, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<ProviderStatusMapping>> GetStatusMappingAsync(long idProvider, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_mapping AS IdMapping,
                id_provider AS IdProvider,
                provider_status_code AS ProviderStatusCode,
                provider_status_name AS ProviderStatusName,
                internal_status_id AS InternalStatusId,
                internal_substatus_id AS InternalSubstatusId,
                auto_update AS AutoUpdate,
                creates_incident_id AS CreatesIncidentId,
                priority AS Priority,
                notes AS Notes,
                is_active AS IsActive,
                created_at AS CreatedAt
            FROM sales_service.provider_status_mapping
            WHERE id_provider = @IdProvider AND is_active = true
            ORDER BY priority DESC;";

        return await connection.QueryAsync<ProviderStatusMapping>(
            new CommandDefinition(sql, new { IdProvider = idProvider }, cancellationToken: ct)
        );
    }

    public async Task<bool> LogSyncAsync(
        long idProvider, 
        long? idOrder, 
        string? statusCode, 
        string result, 
        long? internalStatusBefore = null, 
        long? internalStatusAfter = null, 
        bool? success = null, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        bool finalSuccess = success ?? (!result.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase) && 
                                       !result.StartsWith("FAIL", StringComparison.OrdinalIgnoreCase));

        const string sql = @"
            INSERT INTO sales_service.provider_sync_log 
                (sync_date, id_provider, id_order, sync_type, provider_status_code, internal_status_before, internal_status_after, success, error_detail, executed_by, register)
            VALUES 
                (CURRENT_DATE, @IdProvider, @IdOrder, 'STATUS_UPDATE', @StatusCode, @InternalStatusBefore, @InternalStatusAfter, @Success, @ErrorDetail, 'SYSTEM', NOW());";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new 
            { 
                IdProvider = idProvider, 
                IdOrder = idOrder, 
                StatusCode = statusCode, 
                InternalStatusBefore = internalStatusBefore, 
                InternalStatusAfter = internalStatusAfter, 
                Success = finalSuccess, 
                ErrorDetail = result 
            }, cancellationToken: ct)
        );

        return rows > 0;
    }

    public async Task<bool> UpdateOrderStatusAsync(long idOrder, long statusId, long? substatusId, long actorUserId, CancellationToken ct = default)
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
                "SELECT set_config('app.current_user_id', @ActorIdStr, true);",
                new { ActorIdStr = actorUserId.ToString() },
                transaction: transaction
            );

            const string sql = @"
                UPDATE sales_service.sales_order 
                SET id_status = @StatusId, 
                    id_substatus = @SubstatusId, 
                    last_update = NOW() 
                WHERE id_order = @IdOrder;";

            var rows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { IdOrder = idOrder, StatusId = statusId, SubstatusId = substatusId }, transaction: transaction, cancellationToken: ct)
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

    public async Task<bool> CreateOrderIncidentAsync(long idOrder, long incidentId, string customName, string customDescription, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Fetch incident catalog details to determine custom_solution if not provided
        const string fetchIncidentDetailsSql = @"
            SELECT name, description, solution_template 
            FROM sales_service.incident_catalog 
            WHERE id_incident = @IncidentId;";

        var details = await connection.QueryFirstOrDefaultAsync<dynamic>(
            new CommandDefinition(fetchIncidentDetailsSql, new { IncidentId = incidentId }, cancellationToken: ct)
        );

        string title = customName ?? (string?)details?.name ?? "Incidencia Homologación Proveedor";
        string desc = customDescription ?? (string?)details?.description ?? "Incidencia creada automáticamente debido a cambio de estado de proveedor.";
        string sol = (string?)details?.solution_template ?? "Verificar con el proveedor.";
        string role = "BACKOFFICE";

        const string sql = @"
            INSERT INTO sales_service.order_incident 
                (id_order, id_incident, custom_name, custom_description, custom_solution, incident_status, detected_by, assigned_to_role, register)
            VALUES 
                (@IdOrder, @IncidentId, @CustomName, @CustomDescription, @CustomSolution, 'OPEN', 1, @Role, NOW());";

        var rows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new 
            { 
                IdOrder = idOrder, 
                IncidentId = incidentId, 
                CustomName = title, 
                CustomDescription = desc, 
                CustomSolution = sol, 
                Role = role 
            }, cancellationToken: ct)
        );

        return rows > 0;
    }

    public async Task<long?> GetOrderCurrentStatusAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_status FROM sales_service.sales_order WHERE id_order = @IdOrder;";
        return await connection.QueryFirstOrDefaultAsync<long?>(
            new CommandDefinition(sql, new { IdOrder = idOrder }, cancellationToken: ct)
        );
    }
}
