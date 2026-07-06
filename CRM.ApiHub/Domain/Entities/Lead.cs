using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("lead", Schema = "lead_service")]
public class Lead
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id_lead")]
    public long IdLead { get; set; }

    [Column("id_cmpg")]
    public long? IdCmpg { get; set; }

    [Column("id_src")]
    public long IdSrc { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("document_number")]
    public string? DocumentNumber { get; set; }

    [Column("raw_data")]
    public string RawData { get; set; } = "{}";

    [Column("current_status_id")]
    public long CurrentStatusId { get; set; } = 1;

    [Column("assigned_user_id")]
    public long? AssignedUserId { get; set; }

    [Column("owner_user_id")]
    public long? OwnerUserId { get; set; }

    [Column("custody_user_id")]
    public long? CustodyUserId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;

    [Column("last_update")]
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}