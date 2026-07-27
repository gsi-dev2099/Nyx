using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class PreSaleService : IPreSaleService
{
    private readonly HttpClient _httpClient;

    public PreSaleService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<IEnumerable<PreSaleViewModel>> GetByUserAsync(long? userId = null)
    {
        try
        {
            var url = userId.HasValue ? $"api/presales?userId={userId}" : "api/presales";
            var result = await _httpClient.GetFromJsonAsync<List<PreSaleViewModel>>(url);
            return result ?? new List<PreSaleViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByUserAsync: {ex.Message}");
            return new List<PreSaleViewModel>();
        }
    }

    public async Task<bool> AddCallLogAsync(long id, string callLog)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/presales/{id}/calls", new { CallLog = callLog });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddCallLogAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ConvertAsync(long id)
    {
        try
        {
            // Empty object or user id, based on backend expect ConvertRequest(int UserId) or nothing if using current user.
            // Backend takes ConvertRequest? so we can send an empty object.
            var response = await _httpClient.PostAsJsonAsync($"api/presales/{id}/convert", new { });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConvertAsync: {ex.Message}");
            return false;
        }
    }
}
