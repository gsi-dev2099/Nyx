using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_form_template", Schema = "sales_service")]
public class FormTemplate
{
    [Key]
    [Column("id_form")]
    public long IdForm { get; set; }

    [Column("id_cmpg")]
    public long IdCmpg { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("register")]
    public DateTime Register { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("id_stage")]
    public long IdStage { get; set; }

    [Column("form_order")]
    public short FormOrder { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("allows_partial")]
    public bool AllowsPartial { get; set; }

    [Column("min_completion_pct")]
    public short MinCompletionPct { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}