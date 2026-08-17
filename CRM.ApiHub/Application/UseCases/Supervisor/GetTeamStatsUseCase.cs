using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Supervisor;

public class GetTeamStatsUseCase
{
    private readonly ISupervisorRepository _repository;

    public GetTeamStatsUseCase(ISupervisorRepository repository)
    {
        _repository = repository;
    }

    public async Task<SupervisorStatsDto> ExecuteAsync(
        long supervisorId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken ct = default)
    {
        var from = dateFrom ?? DateTime.UtcNow.AddDays(-30);
        var to = dateTo ?? DateTime.UtcNow;

        return await _repository.GetTeamStatsAsync(supervisorId, from, to, ct);
    }
}
