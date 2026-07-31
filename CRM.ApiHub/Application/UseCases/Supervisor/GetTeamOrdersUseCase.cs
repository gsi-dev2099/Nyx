using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Supervisor;

public class GetTeamOrdersUseCase
{
    private readonly ISupervisorRepository _repository;

    public GetTeamOrdersUseCase(ISupervisorRepository repository)
    {
        _repository = repository;
    }

    public async Task<CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>> ExecuteAsync(
        long supervisorId,
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return await _repository.GetTeamOrdersAsync(supervisorId, userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
    }
}
