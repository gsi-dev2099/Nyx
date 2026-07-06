using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("lead_pre_sale", Schema = "lead_service")]
public class LeadPreSale
{
    [Key]
    [Column("id_presale")]
    public long IdPresale { get; set; }

    [Column("id_cmpg")]
    public long IdCmpg { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("operator")]
    public string? Operator { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("province")]
    public string? Province { get; set; }

    [Column("coverage_status")]
    public string? CoverageStatus { get; set; }

    [Column("id_status")]
    public long IdStatus { get; set; }

    [Column("owner_user_id")]
    public long OwnerUserId { get; set; }

    [Column("current_user_id")]
    public long CurrentUserId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;
}