using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models.Leads;

namespace CRM.WebFrontend.Client.Services;

public interface ILeadService
{
    Task<IEnumerable<LeadResponse>> GetLeadsAsync(int page, int limit, string? searchTerm = null);
    Task<bool> TakeCustodyAsync(long idLead);
}
