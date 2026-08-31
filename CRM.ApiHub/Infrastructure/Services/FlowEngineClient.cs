using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Services;

public interface IFlowEngineClient
{
    Task<FlowInstanceDto?> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long? actorId = null);
    Task<FlowInstanceDto?> AdvanceStageAsync(long instanceId, long? actorId = null);
    Task<CheckpointInstanceDto?> ResolveCheckpointAsync(long cpInstanceId, string status, long? actorId = null);
    Task<ResolveCheckpointResultDto?> ResolveCheckpointDetailedAsync(long cpInstanceId, string status, long? actorId = null);
    Task<FlowInstanceDto?> GetInstanceByEntityAsync(string entityType, long entityId);
    Task<FlowInstanceWithCheckpointsDto?> GetInstanceWithCheckpointsByEntityAsync(string entityType, long entityId);
    Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByIdAsync(long instanceId);
    Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByEntityAsync(string entityType, long entityId);
    Task<FlowValidationResultDto?> ValidateStageAdvanceAsync(long instanceId);
    Task<IEnumerable<CheckpointCatalogDto>> GetCheckpointCatalogAsync(long? flowId = null);
    Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCatalogAsync(long? flowId = null);
    Task<CheckpointCatalogDto?> CreateCheckpointCatalogAsync(object payload);
    Task<bool> UpdateCheckpointCatalogAsync(long id, object payload);
    Task<IEnumerable<CheckpointStepDto>> GetCheckpointStepsAsync(long checkpointId);
    Task<bool> SaveCheckpointStepsAsync(long checkpointId, object stepsPayload);
    Task<bool> ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long? actorId = null);
    Task<IEnumerable<CheckpointStepProgressDto>> GetStepProgressAsync(long cpInstanceId);
    Task<bool> SetFactsAsync(long instanceId, string factsJson, long? actorId = null);
    Task<bool> ValidateTransitionAsync(string entityType, int currentState, int targetState);
    Task<bool> SetEntityFactsAsync(string entityType, long entityId, string factsJson, long? actorId = null);
    Task<FlowInstanceWithCheckpointsDto?> SyncStageByStatusAsync(string entityType, long entityId, long statusId, long? actorId = null);
}

public class FlowEngineClient : IFlowEngineClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlowEngineClient> _logger;

    public FlowEngineClient(HttpClient httpClient, ILogger<FlowEngineClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlowInstanceDto?> GetInstanceByEntityAsync(string entityType, long entityId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FlowInstanceDto>(
                $"/api/flow/instances/by-entity/{entityType}/{entityId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch flow instance for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task<FlowInstanceWithCheckpointsDto?> GetInstanceWithCheckpointsByEntityAsync(string entityType, long entityId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FlowInstanceWithCheckpointsDto>(
                $"/api/flow/instances/by-entity/{entityType}/{entityId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch flow instance with checkpoints for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByIdAsync(long instanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FlowInstanceDetailDto>(
                $"/api/flow/instances/{instanceId}/detail");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch detailed flow instance for #{InstanceId}.", instanceId);
        }
        return null;
    }

    public async Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByEntityAsync(string entityType, long entityId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FlowInstanceDetailDto>(
                $"/api/flow/instances/by-entity/{entityType}/{entityId}/detail");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch detailed flow instance for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return null;
    }

    public async Task<FlowValidationResultDto?> ValidateStageAdvanceAsync(long instanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FlowValidationResultDto>(
                $"/api/flow/instances/{instanceId}/validate-advance");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not validate stage advance for instance #{InstanceId}.", instanceId);
        }
        return null;
    }

    public async Task<IEnumerable<CheckpointStepDto>> GetCheckpointStepsAsync(long checkpointId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CheckpointStepDto>>($"/api/flow/checkpoints/catalog/{checkpointId}/steps");
            return result ?? Array.Empty<CheckpointStepDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch checkpoint steps for checkpoint #{CheckpointId}.", checkpointId);
        }
        return Array.Empty<CheckpointStepDto>();
    }

    public async Task<bool> SaveCheckpointStepsAsync(long checkpointId, object stepsPayload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/flow/checkpoints/catalog/{checkpointId}/steps", stepsPayload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save checkpoint steps for checkpoint #{CheckpointId}.", checkpointId);
        }
        return false;
    }

    public async Task<FlowInstanceDto?> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                flowCode,
                entityType = entityType.ToLowerInvariant(),
                entityId,
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync("/api/flow/instances/start", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FlowInstanceDto>();
            }
            _logger.LogWarning("Flow Engine returned status code {StatusCode} when starting flow for {EntityType} #{EntityId}", response.StatusCode, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not communicate with FlowEngine service at {BaseUrl}. Safe fallback triggered.", _httpClient.BaseAddress);
        }
        return null;
    }

    public async Task<FlowInstanceDto?> AdvanceStageAsync(long instanceId, long? actorId = null)
    {
        try
        {
            var payload = new { actorId = actorId ?? 1 };
            var response = await _httpClient.PostAsJsonAsync($"/api/flow/instances/{instanceId}/advance", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FlowInstanceDto>();
            }
            
            var errJson = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
            if (!string.IsNullOrEmpty(errJson?.Error))
            {
                throw new InvalidOperationException(errJson.Error);
            }
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw business logic blockers so API returns proper 400 Bad Request
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not advance flow stage for instance #{InstanceId}.", instanceId);
        }
        return null;
    }

    public async Task<CheckpointInstanceDto?> ResolveCheckpointAsync(long cpInstanceId, string status, long? actorId = null)
    {
        try
        {
            var detailed = await ResolveCheckpointDetailedAsync(cpInstanceId, status, actorId);
            if (detailed != null)
            {
                return new CheckpointInstanceDto
                {
                    IdCpInstance = detailed.CheckpointInstanceId,
                    IdCheckpoint = detailed.IdCheckpoint,
                    Status = detailed.ResolvedStatus,
                    ResolvedBy = actorId,
                    ResolvedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve checkpoint instance #{CpInstanceId}.", cpInstanceId);
        }
        return null;
    }

    public async Task<ResolveCheckpointResultDto?> ResolveCheckpointDetailedAsync(long cpInstanceId, string status, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                status = status.ToUpperInvariant(),
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/flow/checkpoints/instances/{cpInstanceId}/resolve", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResolveCheckpointResultDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve checkpoint instance #{CpInstanceId} in detailed mode.", cpInstanceId);
        }
        return null;
    }

    public async Task<IEnumerable<CheckpointCatalogDto>> GetCheckpointCatalogAsync(long? flowId = null)
    {
        try
        {
            var url = flowId.HasValue ? $"/api/flow/checkpoints/catalog?flowId={flowId.Value}" : "/api/flow/checkpoints/catalog";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CheckpointCatalogDto>>(url);
            return result ?? Array.Empty<CheckpointCatalogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch checkpoint catalog from FlowEngine.");
        }
        return Array.Empty<CheckpointCatalogDto>();
    }

    public async Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCatalogAsync(long? flowId = null)
    {
        try
        {
            var url = flowId.HasValue ? $"/api/flow/checkpoints/catalog/full?flowId={flowId.Value}" : "/api/flow/checkpoints/catalog/full";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CheckpointCatalogWithStepsDto>>(url);
            return result ?? Array.Empty<CheckpointCatalogWithStepsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch full checkpoint catalog from FlowEngine.");
        }
        return Array.Empty<CheckpointCatalogWithStepsDto>();
    }

    public async Task<CheckpointCatalogDto?> CreateCheckpointCatalogAsync(object payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/flow/checkpoints/catalog", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CheckpointCatalogDto>();
            }
            var errStr = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("FlowEngine CreateCheckpointCatalog returned status code {StatusCode}: {Error}", response.StatusCode, errStr);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create checkpoint catalog in FlowEngine.");
        }
        return null;
    }

    public async Task<bool> UpdateCheckpointCatalogAsync(long id, object payload)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/flow/checkpoints/catalog/{id}", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not update checkpoint catalog #{Id} in FlowEngine.", id);
        }
        return false;
    }

    public async Task<bool> ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long? actorId = null)
    {
        try
        {
            var payload = new { isCompleted, actorId = actorId ?? 1 };
            var response = await _httpClient.PostAsJsonAsync($"/api/flow/checkpoints/instances/{cpInstanceId}/steps/{stepId}/toggle", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not toggle step progress for CP instance #{CpInstanceId}, Step #{StepId}.", cpInstanceId, stepId);
        }
        return false;
    }

    public async Task<IEnumerable<CheckpointStepProgressDto>> GetStepProgressAsync(long cpInstanceId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CheckpointStepProgressDto>>($"/api/flow/checkpoints/instances/{cpInstanceId}/steps");
            return result ?? Array.Empty<CheckpointStepProgressDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch step progress for CP instance #{CpInstanceId}.", cpInstanceId);
        }
        return Array.Empty<CheckpointStepProgressDto>();
    }

    public async Task<bool> SetFactsAsync(long instanceId, string factsJson, long? actorId = null)
    {
        try
        {
            var payload = new { factsJson, actorId = actorId ?? 1 };
            var response = await _httpClient.PostAsJsonAsync($"/api/flow/instances/{instanceId}/facts", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not update facts for instance #{InstanceId}.", instanceId);
        }
        return false;
    }

    public async Task<bool> ValidateTransitionAsync(string entityType, int currentState, int targetState)
    {
        try
        {
            var payload = new { entityType, currentState, targetState };
            var response = await _httpClient.PostAsJsonAsync("/api/flow/validate-transition", payload);
            
            if (response.IsSuccessStatusCode)
            {
                var isValid = await response.Content.ReadFromJsonAsync<bool>();
                return isValid;
            }
            
            _logger.LogWarning("FlowEngine ValidateTransition returned status code {StatusCode}.", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not validate transition for {EntityType} with FlowEngine.", entityType);
            throw; 
        }
    }

    public async Task<bool> SetEntityFactsAsync(string entityType, long entityId, string factsJson, long? actorId = null)
    {
        try
        {
            var payload = new { factsJson, actorId = actorId ?? 1 };
            var response = await _httpClient.PostAsJsonAsync($"/api/flow/instances/by-entity/{entityType}/{entityId}/facts", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not update entity facts for {EntityType} #{EntityId}.", entityType, entityId);
        }
        return false;
    }

    public async Task<FlowInstanceWithCheckpointsDto?> SyncStageByStatusAsync(string entityType, long entityId, long statusId, long? actorId = null)
    {
        try
        {
            var payload = new
            {
                entityType = entityType.ToLowerInvariant(),
                entityId,
                statusId,
                actorId = actorId ?? 1
            };

            var response = await _httpClient.PostAsJsonAsync("/api/flow/instances/sync-status", payload);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FlowInstanceWithCheckpointsDto>();
            }

            var errJson = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
            if (!string.IsNullOrEmpty(errJson?.Error))
            {
                throw new InvalidOperationException(errJson.Error);
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not sync stage by status for {EntityType} #{EntityId} to status {StatusId}.", entityType, entityId, statusId);
        }
        return null;
    }
}

public class FlowInstanceDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idFlow")]
    public long IdFlow { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("entityId")]
    public long EntityId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("dayCounter")]
    public int DayCounter { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; } = "ACTIVE";

    [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class FlowInstanceWithCheckpointsDto : FlowInstanceDto
{
    [System.Text.Json.Serialization.JsonPropertyName("checkpointInstances")]
    public List<CheckpointInstanceDto> CheckpointInstances { get; set; } = new();
}

public class CheckpointInstanceDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idCpInstance")]
    public long IdCpInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; } = "PENDING";

    [System.Text.Json.Serialization.JsonPropertyName("openedAtStage")]
    public long? OpenedAtStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isRetroactive")]
    public bool IsRetroactive { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("occurrenceNumber")]
    public short OccurrenceNumber { get; set; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("scheduledFor")]
    public DateTime? ScheduledFor { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("resolvedBy")]
    public long? ResolvedBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }
}

public class CheckpointCatalogDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idFlow")]
    public long? IdFlow { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggerStageId")]
    public long? TriggerStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("origin")]
    public string Origin { get; set; } = "INTERNAL";

    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string Scope { get; set; } = "ENTITY";

    [System.Text.Json.Serialization.JsonPropertyName("blocks")]
    public string[] Blocks { get; set; } = Array.Empty<string>();

    [System.Text.Json.Serialization.JsonPropertyName("blocksAdvance")]
    public bool BlocksAdvance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rollbackToStage")]
    public long? RollbackToStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggeredByKo")]
    public long? TriggeredByKo { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isRecurrent")]
    public bool IsRecurrent { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("recurrenceDays")]
    public short? RecurrenceDays { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("maxOccurrences")]
    public short? MaxOccurrences { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("ownerDept")]
    public string? OwnerDept { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string Category { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("division")]
    public string Division { get; set; } = "OPERACIONES";

    [System.Text.Json.Serialization.JsonPropertyName("approvalJobTitle")]
    public string ApprovalJobTitle { get; set; } = "SUPERVISOR";

    [System.Text.Json.Serialization.JsonPropertyName("satellites")]
    public string[] Satellites { get; set; } = Array.Empty<string>();

    [System.Text.Json.Serialization.JsonPropertyName("executionOrder")]
    public int ExecutionOrder { get; set; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("rollbackToCheckpointCode")]
    public string? RollbackToCheckpointCode { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rollbackToStepOrder")]
    public int? RollbackToStepOrder { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("preconditionFact")]
    public string? PreconditionFact { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("campaign")]
    public string Campaign { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("provider")]
    public string Provider { get; set; } = "INTERNO";

    [System.Text.Json.Serialization.JsonPropertyName("finalizesCycle")]
    public bool FinalizesCycle { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("targetRoles")]
    public string TargetRoles { get; set; } = "SUPERVISOR,BACKOFFICE";

    [System.Text.Json.Serialization.JsonPropertyName("approvalStatus")]
    public string ApprovalStatus { get; set; } = "PROPOSED";
}

public class CheckpointCatalogWithStepsDto : CheckpointCatalogDto
{
    [System.Text.Json.Serialization.JsonPropertyName("steps")]
    public List<CheckpointStepDto> Steps { get; set; } = new();
}

public class CheckpointStepProgressDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idProgress")]
    public long IdProgress { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCpInstance")]
    public long IdCpInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idStep")]
    public long IdStep { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("completedBy")]
    public long? CompletedBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public class ErrorResponseDto
{
    public string? Error { get; set; }
}

public class CheckpointStepDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idStep")]
    public long IdStep { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("stepOrder")]
    public short StepOrder { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("instruction")]
    public string Instruction { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// DTOs ENRIQUECIDOS PARA FRONTEND, CLIENTES Y SIMULADOR
// ══════════════════════════════════════════════════════════════════════════════

public class CheckpointStepDetailDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idStep")]
    public long IdStep { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("stepOrder")]
    public short StepOrder { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("instruction")]
    public string Instruction { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("completedBy")]
    public long? CompletedBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointInstanceDetailDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idCpInstance")]
    public long IdCpInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; } = "PENDING";

    [System.Text.Json.Serialization.JsonPropertyName("openedAtStage")]
    public long? OpenedAtStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggerStageId")]
    public long? TriggerStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("openedAtStageName")]
    public string? OpenedAtStageName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isRetroactive")]
    public bool IsRetroactive { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("occurrenceNumber")]
    public short OccurrenceNumber { get; set; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("scheduledFor")]
    public DateTime? ScheduledFor { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("resolvedBy")]
    public long? ResolvedBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Metadatos del catálogo
    [System.Text.Json.Serialization.JsonPropertyName("ownerDept")]
    public string OwnerDept { get; set; } = "Asesor";

    [System.Text.Json.Serialization.JsonPropertyName("category")]
    public string Category { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("division")]
    public string Division { get; set; } = "OPERACIONES";

    [System.Text.Json.Serialization.JsonPropertyName("blocksAdvance")]
    public bool BlocksAdvance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("finalizesCycle")]
    public bool FinalizesCycle { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rollbackToStage")]
    public long? RollbackToStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("rollbackToStageName")]
    public string? RollbackToStageName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggeredByKo")]
    public long? TriggeredByKo { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggeredByKoName")]
    public string? TriggeredByKoName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("executionOrder")]
    public int ExecutionOrder { get; set; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("campaign")]
    public string Campaign { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("provider")]
    public string Provider { get; set; } = "INTERNO";

    [System.Text.Json.Serialization.JsonPropertyName("approvalStatus")]
    public string ApprovalStatus { get; set; } = "ACTIVE";

    // Pasos interactivos
    [System.Text.Json.Serialization.JsonPropertyName("steps")]
    public List<CheckpointStepDetailDto> Steps { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("totalStepsCount")]
    public int TotalStepsCount => Steps.Count;

    [System.Text.Json.Serialization.JsonPropertyName("completedStepsCount")]
    public int CompletedStepsCount => Steps.Count(s => s.IsCompleted);

    [System.Text.Json.Serialization.JsonPropertyName("allRequiredStepsCompleted")]
    public bool AllRequiredStepsCompleted => Steps.Where(s => s.IsRequired).All(s => s.IsCompleted);
}

public class FlowStageDetailDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idStage")]
    public long IdStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idFlow")]
    public long IdFlow { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("stageCode")]
    public string StageCode { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("orderIndex")]
    public short OrderIndex { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isTerminal")]
    public bool IsTerminal { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("slaHours")]
    public short? SlaHours { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = "GENERAL";

    [System.Text.Json.Serialization.JsonPropertyName("campaign")]
    public string Campaign { get; set; } = "GENERAL";
}

public class StageTransitionDetailDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idTransition")]
    public long IdTransition { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("fromStageId")]
    public long? FromStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("fromStageName")]
    public string? FromStageName { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("toStageId")]
    public long ToStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("toStageName")]
    public string ToStageName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("direction")]
    public string Direction { get; set; } = "FORWARD";

    [System.Text.Json.Serialization.JsonPropertyName("triggeredBy")]
    public string? TriggeredBy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("actorId")]
    public long? ActorId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("transitionedAt")]
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
}

public class FlowInstanceDetailDto
{
    [System.Text.Json.Serialization.JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idFlow")]
    public long IdFlow { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("flowCode")]
    public string FlowCode { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("flowName")]
    public string FlowName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("entityId")]
    public long EntityId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStage")]
    public FlowStageDetailDto? CurrentStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("dayCounter")]
    public int DayCounter { get; set; } = 1;

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string Status { get; set; } = "ACTIVE";

    [System.Text.Json.Serialization.JsonPropertyName("facts")]
    public string Facts { get; set; } = "{}";

    [System.Text.Json.Serialization.JsonPropertyName("metadata")]
    public string Metadata { get; set; } = "{}";

    [System.Text.Json.Serialization.JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("checkpoints")]
    public List<CheckpointInstanceDetailDto> Checkpoints { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("recentTransitions")]
    public List<StageTransitionDetailDto> RecentTransitions { get; set; } = new();

    // Consultas de negocio rápidas
    [System.Text.Json.Serialization.JsonPropertyName("canAdvanceStage")]
    public bool CanAdvanceStage => !Checkpoints.Any(c => c.Status == "PENDING" && c.BlocksAdvance);

    [System.Text.Json.Serialization.JsonPropertyName("pendingBlockingCount")]
    public int PendingBlockingCount => Checkpoints.Count(c => c.Status == "PENDING" && c.BlocksAdvance);

    [System.Text.Json.Serialization.JsonPropertyName("pendingCount")]
    public int PendingCount => Checkpoints.Count(c => c.Status == "PENDING");

    [System.Text.Json.Serialization.JsonPropertyName("approvedCount")]
    public int ApprovedCount => Checkpoints.Count(c => c.Status == "APPROVED");

    [System.Text.Json.Serialization.JsonPropertyName("koCount")]
    public int KoCount => Checkpoints.Count(c => c.Status == "KO");
}

public class ResolveCheckpointResultDto
{
    [System.Text.Json.Serialization.JsonPropertyName("checkpointInstanceId")]
    public long CheckpointInstanceId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("resolvedStatus")]
    public string ResolvedStatus { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("nextAction")]
    public string NextAction { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStageName")]
    public string CurrentStageName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("flowStatus")]
    public string FlowStatus { get; set; } = "ACTIVE";

    [System.Text.Json.Serialization.JsonPropertyName("isCycleClosed")]
    public bool IsCycleClosed => FlowStatus == "CLOSED" || FlowStatus == "COMPLETED";

    [System.Text.Json.Serialization.JsonPropertyName("canAdvanceStage")]
    public bool CanAdvanceStage { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("triggeredCheckpoints")]
    public List<CheckpointInstanceDetailDto> TriggeredCheckpoints { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("flowInstance")]
    public FlowInstanceDetailDto? FlowInstance { get; set; }
}

public class FlowValidationResultDto
{
    [System.Text.Json.Serialization.JsonPropertyName("instanceId")]
    public long InstanceId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currentStageName")]
    public string CurrentStageName { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("canAdvance")]
    public bool CanAdvance { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("pendingBlockingCount")]
    public int PendingBlockingCount { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("blockingPendingCheckpoints")]
    public List<CheckpointInstanceDetailDto> BlockingPendingCheckpoints { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("blockingReasons")]
    public List<string> BlockingReasons { get; set; } = new();
}
