using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.WebFrontend.Client.Models;

public class CheckpointStepDetailDto
{
    [JsonPropertyName("idStep")]
    public long IdStep { get; set; }

    [JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [JsonPropertyName("stepOrder")]
    public short StepOrder { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("instruction")]
    public string Instruction { get; set; } = string.Empty;

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("completedBy")]
    public long? CompletedBy { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointInstanceDetailDto
{
    [JsonPropertyName("idCpInstance")]
    public long IdCpInstance { get; set; }

    [JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [JsonPropertyName("idCheckpoint")]
    public long IdCheckpoint { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "PENDING";

    [JsonPropertyName("openedAtStage")]
    public long? OpenedAtStage { get; set; }

    [JsonPropertyName("triggerStageId")]
    public long? TriggerStageId { get; set; }

    [JsonPropertyName("openedAtStageName")]
    public string? OpenedAtStageName { get; set; }

    [JsonPropertyName("isRetroactive")]
    public bool IsRetroactive { get; set; }

    [JsonPropertyName("occurrenceNumber")]
    public short OccurrenceNumber { get; set; } = 1;

    [JsonPropertyName("scheduledFor")]
    public DateTime? ScheduledFor { get; set; }

    [JsonPropertyName("resolvedBy")]
    public long? ResolvedBy { get; set; }

    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Metadatos del catálogo
    [JsonPropertyName("ownerDept")]
    public string OwnerDept { get; set; } = "Asesor";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "GENERAL";

    [JsonPropertyName("division")]
    public string Division { get; set; } = "OPERACIONES";

    [JsonPropertyName("blocksAdvance")]
    public bool BlocksAdvance { get; set; }

    [JsonPropertyName("finalizesCycle")]
    public bool FinalizesCycle { get; set; }

    [JsonPropertyName("rollbackToStage")]
    public long? RollbackToStage { get; set; }

    [JsonPropertyName("rollbackToStageName")]
    public string? RollbackToStageName { get; set; }

    [JsonPropertyName("triggeredByKo")]
    public long? TriggeredByKo { get; set; }

    [JsonPropertyName("triggeredByKoName")]
    public string? TriggeredByKoName { get; set; }

    [JsonPropertyName("executionOrder")]
    public int ExecutionOrder { get; set; } = 1;

    [JsonPropertyName("campaign")]
    public string Campaign { get; set; } = "GENERAL";

    [JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = "GENERAL";

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "INTERNO";

    [JsonPropertyName("steps")]
    public List<CheckpointStepDetailDto> Steps { get; set; } = new();

    [JsonPropertyName("canApprove")]
    public bool CanApprove { get; set; }

    [JsonPropertyName("pendingRequiredStepsCount")]
    public int PendingRequiredStepsCount { get; set; }
}

public class FlowStageDetailDto
{
    [JsonPropertyName("idStage")]
    public long IdStage { get; set; }

    [JsonPropertyName("idFlow")]
    public long IdFlow { get; set; }

    [JsonPropertyName("stageCode")]
    public string StageCode { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("orderIndex")]
    public short OrderIndex { get; set; }

    [JsonPropertyName("isTerminal")]
    public bool IsTerminal { get; set; }

    [JsonPropertyName("slaHours")]
    public short? SlaHours { get; set; }

    [JsonPropertyName("portfolio")]
    public string Portfolio { get; set; } = "GENERAL";

    [JsonPropertyName("campaign")]
    public string Campaign { get; set; } = "GENERAL";
}

public class FlowInstanceDetailDto
{
    [JsonPropertyName("idInstance")]
    public long IdInstance { get; set; }

    [JsonPropertyName("idFlow")]
    public long IdFlow { get; set; }

    [JsonPropertyName("flowCode")]
    public string FlowCode { get; set; } = string.Empty;

    [JsonPropertyName("flowName")]
    public string FlowName { get; set; } = string.Empty;

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("entityId")]
    public long EntityId { get; set; }

    [JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [JsonPropertyName("currentStage")]
    public FlowStageDetailDto? CurrentStage { get; set; }

    [JsonPropertyName("dayCounter")]
    public int DayCounter { get; set; } = 1;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "ACTIVE";

    [JsonPropertyName("facts")]
    public string Facts { get; set; } = "{}";

    [JsonPropertyName("metadata")]
    public string Metadata { get; set; } = "{}";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("checkpoints")]
    public List<CheckpointInstanceDetailDto> Checkpoints { get; set; } = new();

    [JsonPropertyName("pendingBlockingCount")]
    public int PendingBlockingCount { get; set; }

    [JsonPropertyName("approvedCount")]
    public int ApprovedCount { get; set; }

    [JsonPropertyName("koCount")]
    public int KoCount { get; set; }

    [JsonPropertyName("pendingCount")]
    public int PendingCount { get; set; }

    [JsonPropertyName("canAdvanceStage")]
    public bool CanAdvanceStage { get; set; }
}

public class ResolveCheckpointResultDto
{
    [JsonPropertyName("idCpInstance")]
    public long IdCpInstance { get; set; }

    [JsonPropertyName("resolvedStatus")]
    public string ResolvedStatus { get; set; } = string.Empty;

    [JsonPropertyName("nextAction")]
    public string NextAction { get; set; } = "NONE";

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("triggeredCheckpoints")]
    public List<CheckpointInstanceDetailDto> TriggeredCheckpoints { get; set; } = new();

    [JsonPropertyName("currentStageId")]
    public long CurrentStageId { get; set; }

    [JsonPropertyName("currentStageName")]
    public string CurrentStageName { get; set; } = string.Empty;

    [JsonPropertyName("flowStatus")]
    public string FlowStatus { get; set; } = "ACTIVE";

    [JsonPropertyName("isCycleClosed")]
    public bool IsCycleClosed { get; set; }
}
