using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 50)
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        if (userId == -999) userId = 101;
        else if (userId == -998) userId = 9;
        else if (userId == -1000) userId = 237;

        var notifications = await _notificationRepository.GetRecentAsync(userId, limit);
        return Ok(notifications);
    }

    [HttpPatch("{id}/read")] 
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var authenticatedUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identified." });
        }

        if (authenticatedUserId == -999) authenticatedUserId = 101;
        else if (authenticatedUserId == -998) authenticatedUserId = 9;
        else if (authenticatedUserId == -1000) authenticatedUserId = 237;

        await _notificationRepository.MarkReadAsync(id, authenticatedUserId);
        return Ok(new { message = "Notificación marcada como leída." });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        if (userId == -999) userId = 101;
        else if (userId == -998) userId = 9;
        else if (userId == -1000) userId = 237;

        await _notificationRepository.MarkAllReadAsync(userId);
        return Ok(new { message = "Todas las notificaciones marcadas como leídas." });
    }
}