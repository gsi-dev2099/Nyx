using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_audit_item", Schema = "sales_service")]
public class SalesOrderAuditItem
{
    [Key]
    [Column("id_audit_item")]
    public long IdAuditItem { get; set; }

    [Column("id_audit")]
    public long IdAudit { get; set; }

    [Column("id_item")]
    public long IdItem { get; set; }

    [Column("result")]
    public string Result { get; set; } = string.Empty;

    [Column("observation")]
    public string? Observation { get; set; }

    [Column("audio_timestamp")]
    public string? AudioTimestamp { get; set; }

    [Column("crm_value")]
    public string? CrmValue { get; set; }

    [Column("audio_value")]
    public string? AudioValue { get; set; }

    [Column("register")]
    public DateTime Register { get; set; }
}
