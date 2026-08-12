namespace Nyx.FlowEngine.Domain.Entities;

public class FlowDefinition
{
    public long IdFlow { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = "CAMPAIGN";
    public long? ScopeId { get; set; }
    public bool IsActive { get; set; } = true;
    public int CurrentVersion { get; set; } = 1;
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FlowStage
{
    public long IdStage { get; set; }
    public long IdFlow { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short OrderIndex { get; set; }
    public bool IsTerminal { get; set; }
    public short? SlaHours { get; set; }
    public string Metadata { get; set; } = "{}";
}

public class CheckpointCatalog
{
    public long IdCheckpoint { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? IdFlow { get; set; }
    public long? TriggerStageId { get; set; }
    public string Origin { get; set; } = "INTERNAL"; // INTERNAL, EXTERNAL
    public string Scope { get; set; } = "ENTITY"; // ENTITY, ITEM
    public string[] Blocks { get; set; } = Array.Empty<string>(); // COMMISSION, LIQUIDATION, SERVICE_ACTIVATION
    public bool BlocksAdvance { get; set; }
    public long? RollbackToStage { get; set; }
    public long? TriggeredByKo { get; set; }
    public bool IsRecurrent { get; set; }
    public short? RecurrenceDays { get; set; }
    public short? MaxOccurrences { get; set; }
    public string? OwnerDept { get; set; }
    public string ApprovalStatus { get; set; } = "PROPOSED"; // PROPOSED, ACTIVE, DEPRECATED
    public string ApprovedBy { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CheckpointStep
{
    public long IdStep { get; set; }
    public long IdCheckpoint { get; set; }
    public short StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
}

public class FlowInstance
{
    public long IdInstance { get; set; }
    public long IdFlow { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long CurrentStageId { get; set; }
    public int DayCounter { get; set; } = 1;
    public string Metadata { get; set; } = "{}";
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, COMPLETED, CANCELLED
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointInstance
{
    public long IdCpInstance { get; set; }
    public long IdInstance { get; set; }
    public long IdCheckpoint { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, SUBSANADO, KO, SCHEDULED
    public long? OpenedAtStage { get; set; }
    public bool IsRetroactive { get; set; }
    public short OccurrenceNumber { get; set; } = 1;
    public DateTime? ScheduledFor { get; set; }
    public long? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CheckpointStepProgress
{
    public long IdProgress { get; set; }
    public long IdCpInstance { get; set; }
    public long IdStep { get; set; }
    public bool IsCompleted { get; set; }
    public long? CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class StageTransition
{
    public long IdTransition { get; set; }
    public long IdInstance { get; set; }
    public long? FromStageId { get; set; }
    public long ToStageId { get; set; }
    public string Direction { get; set; } = "FORWARD"; // FORWARD, BACKWARD, SKIP
    public string? TriggeredBy { get; set; }
    public long? ActorId { get; set; }
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
}
