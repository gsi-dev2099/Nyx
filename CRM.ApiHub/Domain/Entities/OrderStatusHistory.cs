using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_status_history")]
public class OrderStatusHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("sales_order_id")]
    public int SalesOrderId { get; set; }

    [Column("order_status_id")]
    public int OrderStatusId { get; set; }

    [Column("changed_by_id")]
    public int ChangedById { get; set; }

    [Column("comments")]
    public string? Comments { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}