using System.Security.Claims;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferencesRepository _preferencesRepository;

    public UserPreferencesController(IUserPreferencesRepository preferencesRepository)
    {
        _preferencesRepository = preferencesRepository;
    }

    /// <summary>
    /// Get the current user's theme preference.
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var themeName = await _preferencesRepository.GetThemeAsync(userId.Value);
        return Ok(new { themeName = themeName ?? "theme-default" });
    }

    /// <summary>
    /// Save the current user's theme preference.
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] SavePreferencesRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Validate theme name
        var validThemes = new[] { "theme-default", "theme-protanopia", "theme-tritanopia", "theme-high-contrast" };
        var themeName = request.ThemeName ?? "theme-default";
        if (!System.Array.Exists(validThemes, t => t == themeName))
        {
            return BadRequest(new { message = "Tema no válido." });
        }

        await _preferencesRepository.SaveThemeAsync(userId.Value, themeName);
        return Ok(new { themeName, message = "Preferencia guardada correctamente." });
    }

    private long? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim != null && long.TryParse(claim.Value, out long id))
            return id;
        return null;
    }
}

public class SavePreferencesRequest
{
    public string? ThemeName { get; set; }
}
