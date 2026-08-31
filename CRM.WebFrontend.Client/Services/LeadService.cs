using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models.Leads;
using Microsoft.AspNetCore.Components.Authorization;

namespace CRM.WebFrontend.Client.Services;

public class RateLimitException : Exception
{
    public RateLimitException(string message) : base(message) { }
}

public class LeadService : ILeadService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public LeadService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _authStateProvider = authStateProvider;
    }

    private async Task AddAuthorizationHeaderAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var token = user.FindFirst("access_token")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IEnumerable<LeadResponse>> GetLeadsAsync(int page, int limit, string? searchTerm = null)
    {
        await AddAuthorizationHeaderAsync();

        var query = $"page={page}&limit={limit}";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        }

        var response = await _httpClient.GetAsync($"/api/leads?{query}");
        
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new RateLimitException("Demasiadas peticiones. Por favor, espere un momento antes de reintentar.");
        }
        
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<LeadResponse>>();
        return result ?? Array.Empty<LeadResponse>();
    }

    public async Task<bool> TakeCustodyAsync(long idLead)
    {
        await AddAuthorizationHeaderAsync();

        var request = new LeadUpdateStatusRequest
        {
            IdStatus = 2, // EN PROCESO
            Comment = "Tomado desde Bolsa de Trabajo"
        };

        var response = await _httpClient.PatchAsJsonAsync($"/api/leads/{idLead}/status", request);
        
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new RateLimitException("Demasiadas peticiones al intentar asignar. Por favor, espere un momento.");
        }

        return response.IsSuccessStatusCode;
    }
}
