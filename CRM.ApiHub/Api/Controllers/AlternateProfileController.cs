using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRM.ApiHub.Domain.Repositories; 
using CRM.ApiHub.Domain.Entities;     

namespace CRM.ApiHub.Api.Controllers;

public class AlternateProfileRequestDto
{
    public string AlternateType { get; set; } = null!;
    public string AlternateData { get; set; } = null!; // Representing jsonb as string
    public string OriginalData { get; set; } = null!;  // Representing jsonb as string
    public string Reason { get; set; } = null!;
    public long CreatedBy { get; set; }
}

[ApiController]
[Route("api/orders")]
[Authorize]
public class AlternateProfileController : ControllerBase
{
    private readonly IAlternateProfileRepository _repository;

    public AlternateProfileController(IAlternateProfileRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("{id:long}/alternate-profile")]
    public async Task<IActionResult> Create(long id, [FromBody] AlternateProfileRequestDto dto)
    {
        if (dto == null) return BadRequest("Datos de perfil alterno inválidos.");

        var userClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        if (userClaim == null || !long.TryParse(userClaim.Value, out long createdBy))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (createdBy == -999) createdBy = 101;
        else if (createdBy == -1000) createdBy = 237;
        else if (createdBy == -998) createdBy = 9;

        var profile = new AlternateProfile 
        {
            IdOrder = id,
            AlternateType = dto.AlternateType,
            AlternateData = dto.AlternateData,
            OriginalData = dto.OriginalData,
            Reason = dto.Reason,
            CreatedBy = createdBy
        };

        var alternateId = await _repository.CreateAsync(profile);
        return CreatedAtAction(nameof(GetByOrderId), new { id = id }, new { message = "Ficha alterna creada.", id = alternateId });
    }

    [HttpGet("{id:long}/alternate-profile")]
    public async Task<IActionResult> GetByOrderId(long id)
    {
        var profile = await _repository.GetByOrderIdAsync(id);
        if (profile == null)
        {
            return NotFound(new { message = "Ficha alterna no encontrada para esta orden." });
        }
        return Ok(profile);
    }
}