using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Providers;

public class GetProviderStatusMappingUseCase
{
    private readonly IProviderRepository _providerRepository;

    public GetProviderStatusMappingUseCase(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<IEnumerable<ProviderStatusMapping>> ExecuteAsync(long idProvider, CancellationToken ct = default)
    {
        return await _providerRepository.GetStatusMappingAsync(idProvider, ct);
    }
}
