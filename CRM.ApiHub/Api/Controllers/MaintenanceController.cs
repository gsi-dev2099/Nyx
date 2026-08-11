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

public record UserWorkShiftDto(Guid? Id, long IdUser, string? Username, int DayOfWeek, string StartTime, string EndTime, bool IsActive);
public record SaveUserWorkShiftRequest(long IdUser, int DayOfWeek, string StartTime, string EndTime, bool IsActive);

// ---- STATUS FULL CRUD ----
public record CreateStatusRequest(string Code, string Name, string Category, string? Color, string? Icon, bool IsTerminal, bool RequiresSubstatus, bool RequiresComment, bool AllowsEditByAsesor, bool AllowsEditBySupervisor, short OrderIndex);
public record UpdateStatusRequest(string? Code, string? Name, string? Category, string? Color, string? Icon, bool? IsTerminal, bool? RequiresSubstatus, bool? RequiresComment, bool? AllowsEditByAsesor, bool? AllowsEditBySupervisor, short? OrderIndex, bool? IsActive);

// ---- SUBSTATUS FULL CRUD ----
public record CreateSubstatusRequest(long IdStatus, string Code, string Name, string? Color, string? Description, short OrderIndex);
public record UpdateSubstatusRequest(string? Code, string? Name, string? Color, string? Description, short? OrderIndex, bool? IsActive);

// ---- TRANSITIONS ----
public record CreateTransitionRequest(long FromStatusId, long ToStatusId, string[] AllowedRoles, bool RequiresComment, bool RequiresFormComplete, long? IdFormRequired, bool IsBulkAllowed, string? Description);
public record UpdateTransitionRequest(string[]? AllowedRoles, bool? RequiresComment, bool? RequiresFormComplete, long? IdFormRequired, bool? IsBulkAllowed, string? Description, bool? IsActive);

// ---- PIPELINE STAGES ----
public record CreatePipelineStageRequest(long IdCmpg, string Name, string StageCode, string? Description, short OrderIndex, long? IdStatusEnter, long? IdStatusExit, bool RequiresAudit, short? SlaHours);
public record UpdatePipelineStageRequest(string? Name, string? Description, short? OrderIndex, long? IdStatusEnter, long? IdStatusExit, bool? RequiresAudit, short? SlaHours, bool? IsActive);

// ---- PROVIDER MAPPINGS ----
public record CreateProviderMappingRequest(long IdProvider, string ProviderStatusCode, string? ProviderStatusName, long InternalStatusId, long? InternalSubstatusId, bool AutoUpdate, long? CreatesIncidentId, short Priority, string? Notes);
public record UpdateProviderMappingRequest(string? ProviderStatusName, long? InternalStatusId, long? InternalSubstatusId, bool? AutoUpdate, long? CreatesIncidentId, short? Priority, string? Notes, bool? IsActive);
public record CreateProviderRequest(string Code, string Name, string IntegrationType, string? ApiBaseUrl, string? Notes);

// ---- ROLES & PERMISSIONS ----
public record CreateRoleRequest(string Name, string? Description);
public record SavePermissionRequest(long RoleId, string PermissionKey, long? StatusId, bool IsAllowed);

// ---- QUALITY AUDIT ----
public record CreateChecklistRequest(string Name, long? IdCmpg, string? Description, decimal TargetScore);

[ApiController]

[Route("api/maintenance")]
[Authorize(Roles = "ADMIN_CRM,BACKOFFICE")]

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

    // ===================== HORARIOS LABORALES COLABORADORES (NXFORTRESS SLA) =====================

    /// <summary>
    /// Get all collaborators with their configured work shifts
    /// </summary>
    [HttpGet("work-shifts")]
    public async Task<IActionResult> GetCollaboratorsWorkShifts()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                ws.id AS Id,
                u.id_user AS IdUser,
                u.username AS Username,
                COALESCE(ws.day_of_week, 1) AS DayOfWeek,
                COALESCE(ws.start_time::text, '08:00:00') AS StartTime,
                COALESCE(ws.end_time::text, '17:00:00') AS EndTime,
                COALESCE(ws.is_active, true) AS IsActive
            FROM user_service.users u
            LEFT JOIN nxf_sla.user_work_shifts ws ON u.id_user = ws.id_user
            ORDER BY u.username, ws.day_of_week;";
        var shifts = await connection.QueryAsync<UserWorkShiftDto>(sql);
        return Ok(shifts);
    }

    /// <summary>
    /// Save or update a work shift for a specific collaborator
    /// </summary>
    [HttpPost("work-shifts")]
    public async Task<IActionResult> SaveUserWorkShift([FromBody] SaveUserWorkShiftRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO nxf_sla.user_work_shifts (id_user, day_of_week, start_time, end_time, is_active)
            VALUES (@IdUser, @DayOfWeek, @StartTime::TIME, @EndTime::TIME, @IsActive)
            ON CONFLICT (id_user, day_of_week) 
            DO UPDATE SET start_time = EXCLUDED.start_time, end_time = EXCLUDED.end_time, is_active = EXCLUDED.is_active;";

        await connection.ExecuteAsync(sql, new 
        { 
            IdUser = request.IdUser, 
            DayOfWeek = request.DayOfWeek, 
            StartTime = request.StartTime, 
            EndTime = request.EndTime, 
            IsActive = request.IsActive 
        });

        return Ok(new { message = "Horario laboral de colaborador guardado exitosamente (NxFortress SLA Engine)." });
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

    /// <summary>
    /// Get ALL order substatuses for management
    /// </summary>
    [HttpGet("substatuses")]
    public async Task<IActionResult> GetAllSubstatuses()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT os.id_substatus AS IdSubstatus, os.id_status AS IdStatus, st.name AS StatusName,
                   os.code AS Code, os.name AS Name, os.color AS Color, os.description AS Description,
                   os.order_index AS OrderIndex, os.is_active AS IsActive
            FROM sales_service.order_substatus os
            LEFT JOIN sales_service.order_status st ON os.id_status = st.id_status
            ORDER BY os.id_status, os.order_index;";
        var substatuses = await connection.QueryAsync(sql);
        return Ok(substatuses);
    }

    /// <summary>
    /// Toggle is_active on an order substatus
    /// </summary>
    [HttpPatch("substatuses/{id:int}/toggle")]
    public async Task<IActionResult> ToggleSubstatusActive(int id, [FromBody] ToggleStatusRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE sales_service.order_substatus SET is_active = @IsActive WHERE id_substatus = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { IsActive = request.IsActive, Id = id });
        if (rows == 0) return NotFound(new { message = "Subestado no encontrado." });
        return Ok(new { message = $"Subestado {(request.IsActive ? "activado" : "desactivado")} correctamente." });
    }


    [HttpGet("testschema")]
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
        const string checkSql = "SELECT valid_from::timestamp FROM sales_service.exchange_rate WHERE from_currency = @FromCurrency AND to_currency = @ToCurrency AND valid_to IS NULL;";
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

    // ===================== ESTADOS CRUD COMPLETO =====================

    [HttpPost("statuses")]
    public async Task<IActionResult> CreateStatus([FromBody] CreateStatusRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.order_status
                (code, name, category, color, icon, is_terminal, requires_substatus, requires_comment,
                 allows_edit_by_asesor, allows_edit_by_supervisor, order_index, is_active)
            VALUES (@Code, @Name, @Category, @Color, @Icon, @IsTerminal, @RequiresSubstatus, @RequiresComment,
                    @AllowsEditByAsesor, @AllowsEditBySupervisor, @OrderIndex, true)
            RETURNING id_status;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/statuses/{newId}", new { id = newId, message = "Estado creado." });
    }

    [HttpPut("statuses/{id:long}")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_status SET
                code = COALESCE(@Code, code), name = COALESCE(@Name, name), category = COALESCE(@Category, category),
                color = COALESCE(@Color, color), icon = COALESCE(@Icon, icon),
                is_terminal = COALESCE(@IsTerminal, is_terminal),
                requires_substatus = COALESCE(@RequiresSubstatus, requires_substatus),
                requires_comment = COALESCE(@RequiresComment, requires_comment),
                allows_edit_by_asesor = COALESCE(@AllowsEditByAsesor, allows_edit_by_asesor),
                allows_edit_by_supervisor = COALESCE(@AllowsEditBySupervisor, allows_edit_by_supervisor),
                order_index = COALESCE(@OrderIndex, order_index),
                is_active = COALESCE(@IsActive, is_active)
            WHERE id_status = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, req.Code, req.Name, req.Category, req.Color, req.Icon, req.IsTerminal, req.RequiresSubstatus, req.RequiresComment, req.AllowsEditByAsesor, req.AllowsEditBySupervisor, req.OrderIndex, req.IsActive });
        if (rows == 0) return NotFound(new { message = "Estado no encontrado." });
        return Ok(new { message = "Estado actualizado." });
    }

    [HttpDelete("statuses/{id:long}")]
    public async Task<IActionResult> DeleteStatus(long id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync("DELETE FROM sales_service.order_status WHERE id_status = @Id;", new { Id = id });
            if (rows == 0) return NotFound(new { message = "Estado no encontrado." });
            return Ok(new { message = "Estado eliminado." });
        }
        catch (Exception ex) when (ex.Message.Contains("23503") || ex.Message.Contains("foreign key"))
        {
            return BadRequest(new { message = "No se puede eliminar: el estado tiene ventas o subestados asociados. Desactívalo en su lugar." });
        }
    }

    // ===================== SUBESTADOS CRUD COMPLETO =====================

    [HttpPost("substatuses")]
    public async Task<IActionResult> CreateSubstatus([FromBody] CreateSubstatusRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.order_substatus (id_status, code, name, color, description, order_index, is_active)
            VALUES (@IdStatus, @Code, @Name, @Color, @Description, @OrderIndex, true)
            RETURNING id_substatus;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/substatuses/{newId}", new { id = newId, message = "Subestado creado." });
    }

    [HttpPut("substatuses/{id:long}")]
    public async Task<IActionResult> UpdateSubstatus(long id, [FromBody] UpdateSubstatusRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_substatus SET
                code = COALESCE(@Code, code), name = COALESCE(@Name, name),
                color = COALESCE(@Color, color), description = COALESCE(@Description, description),
                order_index = COALESCE(@OrderIndex, order_index), is_active = COALESCE(@IsActive, is_active)
            WHERE id_substatus = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, req.Code, req.Name, req.Color, req.Description, req.OrderIndex, req.IsActive });
        if (rows == 0) return NotFound(new { message = "Subestado no encontrado." });
        return Ok(new { message = "Subestado actualizado." });
    }

    [HttpDelete("substatuses/{id:long}")]
    public async Task<IActionResult> DeleteSubstatus(long id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync("DELETE FROM sales_service.order_substatus WHERE id_substatus = @Id;", new { Id = id });
            if (rows == 0) return NotFound(new { message = "Subestado no encontrado." });
            return Ok(new { message = "Subestado eliminado." });
        }
        catch (Exception ex) when (ex.Message.Contains("23503") || ex.Message.Contains("foreign key"))
        {
            return BadRequest(new { message = "No se puede eliminar: el subestado tiene referencias activas. Desactívalo." });
        }
    }

    // ===================== TRANSICIONES DE ESTADO =====================

    [HttpGet("transitions")]
    public async Task<IActionResult> GetAllTransitions()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT t.id_transition AS IdTransition, t.from_status_id AS FromStatusId, t.to_status_id AS ToStatusId,
                   sf.name AS FromStatusName, st.name AS ToStatusName,
                   t.allowed_roles AS AllowedRoles, t.requires_comment AS RequiresComment,
                   t.requires_form_complete AS RequiresFormComplete, t.id_form_required AS IdFormRequired,
                   t.is_bulk_allowed AS IsBulkAllowed, t.description AS Description, t.is_active AS IsActive
            FROM sales_service.order_status_transition t
            LEFT JOIN sales_service.order_status sf ON t.from_status_id = sf.id_status
            LEFT JOIN sales_service.order_status st ON t.to_status_id = st.id_status
            ORDER BY t.from_status_id, t.to_status_id;";
        var transitions = await connection.QueryAsync(sql);
        return Ok(transitions);
    }

    [HttpPost("transitions")]
    public async Task<IActionResult> CreateTransition([FromBody] CreateTransitionRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.order_status_transition
                (from_status_id, to_status_id, allowed_roles, requires_comment,
                 requires_form_complete, id_form_required, is_bulk_allowed, description, is_active)
            VALUES (@FromStatusId, @ToStatusId, @AllowedRoles, @RequiresComment,
                    @RequiresFormComplete, @IdFormRequired, @IsBulkAllowed, @Description, true)
            RETURNING id_transition;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/transitions/{newId}", new { id = newId, message = "Transición creada." });
    }

    [HttpPut("transitions/{id:long}")]
    public async Task<IActionResult> UpdateTransition(long id, [FromBody] UpdateTransitionRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_status_transition SET
                allowed_roles = COALESCE(@AllowedRoles, allowed_roles),
                requires_comment = COALESCE(@RequiresComment, requires_comment),
                requires_form_complete = COALESCE(@RequiresFormComplete, requires_form_complete),
                id_form_required = COALESCE(@IdFormRequired, id_form_required),
                is_bulk_allowed = COALESCE(@IsBulkAllowed, is_bulk_allowed),
                description = COALESCE(@Description, description),
                is_active = COALESCE(@IsActive, is_active)
            WHERE id_transition = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, req.AllowedRoles, req.RequiresComment, req.RequiresFormComplete, req.IdFormRequired, req.IsBulkAllowed, req.Description, req.IsActive });
        if (rows == 0) return NotFound(new { message = "Transición no encontrada." });
        return Ok(new { message = "Transición actualizada." });
    }

    [HttpDelete("transitions/{id:long}")]
    public async Task<IActionResult> DeleteTransition(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync("DELETE FROM sales_service.order_status_transition WHERE id_transition = @Id;", new { Id = id });
        if (rows == 0) return NotFound(new { message = "Transición no encontrada." });
        return Ok(new { message = "Transición eliminada." });
    }

    // ===================== PIPELINE POR CAMPAÑA =====================

    [HttpGet("pipeline/{idCmpg:long}")]
    public async Task<IActionResult> GetCampaignPipeline(long idCmpg)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ps.id_stage AS IdStage, ps.id_cmpg AS IdCmpg, ps.name AS Name, ps.stage_code AS StageCode,
                   ps.description AS Description, ps.order_index AS OrderIndex,
                   ps.id_status_enter AS IdStatusEnter, ps.id_status_exit AS IdStatusExit,
                   se.name AS StatusEnterName, sx.name AS StatusExitName,
                   ps.requires_audit AS RequiresAudit, ps.sla_hours AS SlaHours, ps.is_active AS IsActive
            FROM sales_service.campaign_pipeline_stage ps
            LEFT JOIN sales_service.order_status se ON ps.id_status_enter = se.id_status
            LEFT JOIN sales_service.order_status sx ON ps.id_status_exit = sx.id_status
            WHERE ps.id_cmpg = @IdCmpg
            ORDER BY ps.order_index;";
        var stages = await connection.QueryAsync(sql, new { IdCmpg = idCmpg });
        return Ok(stages);
    }

    [HttpPost("pipeline")]
    public async Task<IActionResult> CreatePipelineStage([FromBody] CreatePipelineStageRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.campaign_pipeline_stage
                (id_cmpg, name, stage_code, description, order_index, id_status_enter, id_status_exit, requires_audit, sla_hours, is_active)
            VALUES (@IdCmpg, @Name, @StageCode, @Description, @OrderIndex, @IdStatusEnter, @IdStatusExit, @RequiresAudit, @SlaHours, true)
            RETURNING id_stage;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/pipeline/{newId}", new { id = newId, message = "Etapa creada." });
    }

    [HttpPut("pipeline/{id:long}")]
    public async Task<IActionResult> UpdatePipelineStage(long id, [FromBody] UpdatePipelineStageRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.campaign_pipeline_stage SET
                name = COALESCE(@Name, name), description = COALESCE(@Description, description),
                order_index = COALESCE(@OrderIndex, order_index),
                id_status_enter = COALESCE(@IdStatusEnter, id_status_enter),
                id_status_exit = COALESCE(@IdStatusExit, id_status_exit),
                requires_audit = COALESCE(@RequiresAudit, requires_audit),
                sla_hours = COALESCE(@SlaHours, sla_hours),
                is_active = COALESCE(@IsActive, is_active)
            WHERE id_stage = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, req.Name, req.Description, req.OrderIndex, req.IdStatusEnter, req.IdStatusExit, req.RequiresAudit, req.SlaHours, req.IsActive });
        if (rows == 0) return NotFound(new { message = "Etapa no encontrada." });
        return Ok(new { message = "Etapa actualizada." });
    }

    [HttpDelete("pipeline/{id:long}")]
    public async Task<IActionResult> DeletePipelineStage(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync("DELETE FROM sales_service.campaign_pipeline_stage WHERE id_stage = @Id;", new { Id = id });
        if (rows == 0) return NotFound(new { message = "Etapa no encontrada." });
        return Ok(new { message = "Etapa eliminada." });
    }

    // ===================== PROVEEDORES =====================

    [HttpGet("providers")]
    public async Task<IActionResult> GetProviders()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_provider AS IdProvider, code AS Code, name AS Name,
                   integration_type AS IntegrationType, api_base_url AS ApiBaseUrl,
                   is_active AS IsActive, notes AS Notes, created_at AS CreatedAt
            FROM sales_service.provider_catalog ORDER BY name;";
        return Ok(await connection.QueryAsync(sql));
    }

    [HttpPost("providers")]
    public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.provider_catalog (code, name, integration_type, api_base_url, notes, is_active)
            VALUES (@Code, @Name, @IntegrationType, @ApiBaseUrl, @Notes, true)
            RETURNING id_provider;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/providers/{newId}", new { id = newId, message = "Proveedor creado." });
    }

    // ===================== MAPEADOR DE PROVEEDORES =====================

    [HttpGet("provider-mappings")]
    public async Task<IActionResult> GetProviderMappings()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pm.id_mapping AS IdMapping, pm.id_provider AS IdProvider, p.name AS ProviderName,
                   pm.provider_status_code AS ProviderStatusCode, pm.provider_status_name AS ProviderStatusName,
                   pm.internal_status_id AS InternalStatusId, s.name AS InternalStatusName,
                   pm.internal_substatus_id AS InternalSubstatusId, sub.name AS InternalSubstatusName,
                   pm.auto_update AS AutoUpdate, pm.creates_incident_id AS CreatesIncidentId,
                   pm.priority AS Priority, pm.notes AS Notes, pm.is_active AS IsActive
            FROM sales_service.provider_status_mapping pm
            LEFT JOIN sales_service.provider_catalog p ON pm.id_provider = p.id_provider
            LEFT JOIN sales_service.order_status s ON pm.internal_status_id = s.id_status
            LEFT JOIN sales_service.order_substatus sub ON pm.internal_substatus_id = sub.id_substatus
            ORDER BY p.name, pm.priority;";
        return Ok(await connection.QueryAsync(sql));
    }

    [HttpPost("provider-mappings")]
    public async Task<IActionResult> CreateProviderMapping([FromBody] CreateProviderMappingRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.provider_status_mapping
                (id_provider, provider_status_code, provider_status_name, internal_status_id,
                 internal_substatus_id, auto_update, creates_incident_id, priority, notes, is_active)
            VALUES (@IdProvider, @ProviderStatusCode, @ProviderStatusName, @InternalStatusId,
                    @InternalSubstatusId, @AutoUpdate, @CreatesIncidentId, @Priority, @Notes, true)
            RETURNING id_mapping;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/provider-mappings/{newId}", new { id = newId, message = "Mapeo creado." });
    }

    [HttpPut("provider-mappings/{id:long}")]
    public async Task<IActionResult> UpdateProviderMapping(long id, [FromBody] UpdateProviderMappingRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.provider_status_mapping SET
                provider_status_name = COALESCE(@ProviderStatusName, provider_status_name),
                internal_status_id = COALESCE(@InternalStatusId, internal_status_id),
                internal_substatus_id = COALESCE(@InternalSubstatusId, internal_substatus_id),
                auto_update = COALESCE(@AutoUpdate, auto_update),
                creates_incident_id = COALESCE(@CreatesIncidentId, creates_incident_id),
                priority = COALESCE(@Priority, priority),
                notes = COALESCE(@Notes, notes),
                is_active = COALESCE(@IsActive, is_active)
            WHERE id_mapping = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, req.ProviderStatusName, req.InternalStatusId, req.InternalSubstatusId, req.AutoUpdate, req.CreatesIncidentId, req.Priority, req.Notes, req.IsActive });
        if (rows == 0) return NotFound(new { message = "Mapeo no encontrado." });
        return Ok(new { message = "Mapeo actualizado." });
    }

    [HttpDelete("provider-mappings/{id:long}")]
    public async Task<IActionResult> DeleteProviderMapping(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync("DELETE FROM sales_service.provider_status_mapping WHERE id_mapping = @Id;", new { Id = id });
        if (rows == 0) return NotFound(new { message = "Mapeo no encontrado." });
        return Ok(new { message = "Mapeo eliminado." });
    }

    // ===================== ROLES & PERMISOS =====================

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_role AS IdRole, name AS Name, description AS Description, is_active AS IsActive, register AS Register FROM access_control.role ORDER BY id_role;";
        return Ok(await connection.QueryAsync(sql));
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "INSERT INTO access_control.role (name, description, is_active, register) VALUES (@Name, @Description, true, NOW()) RETURNING id_role;";
        var newId = await connection.ExecuteScalarAsync<long>(sql, req);
        return Created($"/api/maintenance/roles/{newId}", new { id = newId, message = "Rol creado." });
    }

    [HttpGet("roles/{roleId:long}/permissions")]
    public async Task<IActionResult> GetRolePermissions(long roleId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_rps AS IdRps, id_role AS IdRole, permission_key AS PermissionKey, id_status AS IdStatus, is_allowed AS IsAllowed FROM access_control.role_permission_by_status WHERE id_role = @RoleId AND is_active = true;";
        return Ok(await connection.QueryAsync(sql, new { RoleId = roleId }));
    }

    [HttpPost("roles/permissions")]
    public async Task<IActionResult> SaveRolePermission([FromBody] SavePermissionRequest req)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO access_control.role_permission_by_status (id_role, permission_key, id_status, is_allowed, is_active)
            VALUES (@RoleId, @PermissionKey, @StatusId, @IsAllowed, true)
            ON CONFLICT (id_role, permission_key, id_status)
            DO UPDATE SET is_allowed = EXCLUDED.is_allowed;";
        await connection.ExecuteAsync(sql, req);
        return Ok(new { message = "Permiso guardado." });
    }

    // ===================== LOGS DE CUSTODIA & SISTEMA =====================

    [HttpGet("custody-logs")]
    public async Task<IActionResult> GetCustodyLogs([FromQuery] int limit = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cl.id_log AS IdLog, cl.id_order AS IdOrder, cl.from_user_id AS FromUserId,
                   cl.to_user_id AS ToUserId, u1.username AS FromUsername, u2.username AS ToUsername,
                   cl.from_role AS FromRole, cl.to_role AS ToRole, cl.transfer_type AS TransferType,
                   cl.id_status_at AS IdStatusAt, cl.comment AS Comment, cl.is_bulk AS IsBulk,
                   cl.register AS Register
            FROM sales_service.sales_order_custody_log cl
            LEFT JOIN user_service.users u1 ON cl.from_user_id = u1.id_user
            LEFT JOIN user_service.users u2 ON cl.to_user_id = u2.id_user
            ORDER BY cl.register DESC LIMIT @Limit;";
        return Ok(await connection.QueryAsync(sql, new { Limit = limit }));
    }

    [HttpGet("system-logs")]
    public async Task<IActionResult> GetSystemAuditLogs([FromQuery] int limit = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_audit AS IdAudit, schema_name AS SchemaName, table_name AS TableName,
                   operation AS Operation, record_id AS RecordId, old_value AS OldValue,
                   new_value AS NewValue, changed_by AS ChangedBy, client_ip AS ClientIp,
                   register AS Register
            FROM system_audit.audit_log
            ORDER BY register DESC LIMIT @Limit;";
        return Ok(await connection.QueryAsync(sql, new { Limit = limit }));
    }

    // ===================== COMISIONES Y CHECKLISTS =====================

    [HttpGet("quality-checklists")]
    public async Task<IActionResult> GetQualityChecklists()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id_checklist AS IdChecklist, name AS Name, id_cmpg AS IdCmpg,
                   version AS Version, is_active AS IsActive, created_by AS CreatedBy,
                   created_at AS CreatedAt
            FROM sales_service.audit_checklist_template ORDER BY name;";
        return Ok(await connection.QueryAsync(sql));
    }

    [HttpGet("commissions")]
    public async Task<IActionResult> GetCommissions()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cs.id_settlement AS IdSettlement, cs.id_user AS IdUser, u.username AS Username,
                   cs.period_start AS PeriodStart, cs.period_end AS PeriodEnd,
                   cs.total_orders AS TotalOrders, cs.total_eur AS TotalEur, cs.total_pen AS TotalPen,
                   cs.status AS Status, cs.approved_by AS ApprovedBy, cs.paid_at AS PaidAt,
                   cs.notes AS Notes, cs.created_at AS CreatedAt
            FROM sales_service.commission_settlement cs
            LEFT JOIN user_service.users u ON cs.id_user = u.id_user
            ORDER BY cs.created_at DESC;";
        return Ok(await connection.QueryAsync(sql));
    }
}
