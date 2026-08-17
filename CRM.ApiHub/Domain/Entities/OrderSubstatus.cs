using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_substatus", Schema = "sales_service")]
public class OrderSubstatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_substatus")]
    public int Id { get; set; }

    [Column("id_status")]
    public int OrderStatusId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("color")]
    public string? Color { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("order_index")]
    public short OrderIndex { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}