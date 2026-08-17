using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_custody_log", Schema = "sales_service")]
public class SalesOrderCustodyLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_log")]
    public long IdLog { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("log_date")]
    public DateTime LogDate { get; set; }

    [Column("from_user_id")]
    public long? FromUserId { get; set; }

    [Column("to_user_id")]
    public long ToUserId { get; set; }

    [Column("from_role")]
    public string? FromRole { get; set; }

    [Column("to_role")]
    public string? ToRole { get; set; }

    [Column("transfer_type")]
    public string TransferType { get; set; } = string.Empty;

    [Column("id_status_at")]
    public long? IdStatusAt { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("is_bulk")]
    public bool IsBulk { get; set; }

    [Column("batch_id")]
    public Guid? BatchId { get; set; }

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;
}