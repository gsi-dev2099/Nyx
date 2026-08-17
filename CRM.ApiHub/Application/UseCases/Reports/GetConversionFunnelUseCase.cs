using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Reports;

public class GetConversionFunnelUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetConversionFunnelUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<ConversionFunnelStageDto>> ExecuteAsync(
        long idCmpg, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        return await _reportRepository.GetConversionFunnelAsync(idCmpg, dateFrom, dateTo, ct);
    }
}
