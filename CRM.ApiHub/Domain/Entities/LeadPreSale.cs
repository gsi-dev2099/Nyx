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

    [Column("target_operator")]
    public string? TargetOperator { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("province")]
    public string? Province { get; set; }

    [Column("dni")]
    public string? Dni { get; set; }

    [Column("coverage_status")]
    public string? CoverageStatus { get; set; }

    [Column("id_status")]
    public long IdStatus { get; set; }

    [Column("owner_user_id")]
    public long OwnerUserId { get; set; }

    [Column("current_user_id")]
    public long CurrentUserId { get; set; }

    [Column("assigned_advisor_1_id")]
    public long? AssignedAdvisor1Id { get; set; }

    [Column("assigned_advisor_2_id")]
    public long? AssignedAdvisor2Id { get; set; }

    [Column("assigned_advisor_3_id")]
    public long? AssignedAdvisor3Id { get; set; }

    [NotMapped]
    public string? OwnerUserName { get; set; }

    [NotMapped]
    public string? CurrentUserName { get; set; }

    [NotMapped]
    public string? AssignedAdvisor1Name { get; set; }

    [NotMapped]
    public string? AssignedAdvisor2Name { get; set; }

    [NotMapped]
    public string? AssignedAdvisor3Name { get; set; }

    [NotMapped]
    public string? CampaignName { get; set; }

    [NotMapped]
    public bool Call1Completed { get; set; }

    [NotMapped]
    public bool Call2Completed { get; set; }

    [NotMapped]
    public bool Call3Completed { get; set; }

    [NotMapped]
    public bool RetentionCompleted { get; set; }

    [NotMapped]
    public bool BotadaCompleted { get; set; }

    [NotMapped]
    public bool AlternasCompleted { get; set; }

    [Column("assignment_status")]
    public string AssignmentStatus { get; set; } = "NONE";

    [Column("assigned_call_step")]
    public int AssignedCallStep { get; set; } = 0;

    [Column("assignment_rejected_reason")]
    public string? AssignmentRejectedReason { get; set; }

    [Column("assignment_requested_at")]
    public DateTime? AssignmentRequestedAt { get; set; }

    [Column("assignment_responded_at")]
    public DateTime? AssignmentRespondedAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;

    [Column("last_activity_at")]
    public DateTime? LastActivityAt { get; set; }

    [Column("discard_reason")]
    public string? DiscardReason { get; set; }

    [Column("discarded_at")]
    public DateTime? DiscardedAt { get; set; }

    [Column("discarded_by")]
    public long? DiscardedBy { get; set; }

    [NotMapped]
    public bool IsClosed => IdStatus == 4 || DiscardedAt != null || !string.IsNullOrEmpty(DiscardReason);

    [NotMapped]
    public string? LastCallDataObtained { get; set; }
}

public class PreSaleAssignmentHistoryDto
{
    public long IdLog { get; set; }
    public int IdPresale { get; set; }
    public long FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public long ToUserId { get; set; }
    public string? ToUserName { get; set; }
    public int CallStep { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}