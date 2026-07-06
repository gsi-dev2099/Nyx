using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

public class Currency
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("iso_code")] 
    public string IsoCode { get; set; } = string.Empty;

    [Column("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; }
}