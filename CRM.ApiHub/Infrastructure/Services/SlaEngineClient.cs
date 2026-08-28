using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Services;

public interface ISlaEngineClient
{
    Task<SlaMeasurementDto?> StartMeasurementAsync(string entityType, long entityId, string policyCode, long? ownerUserId = null, long? actorId = null);
    Task<SlaMeasurementDto?> ResolveMeasurementAsync(string entityType, long entityId, string policyCode, long? actorId = null);
    Task<SlaMeasurementDto?> PauseMeasurementAsync(string entityType, long entityId, string policyCode, long? actorId = null);
    Task<SlaMeasurementDto?> GetStatusAsync(string entityType, long entityId, string policyCode);
    Task TrackStateChangeAsync(string entityType, long entityId, int targetStatus, long? custodyUserId);
}

public class SlaEngineClient : ISlaEngineClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlaEngineClient> _logger;

    public SlaEngineClient(HttpClient httpClient, ILogger<SlaEngineClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SlaMeasurementDto?> StartMeasurementAsync(string entityType, long entityId, string policyCode, long? ownerUserId = null, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                entityType = entityType.ToLowerInvariant(),
                entityId,
                policyCode,
                ownerUserId,
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync("/api/sla/measurements/start", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SlaMeasurementDto>();
            }
            _logger.LogWarning("SLA Engine returned non-success status code {StatusCode} when starting SLA for {EntityType} #{EntityId}", response.StatusCode, entityType, entityId);
        }
        catch (Exception ex)
        {
            // Resilient fallback: Log warning and allow main CRM transaction to proceed cleanly
            _logger.LogWarning(ex, "Could not communicate with SlaEngine service at {BaseUrl}. Safe fallback triggered.", _httpClient.BaseAddress);
        }
        return null;
    }

    public async Task<SlaMeasurementDto?> ResolveMeasurementAsync(string entityType, long entityId, string policyCode, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                entityType = entityType.ToLowerInvariant(),
                entityId,
                policyCode,
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync("/api/sla/measurements/resolve", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SlaMeasurementDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve SLA measurement for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task<SlaMeasurementDto?> PauseMeasurementAsync(string entityType, long entityId, string policyCode, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                entityType = entityType.ToLowerInvariant(),
                entityId,
                policyCode,
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync("/api/sla/measurements/pause", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SlaMeasurementDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not pause SLA measurement for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task<SlaMeasurementDto?> GetStatusAsync(string entityType, long entityId, string policyCode)
    {
        try
        {
            var url = $"/api/sla/measurements/status?entityType={Uri.EscapeDataString(entityType)}&entityId={entityId}&policyCode={Uri.EscapeDataString(policyCode)}";
            return await _httpClient.GetFromJsonAsync<SlaMeasurementDto>(url);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch SLA status for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task TrackStateChangeAsync(string entityType, long entityId, int targetStatus, long? custodyUserId)
    {
        try
        {
            var payload = new SlaTrackEventDto
            {
                EntityType = entityType.ToLowerInvariant(),
                EntityId = entityId,
                TargetStatus = targetStatus,
                CustodyUserId = custodyUserId
            };

            var response = await _httpClient.PostAsJsonAsync("/api/sla/track", payload);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SLA Engine returned non-success status code {StatusCode} when tracking state change for {EntityType} #{EntityId}", response.StatusCode, entityType, entityId);
            }
        }
        catch (Exception ex)
        {
            // Re-throw so the caller (UseCase) can catch it and log it as requested by the user
            throw;
        }
    }
}

public class SlaMeasurementDto
{
    public long IdMeasurement { get; set; }
    public long IdPolicy { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long? OwnerUserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int ElapsedMinutes { get; set; }
    public string Status { get; set; } = "RUNNING"; // RUNNING, PAUSED, WARNING, BREACHED, COMPLETED
    public DateTime? BreachAt { get; set; }
    public string Metadata { get; set; } = "{}";
}

public class SlaTrackEventDto
{
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public int TargetStatus { get; set; }
    public long? CustodyUserId { get; set; }
}
