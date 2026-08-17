using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using CRM.ApiHub.Api.Filters;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CRM.ApiHub.Api.Controllers;

// DTOs auxiliares para tipado fuerte y correcta recepción de JSON
public record CallLogRequest(string CallLog);
public record AssignRequest(int ToUserId, string Context);
public record ConvertRequest(int UserId);
public record PreSaleCreateDto(
    long IdCmpg,
    string? Phone,
    string? Operator,
    string? FirstName,
    string? LastName,
    string? Address,
    string? Province,
    string? CoverageStatus,
    long IdStatus,
    long OwnerUserId,
    long CurrentUserId,
    string? Notes
);

[Authorize]
[ApiController]
[Route("api/presales")]
public class PreSaleController : ControllerBase
{
    private readonly IPreSaleRepository _repository;

    public PreSaleController(IPreSaleRepository repository)
    {
        _repository = repository;
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

            if (userRoles.Contains("SUPERVISOR") || userRoles.Contains("BACKOFFICE"))
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PreSaleCreateDto dto)
    {
        var preSale = new LeadPreSale
        {
            IdCmpg = dto.IdCmpg,
            Phone = dto.Phone,
            Operator = dto.Operator,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            Province = dto.Province,
            CoverageStatus = dto.CoverageStatus,
            IdStatus = dto.IdStatus,
            OwnerUserId = dto.OwnerUserId,
            CurrentUserId = dto.CurrentUserId,
            Notes = dto.Notes
        };

        var id = await _repository.CreateAsync(preSale);
        return CreatedAtAction(nameof(GetByUser), new { userId = preSale.CurrentUserId }, new { id });
    }

    [HttpPost("{id}/calls")]
    public async Task<IActionResult> AddCallLog(int id, [FromBody] CallLogRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (userId == -999) userId = 101;
        else if (userId == -1000) userId = 237;
        else if (userId == -998) userId = 9;

        var result = await _repository.AddCallLogAsync(id, request.CallLog, userId);
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

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> Convert(int id, [FromBody] ConvertRequest? request)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        long authenticatedUserId = 1; // fallback
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

        var leadId = await _repository.ConvertAsync(id, new { UserId = (int)authenticatedUserId });
        if (leadId <= 0) return BadRequest(new { message = "No se pudo convertir la pre-venta o ya fue convertida." });
        return Ok(new { message = "Pre-venta convertida a cliente con éxito.", leadId = leadId });
    }
}
