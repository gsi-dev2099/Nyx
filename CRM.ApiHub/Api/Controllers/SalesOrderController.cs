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
        if (!userId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId))
            {
                userId = parsedId;
            }
        }

        if (!userId.HasValue)
        {
            return Unauthorized(new { message = "El ID de usuario es requerido para realizar esta consulta." });
        }

        var pagedResult = await _getSalesOrdersUseCase.ExecuteAsync(userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
        return Ok(pagedResult);
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
        var history = await _getSalesOrderHistoryUseCase.ExecuteAsync(id, ct);
        return Ok(history);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] SalesOrderCreateDto dto, CancellationToken ct)
    {
        var createdOrder = await _createSalesOrderUseCase.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.IdOrder }, createdOrder);
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] SalesOrderUpdateStatusDto dto, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long actorId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        var success = await _updateSalesOrderStatusUseCase.ExecuteAsync(id, dto, actorId, ct);
        if (!success)
        {
            return NotFound(new { message = "Orden de venta no encontrada." });
        }
        return Ok(new { message = "Estado de orden de venta actualizado correctamente." });
    }
}
