using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.UseCases.Activations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class ActivationController : ControllerBase
{
    private readonly GetPendingActivationsUseCase _getPendingActivationsUseCase;
    private readonly GetActivationsByOrderUseCase _getActivationsByOrderUseCase;
    private readonly UpdateActivationUseCase _updateActivationUseCase;
    private readonly GetDelayedActivationsUseCase _getDelayedActivationsUseCase;

    public ActivationController(
        GetPendingActivationsUseCase getPendingActivationsUseCase,
        GetActivationsByOrderUseCase getActivationsByOrderUseCase,
        UpdateActivationUseCase updateActivationUseCase,
        GetDelayedActivationsUseCase getDelayedActivationsUseCase)
    {
        _getPendingActivationsUseCase = getPendingActivationsUseCase;
        _getActivationsByOrderUseCase = getActivationsByOrderUseCase;
        _updateActivationUseCase = updateActivationUseCase;
        _getDelayedActivationsUseCase = getDelayedActivationsUseCase;
    }

    [HttpGet("api/activations/pending")]
    public async Task<IActionResult> GetPendingActivations([FromQuery] long idProvider, CancellationToken ct)
    {
        try
        {
            var activations = await _getPendingActivationsUseCase.ExecuteAsync(idProvider, ct);
            return Ok(activations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener activaciones pendientes.", details = ex.Message });
        }
    }

    [HttpGet("api/orders/{id:long}/activations")]
    public async Task<IActionResult> GetActivationsByOrder(long id, CancellationToken ct)
    {
        try
        {
            var activations = await _getActivationsByOrderUseCase.ExecuteAsync(id, ct);
            return Ok(activations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener activaciones por orden.", details = ex.Message });
        }
    }

    [HttpGet("api/activations/delayed")]
    public async Task<IActionResult> GetDelayedActivations(CancellationToken ct)
    {
        try
        {
            var activations = await _getDelayedActivationsUseCase.ExecuteAsync(ct);
            return Ok(activations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener activaciones retrasadas.", details = ex.Message });
        }
    }

    [HttpPatch("api/activations/{idItem:long}")]
    public async Task<IActionResult> UpdateActivation(long idItem, [FromBody] UpdateActivationRequestDto dto, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest(new { message = "El estado de activación es requerido." });
            }

            var actualDate = dto.ActualDate;
            if (actualDate.HasValue && actualDate.Value.Kind == DateTimeKind.Utc)
            {
                actualDate = DateTime.SpecifyKind(actualDate.Value, DateTimeKind.Unspecified);
            }

            var success = await _updateActivationUseCase.ExecuteAsync(idItem, dto.Status, actualDate, ct);
            if (!success)
            {
                return NotFound(new { message = $"No se encontró el registro de activación con ID {idItem}." });
            }

            return Ok(new { message = "Seguimiento de activación actualizado correctamente." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in UpdateActivation: " + ex.ToString());
            return StatusCode(500, new { message = "Error al actualizar la activación.", details = ex.Message });
        }
    }
}

public class UpdateActivationRequestDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime? ActualDate { get; set; }
}
