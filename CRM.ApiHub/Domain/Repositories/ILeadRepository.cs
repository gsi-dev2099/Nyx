using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public class LeadFilters
{
    public string? SearchTerm { get; set; }
    public long? StatusId { get; set; }
    public int? Page { get; set; }
    public int? Limit { get; set; }
}

public interface ILeadRepository
{
    Task<IEnumerable<Lead>> GetByAssignedUserAsync(long userId, LeadFilters? filters = null, CancellationToken ct = default);
    Task<Lead?> GetByIdAsync(long idLead, CancellationToken ct = default);
    Task<long> CreateAsync(Lead lead, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(long idLead, int idStatus, string? comment, long actorId, CancellationToken ct = default);
}
