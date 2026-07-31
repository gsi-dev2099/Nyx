using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface INotificationRepository
{
    Task CreateAsync(long userId, string title, string message, string? module, string? actionData);
    Task<IEnumerable<UserAlert>> GetRecentAsync(long userId, int limit = 50);
    Task MarkReadAsync(int idAlert, long userId);
    Task MarkAllReadAsync(long userId); 
}