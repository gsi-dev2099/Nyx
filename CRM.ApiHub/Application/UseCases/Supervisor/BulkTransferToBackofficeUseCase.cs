using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.DTOs;

namespace CRM.ApiHub.Application.UseCases.Supervisor;

public class BulkTransferToBackofficeUseCase
{
    private readonly ISupervisorRepository _repository;

    public BulkTransferToBackofficeUseCase(ISupervisorRepository repository)
    {
        _repository = repository;
    }

    public async Task<BulkTransferResultDto> ExecuteAsync(
        long[] orderIds,
        long supervisorId,
        long backofficeUserId,
        string? comment,
        CancellationToken ct = default)
    {
        return await _repository.BulkTransferToBackofficeAsync(orderIds, supervisorId, backofficeUserId, comment, ct);
    }
}
