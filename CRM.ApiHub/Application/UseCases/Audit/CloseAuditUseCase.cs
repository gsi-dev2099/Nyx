using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Audit;

public class CloseAuditUseCase
{
    private readonly IAuditRepository _auditRepository;

    public CloseAuditUseCase(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<bool> ExecuteAsync(long idAudit, string status, CancellationToken ct = default)
    {
        return await _auditRepository.CloseAuditAsync(idAudit, status, ct);
    }
}
