using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Services;

public interface IApprovalEngineClient
{
    Task<ApprovalRequestResponseDto?> SubmitRequestAsync(string policyCode, string entityType, long entityId, long requestedBy, string? entityContextJson = null, string? callbackUrl = null);
    Task<ApprovalRequestResponseDto?> DecideRequestAsync(long requestId, long decidedBy, string decision, string? reason = null, string? evidencePath = null);
    Task<IEnumerable<ApprovalRequestResponseDto>> GetPendingApprovalsAsync(long approverId, string approverRole);
    Task<ApprovalDelegationResponseDto?> CreateDelegationAsync(long delegatorId, long delegateId, long? policyId, string reason, DateTime validFrom, DateTime validUntil);
}

public class ApprovalEngineClient : IApprovalEngineClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApprovalEngineClient> _logger;

    public ApprovalEngineClient(HttpClient httpClient, ILogger<ApprovalEngineClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApprovalRequestResponseDto?> SubmitRequestAsync(string policyCode, string entityType, long entityId, long requestedBy, string? entityContextJson = null, string? callbackUrl = null)
    {
        try
        {
            var payload = new
            {
                policyCode,
                entityType = entityType.ToLowerInvariant(),
                entityId,
                requestedBy,
                entityContextJson = entityContextJson ?? "{}",
                callbackUrl
            };

            var response = await _httpClient.PostAsJsonAsync("/api/approval/requests/submit", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApprovalRequestResponseDto>();
            }
            _logger.LogWarning("Approval Engine returned status code {StatusCode} when submitting request for {EntityType} #{EntityId}", response.StatusCode, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not communicate with ApprovalEngine service at {BaseUrl}. Safe fallback triggered.", _httpClient.BaseAddress);
        }
        return null;
    }

    public async Task<ApprovalRequestResponseDto?> DecideRequestAsync(long requestId, long decidedBy, string decision, string? reason = null, string? evidencePath = null)
    {
        try
        {
            var payload = new
            {
                decidedBy,
                decision = decision.ToUpperInvariant(),
                reason,
                evidencePath
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/approval/requests/{requestId}/decide", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApprovalRequestResponseDto>();
            }

            var errJson = await response.Content.ReadFromJsonAsync<ApprovalErrorDto>();
            if (!string.IsNullOrEmpty(errJson?.Error))
            {
                throw new InvalidOperationException(errJson.Error);
            }
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw SoD (Segregation of Duties) or business rule exceptions
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not decide approval request #{RequestId}.", requestId);
        }
        return null;
    }

    public async Task<IEnumerable<ApprovalRequestResponseDto>> GetPendingApprovalsAsync(long approverId, string approverRole)
    {
        try
        {
            var url = $"/api/approval/requests/pending?approverId={approverId}&approverRole={Uri.EscapeDataString(approverRole)}";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<ApprovalRequestResponseDto>>(url);
            return result ?? Array.Empty<ApprovalRequestResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch pending approvals from ApprovalEngine.");
        }
        return Array.Empty<ApprovalRequestResponseDto>();
    }

    public async Task<ApprovalDelegationResponseDto?> CreateDelegationAsync(long delegatorId, long delegateId, long? policyId, string reason, DateTime validFrom, DateTime validUntil)
    {
        try
        {
            var payload = new
            {
                delegatorId,
                delegateId,
                policyId,
                reason,
                validFrom,
                validUntil
            };

            var response = await _httpClient.PostAsJsonAsync("/api/approval/delegations", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApprovalDelegationResponseDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create delegation in ApprovalEngine.");
        }
        return null;
    }
}

public class ApprovalRequestResponseDto
{
    public long IdRequest { get; set; }
    public long IdPolicy { get; set; }
    public int PolicyVersion { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string EntityContext { get; set; } = "{}";
    public string Status { get; set; } = "PENDING";
    public short CurrentStep { get; set; }
    public long RequestedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ApprovalDelegationResponseDto
{
    public long IdDelegation { get; set; }
    public long DelegatorId { get; set; }
    public long DelegateId { get; set; }
    public long? IdPolicy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
}

public class ApprovalErrorDto
{
    public string? Error { get; set; }
}
