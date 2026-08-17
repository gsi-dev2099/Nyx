using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Activations;

public class UpdateActivationUseCase
{
    private readonly IActivationRepository _activationRepository;

    public UpdateActivationUseCase(IActivationRepository activationRepository)
    {
        _activationRepository = activationRepository;
    }

    public async Task<bool> ExecuteAsync(
        long idTracking, 
        string status, 
        DateTime? actualDate, 
        CancellationToken ct = default)
    {
        var tracking = await _activationRepository.GetByIdAsync(idTracking, ct);
        if (tracking == null)
        {
            return false;
        }

        return await _activationRepository.UpdateActivationAsync(
            idTracking, 
            status, 
            actualDate, 
            ct
        );
    }
}
