using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Activations;

public class GetActivationsByOrderUseCase
{
    private readonly IActivationRepository _activationRepository;

    public GetActivationsByOrderUseCase(IActivationRepository activationRepository)
    {
        _activationRepository = activationRepository;
    }

    public async Task<IEnumerable<ProductActivationTracking>> ExecuteAsync(long idOrder, CancellationToken ct = default)
    {
        return await _activationRepository.GetByOrderAsync(idOrder, ct);
    }
}
