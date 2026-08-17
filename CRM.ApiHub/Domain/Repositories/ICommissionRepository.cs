using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface ICommissionRepository
{
    Task<IEnumerable<CommissionSettlement>> GetSettlementsAsync(CancellationToken ct = default);
    Task<IEnumerable<CommissionSettlement>> GetSettlementsByUserAsync(long userId, CancellationToken ct = default);
    Task<CommissionSettlement?> GetSettlementByIdAsync(long idSettlement, CancellationToken ct = default);
    Task<IEnumerable<CommissionSettlementItem>> GetSettlementItemsAsync(long idSettlement, CancellationToken ct = default);
    Task<long> CreateSettlementAsync(long userId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    Task<bool> AddSettlementItemsAsync(long idSettlement, long[] orderIds, CancellationToken ct = default);
    Task<bool> UpdateSettlementStatusAsync(long idSettlement, string status, long? actorUserId, string? notes, CancellationToken ct = default);
    Task<bool> DeleteSettlementAsync(long idSettlement, CancellationToken ct = default);
}
