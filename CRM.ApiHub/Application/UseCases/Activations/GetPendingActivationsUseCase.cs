using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Activations;

public class GetPendingActivationsUseCase
{
    private readonly IActivationRepository _activationRepository;

    public GetPendingActivationsUseCase(IActivationRepository activationRepository)
    {
        _activationRepository = activationRepository;
    }

    public async Task<IEnumerable<ProductActivationTracking>> ExecuteAsync(long idProvider, CancellationToken ct = default)
    {
        return await _activationRepository.GetPendingActivationsAsync(idProvider, ct);
    }
}
