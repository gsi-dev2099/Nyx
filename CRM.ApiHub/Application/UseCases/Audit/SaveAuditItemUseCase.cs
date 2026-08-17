using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Audit;

public class SaveAuditItemUseCase
{
    private readonly IAuditRepository _auditRepository;

    public SaveAuditItemUseCase(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<bool> ExecuteAsync(long idAudit, long idItem, string result, string? obs, string? audioTimestamp, CancellationToken ct = default)
    {
        return await _auditRepository.SaveItemAsync(idAudit, idItem, result, obs, audioTimestamp, ct);
    }
}
