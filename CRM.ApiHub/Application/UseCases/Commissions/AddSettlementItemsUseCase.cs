using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class AddSettlementItemsUseCase
{
    private readonly ICommissionRepository _commissionRepository;

    public AddSettlementItemsUseCase(ICommissionRepository commissionRepository)
    {
        _commissionRepository = commissionRepository;
    }

    public async Task<bool> ExecuteAsync(long idSettlement, AddSettlementItemsRequestDto request, CancellationToken ct = default)
    {
        var settlement = await _commissionRepository.GetSettlementByIdAsync(idSettlement, ct);
        if (settlement == null)
        {
            throw new InvalidOperationException($"No se encontró la liquidación de comisión con ID {idSettlement}.");
        }

        if (settlement.Status.ToUpper() != "DRAFT")
        {
            throw new InvalidOperationException("Solo se pueden agregar ítems de comisión a liquidaciones en estado 'DRAFT'.");
        }

        return await _commissionRepository.AddSettlementItemsAsync(idSettlement, request.OrderIds, ct);
    }
}
