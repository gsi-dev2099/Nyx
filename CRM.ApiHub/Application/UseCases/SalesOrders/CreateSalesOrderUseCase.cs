using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class CreateSalesOrderUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly ISlaEngineClient _slaEngineClient;
    private readonly IFlowEngineClient _flowEngineClient;
    private readonly IApprovalEngineClient _approvalEngineClient;
    private readonly ILogger<CreateSalesOrderUseCase> _logger;

    public CreateSalesOrderUseCase(
        ISalesOrderRepository salesOrderRepository,
        ISlaEngineClient slaEngineClient,
        IFlowEngineClient flowEngineClient,
        IApprovalEngineClient approvalEngineClient,
        ILogger<CreateSalesOrderUseCase> logger)
    {
        _salesOrderRepository = salesOrderRepository;
        _slaEngineClient = slaEngineClient;
        _flowEngineClient = flowEngineClient;
        _approvalEngineClient = approvalEngineClient;
        _logger = logger;
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

        bool requiresApproval = dto.DiscountPercentage > 10;
        if (requiresApproval)
        {
            order.Status = "PENDING_APPROVAL";
        }


        var newId = await _salesOrderRepository.CreateAsync(order, ct);
        order.IdOrder = newId;

        // Disparar reloj SLA autonomo en Nyx.SlaEngine
        await _slaEngineClient.StartMeasurementAsync("order", newId, "SLA_SALES_VALIDATION", order.OwnerUserId, order.IdUser);

        // Instanciar pipeline y checkpoints autonomos en Nyx.FlowEngine
        var flowCode = ResolveFlowCode(order.IdCmpg);
        await _flowEngineClient.StartFlowInstanceAsync(flowCode, "order", newId, order.IdUser);

        if (requiresApproval)
        {
            try
            {
                var context = $"{{\"discount\":{dto.DiscountPercentage}}}";
                await _approvalEngineClient.SubmitRequestAsync("HIGH_DISCOUNT", "order", newId, order.IdUser, context);
            }
            catch (Exception ex)
            {
                // El log se delega o la excepción se captura en el cliente que tiene un try/catch, 
                // pero por las dudas (si queremos abortar la transacción se relanza, el user dijo: 
                // "asegurando que retenga la transacción inmutable en Dapper si se requiere aprobación"
                // lo que significa que la inserción de Dapper ya ocurrió inmutablemente, 
                // y usamos la resiliencia para el call. 
                // Wait, el cliente ApprovalEngineClient TIENE SU PROPIO try/catch y devuelve null en error
                // Así que el motor de polly absorberá los reintentos.
                _logger.LogWarning(ex, "Failed to submit approval request for SalesOrder #{OrderId}", newId);
            }
        }

        return order;
    }

    private static string ResolveFlowCode(long idCmpg) => idCmpg switch
    {
        1 or 2 or 3 or 4 or 5 => "PIPELINE_TELECOM",  // VODAFONE, LOWI, YOIGO, MASMOVIL, ORANGE
        _ => "PIPELINE_ALARMAS"                         // Todo lo demás
    };
}

