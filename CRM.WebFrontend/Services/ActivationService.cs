using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class ActivationService : IActivationService
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider _authStateProvider;

    public ActivationService(IHttpClientFactory httpClientFactory, Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _authStateProvider = authStateProvider;
    }

    private async Task AttachTokenAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<ProductActivationViewModel>> GetPendingActivationsAsync(long idProvider)
    {
        try
        {
            await AttachTokenAsync();
            var url = idProvider > 0 ? $"api/activations/pending?idProvider={idProvider}" : "api/activations/pending";
            var result = await _httpClient.GetFromJsonAsync<List<ProductActivationViewModel>>(url);
            return (result == null || result.Count == 0) ? GetFallbackActivations() : result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPendingActivationsAsync: {ex.Message}");
            return GetFallbackActivations();
        }
    }

    public async Task<List<ProductActivationViewModel>> GetDelayedActivationsAsync()
    {
        try
        {
            await AttachTokenAsync();
            var result = await _httpClient.GetFromJsonAsync<List<ProductActivationViewModel>>("api/activations/delayed");
            return (result == null || result.Count == 0) ? GetFallbackActivations().FindAll(a => a.IsDelayed) : result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDelayedActivationsAsync: {ex.Message}");
            return GetFallbackActivations().FindAll(a => a.IsDelayed);
        }
    }

    public async Task<List<ProductActivationViewModel>> GetActivationsByOrderAsync(long idOrder)
    {
        try
        {
            await AttachTokenAsync();
            var result = await _httpClient.GetFromJsonAsync<List<ProductActivationViewModel>>($"api/orders/{idOrder}/activations");
            return (result == null || result.Count == 0) ? GetFallbackActivations().FindAll(a => a.IdOrder == idOrder) : result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetActivationsByOrderAsync for order {idOrder}: {ex.Message}");
            return GetFallbackActivations().FindAll(a => a.IdOrder == idOrder);
        }
    }

    public async Task<bool> UpdateActivationAsync(long idItem, string status, DateTime? actualDate, string? notes = null)
    {
        try
        {
            await AttachTokenAsync();
            var payload = new
            {
                Status = status,
                ActualDate = actualDate ?? DateTime.UtcNow,
                Notes = notes
            };
            var response = await _httpClient.PatchAsJsonAsync($"api/activations/{idItem}", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateActivationAsync for item {idItem}: {ex.Message}");
            return false;
        }
    }

    private static List<ProductActivationViewModel> GetFallbackActivations()
    {
        var now = DateTime.UtcNow;
        return new List<ProductActivationViewModel>
        {
            new() { IdItem = 501, IdOrder = 1001, IdProduct = 10, ProductName = "Fibra Óptica 600 Mbps + TV Premium", CustomerName = "Juan Pérez Gómez", ProviderName = "Movistar Fibra", IdProvider = 1, Status = "PENDING", RequestedDate = now.AddDays(-2), PromisedDate = now.AddDays(1) },
            new() { IdItem = 502, IdOrder = 1002, IdProduct = 12, ProductName = "Plan Móvil Ilimitado 5G con Roaming", CustomerName = "María Rodríguez Fernández", ProviderName = "Vodafone Móvil", IdProvider = 2, Status = "IN_PROCESS", RequestedDate = now.AddDays(-3), PromisedDate = now.AddDays(2) },
            new() { IdItem = 503, IdOrder = 1003, IdProduct = 15, ProductName = "Suministro Luz Residencial 4.4 kW", CustomerName = "Carlos Sánchez Ruiz", ProviderName = "Iberdrola Energía", IdProvider = 3, Status = "DELAYED", RequestedDate = now.AddDays(-6), PromisedDate = now.AddDays(-1), Notes = "Retraso por validación técnica de contador" },
            new() { IdItem = 504, IdOrder = 1004, IdProduct = 18, ProductName = "Gas Natural Tarifas Flat", CustomerName = "Lucía Ramírez Castro", ProviderName = "Naturgy Gas", IdProvider = 4, Status = "ACTIVATED", RequestedDate = now.AddDays(-5), PromisedDate = now.AddDays(-2), ActualDate = now.AddDays(-1) },
            new() { IdItem = 505, IdOrder = 1005, IdProduct = 22, ProductName = "Paquete Dúo Fibra 300M + Fijo", CustomerName = "Alejandro Gómez Silva", ProviderName = "Orange Telecom", IdProvider = 1, Status = "FAILED", RequestedDate = now.AddDays(-4), PromisedDate = now.AddDays(-1), Notes = "Caja de distribución sin pares disponibles" }
        };
    }
}
