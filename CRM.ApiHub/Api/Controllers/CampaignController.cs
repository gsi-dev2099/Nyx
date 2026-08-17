using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Persistence;
using Dapper;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize]
public class CampaignController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(IDbConnectionFactory connectionFactory, ILogger<CampaignController> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCampaigns()
    {
        using var connection = _connectionFactory.CreateConnection();
        // Usamos SQL directo con alias para asegurar que el JSON tenga "id" y no venga en 0
        // ya que el Entity 'Campaign' tiene [Column] que Dapper ignora.
        const string sql = "SELECT id_cmpg as Id, name as Name FROM campaign_service.campaign WHERE is_active = true;";
        var campaigns = await connection.QueryAsync(sql);
        
        return Ok(campaigns);
    }

    [HttpGet("{id}/statuses")]
    public async Task<IActionResult> GetStatuses(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_status as Id, name as Name, color as Color FROM sales_service.order_status WHERE is_active = true;";
        var statuses = await connection.QueryAsync(sql);
        
        return Ok(statuses);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] long? userId,
        [FromQuery] long? statusId,
        [FromQuery] long? campaignId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new System.Text.StringBuilder(@"
            WITH orders_with_clients AS (
                SELECT 
                    o.id_order as IdOrder,
                    o.id_cmpg as IdCmpg,
                    o.id_status as IdStatus,
                    o.id_lead as IdLead,
                    o.id_user as IdUser,
                    o.sales_date as SalesDate,
                    EXTRACT(DAY FROM (NOW() - o.sales_date))::int as DaysElapsed,
                    (
                        SELECT sod.value_text 
                        FROM sales_service.sales_order_data sod
                        JOIN sales_service.sales_form_field sff ON sod.id_fld = sff.id_fld
                        WHERE sod.id_order = o.id_order 
                          AND (sff.field_key ILIKE '%nombre%' OR sff.field_key ILIKE '%name%' OR sff.label ILIKE '%nombre%' OR sff.label ILIKE '%name%')
                        LIMIT 1
                    ) as ClientName
                FROM sales_service.sales_order o
            )
            SELECT * FROM orders_with_clients
            WHERE ClientName IS NOT NULL AND ClientName <> ''");

        var parameters = new DynamicParameters();

        if (userId.HasValue)
        {
            sql.Append(" AND IdUser = @UserId");
            parameters.Add("UserId", userId.Value);
        }
        if (statusId.HasValue)
        {
            sql.Append(" AND IdStatus = @StatusId");
            parameters.Add("StatusId", statusId.Value);
        }
        if (campaignId.HasValue)
        {
            sql.Append(" AND IdCmpg = @CampaignId");
            parameters.Add("CampaignId", campaignId.Value);
        }
        if (dateFrom.HasValue)
        {
            sql.Append(" AND SalesDate >= @DateFrom");
            parameters.Add("DateFrom", dateFrom.Value);
        }
        if (dateTo.HasValue)
        {
            sql.Append(" AND SalesDate <= @DateTo");
            parameters.Add("DateTo", dateTo.Value);
        }

        sql.Append(" ORDER BY SalesDate DESC;");

        var orders = await connection.QueryAsync(sql.ToString(), parameters);
        return Ok(orders);
    }

    [HttpGet("advisors")]
    public async Task<IActionResult> GetAdvisors([FromQuery] string role = "ASESOR")
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                u.id_user as Id,
                COALESCE(NULLIF(TRIM(CONCAT_WS(' ', col.name, col.paternal_surname, col.maternal_surname)), ''), u.username) as Name
            FROM user_service.users u
            LEFT JOIN ext_ecosystem.collaborators col ON u.id_user = col.id_user
            LEFT JOIN access_control.user_role ur ON u.id_user = ur.id_user AND ur.is_active = true
            LEFT JOIN access_control.role r ON ur.id_role = r.id_role AND r.is_active = true
            WHERE r.name = @Role
            ORDER BY Name;";
        
        try
        {
            var advisors = await connection.QueryAsync(sql, new { Role = role.ToUpper() });
            return Ok(advisors);
        }
        catch (DbException ex)
        {
            _logger.LogWarning(ex, "Error al acceder a ext_ecosystem.collaborators (FDW) al obtener asesores. Se usará consulta de respaldo local sin FDW.");

            const string fallbackSql = @"
                SELECT 
                    u.id_user as Id,
                    u.username as Name
                FROM user_service.users u
                LEFT JOIN access_control.user_role ur ON u.id_user = ur.id_user AND ur.is_active = true
                LEFT JOIN access_control.role r ON ur.id_role = r.id_role AND r.is_active = true
                WHERE r.name = @Role
                ORDER BY Name;";

            var advisors = await connection.QueryAsync(fallbackSql, new { Role = role.ToUpper() });
            return Ok(advisors);
        }
    }
}
