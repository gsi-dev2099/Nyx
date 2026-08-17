using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IIncidentRepository
{
    Task<IEnumerable<IncidentCatalog>> GetCatalogAsync(long idCmpg, long idStatus);
    Task<IEnumerable<OrderIncident>> GetByOrderAsync(long idOrder);
    Task<long> CreateAsync(OrderIncident incident);
    Task CreateResponseAsync(long idOrderIncident, string responseText, string responseType, long respondedBy);
    Task ResolveAsync(long idOrderIncident, string notes, long resolvedBy);
    Task<OrderIncident?> GetByIdAsync(long id);
    Task<bool> UpdateAsync(long id, string customName, string customDescription, string? customSolution, string? assignedToRole, DateTime? dueAt);
    Task<bool> DeleteAsync(long id);
    
    Task<IEnumerable<KbArticleSuggestion>> GetKbSuggestionsAsync(long idIncident);
    Task<IEnumerable<OrderIncident>> GetFilteredAsync(string? assignedToRole, string? status, CancellationToken ct = default);
}