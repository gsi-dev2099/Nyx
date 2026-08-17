using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_audit", Schema = "sales_service")]
public class SalesOrderAudit
{
    [Key]
    [Column("id_audit")]
    public long IdAudit { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("id_checklist")]
    public long IdChecklist { get; set; }

    [Column("audit_status")]
    public string AuditStatus { get; set; } = "IN_PROGRESS";

    [Column("audited_by")]
    public long AuditedBy { get; set; }

    [Column("audio_path")]
    public string? AudioPath { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
