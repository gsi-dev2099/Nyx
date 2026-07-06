namespace CRM.ApiHub.Application.DTOs;

public class LeadCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public long? IdCmpg { get; set; }
    public long IdSrc { get; set; }
    public string? DocumentNumber { get; set; }
    public string? RawData { get; set; }
    public long? AssignedUserId { get; set; }
}
