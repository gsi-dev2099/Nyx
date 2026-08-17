using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize(Roles = "BACKOFFICE,SUPERVISOR")]
[ApiController]
[Route("api/backoffice")]
public class BackofficeController : ControllerBase
{
    private readonly GetAssignedOrdersUseCase _getAssignedOrdersUseCase;
    private readonly GetPendingVerificationUseCase _getPendingVerificationUseCase;
    private readonly UpdateBackofficeOrderStatusUseCase _updateBackofficeOrderStatusUseCase;
    private readonly VerifyBackofficeDocumentUseCase _verifyBackofficeDocumentUseCase;

    public BackofficeController(
        GetAssignedOrdersUseCase getAssignedOrdersUseCase,
        GetPendingVerificationUseCase getPendingVerificationUseCase,
        UpdateBackofficeOrderStatusUseCase updateBackofficeOrderStatusUseCase,
        VerifyBackofficeDocumentUseCase verifyBackofficeDocumentUseCase)
    {
        _getAssignedOrdersUseCase = getAssignedOrdersUseCase;
        _getPendingVerificationUseCase = getPendingVerificationUseCase;
        _updateBackofficeOrderStatusUseCase = updateBackofficeOrderStatusUseCase;
        _verifyBackofficeDocumentUseCase = verifyBackofficeDocumentUseCase;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAssignedOrders(
        [FromQuery] long? userId,
        [FromQuery] long? statusId,
        [FromQuery] long? campaignId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var backofficeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (backofficeIdClaim == null || !long.TryParse(backofficeIdClaim.Value, out long backofficeId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (backofficeId == -1000) backofficeId = 237;
        else if (backofficeId == -999) backofficeId = 101;
        else if (backofficeId == -998) backofficeId = 9;

        try
        {
            var pagedResult = await _getAssignedOrdersUseCase.ExecuteAsync(backofficeId, userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las órdenes asignadas.", details = ex.Message });
        }
    }

    [HttpGet("pending-docs")]
    public async Task<IActionResult> GetPendingDocs(CancellationToken ct)
    {
        var backofficeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (backofficeIdClaim == null || !long.TryParse(backofficeIdClaim.Value, out long backofficeId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (backofficeId == -1000) backofficeId = 237;
        else if (backofficeId == -999) backofficeId = 101;
        else if (backofficeId == -998) backofficeId = 9;

        try
        {
            var docs = await _getPendingVerificationUseCase.ExecuteAsync(backofficeId, ct);
            return Ok(docs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los documentos pendientes de verificación.", details = ex.Message });
        }
    }

    [Route("orders/{id}/status")]
    [HttpPatch]
    public async Task<IActionResult> UpdateStatus([FromRoute] long id, [FromBody] UpdateOrderStatusRequestDto dto, CancellationToken ct)
    {
        if (dto == null || dto.ToStatusId <= 0)
        {
            return BadRequest(new { message = "Se debe proporcionar un estado de destino válido." });
        }

        var backofficeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (backofficeIdClaim == null || !long.TryParse(backofficeIdClaim.Value, out long backofficeId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (backofficeId == -1000) backofficeId = 237;
        else if (backofficeId == -999) backofficeId = 101;
        else if (backofficeId == -998) backofficeId = 9;

        try
        {
            var success = await _updateBackofficeOrderStatusUseCase.ExecuteAsync(
                id,
                dto.ToStatusId,
                dto.ToSubstatusId,
                dto.Comment,
                backofficeId,
                ct
            );

            if (!success)
            {
                return NotFound(new { message = $"No se encontró la orden con ID {id} o no se pudo actualizar su estado." });
            }

            return Ok(new { message = "Estado de orden actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el estado de la orden.", details = ex.Message });
        }
    }

    [Route("documents/{id}/verify")]
    [HttpPatch]
    public async Task<IActionResult> VerifyDocument([FromRoute] long id, [FromBody] DocumentVerifyRequestDto dto, CancellationToken ct)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "Se debe proporcionar un estado de verificación válido (PENDING, VALID, INVALID, MISMATCH)." });
        }

        var backofficeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (backofficeIdClaim == null || !long.TryParse(backofficeIdClaim.Value, out long backofficeId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (backofficeId == -1000) backofficeId = 237;
        else if (backofficeId == -999) backofficeId = 101;
        else if (backofficeId == -998) backofficeId = 9;

        try
        {
            var success = await _verifyBackofficeDocumentUseCase.ExecuteAsync(
                id,
                dto.Status,
                dto.Notes,
                backofficeId,
                ct
            );

            if (!success)
            {
                return NotFound(new { message = $"No se encontró el documento con ID {id} o no se pudo actualizar su verificación." });
            }

            return Ok(new { message = "Verificación de documento actualizada correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la verificación del documento.", details = ex.Message });
        }
    }
}
