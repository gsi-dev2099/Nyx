using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class GetLeadsUseCase
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadsUseCase(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<IEnumerable<Lead>> ExecuteAsync(long userId, LeadFilters? filters = null, CancellationToken ct = default)
    {
        return await _leadRepository.GetByAssignedUserAsync(userId, filters, ct);
    }
}
