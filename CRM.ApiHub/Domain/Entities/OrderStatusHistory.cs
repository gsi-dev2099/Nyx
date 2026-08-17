using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_status_history", Schema = "sales_service")]
public class OrderStatusHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_history")]
    public long IdHistory { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("order_date")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;

    [Column("from_status_id")]
    public long? FromStatusId { get; set; }

    [Column("to_status_id")]
    public long ToStatusId { get; set; }

    [Column("from_substatus_id")]
    public long? FromSubstatusId { get; set; }

    [Column("to_substatus_id")]
    public long? ToSubstatusId { get; set; }

    [Column("changed_by")]
    public long ChangedBy { get; set; }

    [Column("is_bulk")]
    public bool IsBulk { get; set; } = false;

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("metadata")]
    public string Metadata { get; set; } = "{}";

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;
}