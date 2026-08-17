using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class AuditService : IAuditService
{
    private readonly HttpClient _httpClient;

    public AuditService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<AuditChecklistViewModel?> GetChecklistAsync(long idCmpg)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/audit/checklist/{idCmpg}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetChecklistAsync failed with status code: {response.StatusCode}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<AuditChecklistViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetChecklistAsync for campaign {idCmpg}: {ex.Message}");
            return null;
        }
    }

    public async Task<long> CreateAuditAsync(long idOrder, long idChecklist)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/orders/{idOrder}/audit", new { IdChecklist = idChecklist });
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create audit. API returned: {response.StatusCode} - {errorBody}");
            }

            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
            if (doc != null && doc.RootElement.TryGetProperty("idAudit", out var idProp))
            {
                return idProp.GetInt64();
            }
            throw new Exception("CreateAudit response did not contain 'idAudit' property.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateAuditAsync for order {idOrder}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SaveItemAsync(long idAudit, long idItem, string result, string? observation, string? audioTimestamp)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/audit/{idAudit}/items", new
            {
                IdItem = idItem,
                Result = result,
                Observation = observation,
                AudioTimestamp = audioTimestamp
            });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveItemAsync for audit {idAudit}, item {idItem}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CloseAuditAsync(long idAudit, string status)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/audit/{idAudit}/close", new { Status = status });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CloseAuditAsync for audit {idAudit}: {ex.Message}");
            return false;
        }
    }
}
