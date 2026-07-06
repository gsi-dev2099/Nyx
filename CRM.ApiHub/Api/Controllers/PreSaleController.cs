using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using CRM.ApiHub.Api.Filters;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("presales")]
public class PreSaleController : ControllerBase
{
    private readonly IPreSaleRepository _repository;

    public PreSaleController(IPreSaleRepository repository)
    {
        _repository = repository;
    }

    // GET /presales?userId=123
    [HttpGet]
    public async Task<IActionResult> GetByUser([FromQuery] int userId)
    {
        var preSales = await _repository.GetByUserAsync(userId);
        return Ok(preSales);
    }

    // POST /presales
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeadPreSale preSale)
    {
        var id = await _repository.CreateAsync(preSale);
        return CreatedAtAction(nameof(GetByUser), new { userId = preSale.CurrentUserId }, new { id });
    }

    // POST /presales/{id}/calls
    [HttpPost("{id}/calls")]
    public async Task<IActionResult> AddCallLog(int id, [FromBody] string callLog)
    {
        var result = await _repository.AddCallLogAsync(id, callLog);
        if (!result) return BadRequest(new { message = "No se pudo registrar el log de la llamada." });
        return Ok(new { message = "Log de llamada registrado con éxito." });
    }

    // POST /presales/{id}/assign?toUserId=456
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromQuery] int toUserId, [FromBody] string context)
    {
        var result = await _repository.AssignAsync(id, toUserId, context);
        if (!result) return BadRequest(new { message = "No se pudo reasignar la pre-venta." });
        return Ok(new { message = "Pre-venta reasignada con éxito." });
    }

    // POST /presales/{id}/convert
    [HttpPost("{id}/convert")]
    public async Task<IActionResult> Convert(int id, [FromBody] dynamic paramsData)
    {
        var result = await _repository.ConvertAsync(id, (object)paramsData);
        if (!result) return BadRequest(new { message = "No se pudo convertir la pre-venta." });
        return Ok(new { message = "Pre-venta convertida a cliente con éxito." });
    }
}