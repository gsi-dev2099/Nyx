using System;

namespace CRM.WebFrontend.Client.Models;

public class PreSaleDto
{
    public long IdPresale { get; set; }
    public long IdCmpg { get; set; }
    public string? CampaignName { get; set; }
    public string? Phone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? Operator { get; set; }
    public string? TargetOperator { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? Dni { get; set; }
    public string? Province { get; set; }
    public string? CoverageStatus { get; set; }
    public long IdStatus { get; set; }
    public long OwnerUserId { get; set; }
    public long CurrentUserId { get; set; }
    public long? AssignedAdvisor1Id { get; set; }
    public long? AssignedAdvisor2Id { get; set; }
    public long? AssignedAdvisor3Id { get; set; }
    public string? OwnerUserName { get; set; }
    public string? CurrentUserName { get; set; }
    public string? AssignedAdvisor1Name { get; set; }
    public string? AssignedAdvisor2Name { get; set; }
    public string? AssignedAdvisor3Name { get; set; }
    public bool Call1Completed { get; set; }
    public bool Call2Completed { get; set; }
    public bool Call3Completed { get; set; }
    public bool RetentionCompleted { get; set; }
    public bool BotadaCompleted { get; set; }
    public bool AlternasCompleted { get; set; }
    public string AssignmentStatus { get; set; } = "NONE";
    public int AssignedCallStep { get; set; } = 0;
    public string? AssignmentRejectedReason { get; set; }
    public DateTime? AssignmentRequestedAt { get; set; }
    public DateTime? AssignmentRespondedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime Register { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public string? LastCallDataObtained { get; set; }
    public string? DiscardReason { get; set; }
    public DateTime? DiscardedAt { get; set; }
    public long? DiscardedBy { get; set; }
    public bool IsClosed => IdStatus == 4 || DiscardedAt != null || !string.IsNullOrEmpty(DiscardReason);
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
