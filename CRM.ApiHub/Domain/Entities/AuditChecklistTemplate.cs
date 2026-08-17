using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("audit_checklist_template", Schema = "sales_service")]
public class AuditChecklistTemplate
{
    [Key]
    [Column("id_checklist")]
    public long IdChecklist { get; set; }

    [Column("id_cmpg")]
    public long IdCmpg { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("version")]
    public string Version { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("deprecated_at")]
    public DateTime? DeprecatedAt { get; set; }
}
