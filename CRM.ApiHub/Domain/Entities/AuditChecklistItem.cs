using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("audit_checklist_item", Schema = "sales_service")]
public class AuditChecklistItem
{
    [Key]
    [Column("id_item")]
    public long IdItem { get; set; }

    [Column("id_checklist")]
    public long IdChecklist { get; set; }

    [Column("order_index")]
    public short OrderIndex { get; set; }

    [Column("item_type")]
    public string ItemType { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("expected_text")]
    public string? ExpectedText { get; set; }

    [Column("crm_field_key")]
    public string? CrmFieldKey { get; set; }

    [Column("is_critical")]
    public bool IsCritical { get; set; }

    [Column("ko_incident_id")]
    public long? KoIncidentId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}
