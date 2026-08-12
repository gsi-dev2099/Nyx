namespace Nyx.ApprovalEngine.Domain.Entities;

public class ApprovalPolicy
{
    public long IdPolicy { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = "GLOBAL"; // GLOBAL, ORGANIZATION, DIVISION, CAMPAIGN
    public long? ScopeId { get; set; }
    public bool IsActive { get; set; } = true;
    public int CurrentVersion { get; set; } = 1;
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApprovalPolicyVersion
{
    public long IdVersion { get; set; }
    public long IdPolicy { get; set; }
    public int VersionNumber { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = "{}";
    public long PublishedBy { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}

public class ApprovalChain
{
    public long IdChain { get; set; }
    public long IdPolicy { get; set; }
    public string ChainMode { get; set; } = "SEQUENTIAL"; // SEQUENTIAL, PARALLEL, ANY_ONE, UNANIMOUS
    public short? MaxSlaHours { get; set; }
    public string OnTimeoutAction { get; set; } = "ESCALATE"; // ESCALATE, AUTO_APPROVE, AUTO_REJECT
}

public class ApprovalChainStep
{
    public long IdStep { get; set; }
    public long IdChain { get; set; }
    public short StepOrder { get; set; }
    public string ApproverType { get; set; } = "ROLE"; // USER, ROLE, DIVISION, POSITION, CONDITIONAL
    public string ApproverRef { get; set; } = string.Empty;
    public string? ConditionExpr { get; set; }
    public bool CanDelegate { get; set; } = true;
    public short? SlaHours { get; set; }
    public bool IsOptional { get; set; }
}

public class ApprovalRequest
{
    public long IdRequest { get; set; }
    public long IdPolicy { get; set; }
    public int PolicyVersion { get; set; } = 1;
    public string EntityType { get; set; } = string.Empty; // "sales_order", "purchase_order"
    public long EntityId { get; set; }
    public string EntityContext { get; set; } = "{}";
    public string Status { get; set; } = "PENDING"; // PENDING, IN_PROGRESS, APPROVED, REJECTED, ESCALATED, EXPIRED, CANCELLED
    public short CurrentStep { get; set; } = 1;
    public long RequestedBy { get; set; }
    public string? CallbackUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public class ApprovalDecision
{
    public long IdDecision { get; set; }
    public long IdRequest { get; set; }
    public short StepOrder { get; set; }
    public long DecidedBy { get; set; }
    public long? OriginalApprover { get; set; }
    public string DecisionType { get; set; } = "APPROVED"; // APPROVED, REJECTED, ESCALATED
    public string? Reason { get; set; }
    public string? EvidencePath { get; set; }
    public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
}

public class ApprovalDelegation
{
    public long IdDelegation { get; set; }
    public long DelegatorId { get; set; }
    public long DelegateId { get; set; }
    public long? IdPolicy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApprovalAuditLog
{
    public long IdLog { get; set; }
    public long? IdRequest { get; set; }
    public long? IdPolicy { get; set; }
    public string Action { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string? ActorIp { get; set; }
    public string Detail { get; set; } = "{}";
    public string Checksum { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
