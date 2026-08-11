using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order", Schema = "sales_service")]
public class SalesOrder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("id_lead")]
    public long IdLead { get; set; }

    [Column("id_cmpg")]
    public long IdCmpg { get; set; }

    [Column("id_user")]
    public long IdUser { get; set; }

    [Column("owner_user_id")]
    public long? OwnerUserId { get; set; }

    [Column("custody_user_id")]
    public long? CustodyUserId { get; set; }

    [Column("id_status")]
    public long? IdStatus { get; set; }

    [Column("id_substatus")]
    public long? IdSubstatus { get; set; }

    [Column("currency_code")]
    public string? CurrencyCode { get; set; } = "EUR";

    [Column("commission_currency")]
    public string? CommissionCurrency { get; set; } = "PEN";

    [Column("status")]
    public string? Status { get; set; } = "PENDING_VALIDATION";

    [Column("sales_date")]
    public DateTime SalesDate { get; set; } = DateTime.UtcNow;

    [Column("total_products")]
    public int TotalProducts { get; set; } = 0;

    [Column("total_value")]
    public decimal? TotalValue { get; set; }

    [Column("is_alternate")]
    public bool IsAlternate { get; set; } = false;

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;

    [Column("last_update")]
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public int SlaMinutes { get; set; }
}