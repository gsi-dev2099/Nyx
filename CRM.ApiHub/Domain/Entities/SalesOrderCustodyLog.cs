using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_custody_log")]
public class SalesOrderCustodyLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("sales_order_id")]
    public int SalesOrderId { get; set; }

    [Column("previous_custodian_id")]
    public int? PreviousCustodianId { get; set; }

    [Column("current_custodian_id")]
    public int CurrentCustodianId { get; set; }

    [Column("observations")]
    public string? Observations { get; set; }

    [Column("action_date")]
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}