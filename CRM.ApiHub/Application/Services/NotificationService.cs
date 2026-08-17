using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using CRM.ApiHub.Api.Hubs;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository, 
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationAsync(long userId, string title, string message, string? module = null, string? actionData = null)
    {
        await _notificationRepository.CreateAsync(userId, title, message, module, actionData);
        try
        {
            await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", title, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo entregar la notificación en tiempo real vía SignalR para el usuario {UserId}.", userId);
        }
    }
}