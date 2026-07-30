using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("product_activation_tracking", Schema = "sales_service")]
public class ProductActivationTracking
{
    [Key]
    [Column("id_tracking")]
    public long IdTracking { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("id_order_item")]
    public long IdOrderItem { get; set; }

    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [NotMapped]
    public string CustomerName { get; set; } = string.Empty;

    [NotMapped]
    public string ProviderName { get; set; } = string.Empty;

    [Column("id_provider")]
    public long? IdProvider { get; set; }

    [Column("provider_ref")]
    public string? ProviderRef { get; set; }

    [Column("order_loaded_at")]
    public DateTime? OrderLoadedAt { get; set; }

    [Column("expected_activation_date")]
    public DateTime? ExpectedActivationDate { get; set; }

    [Column("actual_activation_date")]
    public DateTime? ActualActivationDate { get; set; }

    [Column("activation_status")]
    public string ActivationStatus { get; set; } = "PENDING";

    [Column("delay_days")]
    public int? DelayDays { get; set; }

    [Column("delay_reason")]
    public string? DelayReason { get; set; }

    [Column("alert_sent_at")]
    public DateTime? AlertSentAt { get; set; }

    [Column("last_checked_at")]
    public DateTime? LastCheckedAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
