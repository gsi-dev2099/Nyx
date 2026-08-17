using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface ISupervisorRepository
{
    Task<CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>> GetTeamOrdersAsync(
        long supervisorId,
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<SupervisorStatsDto> GetTeamStatsAsync(
        long supervisorId,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default);

    Task<CRM.ApiHub.Domain.DTOs.BulkTransferResultDto> BulkTransferToBackofficeAsync(
        long[] orderIds,
        long supervisorId,
        long backofficeUserId,
        string? comment,
        CancellationToken ct = default);
}
