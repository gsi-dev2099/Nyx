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
    public string Portfolio { get; set; } = "GENERAL";
    public string Campaign { get; set; } = "GENERAL";
    public string Metadata { get; set; } = "{}";
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
    public string Facts { get; set; } = "{}";
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, COMPLETED, CANCELLED
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointCatalogWithStepsDto : CheckpointCatalog
{
    public List<CheckpointStep> Steps { get; set; } = new();
}

public class FlowInstanceWithCheckpointsDto : FlowInstance
{
    public List<CheckpointInstance> CheckpointInstances { get; set; } = new();
}

public class FlowAuditLog
{
    public long IdLog { get; set; }
    public long? IdInstance { get; set; }
    public long? IdCheckpoint { get; set; }
    public string Action { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string Detail { get; set; } = "{}";
    public string Checksum { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FlowStageDetailDto
{
    public long IdStage { get; set; }
    public long IdFlow { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short OrderIndex { get; set; }
    public bool IsTerminal { get; set; }
    public short? SlaHours { get; set; }
    public string Portfolio { get; set; } = "GENERAL";
    public string Campaign { get; set; } = "GENERAL";
}

public class StageTransitionDetailDto
{
    public long IdTransition { get; set; }
    public long IdInstance { get; set; }
    public long? FromStageId { get; set; }
    public string? FromStageName { get; set; }
    public long ToStageId { get; set; }
    public string ToStageName { get; set; } = string.Empty;
    public string Direction { get; set; } = "FORWARD"; // FORWARD, BACKWARD, SKIP
    public string? TriggeredBy { get; set; }
    public long? ActorId { get; set; }
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
}

public class FlowInstanceDetailDto
{
    public long IdInstance { get; set; }
    public long IdFlow { get; set; }
    public string FlowCode { get; set; } = string.Empty;
    public string FlowName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long CurrentStageId { get; set; }
    public FlowStageDetailDto? CurrentStage { get; set; }
    public int DayCounter { get; set; } = 1;
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, CLOSED, COMPLETED, CANCELLED
    public string Facts { get; set; } = "{}";
    public string Metadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public List<CheckpointInstanceDetailDto> Checkpoints { get; set; } = new();
    public List<StageTransitionDetailDto> RecentTransitions { get; set; } = new();

    // Consultas de negocio rápidas
    public bool CanAdvanceStage => !Checkpoints.Any(c => c.Status == "PENDING" && c.BlocksAdvance);
    public int PendingBlockingCount => Checkpoints.Count(c => c.Status == "PENDING" && c.BlocksAdvance);
    public int PendingCount => Checkpoints.Count(c => c.Status == "PENDING");
    public int ApprovedCount => Checkpoints.Count(c => c.Status == "APPROVED");
    public int KoCount => Checkpoints.Count(c => c.Status == "KO");
}

public class FlowValidationResultDto
{
    public long InstanceId { get; set; }
    public long CurrentStageId { get; set; }
    public string CurrentStageName { get; set; } = string.Empty;
    public bool CanAdvance { get; set; }
    public int PendingBlockingCount { get; set; }
    public List<CheckpointInstanceDetailDto> BlockingPendingCheckpoints { get; set; } = new();
    public List<string> BlockingReasons { get; set; } = new();
}
