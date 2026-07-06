using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Leads;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CRM.ApiHub.Api.Filters;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/leads")]
public class LeadController : ControllerBase
{
    private readonly GetLeadsUseCase _getLeadsUseCase;
    private readonly GetLeadByIdUseCase _getLeadByIdUseCase;
    private readonly CreateLeadUseCase _createLeadUseCase;
    private readonly UpdateLeadStatusUseCase _updateLeadStatusUseCase;

    public LeadController(
        GetLeadsUseCase getLeadsUseCase,
        GetLeadByIdUseCase getLeadByIdUseCase,
        CreateLeadUseCase createLeadUseCase,
        UpdateLeadStatusUseCase updateLeadStatusUseCase)
    {
        _getLeadsUseCase = getLeadsUseCase;
        _getLeadByIdUseCase = getLeadByIdUseCase;
        _createLeadUseCase = createLeadUseCase;
        _updateLeadStatusUseCase = updateLeadStatusUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeads(
        [FromQuery] string? searchTerm,
        [FromQuery] long? statusId,
        [FromQuery] int? page,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        var filters = new LeadFilters
        {
            SearchTerm = searchTerm,
            StatusId = statusId,
            Page = page,
            Limit = limit
        };

        var leads = await _getLeadsUseCase.ExecuteAsync(userId, filters, ct);
        return Ok(leads);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetLeadById(long id, CancellationToken ct)
    {
        var lead = await _getLeadByIdUseCase.ExecuteAsync(id, ct);
        if (lead == null)
        {
            return NotFound(new { message = "Lead no encontrado." });
        }
        return Ok(lead);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLead([FromBody] LeadCreateDto dto, CancellationToken ct)
    {
        try
        {
            var createdLead = await _createLeadUseCase.ExecuteAsync(dto, ct);
            return CreatedAtAction(nameof(GetLeadById), new { id = createdLead.IdLead }, createdLead);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:long}/status")]
    [RequiresPermission("update_lead_status")]
    public async Task<IActionResult> UpdateLeadStatus(long id, [FromBody] LeadUpdateStatusDto dto, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long actorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        try
        {
            var success = await _updateLeadStatusUseCase.ExecuteAsync(id, dto, actorId, ct);
            if (!success)
            {
                return NotFound(new { message = "Lead no encontrado." });
            }
            return Ok(new { message = "Estado de lead actualizado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error inesperado al actualizar el estado.", details = ex.Message });
        }
    }
}
