using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("commission_settlement_item", Schema = "sales_service")]
public class CommissionSettlementItem
{
    [Key]
    [Column("id_item")]
    public long IdItem { get; set; }

    [Column("id_settlement")]
    public long IdSettlement { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("id_order_item")]
    public long? IdOrderItem { get; set; }

    [Column("commission_eur")]
    public decimal CommissionEur { get; set; }

    [Column("commission_pen")]
    public decimal CommissionPen { get; set; }

    [Column("product_name")]
    public string? ProductName { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}
