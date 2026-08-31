using System;

namespace CRM.WebFrontend.Client.Models.Leads;

public class LeadResponse
{
    public long IdLead { get; set; }
    public long? IdCmpg { get; set; }
    public long IdSrc { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? DocumentNumber { get; set; }
    public long CurrentStatusId { get; set; }
    public long? AssignedUserId { get; set; }
    public long? OwnerUserId { get; set; }
    public long? CustodyUserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime Register { get; set; }
}
