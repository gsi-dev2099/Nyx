namespace Nyx.SlaEngine.Domain.Entities;

public class SlaPolicy
{
    public long IdPolicy { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScopeType { get; set; } = "GLOBAL"; // GLOBAL, CAMPAIGN, DIVISION, USER
    public long? ScopeId { get; set; }
    public int TargetMinutes { get; set; }
    public short WarningPct { get; set; } = 75;
    public short CriticalPct { get; set; } = 100;
    public short? EscalationPct { get; set; } = 120;
    public string AppliesTo { get; set; } = "ORDER"; // ORDER, LEAD, INCIDENT, CUSTOM
    public bool IsActive { get; set; } = true;
    public long CreatedBy { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkCalendar
{
    public long IdCalendar { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = "America/Lima";
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkSchedule
{
    public long IdSchedule { get; set; }
    public long IdCalendar { get; set; }
    public short DayOfWeek { get; set; } // 0=Sunday..6=Saturday
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class Holiday
{
    public long IdHoliday { get; set; }
    public long IdCalendar { get; set; }
    public DateTime HolidayDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsHalfDay { get; set; }
}

public class SlaMeasurement
{
    public long IdMeasurement { get; set; }
    public long IdPolicy { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long? OwnerUserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PausedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int ElapsedMinutes { get; set; }
    public string Status { get; set; } = "RUNNING"; // RUNNING, PAUSED, WARNING, BREACHED, COMPLETED
    public DateTime? BreachAt { get; set; }
    public string Metadata { get; set; } = "{}";
}

public class SlaAlert
{
    public long IdAlert { get; set; }
    public long IdMeasurement { get; set; }
    public string AlertLevel { get; set; } = "WARNING"; // WARNING, CRITICAL, BREACH, ESCALATED
    public long? NotifiedTo { get; set; }
    public bool CallbackSent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SlaAuditLog
{
    public long IdLog { get; set; }
    public long? IdMeasurement { get; set; }
    public long? IdPolicy { get; set; }
    public string Action { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string Detail { get; set; } = "{}";
    public string Checksum { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
