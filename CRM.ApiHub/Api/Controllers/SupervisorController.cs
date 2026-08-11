using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Application.UseCases.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize(Roles = "SUPERVISOR,COORDINADOR")]
[ApiController]
[Route("api/supervisor")]

public class SupervisorController : ControllerBase
{
    private readonly GetTeamOrdersUseCase _getTeamOrdersUseCase;
    private readonly GetTeamStatsUseCase _getTeamStatsUseCase;
    private readonly BulkTransferToBackofficeUseCase _bulkTransferToBackofficeUseCase;
    private readonly INotificationService _notificationService;

    public SupervisorController(
        GetTeamOrdersUseCase getTeamOrdersUseCase,
        GetTeamStatsUseCase getTeamStatsUseCase,
        BulkTransferToBackofficeUseCase bulkTransferToBackofficeUseCase,
        INotificationService notificationService)
    {
        _getTeamOrdersUseCase = getTeamOrdersUseCase;
        _getTeamStatsUseCase = getTeamStatsUseCase;
        _bulkTransferToBackofficeUseCase = bulkTransferToBackofficeUseCase;
        _notificationService = notificationService;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetTeamOrders(
        [FromQuery] long? userId,
        [FromQuery] long? statusId,
        [FromQuery] long? campaignId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (supervisorIdClaim == null || !long.TryParse(supervisorIdClaim.Value, out long supervisorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (supervisorId == -998) supervisorId = 9;
        else if (supervisorId == -999) supervisorId = 101;
        else if (supervisorId == -1000) supervisorId = 237;

        try
        {
            var pagedResult = await _getTeamOrdersUseCase.ExecuteAsync(supervisorId, userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener órdenes del equipo.", details = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetTeamStats(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (supervisorIdClaim == null || !long.TryParse(supervisorIdClaim.Value, out long supervisorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (supervisorId == -998) supervisorId = 9;
        else if (supervisorId == -999) supervisorId = 101;
        else if (supervisorId == -1000) supervisorId = 237;

        try
        {
            var stats = await _getTeamStatsUseCase.ExecuteAsync(supervisorId, dateFrom, dateTo, ct);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas del equipo.", details = ex.Message });
        }
    }

    [HttpPost("bulk-transfer")]
    public async Task<IActionResult> BulkTransfer([FromBody] BulkTransferRequestDto dto, CancellationToken ct)
    {
        if (dto == null || dto.OrderIds == null || dto.OrderIds.Length == 0)
        {
            return BadRequest(new { message = "Se deben proporcionar los IDs de órdenes para transferir." });
        }

        if (dto.BackofficeUserId <= 0)
        {
            return BadRequest(new { message = "Se debe proporcionar un ID de usuario de Backoffice de destino válido." });
        }

        var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (supervisorIdClaim == null || !long.TryParse(supervisorIdClaim.Value, out long supervisorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (supervisorId == -998) supervisorId = 9;
        else if (supervisorId == -999) supervisorId = 101;
        else if (supervisorId == -1000) supervisorId = 237;

        try
        {
            var result = await _bulkTransferToBackofficeUseCase.ExecuteAsync(
                dto.OrderIds,
                supervisorId,
                dto.BackofficeUserId,
                dto.Comment,
                ct
            );

            if (result.SuccessfulCount == 0 && result.FailedCount > 0)
            {
                return BadRequest(new { message = "No se pudo realizar la transferencia masiva de ninguna orden.", details = result });
            }

            // Send notification to the Backoffice analyst
            if (result.SuccessfulCount > 0)
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        dto.BackofficeUserId,
                        $"Nuevas órdenes asignadas ({result.SuccessfulCount})",
                        $"Se te han asignado {result.SuccessfulCount} órdenes para revisión desde el Supervisor.",
                        "TRANSFER",
                        null
                    );
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[SupervisorController] Error sending notification: {notifEx.Message}");
                }
            }

            return Ok(new { message = "Transferencia masiva procesada.", details = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al realizar la transferencia masiva.", details = ex.Message });
        }
    }
}
