using CRM.WebFrontend.Client.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace CRM.WebFrontend.Client.Services;

public class ActivationService : IActivationService
{
    private readonly HttpClient _httpClient;

    public ActivationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<List<ProviderDto>> GetProvidersAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<ProviderDto>>("api/providers");
            return response ?? new List<ProviderDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ActivationService] Error fetching providers: {ex.Message}");
            return new List<ProviderDto>();
        }
    }

    public async Task<List<ProductActivationTrackingDto>> GetPendingActivationsAsync(long idProvider)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<ProductActivationTrackingDto>>($"api/activations/pending?idProvider={idProvider}");
            return response ?? new List<ProductActivationTrackingDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ActivationService] Error fetching pending activations: {ex.Message}");
            return new List<ProductActivationTrackingDto>();
        }
    }

    public async Task<bool> UpdateActivationAsync(long idItem, UpdateActivationRequestDto dto)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/activations/{idItem}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ActivationService] Error updating activation: {ex.Message}");
            return false;
        }
    }
}
