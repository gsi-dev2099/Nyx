using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class UpdateSettlementStatusUseCase
{
    private readonly ICommissionRepository _commissionRepository;

    public UpdateSettlementStatusUseCase(ICommissionRepository commissionRepository)
    {
        _commissionRepository = commissionRepository;
    }

    public async Task<bool> ExecuteAsync(long idSettlement, UpdateSettlementRequestDto request, long? actorUserId, CancellationToken ct = default)
    {
        var settlement = await _commissionRepository.GetSettlementByIdAsync(idSettlement, ct);
        if (settlement == null)
        {
            throw new InvalidOperationException($"No se encontró la liquidación de comisión con ID {idSettlement}.");
        }

        var currentStatus = settlement.Status.ToUpper();
        var targetStatus = request.Status.ToUpper();

        // Validate workflow transitions
        if (targetStatus == "APPROVED" && currentStatus != "DRAFT")
        {
            throw new InvalidOperationException("Solo se pueden aprobar liquidaciones que estén en estado 'DRAFT'.");
        }

        if (targetStatus == "PAID" && currentStatus != "APPROVED")
        {
            throw new InvalidOperationException("Solo se pueden pagar liquidaciones que ya estén en estado 'APPROVED'.");
        }

        if (targetStatus == "DRAFT")
        {
            throw new InvalidOperationException("No se puede revertir una liquidación a estado 'DRAFT'.");
        }

        return await _commissionRepository.UpdateSettlementStatusAsync(idSettlement, targetStatus, actorUserId, request.Notes, ct);
    }
}
