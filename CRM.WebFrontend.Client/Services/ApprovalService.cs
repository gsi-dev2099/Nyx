using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Client.Services;

public class ApprovalService : IApprovalService
{
    private readonly HttpClient _httpClient;

    public ApprovalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ApprovalRequestDto>> GetPendingApprovalsAsync(long approverId, string approverRole)
    {
        // El ApiHub usa el usuario en sesión por JWT, pero en su firma no exige pasarlo por query, 
        // porque extrae el approverId del JWT. No obstante, por si cambian la firma, 
        // en este caso el ApiHub ya lo extrae y llama al ApprovalEngine.
        var response = await _httpClient.GetAsync("/api/approvals/pending");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ApprovalRequestDto>>();
            return result ?? new List<ApprovalRequestDto>();
        }
        return new List<ApprovalRequestDto>();
    }

    public async Task<bool> DecideRequestAsync(long requestId, ApprovalPatchDto dto)
    {
        var response = await _httpClient.PatchAsJsonAsync($"/api/approvals/{requestId}", dto);
        return response.IsSuccessStatusCode;
    }
}
