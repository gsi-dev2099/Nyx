using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IBackofficeRepository
{
    Task<CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>> GetAssignedOrdersAsync(
        long backofficeId,
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IEnumerable<OrderDocument>> GetPendingVerificationAsync(long backofficeId, CancellationToken ct = default);

    Task<bool> UpdateOrderStatusAsync(
        long idOrder,
        long toStatusId,
        long? toSubstatusId,
        string? comment,
        long actorId,
        CancellationToken ct = default);

    Task<bool> VerifyDocumentAsync(
        long idDoc,
        string status,
        string? notes,
        long verifiedBy,
        CancellationToken ct = default);
}
