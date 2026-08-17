using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Reports;

public class GetSalesByAsesorUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetSalesByAsesorUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<SalesByAsesorDto>> ExecuteAsync(
        long supervisorId, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        return await _reportRepository.GetSalesByAsesorAsync(supervisorId, dateFrom, dateTo, ct);
    }
}
