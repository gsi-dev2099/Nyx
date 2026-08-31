namespace CRM.WebFrontend.Client.Models.Leads;

public class LeadUpdateStatusRequest
{
    public int IdStatus { get; set; }
    public string? Comment { get; set; }
}
