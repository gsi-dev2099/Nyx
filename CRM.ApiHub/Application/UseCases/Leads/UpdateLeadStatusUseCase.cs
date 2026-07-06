using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class UpdateLeadStatusUseCase
{
    private readonly ILeadRepository _leadRepository;

    public UpdateLeadStatusUseCase(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<bool> ExecuteAsync(long idLead, LeadUpdateStatusDto dto, long actorId, CancellationToken ct = default)
    {
        return await _leadRepository.UpdateStatusAsync(idLead, dto.IdStatus, dto.Comment, actorId, ct);
    }
}
