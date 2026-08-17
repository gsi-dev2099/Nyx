using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_status", Schema = "sales_service")]
public class OrderStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_status")]
    public int Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Column("color")]
    public string? Color { get; set; }

    [Column("icon")]
    public string? Icon { get; set; }

    [Column("is_terminal")]
    public bool IsTerminal { get; set; }

    [Column("requires_substatus")]
    public bool RequiresSubstatus { get; set; }

    [Column("requires_comment")]
    public bool RequiresComment { get; set; }

    [Column("allows_edit_by_asesor")]
    public bool AllowsEditByAsesor { get; set; }

    [Column("allows_edit_by_supervisor")]
    public bool AllowsEditBySupervisor { get; set; }

    [Column("order_index")]
    public short OrderIndex { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}