using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class CreateSettlementUseCase
{
    private readonly ICommissionRepository _commissionRepository;

    public CreateSettlementUseCase(ICommissionRepository commissionRepository)
    {
        _commissionRepository = commissionRepository;
    }

    public async Task<long> ExecuteAsync(CreateSettlementRequestDto request, CancellationToken ct = default)
    {
        if (request.PeriodStart > request.PeriodEnd)
        {
            throw new ArgumentException("La fecha de inicio del periodo no puede ser posterior a la fecha de fin.");
        }

        return await _commissionRepository.CreateSettlementAsync(request.UserId, request.PeriodStart, request.PeriodEnd, ct);
    }
}
