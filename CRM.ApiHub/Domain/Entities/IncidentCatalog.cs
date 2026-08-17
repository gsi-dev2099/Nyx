using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("incident_catalog", Schema = "sales_service")]
public class IncidentCatalog
{
    [Key]
    [Column("id_incident")]
    public long IdIncident { get; set; }
    [Column("id_cmpg")]
    public long IdCmpg { get; set; }
    [Column("id_status")]
    public long IdStatus { get; set; }
    [Column("code")]
    public string? Code { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    [Column("description")]
    public string? Description { get; set; }
    [Column("solution_template")]
    public string? SolutionTemplate { get; set; }
    [Column("resolution_type")]
    public string? ResolutionType { get; set; }
    [Column("requires_response")]
    public bool RequiresResponse { get; set; }
    [Column("is_recurrent")]
    public bool IsRecurrent { get; set; }
    [Column("priority")]
    public short Priority { get; set; }
    [Column("sla_hours")]
    public short SlaHours { get; set; }
    [Column("created_by")]
    public long CreatedBy { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}