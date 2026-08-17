using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class DeleteSettlementUseCase
{
    private readonly ICommissionRepository _commissionRepository;

    public DeleteSettlementUseCase(ICommissionRepository commissionRepository)
    {
        _commissionRepository = commissionRepository;
    }

    public async Task<bool> ExecuteAsync(long idSettlement, CancellationToken ct = default)
    {
        var settlement = await _commissionRepository.GetSettlementByIdAsync(idSettlement, ct);
        if (settlement == null)
        {
            throw new InvalidOperationException($"No se encontró la liquidación de comisión con ID {idSettlement}.");
        }

        if (settlement.Status.ToUpper() != "DRAFT")
        {
            throw new InvalidOperationException("Solo se pueden eliminar liquidaciones en estado 'DRAFT'.");
        }

        return await _commissionRepository.DeleteSettlementAsync(idSettlement, ct);
    }
}
