namespace CRM.ApiHub.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(long userId, string title, string message, string? module = null, string? actionData = null);
}