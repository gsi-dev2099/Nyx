using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("currency", Schema = "sales_service")]
public class Currency
{
    [Key]
    [Column("code")]
    [MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [Column("decimal_places")]
    public short DecimalPlaces { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("is_base")]
    public bool IsBase { get; set; }

    [Column("is_local")]
    public bool IsLocal { get; set; }
}