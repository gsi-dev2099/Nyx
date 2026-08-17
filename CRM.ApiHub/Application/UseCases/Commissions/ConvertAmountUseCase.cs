using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class ConvertAmountUseCase
{
    private readonly ICurrencyRepository _currencyRepository;

    public ConvertAmountUseCase(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<ConvertAmountResponseDto> ExecuteAsync(ConvertAmountRequestDto request, CancellationToken ct = default)
    {
        var targetDate = request.Date ?? DateTime.UtcNow;
        var rate = await _currencyRepository.GetExchangeRateAsync(request.From, request.To, targetDate, ct);
        var converted = request.Amount * rate;

        return new ConvertAmountResponseDto
        {
            From = request.From.ToUpper(),
            To = request.To.ToUpper(),
            OriginalAmount = request.Amount,
            ConvertedAmount = converted,
            ExchangeRate = rate,
            Date = targetDate
        };
    }
}
