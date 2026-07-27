using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("product")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_prod")]
    public int Id { get; set; }

    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("price_base")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public int StockQuantity { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}