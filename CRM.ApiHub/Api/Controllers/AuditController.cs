using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class AuditController : ControllerBase
{
    private readonly GetChecklistUseCase _getChecklistUseCase;
    private readonly CreateAuditUseCase _createAuditUseCase;
    private readonly SaveAuditItemUseCase _saveAuditItemUseCase;
    private readonly CloseAuditUseCase _closeAuditUseCase;

    public AuditController(
        GetChecklistUseCase getChecklistUseCase,
        CreateAuditUseCase createAuditUseCase,
        SaveAuditItemUseCase saveAuditItemUseCase,
        CloseAuditUseCase closeAuditUseCase)
    {
        _getChecklistUseCase = getChecklistUseCase;
        _createAuditUseCase = createAuditUseCase;
        _saveAuditItemUseCase = saveAuditItemUseCase;
        _closeAuditUseCase = closeAuditUseCase;
    }

    [HttpGet("api/audit/checklist/{idCmpg:long}")]
    public async Task<IActionResult> GetChecklist(long idCmpg, CancellationToken ct)
    {
        try
        {
            var checklist = await _getChecklistUseCase.ExecuteAsync(idCmpg, ct);
            if (checklist == null)
            {
                return NotFound(new { message = $"No se encontró un checklist activo para la campaña con ID {idCmpg}." });
            }
            return Ok(checklist);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el checklist.", details = ex.Message });
        }
    }

    [HttpPost("api/orders/{id:long}/audit")]
    public async Task<IActionResult> CreateAudit(long id, [FromBody] CreateAuditRequestDto dto, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        long auditorId = 1;
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId))
        {
            auditorId = parsedId;
        }

        try
        {
            var idAudit = await _createAuditUseCase.ExecuteAsync(id, dto.IdChecklist, auditorId, ct);
            return Ok(new { idAudit });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al iniciar la auditoría.", details = ex.Message });
        }
    }

    [HttpPost("api/audit/{id:long}/items")]
    public async Task<IActionResult> SaveItem(long id, [FromBody] SaveAuditItemRequestDto dto, CancellationToken ct)
    {
        try
        {
            var success = await _saveAuditItemUseCase.ExecuteAsync(id, dto.IdItem, dto.Result, dto.Observation, dto.AudioTimestamp, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudo registrar la respuesta del ítem de auditoría." });
            }
            return Ok(new { message = "Calificación de ítem registrada correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al registrar la calificación del ítem.", details = ex.Message });
        }
    }

    [HttpPatch("api/audit/{id:long}/close")]
    public async Task<IActionResult> CloseAudit(long id, [FromBody] CloseAuditRequestDto dto, CancellationToken ct)
    {
        try
        {
            var success = await _closeAuditUseCase.ExecuteAsync(id, dto.Status, ct);
            if (!success)
            {
                return NotFound(new { message = $"No se encontró la auditoría con ID {id} para realizar el cierre." });
            }
            return Ok(new { message = "Auditoría cerrada correctamente y estado actualizado." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al cerrar la auditoría.", details = ex.Message });
        }
    }
}
