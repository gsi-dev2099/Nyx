using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_order_data", Schema = "sales_service")]
public class OrderData
{
    [Key]
    [Column("id_orddata")]
    public long IdOrddata { get; set; }

    [Column("id_order")]
    public long IdOrder { get; set; }

    [Column("id_fld")]
    public long IdFld { get; set; }

    [Column("value_text")]
    public string? ValueText { get; set; }

    [Column("value_json")]
    public string? ValueJson { get; set; } // jsonb

    [Column("field_status")]
    public string? FieldStatus { get; set; }

    [Column("validated_by")]
    public long? ValidatedBy { get; set; }

    [Column("validated_at")]
    public DateTime? ValidatedAt { get; set; }

    [Column("version")]
    public short Version { get; set; }

    [Column("source_form_id")]
    public long SourceFormId { get; set; }

    [Column("register")]
    public DateTime Register { get; set; }
}