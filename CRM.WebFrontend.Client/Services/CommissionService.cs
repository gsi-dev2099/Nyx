using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace CRM.WebFrontend.Client.Services;

public class CommissionService : ICommissionService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public CommissionService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _authStateProvider = authStateProvider;
    }

    private async Task EnsureTokenAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<CurrencyDto>> GetCurrenciesAsync()
    {
        await EnsureTokenAsync();
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<CurrencyDto>>("/api/currencies");
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommissionService] Error GetCurrenciesAsync: {ex.Message}");
            return new();
        }
    }

    public async Task<ConvertAmountResponseDto?> ConvertAmountAsync(string from, string to, decimal amount)
    {
        await EnsureTokenAsync();
        try
        {
            var url = $"/api/currencies/convert?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}&amount={amount}";
            return await _httpClient.GetFromJsonAsync<ConvertAmountResponseDto>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommissionService] Error ConvertAmountAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<SettlementResponseDto>> GetSettlementsAsync(long? userId = null)
    {
        await EnsureTokenAsync();
        try
        {
            var url = userId.HasValue
                ? $"/api/commissions/settlements?userId={userId}"
                : "/api/commissions/settlements";
            var result = await _httpClient.GetFromJsonAsync<List<SettlementResponseDto>>(url);
            return result ?? new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommissionService] Error GetSettlementsAsync: {ex.Message}");
            return new();
        }
    }

    public async Task<SettlementDetailResponseDto?> GetSettlementDetailAsync(long idSettlement)
    {
        await EnsureTokenAsync();
        try
        {
            return await _httpClient.GetFromJsonAsync<SettlementDetailResponseDto>($"/api/commissions/settlements/{idSettlement}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommissionService] Error GetSettlementDetailAsync: {ex.Message}");
            return null;
        }
    }
}
