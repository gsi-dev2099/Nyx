using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Infrastructure.Persistence;
using Dapper;
using System.Security.Claims;

namespace CRM.ApiHub.Api.Controllers;

public record ToggleStatusRequest(bool IsActive);
public record ToggleVisibilityRequest(bool IsVisible);
public record CreateExchangeRateRequest(string FromCurrency, string ToCurrency, decimal Rate, DateTime ValidFrom, DateTime? ValidTo, string Source);
public record CreateIncidentCatalogRequest(long IdCmpg, long IdStatus, string Code, string Name, string? Description, string? SolutionTemplate, string? ResolutionType, bool RequiresResponse, bool IsRecurrent, short Priority, short SlaHours);
public record UpdateIncidentCatalogRequest(string? Name, string? Description, string? SolutionTemplate, string? ResolutionType, bool? RequiresResponse, bool? IsRecurrent, short? Priority, short? SlaHours, bool? IsActive);

[ApiController]
[Route("api/maintenance")]
[Authorize(Roles = "ADMIN_CRM,COORDINADOR,BACKOFFICE")]
public class MaintenanceController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MaintenanceController(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ===================== ESTADOS DE VENTA =====================

    /// <summary>
    /// Get ALL order statuses (including inactive) for management
    /// </summary>
    [HttpGet("statuses")]
    public async Task<IActionResult> GetAllStatuses()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_status AS Id, code AS Code, name AS Name, category AS Category, 
                   color AS Color, icon AS Icon, is_terminal AS IsTerminal,
                   requires_substatus AS RequiresSubstatus, requires_comment AS RequiresComment,
                   allows_edit_by_asesor AS AllowsEditByAsesor, allows_edit_by_supervisor AS AllowsEditBySupervisor,
                   order_index AS OrderIndex, is_active AS IsActive
            FROM sales_service.order_status
            ORDER BY order_index, id_status;";
        var statuses = await connection.QueryAsync(sql);
        return Ok(statuses);
    }

    /// <summary>
    /// Toggle is_active on an order status
    /// </summary>
    [HttpPatch("statuses/{id:int}/toggle")]
    public async Task<IActionResult> ToggleStatusActive(int id, [FromBody] ToggleStatusRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE sales_service.order_status SET is_active = @IsActive WHERE id_status = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { IsActive = request.IsActive, Id = id });
        if (rows == 0) return NotFound(new { message = "Estado no encontrado." });
        return Ok(new { message = $"Estado {(request.IsActive ? "activado" : "desactivado")} correctamente." });
    }

    [HttpGet("testschema")]
    [AllowAnonymous]
    public async Task<IActionResult> TestSchema()
    {
        using var connection = _connectionFactory.CreateConnection();
        var cols = await connection.QueryAsync<string>("SELECT column_name FROM information_schema.columns WHERE table_name = 'product';");
        return Ok(cols);
    }

    // ===================== PRODUCTOS / TARIFAS =====================

    /// <summary>
    /// Get ALL products (including inactive) for management
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_prod AS Id, sku AS Sku, name AS Name, description AS Description,
                   price_base AS UnitPrice, 0 AS StockQuantity, is_active AS IsActive
            FROM product_service.product
            ORDER BY id_prod;";
        var products = await connection.QueryAsync(sql);
        return Ok(products);
    }

    /// <summary>
    /// Toggle is_active on a product (acts as is_visible)
    /// </summary>
    [HttpPatch("products/{id:int}/toggle")]
    public async Task<IActionResult> ToggleProductActive(int id, [FromBody] ToggleStatusRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE product SET is_active = @IsActive WHERE id_prod = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { IsActive = request.IsActive, Id = id });
        if (rows == 0) return NotFound(new { message = "Producto no encontrado." });
        return Ok(new { message = $"Producto {(request.IsActive ? "visible" : "oculto")} correctamente." });
    }

    // ===================== CATÁLOGO DE INCIDENCIAS =====================

    /// <summary>
    /// Get ALL incident catalog entries (including inactive) for management
    /// </summary>
    [HttpGet("incidents")]
    public async Task<IActionResult> GetAllIncidentCatalog()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ic.id_incident AS IdIncident, ic.id_cmpg AS IdCmpg, ic.id_status AS IdStatus,
                   ic.code AS Code, ic.name AS Name, ic.description AS Description,
                   ic.solution_template AS SolutionTemplate, ic.resolution_type AS ResolutionType,
                   ic.requires_response AS RequiresResponse, ic.is_recurrent AS IsRecurrent,
                   ic.priority AS Priority, ic.sla_hours AS SlaHours, 
                   ic.created_by AS CreatedBy, ic.is_active AS IsActive, ic.created_at AS CreatedAt,
                   c.name AS CampaignName
            FROM sales_service.incident_catalog ic
            LEFT JOIN campaign_service.campaign c ON ic.id_cmpg = c.id_cmpg
            ORDER BY ic.id_incident;";
        var items = await connection.QueryAsync(sql);
        return Ok(items);
    }

    /// <summary>
    /// Create a new incident catalog entry
    /// </summary>
    [HttpPost("incidents")]
    public async Task<IActionResult> CreateIncidentCatalog([FromBody] CreateIncidentCatalogRequest request)
    {
        var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        long createdBy = 1;
        if (actorIdClaim != null && long.TryParse(actorIdClaim.Value, out long parsedId))
            createdBy = parsedId;

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.incident_catalog 
                (id_cmpg, id_status, code, name, description, solution_template, resolution_type,
                 requires_response, is_recurrent, priority, sla_hours, created_by, is_active, created_at)
            VALUES 
                (@IdCmpg, @IdStatus, @Code, @Name, @Description, @SolutionTemplate, @ResolutionType,
                 @RequiresResponse, @IsRecurrent, @Priority, @SlaHours, @CreatedBy, true, NOW())
            RETURNING id_incident;";

        var newId = await connection.ExecuteScalarAsync<long>(sql, new
        {
            request.IdCmpg,
            request.IdStatus,
            request.Code,
            request.Name,
            request.Description,
            request.SolutionTemplate,
            request.ResolutionType,
            request.RequiresResponse,
            request.IsRecurrent,
            request.Priority,
            request.SlaHours,
            CreatedBy = createdBy
        });

        return Created($"/api/maintenance/incidents/{newId}", new { id = newId, message = "Incidencia de catálogo creada correctamente." });
    }

    /// <summary>
    /// Update an incident catalog entry
    /// </summary>
    [HttpPut("incidents/{id:long}")]
    public async Task<IActionResult> UpdateIncidentCatalog(long id, [FromBody] UpdateIncidentCatalogRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.incident_catalog SET
                name = COALESCE(@Name, name),
                description = COALESCE(@Description, description),
                solution_template = COALESCE(@SolutionTemplate, solution_template),
                resolution_type = COALESCE(@ResolutionType, resolution_type),
                requires_response = COALESCE(@RequiresResponse, requires_response),
                is_recurrent = COALESCE(@IsRecurrent, is_recurrent),
                priority = COALESCE(@Priority, priority),
                sla_hours = COALESCE(@SlaHours, sla_hours),
                is_active = COALESCE(@IsActive, is_active)
            WHERE id_incident = @Id;";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            request.Name,
            request.Description,
            request.SolutionTemplate,
            request.ResolutionType,
            request.RequiresResponse,
            request.IsRecurrent,
            request.Priority,
            request.SlaHours,
            request.IsActive
        });

        if (rows == 0) return NotFound(new { message = "Incidencia de catálogo no encontrada." });
        return Ok(new { message = "Incidencia de catálogo actualizada correctamente." });
    }

    /// <summary>
    /// Delete an incident catalog entry
    /// </summary>
    [HttpDelete("incidents/{id:long}")]
    public async Task<IActionResult> DeleteIncidentCatalog(long id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM sales_service.incident_catalog WHERE id_incident = @Id;";
            var rows = await connection.ExecuteAsync(sql, new { Id = id });
            if (rows == 0) return NotFound(new { message = "Incidencia de catálogo no encontrada." });
            return Ok(new { message = "Incidencia de catálogo eliminada correctamente." });
        }
        catch (Exception ex) when (ex.Message.Contains("23503") || ex.Message.Contains("foreign key") || ex.Message.Contains("violates foreign key"))
        {
            return BadRequest(new { message = "No se puede eliminar la incidencia porque ya ha sido asignada a órdenes existentes. Se recomienda desactivarla en su lugar." });
        }
    }

    // ===================== TIPOS DE CAMBIO EUR/PEN =====================

    /// <summary>
    /// Get ALL exchange rates for management
    /// </summary>
    [HttpGet("exchange-rates")]
    public async Task<IActionResult> GetExchangeRates()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_rate AS IdRate, from_currency AS FromCurrency, to_currency AS ToCurrency,
                   rate AS Rate, valid_from AS ValidFrom, valid_to AS ValidTo,
                   source AS Source, created_by AS CreatedBy, created_at AS CreatedAt
            FROM sales_service.exchange_rate
            ORDER BY valid_from DESC;";
        var rates = await connection.QueryAsync(sql);
        return Ok(rates);
    }

    /// <summary>
    /// Insert a new exchange rate
    /// </summary>
    [HttpPost("exchange-rates")]
    public async Task<IActionResult> CreateExchangeRate([FromBody] CreateExchangeRateRequest request)
    {
        var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        long createdBy = 1;
        if (actorIdClaim != null && long.TryParse(actorIdClaim.Value, out long parsedId))
            createdBy = parsedId;

        using var connection = _connectionFactory.CreateConnection();

        // Close previous rate for the same currency pair
        const string checkSql = "SELECT valid_from FROM sales_service.exchange_rate WHERE from_currency = @FromCurrency AND to_currency = @ToCurrency AND valid_to IS NULL;";
        var currentValidFrom = await connection.QuerySingleOrDefaultAsync<DateTime?>(checkSql, new { request.FromCurrency, request.ToCurrency });
        
        if (currentValidFrom.HasValue && request.ValidFrom <= currentValidFrom.Value)
        {
            return BadRequest(new { message = $"La fecha de inicio debe ser mayor a la del tipo de cambio actual ({currentValidFrom.Value:dd/MM/yyyy HH:mm})." });
        }

        const string closeSql = @"
            UPDATE sales_service.exchange_rate 
            SET valid_to = @ValidFrom 
            WHERE from_currency = @FromCurrency AND to_currency = @ToCurrency 
              AND valid_to IS NULL;";
        await connection.ExecuteAsync(closeSql, new { request.FromCurrency, request.ToCurrency, request.ValidFrom });

        const string insertSql = @"
            INSERT INTO sales_service.exchange_rate 
                (from_currency, to_currency, rate, valid_from, valid_to, source, created_by, created_at)
            VALUES 
                (@FromCurrency, @ToCurrency, @Rate, @ValidFrom, @ValidTo, @Source, @CreatedBy, NOW())
            RETURNING id_rate;";

        var newId = await connection.ExecuteScalarAsync<long>(insertSql, new
        {
            FromCurrency = request.FromCurrency.ToUpper(),
            ToCurrency = request.ToCurrency.ToUpper(),
            request.Rate,
            ValidFrom = DateTime.SpecifyKind(request.ValidFrom, DateTimeKind.Unspecified),
            ValidTo = request.ValidTo.HasValue ? DateTime.SpecifyKind(request.ValidTo.Value, DateTimeKind.Unspecified) : (DateTime?)null,
            Source = request.Source ?? "MANUAL",
            CreatedBy = createdBy
        });

        return Created($"/api/maintenance/exchange-rates/{newId}", new { id = newId, message = "Tipo de cambio registrado correctamente." });
    }

    // ===================== CAMPAÑAS (auxiliar) =====================

    /// <summary>
    /// Get campaigns for dropdowns
    /// </summary>
    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_cmpg AS Id, name AS Name FROM campaign_service.campaign WHERE is_active = true ORDER BY name;";
        var campaigns = await connection.QueryAsync(sql);
        return Ok(campaigns);
    }
}
