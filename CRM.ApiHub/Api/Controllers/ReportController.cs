using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.UseCases.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly GetConversionFunnelUseCase _getConversionFunnelUseCase;
    private readonly GetSalesByAsesorUseCase _getSalesByAsesorUseCase;
    private readonly GetIncidentStatsUseCase _getIncidentStatsUseCase;
    private readonly GetActivationStatsUseCase _getActivationStatsUseCase;

    public ReportController(
        GetConversionFunnelUseCase getConversionFunnelUseCase,
        GetSalesByAsesorUseCase getSalesByAsesorUseCase,
        GetIncidentStatsUseCase getIncidentStatsUseCase,
        GetActivationStatsUseCase getActivationStatsUseCase)
    {
        _getConversionFunnelUseCase = getConversionFunnelUseCase;
        _getSalesByAsesorUseCase = getSalesByAsesorUseCase;
        _getIncidentStatsUseCase = getIncidentStatsUseCase;
        _getActivationStatsUseCase = getActivationStatsUseCase;
    }

    [HttpGet("api/reports/funnel")]
    public async Task<IActionResult> GetConversionFunnel(
        [FromQuery] long idCmpg, 
        [FromQuery] DateTime? dateFrom, 
        [FromQuery] DateTime? dateTo, 
        CancellationToken ct)
    {
        try
        {
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var funnel = await _getConversionFunnelUseCase.ExecuteAsync(idCmpg, from, to, ct);
            return Ok(funnel);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el embudo de conversión.", details = ex.Message });
        }
    }

    [HttpGet("api/reports/sales-by-asesor")]
    public async Task<IActionResult> GetSalesByAsesor(
        [FromQuery] DateTime? dateFrom, 
        [FromQuery] DateTime? dateTo, 
        CancellationToken ct)
    {
        try
        {
            var actorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
            if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out long currentUserId))
            {
                return Unauthorized(new { message = "Identidad no encontrada en el token." });
            }

            long supervisorId = User.IsInRole("SUPERVISOR") ? currentUserId : 0;

            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var sales = await _getSalesByAsesorUseCase.ExecuteAsync(supervisorId, from, to, ct);
            return Ok(sales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las ventas por asesor.", details = ex.Message });
        }
    }

    [HttpGet("api/reports/incidents")]
    public async Task<IActionResult> GetIncidentStats(
        [FromQuery] long idCmpg, 
        [FromQuery] DateTime? dateFrom, 
        [FromQuery] DateTime? dateTo, 
        CancellationToken ct)
    {
        try
        {
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var stats = await _getIncidentStatsUseCase.ExecuteAsync(idCmpg, from, to, ct);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas de incidencias.", details = ex.Message });
        }
    }

    [HttpGet("api/reports/activations")]
    public async Task<IActionResult> GetActivationStats(
        [FromQuery] long idProvider, 
        [FromQuery] DateTime? dateFrom, 
        [FromQuery] DateTime? dateTo, 
        CancellationToken ct)
    {
        try
        {
            var from = dateFrom ?? DateTime.Today.AddDays(-30);
            var to = dateTo ?? DateTime.Today;

            var stats = await _getActivationStatsUseCase.ExecuteAsync(idProvider, from, to, ct);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas de activaciones.", details = ex.Message });
        }
    }
}
