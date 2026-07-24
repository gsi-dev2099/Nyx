using CRM.WebFrontend.Client.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace CRM.WebFrontend.Client.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public MaintenanceService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _authStateProvider = authStateProvider;
    }

    private async Task SetAuthHeaderAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ===== STATUSES =====
    public async Task<List<OrderStatusMaintenanceDto>> GetAllStatusesAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<OrderStatusMaintenanceDto>>("api/maintenance/statuses");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error fetching statuses: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> ToggleStatusAsync(int id, bool isActive)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PatchAsJsonAsync($"api/maintenance/statuses/{id}/toggle", new { isActive });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error toggling status: {ex.Message}");
            return false;
        }
    }

    // ===== PRODUCTS =====
    public async Task<List<ProductMaintenanceDto>> GetAllProductsAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<ProductMaintenanceDto>>("api/maintenance/products");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error fetching products: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> ToggleProductAsync(int id, bool isActive)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PatchAsJsonAsync($"api/maintenance/products/{id}/toggle", new { isActive });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error toggling product: {ex.Message}");
            return false;
        }
    }

    // ===== INCIDENT CATALOG =====
    public async Task<List<IncidentCatalogMaintenanceDto>> GetAllIncidentCatalogAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<IncidentCatalogMaintenanceDto>>("api/maintenance/incidents");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error fetching incident catalog: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> CreateIncidentCatalogAsync(CreateIncidentCatalogDto dto)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/maintenance/incidents", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error creating incident: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateIncidentCatalogAsync(long id, UpdateIncidentCatalogDto dto)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/maintenance/incidents/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error updating incident: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteIncidentCatalogAsync(long id)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/maintenance/incidents/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error deleting incident: {ex.Message}");
            return false;
        }
    }

    // ===== EXCHANGE RATES =====
    public async Task<List<ExchangeRateMaintenanceDto>> GetExchangeRatesAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<ExchangeRateMaintenanceDto>>("api/maintenance/exchange-rates");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error fetching exchange rates: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> CreateExchangeRateAsync(CreateExchangeRateDto dto)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/maintenance/exchange-rates", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error creating exchange rate: {ex.Message}");
            return false;
        }
    }

    // ===== CAMPAIGNS (aux) =====
    public async Task<List<CampaignSimpleDto>> GetCampaignsAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var result = await _httpClient.GetFromJsonAsync<List<CampaignSimpleDto>>("api/maintenance/campaigns");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MaintenanceService] Error fetching campaigns: {ex.Message}");
            return new();
        }
    }
}
