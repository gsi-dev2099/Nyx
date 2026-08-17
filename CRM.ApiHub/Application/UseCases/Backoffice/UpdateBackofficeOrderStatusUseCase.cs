using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Backoffice;

public class UpdateBackofficeOrderStatusUseCase
{
    private readonly IBackofficeRepository _repository;

    public UpdateBackofficeOrderStatusUseCase(IBackofficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(
        long idOrder,
        long toStatusId,
        long? toSubstatusId,
        string? comment,
        long actorId,
        CancellationToken ct = default)
    {
        return await _repository.UpdateOrderStatusAsync(idOrder, toStatusId, toSubstatusId, comment, actorId, ct);
    }
}
