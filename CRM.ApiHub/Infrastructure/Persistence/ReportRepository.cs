using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(IDbConnectionFactory connectionFactory, ILogger<ReportRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<ConversionFunnelStageDto>> GetConversionFunnelAsync(
        long idCmpg, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                os.id_status AS StageId,
                os.code AS StageCode,
                os.name AS StageName,
                COALESCE(COUNT(o.id_order), 0)::integer AS Count
            FROM sales_service.order_status os
            LEFT JOIN sales_service.sales_order o ON os.id_status = o.id_status 
                AND o.id_cmpg = @IdCmpg
                AND o.sales_date >= @DateFrom 
                AND o.sales_date <= @DateTo
            GROUP BY os.id_status, os.code, os.name
            ORDER BY os.id_status ASC;";

        return await connection.QueryAsync<ConversionFunnelStageDto>(
            new CommandDefinition(sql, new { IdCmpg = idCmpg, DateFrom = dateFrom.Date, DateTo = dateTo.Date.AddDays(1).AddTicks(-1) }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<SalesByAsesorDto>> GetSalesByAsesorAsync(
        long supervisorId, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
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
            SELECT 
                u.id_user AS IdUser,
                u.username AS Username,
                COALESCE(NULLIF(CONCAT_WS(' ', col.name, col.paternal_surname, col.maternal_surname), ''), u.username) AS AdvisorName,
                COALESCE(COUNT(o.id_order), 0)::integer AS TotalOrders,
                COALESCE(COUNT(CASE WHEN o.id_status = 9 THEN 1 END), 0)::integer AS ApprovedOrders,
                COALESCE(SUM(
                    CASE 
                        WHEN o.currency_code = 'PEN' THEN o.total_value
                        WHEN o.currency_code = 'EUR' THEN o.total_value * 4.0
                        ELSE o.total_value * 4.0
                    END
                ), 0)::numeric AS TotalAmountPen,
                COALESCE(SUM(
                    CASE 
                        WHEN o.currency_code = 'EUR' THEN o.total_value
                        WHEN o.currency_code = 'PEN' THEN o.total_value * 0.25
                        ELSE o.total_value * 0.25
                    END
                ), 0)::numeric AS TotalAmountEur
            FROM team_members tm
            JOIN user_service.users u ON tm.id_user = u.id_user
            LEFT JOIN ext_ecosystem.collaborators col ON u.id_user = col.id_user
            LEFT JOIN sales_service.sales_order o ON u.id_user = o.id_user 
                AND o.sales_date >= @DateFrom 
                AND o.sales_date <= @DateTo
            GROUP BY u.id_user, u.username, col.name, col.paternal_surname, col.maternal_surname
            ORDER BY TotalOrders DESC;";

        try
        {
            return await connection.QueryAsync<SalesByAsesorDto>(
                new CommandDefinition(sql, new { SupervisorId = supervisorId, DateFrom = dateFrom.Date, DateTo = dateTo.Date.AddDays(1).AddTicks(-1) }, cancellationToken: ct)
            );
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Error al acceder a ext_ecosystem.collaborators (FDW) en reporte de asesor. Se usará consulta de respaldo local sin FDW.");

            const string fallbackSql = @"
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
                SELECT 
                    u.id_user AS IdUser,
                    u.username AS Username,
                    u.username AS AdvisorName,
                    COALESCE(COUNT(o.id_order), 0)::integer AS TotalOrders,
                    COALESCE(COUNT(CASE WHEN o.id_status = 9 THEN 1 END), 0)::integer AS ApprovedOrders,
                    COALESCE(SUM(
                        CASE 
                            WHEN o.currency_code = 'PEN' THEN o.total_value
                            WHEN o.currency_code = 'EUR' THEN o.total_value * 4.0
                            ELSE o.total_value * 4.0
                        END
                    ), 0)::numeric AS TotalAmountPen,
                    COALESCE(SUM(
                        CASE 
                            WHEN o.currency_code = 'EUR' THEN o.total_value
                            WHEN o.currency_code = 'PEN' THEN o.total_value * 0.25
                            ELSE o.total_value * 0.25
                        END
                    ), 0)::numeric AS TotalAmountEur
                FROM team_members tm
                JOIN user_service.users u ON tm.id_user = u.id_user
                LEFT JOIN sales_service.sales_order o ON u.id_user = o.id_user 
                    AND o.sales_date >= @DateFrom 
                    AND o.sales_date <= @DateTo
                GROUP BY u.id_user, u.username
                ORDER BY TotalOrders DESC;";

            return await connection.QueryAsync<SalesByAsesorDto>(
                new CommandDefinition(fallbackSql, new { SupervisorId = supervisorId, DateFrom = dateFrom.Date, DateTo = dateTo.Date.AddDays(1).AddTicks(-1) }, cancellationToken: ct)
            );
        }
    }

    public async Task<IncidentStatsDto> GetIncidentStatsAsync(
        long idCmpg, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var adjustedDateFrom = dateFrom.Date;
        var adjustedDateTo = dateTo.Date.AddDays(1).AddTicks(-1);

        const string summarySql = @"
            SELECT 
                COALESCE(COUNT(oi.id_incident), 0)::integer AS TotalIncidents,
                COALESCE(COUNT(CASE WHEN oi.incident_status = 'OPEN' THEN 1 END), 0)::integer AS OpenIncidents,
                COALESCE(COUNT(CASE WHEN oi.incident_status = 'IN_PROGRESS' THEN 1 END), 0)::integer AS InProgressIncidents,
                COALESCE(COUNT(CASE WHEN oi.incident_status = 'RESOLVED' THEN 1 END), 0)::integer AS ResolvedIncidents,
                COALESCE(COUNT(CASE WHEN oi.incident_status = 'CLOSED' THEN 1 END), 0)::integer AS ClosedIncidents
            FROM sales_service.order_incident oi
            JOIN sales_service.sales_order o ON oi.id_order = o.id_order
            WHERE o.id_cmpg = @IdCmpg
              AND oi.register >= @DateFrom
              AND oi.register <= @DateTo;";

        var summary = await connection.QueryFirstOrDefaultAsync<IncidentStatsDto>(
            new CommandDefinition(summarySql, new { IdCmpg = idCmpg, DateFrom = adjustedDateFrom, DateTo = adjustedDateTo }, cancellationToken: ct)
        ) ?? new IncidentStatsDto();

        const string breakdownSql = @"
            SELECT 
                ic.name AS CategoryName,
                oi.incident_status AS Status,
                COUNT(*)::integer AS Count
            FROM sales_service.order_incident oi
            JOIN sales_service.sales_order o ON oi.id_order = o.id_order
            JOIN sales_service.incident_catalog ic ON oi.id_incident = ic.id_incident
            WHERE o.id_cmpg = @IdCmpg
              AND oi.register >= @DateFrom
              AND oi.register <= @DateTo
            GROUP BY ic.name, oi.incident_status
            ORDER BY ic.name ASC, oi.incident_status ASC;";

        var breakdown = await connection.QueryAsync<IncidentCategoryBreakdownDto>(
            new CommandDefinition(breakdownSql, new { IdCmpg = idCmpg, DateFrom = adjustedDateFrom, DateTo = adjustedDateTo }, cancellationToken: ct)
        );

        summary.CategoryBreakdown = breakdown.ToList();
        return summary;
    }

    public async Task<ActivationStatsDto> GetActivationStatsAsync(
        long idProvider, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                COALESCE(COUNT(*), 0)::integer AS TotalActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'PENDING' THEN 1 END), 0)::integer AS PendingActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'IN_PROCESS' THEN 1 END), 0)::integer AS InProcessActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'ACTIVATED' THEN 1 END), 0)::integer AS ActivatedActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'DELAYED' THEN 1 END), 0)::integer AS DelayedActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'FAILED' THEN 1 END), 0)::integer AS FailedActivations,
                COALESCE(COUNT(CASE WHEN activation_status = 'CANCELLED' THEN 1 END), 0)::integer AS CancelledActivations,
                COALESCE(AVG(
                    CASE 
                        WHEN actual_activation_date IS NOT NULL THEN 
                            GREATEST(0, (actual_activation_date - expected_activation_date)::integer)
                        WHEN expected_activation_date < CURRENT_DATE THEN 
                            (CURRENT_DATE - expected_activation_date)::integer
                        ELSE 0 
                    END
                ), 0)::double precision AS AverageDelayDays
            FROM sales_service.product_activation_tracking
            WHERE id_provider = @IdProvider
              AND created_at >= @DateFrom
              AND created_at <= @DateTo;";

        return await connection.QueryFirstOrDefaultAsync<ActivationStatsDto>(
            new CommandDefinition(sql, new { IdProvider = idProvider, DateFrom = dateFrom.Date, DateTo = dateTo.Date.AddDays(1).AddTicks(-1) }, cancellationToken: ct)
        ) ?? new ActivationStatsDto();
    }
}
