using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

public interface ICommissionService
{
    /// <summary>GET /api/currencies</summary>
    Task<List<CurrencyDto>> GetCurrenciesAsync();

    /// <summary>GET /api/currencies/convert?from={from}&to={to}&amount={amount}</summary>
    Task<ConvertAmountResponseDto?> ConvertAmountAsync(string from, string to, decimal amount);

    /// <summary>GET /api/commissions/settlements?userId={userId}</summary>
    Task<List<SettlementResponseDto>> GetSettlementsAsync(long? userId = null);

    /// <summary>GET /api/commissions/settlements/{id}</summary>
    Task<SettlementDetailResponseDto?> GetSettlementDetailAsync(long idSettlement);
}
