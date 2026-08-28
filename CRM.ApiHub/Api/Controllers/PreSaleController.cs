using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using CRM.ApiHub.Api.Filters;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;

using CRM.ApiHub.Infrastructure.Services;

namespace CRM.ApiHub.Api.Controllers;

public record CallLogRequest(string CallLog, string? TargetOperator = null, IEnumerable<string>? CompletedSteps = null, string? Result = null);
public record AssignRequest(int ToUserId, string Context);
public record MultiAssignRequest(long? ToUserId1, long? ToUserId2, long? ToUserId3, string? Context);
public record ConvertRequest(int UserId);
public record PreSaleCreateDto(
    long IdCmpg,
    string? Phone,
    string? Operator,
    string? TargetOperator,
    string? FirstName,
    string? LastName,
    string? Address,
    string? Province,
    string? Dni,
    string? CoverageStatus,
    long IdStatus,
    long OwnerUserId,
    long CurrentUserId,
    long? AssignedAdvisor1Id,
    long? AssignedAdvisor2Id,
    long? AssignedAdvisor3Id,
    string? Notes
);

[Authorize]
[ApiController]
[Route("api/presales")]
public class PreSaleController : ControllerBase
{
    private readonly IPreSaleRepository _repository;
    private readonly IFlowEngineClient _flowClient;
    private readonly ILogger<PreSaleController> _logger;

    public PreSaleController(IPreSaleRepository repository, IFlowEngineClient flowClient, ILogger<PreSaleController> logger)
    {
        _repository = repository;
        _flowClient = flowClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUser([FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        if (userClaim == null || !long.TryParse(userClaim.Value, out long authenticatedUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o token inválido." });
        }

        long targetUserId = authenticatedUserId;
        if (targetUserId == -999) targetUserId = 101;
        else if (targetUserId == -1000) targetUserId = 237;
        else if (targetUserId == -998) targetUserId = 9;

        if (userId.HasValue && userId.Value != authenticatedUserId)
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
                            .Concat(User.FindAll("roles").Select(c => c.Value))
                            .ToList();

            if (userRoles.Contains("SUPERVISOR") || userRoles.Contains("BACKOFFICE") || userRoles.Contains("ADMIN_CRM") || userRoles.Contains("ADMINISTRADOR"))
            {
                targetUserId = userId.Value;
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tienes permisos para consultar las pre-ventas de otro usuario." });
            }
        }

        var preSales = await _repository.GetByUserAsync((int)targetUserId);
        return Ok(preSales);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var preSale = await _repository.GetByIdAsync(id);
        if (preSale == null) return NotFound(new { message = "Pre-venta no encontrada." });
        return Ok(preSale);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PreSaleCreateDto dto)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = dto.OwnerUserId > 0 ? dto.OwnerUserId : 1;
        if (userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var preSale = new LeadPreSale
        {
            IdCmpg = dto.IdCmpg > 0 ? dto.IdCmpg : 1,
            Phone = dto.Phone,
            Operator = dto.Operator,
            TargetOperator = dto.TargetOperator,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            Province = dto.Province,
            Dni = dto.Dni,
            CoverageStatus = "PENDIENTE",
            IdStatus = dto.IdStatus > 0 ? dto.IdStatus : 1,
            OwnerUserId = authUserId,
            CurrentUserId = authUserId,
            AssignedAdvisor1Id = dto.AssignedAdvisor1Id,
            AssignedAdvisor2Id = dto.AssignedAdvisor2Id,
            AssignedAdvisor3Id = dto.AssignedAdvisor3Id,
            Notes = dto.Notes
        };

        var id = await _repository.CreateAsync(preSale);
        
        try
        {
            await _repository.AddCallLogAsync(id, "[CP #11 Aceptación de Fichero]: Presentar Subida completada al registrar pre-venta", authUserId);

            // Auto-iniciar flujo en Nyx.FlowEngine y auto-resolver CP#11
            var startRes = await _flowClient.StartFlowInstanceAsync("PIPELINE_TELECOM", "lead_presale", id, authUserId);
            var flowDetail = await _flowClient.GetFlowInstanceDetailByEntityAsync("lead_presale", id);
            if (flowDetail?.Checkpoints?.Any() == true)
            {
                var cp11 = flowDetail.Checkpoints.FirstOrDefault(c => c.IdCheckpoint == 11 && c.Status == "PENDING");
                if (cp11 != null)
                {
                    await _flowClient.ResolveCheckpointAsync(cp11.IdCpInstance, "APPROVED", authUserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error iniciando flujo automático para lead {Id}", id);
        }

        return CreatedAtAction(nameof(GetByUser), new { userId = preSale.CurrentUserId }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PreSaleCreateDto dto, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : (dto.OwnerUserId > 0 ? dto.OwnerUserId : 101);
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var preSale = new LeadPreSale
        {
            IdPresale = id,
            IdCmpg = dto.IdCmpg > 0 ? dto.IdCmpg : 1,
            Phone = dto.Phone,
            Operator = dto.Operator,
            TargetOperator = dto.TargetOperator,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            Province = dto.Province,
            Dni = dto.Dni,
            Notes = dto.Notes
        };

        var success = await _repository.UpdateAsync(preSale, authUserId);
        if (!success) return BadRequest(new { message = "Error actualizando la pre-venta." });

        return Ok(new { message = "Pre-venta actualizada con éxito." });
    }

    [HttpPost("{id}/calls")]
    public async Task<IActionResult> AddCallLog(int id, [FromBody] CallLogRequest request, [FromQuery] long? userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        long authUserId = 101;
        if (userId.HasValue && userId.Value > 0)
        {
            authUserId = userId.Value;
        }
        else if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        if (authUserId == -999) authUserId = 101;
        else if (authUserId == -1000) authUserId = 237;
        else if (authUserId == -998) authUserId = 9;

        if (!string.IsNullOrWhiteSpace(request.TargetOperator))
        {
            try
            {
                await _repository.UpdateTargetOperatorAsync(id, request.TargetOperator);
            }
            catch { }
        }

        var result = await _repository.AddCallLogAsync(id, request.CallLog, authUserId, request.CompletedSteps, request.Result);
        if (!result) return BadRequest(new { message = "No se pudo registrar el log de la llamada." });
        return Ok(new { message = "Log de llamada registrado con éxito." });
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest request)
    {
        var result = await _repository.AssignAsync(id, request.ToUserId, request.Context);
        if (!result) return BadRequest(new { message = "No se pudo reasignar la pre-venta." });
        return Ok(new { message = "Pre-venta reasignada con éxito." });
    }

    [HttpPost("{id}/assign-multi")]
    public async Task<IActionResult> AssignMulti(int id, [FromBody] MultiAssignRequest request)
    {
        var result = await _repository.AssignMultiAsync(id, request.ToUserId1, request.ToUserId2, request.ToUserId3, request.Context ?? "");
        if (!result) return BadRequest(new { message = "No se pudieron reasignar los asesores." });
        return Ok(new { message = "Hasta 3 asesores asignados con éxito a la pre-venta." });
    }

    [HttpPost("{id}/assign-step")]
    public async Task<IActionResult> AssignStep(int id, [FromBody] AssignStepRequest request, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : 101;
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.AssignStepWithHandshakeAsync(id, request.ToUserId, request.CallStep, authUserId, request.Context ?? "");
        if (!result) return BadRequest(new { message = "No se pudo derivar la llamada con confirmación." });
        return Ok(new { message = $"Llamada {request.CallStep} derivada al asesor con solicitud de confirmación." });
    }

    [HttpPost("{id}/accept-assignment")]
    public async Task<IActionResult> AcceptAssignment(int id, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : 101;
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.AcceptAssignmentAsync(id, authUserId);
        if (!result) return BadRequest(new { message = "No se pudo aceptar la asignación." });
        return Ok(new { message = "Asignación confirmada y aceptada con éxito." });
    }

    [HttpPost("{id}/reject-assignment")]
    public async Task<IActionResult> RejectAssignment(int id, [FromBody] RejectAssignmentRequest request, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : 101;
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.RejectAssignmentAsync(id, authUserId, request.Reason ?? "Sin motivo especificado");
        if (!result) return BadRequest(new { message = "No se pudo rechazar la asignación." });
        return Ok(new { message = "Asignación rechazada. Preventa devuelta al dueño." });
    }

    [HttpPost("{id}/cancel-assignment")]
    public async Task<IActionResult> CancelAssignment(int id, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : 101;
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.CancelAssignmentAsync(id, authUserId);
        if (!result) return BadRequest(new { message = "No se pudo cancelar la derivación. Sólo el propietario puede cancelar derivaciones pendientes de aceptación." });
        return Ok(new { message = "Asignación cancelada con éxito. Gestión devuelta al dueño." });
    }

    [HttpPost("{id}/revert-assignment")]
    public async Task<IActionResult> RevertAssignment(int id, [FromBody] RevertAssignmentRequest request, [FromQuery] long? userId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = (userId.HasValue && userId.Value > 0) ? userId.Value : 101;
        if (!userId.HasValue && userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.RevertAssignmentAsync(id, authUserId, request.Context ?? "Reversión de gestión");
        if (!result) return BadRequest(new { message = "No se pudo revertir la asignación. Sólo el asesor que recibió y aceptó la transferencia puede revertir la gestión al dueño original." });
        return Ok(new { message = "Gestión revertida con éxito. Bitácora consolidada intacta." });
    }

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> Convert(int id, [FromBody] ConvertRequest? request)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        long authenticatedUserId = 1;
        if (userClaim != null && long.TryParse(userClaim.Value, out long parsedId))
        {
            authenticatedUserId = parsedId;
        }
        else if (request != null && request.UserId > 0)
        {
            authenticatedUserId = request.UserId;
        }

        if (authenticatedUserId == -999) authenticatedUserId = 101;
        else if (authenticatedUserId == -1000) authenticatedUserId = 237;
        else if (authenticatedUserId == -998) authenticatedUserId = 9;

        var leadId = await _repository.ConvertAsync(id, new { UserId = authenticatedUserId });
        if (leadId <= 0) return BadRequest(new { message = "Error al convertir la pre-venta a orden de venta." });
        return Ok(new { message = "Pre-venta convertida exitosamente.", leadId });
    }

    [HttpPost("{id}/discard")]
    public async Task<IActionResult> Discard(int id, [FromBody] DiscardRequest request)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        long authUserId = 101;
        if (userClaim != null && long.TryParse(userClaim.Value, out long parsedId) && parsedId > 0)
        {
            authUserId = parsedId;
        }

        var result = await _repository.DiscardPreSaleAsync(id, request.Reason ?? "KO / Descarte Definitivo", authUserId);
        if (!result) return BadRequest(new { message = "No se pudo descartar la pre-venta." });
        return Ok(new { message = "Pre-venta cerrada y descartada con éxito." });
    }

    [HttpGet("{id}/assignments/history")]
    public async Task<IActionResult> GetAssignmentHistory(int id)
    {
        var history = await _repository.GetAssignmentHistoryAsync(id);
        return Ok(history);
    }
}

public class AssignStepRequest
{
    public long ToUserId { get; set; }
    public int CallStep { get; set; }
    public string? Context { get; set; }
}

public class RejectAssignmentRequest
{
    public string? Reason { get; set; }
}

public class RevertAssignmentRequest
{
    public string? Context { get; set; }
}

public record DiscardRequest(string? Reason);

