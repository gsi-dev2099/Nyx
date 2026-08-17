using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Activations;

public class GetDelayedActivationsUseCase
{
    private readonly IActivationRepository _activationRepository;

    public GetDelayedActivationsUseCase(IActivationRepository activationRepository)
    {
        _activationRepository = activationRepository;
    }

    public async Task<IEnumerable<ProductActivationTracking>> ExecuteAsync(CancellationToken ct = default)
    {
        return await _activationRepository.GetDelayedAsync(ct);
    }
}
