using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("order_document")]
public class OrderDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("sales_order_id")]
    public int SalesOrderId { get; set; }

    [Column("document_name")]
    public string DocumentName { get; set; } = string.Empty;

    [Column("document_url")]
    public string DocumentUrl { get; set; } = string.Empty;

    [Column("uploaded_by_id")]
    public int UploadedById { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}