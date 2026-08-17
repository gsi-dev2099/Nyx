using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Reports;

public class GetIncidentStatsUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetIncidentStatsUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IncidentStatsDto> ExecuteAsync(
        long idCmpg, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        return await _reportRepository.GetIncidentStatsAsync(idCmpg, dateFrom, dateTo, ct);
    }
}
