using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class GetSalesOrdersUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;

    public GetSalesOrdersUseCase(ISalesOrderRepository salesOrderRepository)
    {
        _salesOrderRepository = salesOrderRepository;
    }

    public async Task<CRM.ApiHub.Application.DTOs.PagedResult<SalesOrder>> ExecuteAsync(
        long? userId,
        long? statusId,
        long? campaignId,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return await _salesOrderRepository.GetByFiltersAsync(userId, statusId, campaignId, dateFrom, dateTo, page, pageSize, ct);
    }
}
