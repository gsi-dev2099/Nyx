using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_incident", Schema = "sales_service")]
public class OrderIncident
{
    [Key]
    [Column("id_order_incident")]
    public long IdOrderIncident { get; set; }
    [Column("id_order")]
    public long IdOrder { get; set; }
    [Column("id_incident")]
    public long IdIncident { get; set; }
    [Column("custom_name")]
    public string? CustomName { get; set; }
    [Column("custom_description")]
    public string? CustomDescription { get; set; }
    [Column("custom_solution")]
    public string? CustomSolution { get; set; }
    [Column("incident_status")]
    public string? IncidentStatus { get; set; } = "OPEN";
    [Column("detected_by")]
    public long DetectedBy { get; set; }
    [Column("assigned_to_role")]
    public string? AssignedToRole { get; set; } = "BACKOFFICE";
    [Column("resolved_by")]
    public long? ResolvedBy { get; set; }
    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }
    [Column("resolution_notes")]
    public string? ResolutionNotes { get; set; }
    [Column("escalated_by")]
    public long? EscalatedBy { get; set; }
    [Column("escalated_at")]
    public DateTime? EscalatedAt { get; set; }
    [Column("escalation_reason")]
    public string? EscalationReason { get; set; }
    [Column("due_at")]
    public DateTime? DueAt { get; set; }
    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public short? Priority { get; set; }
}