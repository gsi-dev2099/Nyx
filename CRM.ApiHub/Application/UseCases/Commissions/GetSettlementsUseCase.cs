using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Commissions;

public class GetSettlementsUseCase
{
    private readonly ICommissionRepository _commissionRepository;

    public GetSettlementsUseCase(ICommissionRepository commissionRepository)
    {
        _commissionRepository = commissionRepository;
    }

    public async Task<IEnumerable<SettlementResponseDto>> GetListAsync(long? userId, CancellationToken ct = default)
    {
        var list = userId.HasValue 
            ? await _commissionRepository.GetSettlementsByUserAsync(userId.Value, ct)
            : await _commissionRepository.GetSettlementsAsync(ct);

        return list.Select(s => new SettlementResponseDto
        {
            IdSettlement = s.IdSettlement,
            IdUser = s.IdUser,
            PeriodStart = s.PeriodStart,
            PeriodEnd = s.PeriodEnd,
            SettlementDate = s.SettlementDate,
            TotalEur = s.TotalEur,
            TotalPen = s.TotalPen,
            ExchangeRateId = s.ExchangeRateId,
            ExchangeRateApplied = s.ExchangeRateApplied,
            TotalOrders = s.TotalOrders,
            TotalProducts = s.TotalProducts,
            Status = s.Status,
            ApprovedBy = s.ApprovedBy,
            ApprovedAt = s.ApprovedAt,
            PaidAt = s.PaidAt,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt
        });
    }

    public async Task<(SettlementResponseDto Settlement, IEnumerable<SettlementItemResponseDto> Items)?> GetByIdAsync(long idSettlement, CancellationToken ct = default)
    {
        var s = await _commissionRepository.GetSettlementByIdAsync(idSettlement, ct);
        if (s == null) return null;

        var items = await _commissionRepository.GetSettlementItemsAsync(idSettlement, ct);

        var settlementDto = new SettlementResponseDto
        {
            IdSettlement = s.IdSettlement,
            IdUser = s.IdUser,
            PeriodStart = s.PeriodStart,
            PeriodEnd = s.PeriodEnd,
            SettlementDate = s.SettlementDate,
            TotalEur = s.TotalEur,
            TotalPen = s.TotalPen,
            ExchangeRateId = s.ExchangeRateId,
            ExchangeRateApplied = s.ExchangeRateApplied,
            TotalOrders = s.TotalOrders,
            TotalProducts = s.TotalProducts,
            Status = s.Status,
            ApprovedBy = s.ApprovedBy,
            ApprovedAt = s.ApprovedAt,
            PaidAt = s.PaidAt,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt
        };

        var itemsDto = items.Select(i => new SettlementItemResponseDto
        {
            IdItem = i.IdItem,
            IdSettlement = i.IdSettlement,
            IdOrder = i.IdOrder,
            IdOrderItem = i.IdOrderItem,
            CommissionEur = i.CommissionEur,
            CommissionPen = i.CommissionPen,
            ProductName = i.ProductName,
            Notes = i.Notes
        });

        return (settlementDto, itemsDto);
    }
}
