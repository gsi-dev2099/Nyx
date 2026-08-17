using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Audit;

public class CreateAuditUseCase
{
    private readonly IAuditRepository _auditRepository;

    public CreateAuditUseCase(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<long> ExecuteAsync(long idOrder, long idChecklist, long auditorId, CancellationToken ct = default)
    {
        return await _auditRepository.CreateAuditAsync(idOrder, idChecklist, auditorId, ct);
    }
}
