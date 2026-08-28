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
    public string Category { get; set; } = "GENERAL";
    public string Division { get; set; } = "OPERACIONES";
    public string ApprovalJobTitle { get; set; } = "SUPERVISOR";
    public string[] Satellites { get; set; } = Array.Empty<string>();
    public int ExecutionOrder { get; set; } = 1;
    public string? RollbackToCheckpointCode { get; set; }
    public int? RollbackToStepOrder { get; set; }
    public string? PreconditionFact { get; set; }
    public string Portfolio { get; set; } = "GENERAL";
    public string Campaign { get; set; } = "GENERAL";
    public string Provider { get; set; } = "INTERNO";
    public bool FinalizesCycle { get; set; }
    public string TargetRoles { get; set; } = "SUPERVISOR,BACKOFFICE";
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
    private string _name = string.Empty;
    public string Name { get => _name; set => _name = value; }
    public string Instruction { get => _name; set => _name = value; }
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

// ══════════════════════════════════════════════════════════════════════════════
// DTOs ENRIQUECIDOS PARA FRONTEND, CLIENTES Y SIMULADOR
// ══════════════════════════════════════════════════════════════════════════════

public class CheckpointStepDetailDto
{
    public long IdStep { get; set; }
    public long IdCheckpoint { get; set; }
    public short StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public long? CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointInstanceDetailDto
{
    public long IdCpInstance { get; set; }
    public long IdInstance { get; set; }
    public long IdCheckpoint { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, KO, SCHEDULED
    public long? OpenedAtStage { get; set; }
    public string? OpenedAtStageName { get; set; }
    public bool IsRetroactive { get; set; }
    public short OccurrenceNumber { get; set; } = 1;
    public DateTime? ScheduledFor { get; set; }
    public long? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Metadatos del catálogo
    public string OwnerDept { get; set; } = "Asesor";
    public string Category { get; set; } = "GENERAL";
    public string Division { get; set; } = "OPERACIONES";
    public bool BlocksAdvance { get; set; }
    public bool FinalizesCycle { get; set; }
    public long? RollbackToStage { get; set; }
    public string? RollbackToStageName { get; set; }
    public long? TriggeredByKo { get; set; }
    public string? TriggeredByKoName { get; set; }
    public int ExecutionOrder { get; set; } = 1;
    public string Campaign { get; set; } = "GENERAL";
    public string Portfolio { get; set; } = "GENERAL";
    public string Provider { get; set; } = "INTERNO";
    public string ApprovalStatus { get; set; } = "ACTIVE";

    // Pasos interactivos
    public List<CheckpointStepDetailDto> Steps { get; set; } = new();
    public int TotalStepsCount => Steps.Count;
    public int CompletedStepsCount => Steps.Count(s => s.IsCompleted);
    public bool AllRequiredStepsCompleted => Steps.Where(s => s.IsRequired).All(s => s.IsCompleted);
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

public class ResolveCheckpointResultDto
{
    public long CheckpointInstanceId { get; set; }
    public long IdCheckpoint { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ResolvedStatus { get; set; } = string.Empty; // "APPROVED" | "KO"
    public string NextAction { get; set; } = string.Empty; // "STAGE_ADVANCED" | "CHAINED_TRIGGERED" | "STAGE_ROLLBACK" | "CYCLE_FINALIZED" | "SEQUENTIAL_TRIGGERED" | "BLOCKED" | "NONE"
    public string Message { get; set; } = string.Empty;
    public long CurrentStageId { get; set; }
    public string CurrentStageName { get; set; } = string.Empty;
    public string FlowStatus { get; set; } = "ACTIVE";
    public bool IsCycleClosed => FlowStatus == "CLOSED" || FlowStatus == "COMPLETED";
    public bool CanAdvanceStage { get; set; }
    public List<CheckpointInstanceDetailDto> TriggeredCheckpoints { get; set; } = new();
    public FlowInstanceDetailDto? FlowInstance { get; set; }
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
