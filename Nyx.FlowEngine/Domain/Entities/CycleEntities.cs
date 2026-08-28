namespace Nyx.FlowEngine.Domain.Entities;

// ==============================================================================
// 1. CICLOS (Entidad Tope del Ecosistema)
// ==============================================================================
public class CycleDefinition
{
    public long IdCycle { get; set; }
    public string CycleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = "COMMERCIAL"; // COMMERCIAL, RECRUITMENT, AFTER_SALES
    public bool IsActive { get; set; } = true;
    public int CurrentVersion { get; set; } = 1;
    public string EntryPolicyJson { get; set; } = "{}";
    public string ExitPolicyJson { get; set; } = "{}";
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ==============================================================================
// 2. ETAPAS (Fases dentro de un Ciclo)
// ==============================================================================
public class CycleStage
{
    public long IdStage { get; set; }
    public long IdCycle { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short OrderIndex { get; set; } = 1;
    public bool IsTerminal { get; set; } = false;
    public short? SlaHours { get; set; }
    public string PoliciesJson { get; set; } = "{}";
}

// ==============================================================================
// 3. CHECKPOINTS (Hitos de Validación / Gobierno dentro de una Etapa)
// ==============================================================================
public class CheckpointCatalog
{
    public long IdCheckpoint { get; set; }
    public long IdCycle { get; set; }
    public long? IdFlow { get; set; }
    public long? TriggerStageId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Origin { get; set; } = "INTERNAL"; // INTERNAL, EXTERNAL, Data, Shirley+Dayana...
    public string Scope { get; set; } = "ENTITY"; // ENTITY, ITEM, ADVISOR, Telecom, Alarma...
    public string[] Blocks { get; set; } = Array.Empty<string>();
    public bool BlocksAdvance { get; set; } = true;
    public bool FinalizesCycle { get; set; } = false;
    public long? TriggeredByKo { get; set; } // disparaSiKoDe
    public long? RollbackToStage { get; set; } // retrocede
    public string? RollbackToCheckpointCode { get; set; }
    public int? RollbackToStepOrder { get; set; }
    public bool IsRecurrent { get; set; } = false;
    public short? RecurrenceDays { get; set; }
    public short? MaxOccurrences { get; set; }
    public string ActivationTrigger { get; set; } = "IMMEDIATE"; // IMMEDIATE, DELAYED_DAYS, SCHEDULED_DATE, CRON
    public int? DelayDays { get; set; }
    public string? PreconditionFact { get; set; }
    public string TemplateSchemaJson { get; set; } = "{}";
    public string PoliciesJson { get; set; } = "{}";
    public string ProvidersJson { get; set; } = "[\"Genérico\"]";
    public string AllowedActionsJson { get; set; } = "[]";
    public string BranchingRulesJson { get; set; } = "{}";
    public string Category { get; set; } = "GENERAL";
    public string Division { get; set; } = "OPERACIONES";
    public string OwnerDept { get; set; } = "Asesor"; // Asesor, Supervisor, Backoffice, Calidad, etc.
    public string ApprovalJobTitle { get; set; } = "SUPERVISOR";
    public string[] Satellites { get; set; } = Array.Empty<string>();
    public string Portfolio { get; set; } = "GENERAL";
    public string Campaign { get; set; } = "GENERAL";
    public string Provider { get; set; } = "INTERNO";
    public string TargetRoles { get; set; } = "SUPERVISOR,BACKOFFICE";
    public string ApprovalStatus { get; set; } = "PROPOSED";
    public string ApprovedBy { get; set; } = "[]";
    public int ExecutionOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CheckpointPoliciesDto
{
    public bool EnableHandshake { get; set; } = false; // Por defecto desactivado (opt-in)
    public bool AllowOwnerCancelBeforeAccept { get; set; } = false; // Permitir al titular cancelar antes de que el otro acepte
    public bool AllowReceptorRevertIfError { get; set; } = false; // Permitir al receptor devolver si aceptó por error
    public bool OnlyReceptorCanRevert { get; set; } = false; // Exclusividad de reversión
    public int? HandshakeTimeoutMinutes { get; set; } = null; // null = Sin timeout (solo en ciertos puntos)
    public bool RequiresSupervisorApproval { get; set; } = false; // Por defecto sin supervisor obligatorio
    public bool AutoAdvanceOnApproval { get; set; } = false; // Por defecto sin auto-avance
    public int? MaxDurationMinutes { get; set; } = null;
    public string? RequiredRole { get; set; } = null;
}

public class CheckpointStep
{
    public long IdStep { get; set; }
    public long IdCheckpoint { get; set; }
    public short StepOrder { get; set; } = 1;
    private string _name = string.Empty;
    public string Name { get => _name; set => _name = value; }
    public string Instruction { get => _name; set => _name = value; }
    public bool IsRequired { get; set; } = true;
}

// ==============================================================================
// 4. POLÍTICAS Y REGLAS DE ACTUACIÓN (Handshake, Ownership, Reversión)
// ==============================================================================
public class CyclePolicyRule
{
    public long IdRule { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public long? IdCycle { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EntityType { get; set; } = "lead_presale";
    public string ActionTrigger { get; set; } = "CALL_HANDSHAKE";
    public string RuleDefinitionJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ==============================================================================
// 5. INSTANCIAS ACTIVAS Y EJECUCIÓN
// ==============================================================================
public class CycleInstance
{
    public long IdInstance { get; set; }
    public long IdCycle { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long CurrentStageId { get; set; }
    public long? OwnerActorId { get; set; }
    public long? CurrentActorId { get; set; }
    public string HandshakeStatus { get; set; } = "NONE"; // NONE, PENDING_ACCEPTANCE, ACCEPTED, REVERTED
    public long? HandshakeTargetActorId { get; set; }
    public DateTime? HandshakeRequestedAt { get; set; }
    public int DayCounter { get; set; } = 1;
    public string Metadata { get; set; } = "{}";
    public string Facts { get; set; } = "{}";
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, PAUSED, COMPLETED, CANCELLED, CLOSED_KO
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class CheckpointInstance
{
    public long IdCpInstance { get; set; }
    public long IdInstance { get; set; }
    public long IdCheckpoint { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, KO, REJECTED, SCHEDULED
    public long? OpenedAtStage { get; set; }
    public bool IsRetroactive { get; set; }
    public short OccurrenceNumber { get; set; } = 1;
    public DateTime? ScheduledFor { get; set; }
    public long? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string AnswersJson { get; set; } = "{}";
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
    public string Direction { get; set; } = "FORWARD"; // FORWARD, BACKWARD, SKIP, TERMINAL_KO
    public string? TriggeredBy { get; set; }
    public long? ActorId { get; set; }
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;
}

public class CycleAuditLog
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

// ==============================================================================
// 6. DTOs ENRIQUECIDOS PARA CLIENTES, API Y SIMULADOR WEB
// ==============================================================================

public class CycleDefinitionDetailDto : CycleDefinition
{
    public List<CycleStageDetailDto> Stages { get; set; } = new();
    public List<CheckpointCatalogDetailDto> Checkpoints { get; set; } = new();
}

public class MetaRole
{
    public long IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExternalSystemCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MetaPortfolio
{
    public long IdPortfolio { get; set; }
    public string PortfolioCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExternalSystemCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SaveCheckpointDto : CheckpointCatalog
{
    public List<CheckpointStep>? Steps { get; set; }
}

public class CycleStageDetailDto : CycleStage
{
    public List<CheckpointCatalogDetailDto> Checkpoints { get; set; } = new();
}

public class CheckpointCatalogDetailDto : CheckpointCatalog
{
    public List<CheckpointStep> Steps { get; set; } = new();
}

public class CheckpointStepDetailDto
{
    public long IdStep { get; set; }
    public long IdCheckpoint { get; set; }
    public short StepOrder { get; set; }
    private string _name = string.Empty;
    public string Name { get => _name; set => _name = value; }
    public string Instruction { get => _name; set => _name = value; }
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
    public string Status { get; set; } = "PENDING";
    public bool BlocksAdvance { get; set; } = true;
    public bool FinalizesCycle { get; set; } = false;
    public long? TriggeredByKo { get; set; }
    public string? TriggeredByKoName { get; set; }
    public string OwnerDept { get; set; } = "Asesor";
    public string Category { get; set; } = "GENERAL";
    public string Division { get; set; } = "OPERACIONES";
    public string ProvidersJson { get; set; } = "[\"Genérico\"]";
    public long? OpenedAtStage { get; set; }
    public string? OpenedAtStageName { get; set; }
    public long? RollbackToStage { get; set; }
    public string? RollbackToStageName { get; set; }
    public bool IsRetroactive { get; set; }
    public short OccurrenceNumber { get; set; } = 1;
    public DateTime? ScheduledFor { get; set; }
    public long? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string TemplateSchemaJson { get; set; } = "{}";
    public string AnswersJson { get; set; } = "{}";
    public int ExecutionOrder { get; set; } = 1;
    public string Campaign { get; set; } = "GENERAL";
    public string Portfolio { get; set; } = "GENERAL";
    public string Provider { get; set; } = "INTERNO";
    public string ApprovalStatus { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<CheckpointStepDetailDto> Steps { get; set; } = new();
    public int TotalStepsCount => Steps.Count;
    public int CompletedStepsCount => Steps.Count(s => s.IsCompleted);
    public bool AllRequiredStepsCompleted => Steps.Where(s => s.IsRequired).All(s => s.IsCompleted);
}

public class CycleInstanceDetailDto
{
    public long IdInstance { get; set; }
    public long IdCycle { get; set; }
    public string CycleCode { get; set; } = string.Empty;
    public string CycleName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long CurrentStageId { get; set; }
    public string CurrentStageName { get; set; } = string.Empty;
    public long? OwnerActorId { get; set; }
    public long? CurrentActorId { get; set; }
    public string HandshakeStatus { get; set; } = "NONE";
    public long? HandshakeTargetActorId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string Facts { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<CycleStageDetailDto> Stages { get; set; } = new();
    public List<CheckpointInstanceDetailDto> Checkpoints { get; set; } = new();
    public List<StageTransition> Transitions { get; set; } = new();
}

// ==============================================================================
// 7. CONTRATO ZERO-LOGIC UI PARA CRM & CUALQUIER FRONTEND CLIENTE
// ==============================================================================

public class UiContextDto
{
    public long InstanceId { get; set; }
    public string CycleCode { get; set; } = string.Empty;
    public string CycleName { get; set; } = string.Empty;
    public UiStageDto CurrentStage { get; set; } = new();
    public UiOwnershipDto Ownership { get; set; } = new();
    public UiHintsDto UiHints { get; set; } = new();
    public List<CheckpointInstanceDetailDto> ActiveCheckpoints { get; set; } = new();
    public List<AllowedActionDto> AllowedActions { get; set; } = new();
    public UiTargetActorsDto TargetActors { get; set; } = new();
}

public class UiStageDto
{
    public long StageId { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short OrderIndex { get; set; }
    public short? SlaHours { get; set; }
    public bool IsTerminal { get; set; }
}

public class UiOwnershipDto
{
    public long? OwnerActorId { get; set; }
    public long? CurrentActorId { get; set; }
    public bool IsMyTurn { get; set; } = true;
    public string HandshakeStatus { get; set; } = "NONE";
    public long? HandshakeTargetActorId { get; set; }
}

public class UiHintsDto
{
    public bool IsReadOnly { get; set; } = false;
    public bool CanAdvanceStage { get; set; } = false;
    public List<string> BlockingReasons { get; set; } = new();
    public string BadgeStatus { get; set; } = "EN_GESTION";
    public string BadgeColor { get; set; } = "warning"; // success, warning, danger, primary, cyan
    public string? WarningMessage { get; set; }
}

public class AllowedActionDto
{
    public string ActionCode { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ButtonStyle { get; set; } = "btn-primary"; // btn-success, btn-danger, btn-warning, btn-secondary
    public bool RequiresConfirmation { get; set; } = false;
    public bool RequiresReason { get; set; } = false;
    public List<string> ReasonOptions { get; set; } = new();
    public bool RequiresActorSelection { get; set; } = false;
    public string Effect { get; set; } = "RESOLVE"; // ADVANCE_STAGE, TRIGGER_KO_CHAIN, ROLLBACK_STAGE, FINALIZE_CYCLE, HANDSHAKE_TRANSFER
    public string? TargetStageCode { get; set; }
    public string? TargetCheckpointCode { get; set; }
    public long? CheckpointInstanceId { get; set; }
}

public class UiTargetActorsDto
{
    public List<UiTargetActorDto> Supervisors { get; set; } = new();
    public List<UiTargetActorDto> PeerAdvisors { get; set; } = new();
    public List<UiTargetActorDto> Backoffice { get; set; } = new();
    public List<UiTargetActorDto> QualityAuditors { get; set; } = new();
}

public class UiTargetActorDto
{
    public long ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = "Asesor";
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE, BUSY, OFFLINE
}

public class ExecuteActionRequest
{
    public long ActorId { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public long? CheckpointInstanceId { get; set; }
    public string? Reason { get; set; }
    public long? TargetActorId { get; set; }
    public string? AnswersJson { get; set; } = "{}";
}

public class ExecuteActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ResultingState { get; set; } = "ACTIVE";
    public UiContextDto? UpdatedUiContext { get; set; }
    public CycleInstanceDetailDto? InstanceDetail { get; set; }
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
    public bool Success { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public bool StageCompleted { get; set; }
    public bool AutoAdvancedToNextStage { get; set; }
    public long? NextStageId { get; set; }
    public string? NextStageName { get; set; }
}

public class CycleValidationResultDto
{
    public bool CanAdvance { get; set; }
    public List<string> BlockingReasons { get; set; } = new();
    public List<CheckpointInstanceDetailDto> BlockingCheckpoints { get; set; } = new();
}

public class HandshakeActionRequest
{
    public long ActorId { get; set; }
    public long? TargetActorId { get; set; }
    public string? Reason { get; set; }
    public string? Context { get; set; }
}

public class GsiImportResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StagesProcessed { get; set; }
    public int CheckpointsImported { get; set; }
}
