using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("exchange_rate", Schema = "sales_service")]
public class ExchangeRate
{
    [Key]
    [Column("id_rate")]
    public long IdRate { get; set; }

    [Column("from_currency")]
    public string FromCurrency { get; set; } = string.Empty;

    [Column("to_currency")]
    public string ToCurrency { get; set; } = string.Empty;

    [Column("rate")]
    public decimal Rate { get; set; }

    [Column("valid_from")]
    public DateTime ValidFrom { get; set; }

    [Column("valid_to")]
    public DateTime? ValidTo { get; set; }

    [Column("source")]
    public string Source { get; set; } = "SYSTEM";

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
