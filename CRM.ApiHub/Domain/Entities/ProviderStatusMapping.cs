using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("provider_status_mapping", Schema = "sales_service")]
public class ProviderStatusMapping
{
    [Key]
    [Column("id_mapping")]
    public long IdMapping { get; set; }

    [Column("id_provider")]
    public long IdProvider { get; set; }

    [Column("provider_status_code")]
    public string ProviderStatusCode { get; set; } = string.Empty;

    [Column("provider_status_name")]
    public string? ProviderStatusName { get; set; }

    [Column("internal_status_id")]
    public long InternalStatusId { get; set; }

    [Column("internal_substatus_id")]
    public long? InternalSubstatusId { get; set; }

    [Column("auto_update")]
    public bool AutoUpdate { get; set; }

    [Column("creates_incident_id")]
    public long? CreatesIncidentId { get; set; }

    [Column("priority")]
    public short Priority { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
