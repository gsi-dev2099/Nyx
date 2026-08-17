using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Providers;

public class GetProviderCatalogUseCase
{
    private readonly IProviderRepository _providerRepository;

    public GetProviderCatalogUseCase(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<IEnumerable<ProviderCatalog>> ExecuteAsync(CancellationToken ct = default)
    {
        return await _providerRepository.GetCatalogAsync(ct);
    }
}
