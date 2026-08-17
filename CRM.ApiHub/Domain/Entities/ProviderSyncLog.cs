using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("provider_sync_log", Schema = "sales_service")]
public class ProviderSyncLog
{
    [Key]
    [Column("id_sync")]
    public long IdSync { get; set; }

    [Column("sync_date")]
    public DateTime SyncDate { get; set; }

    [Column("id_provider")]
    public long IdProvider { get; set; }

    [Column("id_order")]
    public long? IdOrder { get; set; }

    [Column("sync_type")]
    public string SyncType { get; set; } = "STATUS_UPDATE";

    [Column("provider_status_code")]
    public string? ProviderStatusCode { get; set; }

    [Column("internal_status_before")]
    public long? InternalStatusBefore { get; set; }

    [Column("internal_status_after")]
    public long? InternalStatusAfter { get; set; }

    [Column("success")]
    public bool Success { get; set; }

    [Column("error_detail")]
    public string? ErrorDetail { get; set; }

    [Column("executed_by")]
    public string ExecutedBy { get; set; } = "SYSTEM";

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;
}
