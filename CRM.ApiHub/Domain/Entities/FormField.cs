using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("sales_form_field", Schema = "sales_service")]
public class FormField
{
    [Key]
    [Column("id_fld")]
    public long IdFld { get; set; }

    [Column("id_form")]
    public long IdForm { get; set; }

    [Column("label")]
    public string Label { get; set; } = string.Empty;

    [Column("field_key")]
    public string FieldKey { get; set; } = string.Empty;

    [Column("field_type")]
    public string FieldType { get; set; } = string.Empty;

    [Column("is_required")]
    public bool IsRequired { get; set; }

    [Column("validation_regex")]
    public string? ValidationRegex { get; set; }

    [Column("options")]
    public string? Options { get; set; } // jsonb se mapea como string en Dapper

    [Column("order_index")]
    public int OrderIndex { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("validation_type")]
    public string? ValidationType { get; set; }

    [Column("source_table")]
    public string? SourceTable { get; set; }

    [Column("source_filter")]
    public string? SourceFilter { get; set; } // jsonb

    [Column("placeholder")]
    public string? Placeholder { get; set; }

    [Column("help_text")]
    public string? HelpText { get; set; }

    [Column("group_name")]
    public string? GroupName { get; set; }

    [Column("depends_on_field")]
    public long? DependsOnField { get; set; }

    [Column("depends_on_value")]
    public string? DependsOnValue { get; set; }

    [Column("is_data_real_flag")]
    public bool? IsDataRealFlag { get; set; }
}