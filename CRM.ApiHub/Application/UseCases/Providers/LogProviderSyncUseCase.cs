using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Providers;

public class LogProviderSyncUseCase
{
    private readonly IProviderRepository _providerRepository;

    public LogProviderSyncUseCase(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<bool> ExecuteAsync(long idProvider, long idOrder, string statusCode, string result, CancellationToken ct = default)
    {
        return await _providerRepository.LogSyncAsync(idProvider, idOrder, statusCode, result, ct: ct);
    }
}
