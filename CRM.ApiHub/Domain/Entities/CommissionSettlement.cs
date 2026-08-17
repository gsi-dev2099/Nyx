using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("commission_settlement", Schema = "sales_service")]
public class CommissionSettlement
{
    [Key]
    [Column("id_settlement")]
    public long IdSettlement { get; set; }

    [Column("id_user")]
    public long IdUser { get; set; }

    [Column("period_start")]
    public DateTime PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime PeriodEnd { get; set; }

    [Column("settlement_date")]
    public DateTime SettlementDate { get; set; }

    [Column("total_eur")]
    public decimal TotalEur { get; set; }

    [Column("total_pen")]
    public decimal TotalPen { get; set; }

    [Column("exchange_rate_id")]
    public long? ExchangeRateId { get; set; }

    [Column("exchange_rate_applied")]
    public decimal? ExchangeRateApplied { get; set; }

    [Column("total_orders")]
    public int TotalOrders { get; set; }

    [Column("total_products")]
    public int TotalProducts { get; set; }

    [Column("status")]
    public string Status { get; set; } = "DRAFT";

    [Column("approved_by")]
    public long? ApprovedBy { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
