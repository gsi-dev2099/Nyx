using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;

namespace CRM.ApiHub.Domain.Repositories;

public interface IReportRepository
{
    Task<IEnumerable<ConversionFunnelStageDto>> GetConversionFunnelAsync(long idCmpg, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
    Task<IEnumerable<SalesByAsesorDto>> GetSalesByAsesorAsync(long supervisorId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
    Task<IncidentStatsDto> GetIncidentStatsAsync(long idCmpg, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
    Task<ActivationStatsDto> GetActivationStatsAsync(long idProvider, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
}
