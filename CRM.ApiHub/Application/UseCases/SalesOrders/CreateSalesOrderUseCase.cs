using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Services;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class CreateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISlaEngineClient _slaEngineClient;
    private readonly IFlowEngineClient _flowEngineClient;

    public CreateSalesOrderUseCase(
        ISalesOrderRepository salesOrderRepository,
        ISlaEngineClient slaEngineClient,
        IFlowEngineClient flowEngineClient)
    {
        _salesOrderRepository = salesOrderRepository;
        _slaEngineClient = slaEngineClient;
        _flowEngineClient = flowEngineClient;
    }

    public async Task<SalesOrder> ExecuteAsync(SalesOrderCreateDto dto, CancellationToken ct = default)
    {
        var order = new SalesOrder
        {
            IdLead = dto.IdLead,
            IdCmpg = dto.IdCmpg,
            IdUser = dto.IdUser,
            OwnerUserId = dto.OwnerUserId ?? dto.IdUser,
            CustodyUserId = dto.CustodyUserId ?? dto.IdUser,
            IdStatus = dto.IdStatus ?? 1,
            IdSubstatus = dto.IdSubstatus,
            CurrencyCode = dto.CurrencyCode ?? "EUR",
            CommissionCurrency = dto.CommissionCurrency ?? "PEN",
            Status = dto.Status ?? "BORRADOR",
            SalesDate = dto.SalesDate ?? DateTime.UtcNow,
            TotalProducts = dto.TotalProducts,
            TotalValue = dto.TotalValue,
            IsAlternate = dto.IsAlternate,
            Register = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow
        };

        var newId = await _salesOrderRepository.CreateAsync(order, ct);
        order.IdOrder = newId;

        // Disparar reloj SLA autonomo en Nyx.SlaEngine
        await _slaEngineClient.StartMeasurementAsync("order", newId, "SLA_SALES_VALIDATION", order.OwnerUserId, order.IdUser);

        // Instanciar pipeline y checkpoints autonomos en Nyx.FlowEngine
        await _flowEngineClient.StartFlowInstanceAsync("PIPELINE_ALARMAS", "order", newId, order.IdUser);

        return order;
    }

}

