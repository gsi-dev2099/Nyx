using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class GetLeadByIdUseCase
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadByIdUseCase(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Lead?> ExecuteAsync(long idLead, CancellationToken ct = default)
    {
        return await _leadRepository.GetByIdAsync(idLead, ct);
    }
}
