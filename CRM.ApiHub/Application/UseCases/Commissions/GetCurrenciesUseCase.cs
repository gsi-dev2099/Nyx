using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class GetCurrenciesUseCase
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetCurrenciesUseCase(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<IEnumerable<Currency>> ExecuteAsync(CancellationToken ct = default)
    {
        return await _currencyRepository.GetCurrenciesAsync(ct);
    }
}
