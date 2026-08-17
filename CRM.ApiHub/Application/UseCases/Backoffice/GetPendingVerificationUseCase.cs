using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Backoffice;

public class GetPendingVerificationUseCase
{
    private readonly IBackofficeRepository _repository;

    public GetPendingVerificationUseCase(IBackofficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderDocument>> ExecuteAsync(long backofficeId, CancellationToken ct = default)
    {
        return await _repository.GetPendingVerificationAsync(backofficeId, ct);
    }
}
