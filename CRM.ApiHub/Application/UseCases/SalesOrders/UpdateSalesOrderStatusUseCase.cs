using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Infrastructure.Services;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class UpdateSalesOrderStatusUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly INotificationService _notificationService;
    private readonly ISlaEngineClient _slaEngineClient;
    private readonly IFlowEngineClient _flowEngineClient;

    public UpdateSalesOrderStatusUseCase(
        ISalesOrderRepository salesOrderRepository,
        INotificationService notificationService,
        ISlaEngineClient slaEngineClient,
        IFlowEngineClient flowEngineClient)
    {
        _salesOrderRepository = salesOrderRepository;
        _notificationService = notificationService;
        _slaEngineClient = slaEngineClient;
        _flowEngineClient = flowEngineClient;
    }

    public async Task<bool> ExecuteAsync(long idOrder, SalesOrderUpdateStatusDto dto, long actorId, CancellationToken ct = default)
    {
        var existingOrder = await _salesOrderRepository.GetByIdAsync(idOrder, ct);
        if (existingOrder == null) return false;

        // Si la orden está en estado >= 3 (EN BACKOFFICE en adelante) y el actor no es el usuario en custodia ni posee el rol correspondiente, rechazar la modificación.
        if (existingOrder.IdStatus >= 3 && existingOrder.CustodyUserId.HasValue && existingOrder.CustodyUserId.Value != actorId)
        {
            throw new InvalidOperationException($"Transición de estado no permitida. La orden #{idOrder} se encuentra en custodia del usuario {existingOrder.CustodyUserId.Value}.");
        }

        // Sincronizar etapa exacta en Nyx.FlowEngine según el status destino (validando checkpoints bloqueantes)
        try
        {
            await _flowEngineClient.SyncStageByStatusAsync("order", idOrder, dto.ToStatusId, actorId);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Transición bloqueada por el Motor de Flujos (Nyx.FlowEngine): {ex.Message}");
        }


        var success = await _salesOrderRepository.UpdateStatusAsync(
            idOrder,
            dto.ToStatusId,
            dto.ToSubstatusId,
            dto.Comment,
            actorId,
            dto.IsBulk,
            ct
        );

        if (success)
        {
            var order = await _salesOrderRepository.GetByIdAsync(idOrder, ct);
            if (order != null)
            {
                // Disparar resolución de SLA si el nuevo estado es terminal (9=ACTIVADO, 13=CANCELADO, 14=BAJA)
                if (dto.ToStatusId == 9 || dto.ToStatusId == 13 || dto.ToStatusId == 14)
                {
                    await _slaEngineClient.ResolveMeasurementAsync("order", idOrder, "SLA_SALES_VALIDATION", actorId);
                }

                // Notify the original asesor about the status change
                await _notificationService.SendNotificationAsync(
                    userId: order.IdUser,
                    title: "Estado de Orden Actualizado",
                    message: $"El estado de tu orden #{idOrder} ha cambiado al estado {dto.ToStatusId}.",
                    module: "SalesOrder",
                    actionData: idOrder.ToString()
                );

                // When sent to Supervisor for revision (Status 2), notify custody supervisor
                const int SUPERVISOR_STATUS_ID = 2;
                if (dto.ToStatusId == SUPERVISOR_STATUS_ID)
                {
                    long targetSupervisorId = order.CustodyUserId.HasValue && order.CustodyUserId.Value != order.IdUser
                        ? order.CustodyUserId.Value
                        : 9; // Default Supervisor ID fallback (cnaranjo)

                    await _notificationService.SendNotificationAsync(
                        userId: targetSupervisorId,
                        title: $"Nueva Orden #{idOrder} lista para revisión",
                        message: $"El asesor ha enviado la orden #{idOrder} para revisión.",
                        module: "SUPERVISOR_REVISION",
                        actionData: idOrder.ToString()
                    );
                }

                // When sent to BackOffice individually, also notify the custody holder
                const int BACKOFFICE_STATUS_ID = 3;
                if (dto.ToStatusId == BACKOFFICE_STATUS_ID && order.CustodyUserId.HasValue && order.CustodyUserId.Value != order.IdUser)
                {
                    await _notificationService.SendNotificationAsync(
                        userId: order.CustodyUserId.Value,
                        title: $"Orden #{idOrder} asignada para revisión BAC",
                        message: $"Se te ha asignado la orden #{idOrder} para revisión desde el Supervisor.",
                        module: "TRANSFER",
                        actionData: idOrder.ToString()
                    );
                }
            }
        }

        return success;
    }
}

