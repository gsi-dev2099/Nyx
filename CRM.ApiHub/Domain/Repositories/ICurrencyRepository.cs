using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetCurrenciesAsync(CancellationToken ct = default);
    Task<decimal> GetExchangeRateAsync(string from, string to, DateTime date, CancellationToken ct = default);
    Task<decimal> ConvertAmountAsync(decimal amount, string from, string to, DateTime date, CancellationToken ct = default);
}
