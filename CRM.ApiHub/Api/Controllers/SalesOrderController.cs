using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.SalesOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class SalesOrderController : ControllerBase
{
    private readonly GetSalesOrdersUseCase _getSalesOrdersUseCase;
    private readonly GetSalesOrderByIdUseCase _getSalesOrderByIdUseCase;
    private readonly CreateSalesOrderUseCase _createSalesOrderUseCase;
    private readonly UpdateSalesOrderStatusUseCase _updateSalesOrderStatusUseCase;
    private readonly GetSalesOrderHistoryUseCase _getSalesOrderHistoryUseCase;

    public SalesOrderController(
        GetSalesOrdersUseCase getSalesOrdersUseCase,
        GetSalesOrderByIdUseCase getSalesOrderByIdUseCase,
        CreateSalesOrderUseCase createSalesOrderUseCase,
        UpdateSalesOrderStatusUseCase updateSalesOrderStatusUseCase,
        GetSalesOrderHistoryUseCase getSalesOrderHistoryUseCase)
    {
        _getSalesOrdersUseCase = getSalesOrdersUseCase;
        _getSalesOrderByIdUseCase = getSalesOrderByIdUseCase;
        _createSalesOrderUseCase = createSalesOrderUseCase;
        _updateSalesOrderStatusUseCase = updateSalesOrderStatusUseCase;
        _getSalesOrderHistoryUseCase = getSalesOrderHistoryUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] long? userId,
        [FromQuery] long? statusId,
        [FromQuery] long? campaignId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        try
        {
            if (!userId.HasValue)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId))
                {
                    userId = parsedId;
                }
            }

            if (userId == -999)
            {
                userId = 101; // Fallback: Map test.asesor to real asesor 'patricia' (ID 101)
            }
            else if (userId == -1000)
            {
                userId = 237; // Fallback: Map test.backoffice to real backoffice 'gvillanueva' (ID 237)
            }
            else if (userId == -998)
            {
                userId = 9; // Fallback: Map test.supervisor to real supervisor 'cnaranjo' (ID 9)
            }

            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "El ID de usuario es requerido para realizar esta consulta." });
            }

            var pagedResult = await _getSalesOrdersUseCase.ExecuteAsync(userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener órdenes.", details = ex.Message });
        }
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOrderById(long id, CancellationToken ct)
    {
        var order = await _getSalesOrderByIdUseCase.ExecuteAsync(id, ct);
        if (order == null)
        {
            return NotFound(new { message = "Orden de venta no encontrada." });
        }
        return Ok(order);
    }

    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> GetOrderHistory(long id, CancellationToken ct)
    {
        try
        {
            var history = await _getSalesOrderHistoryUseCase.ExecuteAsync(id, ct);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el historial de la orden.", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] SalesOrderCreateDto dto, CancellationToken ct)
    {
        try
        {
            var createdOrder = await _createSalesOrderUseCase.ExecuteAsync(dto, ct);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.IdOrder }, createdOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al crear orden de venta.", details = ex.Message });
        }
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] SalesOrderUpdateStatusDto dto, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long actorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (actorId == -999) actorId = 101;
        else if (actorId == -1000) actorId = 237;
        else if (actorId == -998) actorId = 9;

        try
        {
            var success = await _updateSalesOrderStatusUseCase.ExecuteAsync(id, dto, actorId, ct);
            if (!success)
            {
                return NotFound(new { message = "Orden de venta no encontrada." });
            }
            return Ok(new { message = "Estado de orden de venta actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar estado de la orden.", details = ex.Message });
        }
    }
}
