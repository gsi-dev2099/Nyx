using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Reports;

public class GetActivationStatsUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetActivationStatsUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ActivationStatsDto> ExecuteAsync(
        long idProvider, 
        DateTime dateFrom, 
        DateTime dateTo, 
        CancellationToken ct = default)
    {
        return await _reportRepository.GetActivationStatsAsync(idProvider, dateFrom, dateTo, ct);
    }
}
