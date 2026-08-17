using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("provider_catalog", Schema = "sales_service")]
public class ProviderCatalog
{
    [Key]
    [Column("id_provider")]
    public long IdProvider { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("integration_type")]
    public string IntegrationType { get; set; } = string.Empty;

    [Column("api_base_url")]
    public string? ApiBaseUrl { get; set; }

    [Column("api_version")]
    public string? ApiVersion { get; set; }

    [Column("rpa_config")]
    public string RpaConfig { get; set; } = "{}";

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
