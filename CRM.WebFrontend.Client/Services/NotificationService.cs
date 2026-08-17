using System.Net.Http.Json;
using System.Net.Http.Headers;
using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

public class NotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateAuthenticatedClient(string? token)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<List<NotificationDto>> GetRecentAsync(long userId, string? token = null)
    {
        try
        {
            var client = CreateAuthenticatedClient(token);
            var result = await client.GetFromJsonAsync<List<NotificationDto>>($"/api/notifications?userId={userId}&limit=50");
            return result ?? new List<NotificationDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error fetching notifications: {ex.Message}");
            return new List<NotificationDto>();
        }
    }

    public async Task MarkAsReadAsync(int idAlert, string? token = null)
    {
        try
        {
            var client = CreateAuthenticatedClient(token);
            await client.PatchAsync($"/api/notifications/{idAlert}/read", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error marking as read: {ex.Message}");
        }
    }

    public async Task MarkAllAsReadAsync(long userId, string? token = null)
    {
        try
        {
            var client = CreateAuthenticatedClient(token);
            await client.PostAsync($"/api/notifications/read-all?userId={userId}", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error marking all as read: {ex.Message}");
        }
    }
}
