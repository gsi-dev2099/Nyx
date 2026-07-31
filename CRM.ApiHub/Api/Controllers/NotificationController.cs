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
    public async Task<IActionResult> GetRecent([FromQuery] long userId, [FromQuery] int limit = 50)
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var authenticatedUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        if (authenticatedUserId != userId)
        {
            return StatusCode(403, new { message = "No tienes permiso para consultar las notificaciones de otro usuario." });
        }

        var notifications = await _notificationRepository.GetRecentAsync(userId, limit);
        return Ok(notifications);
    }

    [HttpPatch("{id}/read")] 
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var authenticatedUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        await _notificationRepository.MarkReadAsync(id, authenticatedUserId);
        return Ok(new { message = "Notificación marcada como leída." });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] long userId)
    {
        var authenticatedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (authenticatedUserIdClaim == null || !long.TryParse(authenticatedUserIdClaim.Value, out var authenticatedUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        if (authenticatedUserId != userId)
        {
            return StatusCode(403, new { message = "No tienes permiso para marcar las notificaciones de otro usuario." });
        }

        await _notificationRepository.MarkAllReadAsync(userId);
        return Ok(new { message = "Todas las notificaciones marcadas como leídas." });
    }
}