using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using CRM.ApiHub.Api.Hubs;

namespace CRM.ApiHub.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(INotificationRepository notificationRepository, IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(long userId, string title, string message, string? module = null, string? actionData = null)
    {
        await _notificationRepository.CreateAsync(userId, title, message, module, actionData);
        await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", title, message);
    }
}