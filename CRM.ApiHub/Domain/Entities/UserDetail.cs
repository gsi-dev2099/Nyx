using System;

namespace CRM.ApiHub.Domain.Entities;

public class UserDetail
{
    public long IdUser { get; set; }
    public string Username { get; set; } = null!;
    public string? RoleName { get; set; }
    public string? CampaignName { get; set; }
}
