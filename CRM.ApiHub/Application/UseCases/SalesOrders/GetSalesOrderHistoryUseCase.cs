using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class GetSalesOrderHistoryUseCase
{
    private readonly ISalesOrderRepository _repository;

    public GetSalesOrderHistoryUseCase(ISalesOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SalesOrderTimelineItemDto>> ExecuteAsync(long idOrder, CancellationToken ct = default)
    {
        var rawEvents = await _repository.GetOrderHistoryTimelineAsync(idOrder, ct);

        return rawEvents.Select(raw => new SalesOrderTimelineItemDto
        {
            Timestamp = raw.Timestamp,
            EventType = raw.EventType,
            ActorId = raw.ActorId,
            ActorName = string.IsNullOrWhiteSpace(raw.ActorName) ? "Sistema / Desconocido" : raw.ActorName,
            Description = raw.Description,
            Details = string.IsNullOrWhiteSpace(raw.DetailsJson) 
                ? (JsonElement?)null 
                : JsonSerializer.Deserialize<JsonElement>(raw.DetailsJson)
        });
    }
}
