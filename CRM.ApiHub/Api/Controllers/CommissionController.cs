using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Commissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class CommissionController : ControllerBase
{
    private readonly GetCurrenciesUseCase _getCurrenciesUseCase;
    private readonly ConvertAmountUseCase _convertAmountUseCase;
    private readonly GetSettlementsUseCase _getSettlementsUseCase;
    private readonly CreateSettlementUseCase _createSettlementUseCase;
    private readonly AddSettlementItemsUseCase _addSettlementItemsUseCase;
    private readonly UpdateSettlementStatusUseCase _updateSettlementStatusUseCase;
    private readonly DeleteSettlementUseCase _deleteSettlementUseCase;

    public CommissionController(
        GetCurrenciesUseCase getCurrenciesUseCase,
        ConvertAmountUseCase convertAmountUseCase,
        GetSettlementsUseCase getSettlementsUseCase,
        CreateSettlementUseCase createSettlementUseCase,
        AddSettlementItemsUseCase addSettlementItemsUseCase,
        UpdateSettlementStatusUseCase updateSettlementStatusUseCase,
        DeleteSettlementUseCase deleteSettlementUseCase)
    {
        _getCurrenciesUseCase = getCurrenciesUseCase;
        _convertAmountUseCase = convertAmountUseCase;
        _getSettlementsUseCase = getSettlementsUseCase;
        _createSettlementUseCase = createSettlementUseCase;
        _addSettlementItemsUseCase = addSettlementItemsUseCase;
        _updateSettlementStatusUseCase = updateSettlementStatusUseCase;
        _deleteSettlementUseCase = deleteSettlementUseCase;
    }

    [HttpGet("api/currencies")]
    public async Task<IActionResult> GetCurrencies(CancellationToken ct)
    {
        try
        {
            var currencies = await _getCurrenciesUseCase.ExecuteAsync(ct);
            return Ok(currencies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las divisas.", details = ex.Message });
        }
    }

    [HttpGet("api/currencies/convert")]
    public async Task<IActionResult> ConvertAmount(
        [FromQuery] string from,
        [FromQuery] string to,
        [FromQuery] decimal amount,
        [FromQuery] DateTime? date,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                return BadRequest(new { message = "Las divisas origen y destino son requeridas." });
            }

            var request = new ConvertAmountRequestDto
            {
                From = from,
                To = to,
                Amount = amount,
                Date = date
            };

            var response = await _convertAmountUseCase.ExecuteAsync(request, ct);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al realizar la conversión de divisa.", details = ex.Message });
        }
    }

    [HttpGet("api/commissions/settlements")]
    public async Task<IActionResult> GetSettlements([FromQuery] long? userId, CancellationToken ct)
    {
        try
        {
            var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out long authenticatedUserId))
            {
                return Unauthorized(new { message = "Usuario no autorizado." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            if (userRole == "ASESOR")
            {
                if (userId.HasValue && userId.Value != authenticatedUserId)
                {
                    return StatusCode(403, new { message = "No tienes permisos para consultar las liquidaciones de otro usuario." });
                }
                userId = authenticatedUserId;
            }

            var list = await _getSettlementsUseCase.GetListAsync(userId, ct);
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el listado de liquidaciones.", details = ex.Message });
        }
    }

    [HttpGet("api/commissions/settlements/{id:long}")]
    public async Task<IActionResult> GetSettlementById(long id, CancellationToken ct)
    {
        try
        {
            var result = await _getSettlementsUseCase.GetByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(new { message = $"No se encontró la liquidación de comisión con ID {id}." });
            }

            var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out long authenticatedUserId))
            {
                return Unauthorized(new { message = "Usuario no autorizado." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            if (userRole == "ASESOR" && result.Value.Settlement.IdUser != authenticatedUserId)
            {
                return StatusCode(403, new { message = "No tienes permisos para consultar esta liquidación de comisión." });
            }

            return Ok(new
            {
                settlement = result.Value.Settlement,
                items = result.Value.Items
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el detalle de la liquidación.", details = ex.Message });
        }
    }

    [HttpPost("api/commissions/settlements")]
    public async Task<IActionResult> CreateSettlement([FromBody] CreateSettlementRequestDto dto, CancellationToken ct)
    {
        try
        {
            var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out long authenticatedUserId))
            {
                return Unauthorized(new { message = "Usuario no autorizado." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            if (userRole == "ASESOR" && dto.UserId != authenticatedUserId)
            {
                return StatusCode(403, new { message = "No tienes permisos para crear liquidaciones para otro usuario." });
            }

            var id = await _createSettlementUseCase.ExecuteAsync(dto, ct);
            return CreatedAtAction(nameof(GetSettlementById), new { id = id }, new { id_settlement = id, message = "Liquidación creada en borrador (DRAFT) correctamente." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la liquidación de comisión.", details = ex.Message });
        }
    }

    [HttpPost("api/commissions/settlements/{id:long}/items")]
    public async Task<IActionResult> AddSettlementItems(long id, [FromBody] AddSettlementItemsRequestDto dto, CancellationToken ct)
    {
        try
        {
            var success = await _addSettlementItemsUseCase.ExecuteAsync(id, dto, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudieron agregar los ítems de comisión a la liquidación." });
            }

            return Ok(new { message = "Ítems de comisión agregados y totales recalculados con éxito." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al agregar ítems de comisión a la liquidación.", details = ex.Message });
        }
    }

    [HttpPut("api/commissions/settlements/{id:long}")]
    public async Task<IActionResult> UpdateSettlement(long id, [FromBody] UpdateSettlementRequestDto dto, CancellationToken ct)
    {
        try
        {
            var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            long? actorUserId = null;
            if (actorIdClaim != null && long.TryParse(actorIdClaim.Value, out long parsedId))
            {
                actorUserId = parsedId;
            }

            var success = await _updateSettlementStatusUseCase.ExecuteAsync(id, dto, actorUserId, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudo actualizar el estado de la liquidación de comisión." });
            }

            return Ok(new { message = $"Liquidación actualizada a estado {dto.Status.ToUpper()} correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la liquidación de comisión.", details = ex.Message });
        }
    }

    [HttpDelete("api/commissions/settlements/{id:long}")]
    public async Task<IActionResult> DeleteSettlement(long id, CancellationToken ct)
    {
        try
        {
            var success = await _deleteSettlementUseCase.ExecuteAsync(id, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudo eliminar la liquidación de comisión. Verifique que exista y esté en estado 'DRAFT'." });
            }

            return Ok(new { message = "Liquidación de comisión y sus ítems eliminados correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar la liquidación de comisión.", details = ex.Message });
        }
    }
}
