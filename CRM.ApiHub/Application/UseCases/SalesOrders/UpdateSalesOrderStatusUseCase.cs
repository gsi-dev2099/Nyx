using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class UpdateSalesOrderStatusUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly INotificationService _notificationService;

    public UpdateSalesOrderStatusUseCase(
        ISalesOrderRepository salesOrderRepository,
        INotificationService notificationService)
    {
        _salesOrderRepository = salesOrderRepository;
        _notificationService = notificationService;
    }

    public async Task<bool> ExecuteAsync(long idOrder, SalesOrderUpdateStatusDto dto, long actorId, CancellationToken ct = default)
    {
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
                // Notify the original asesor about the status change
                await _notificationService.SendNotificationAsync(
                    userId: order.IdUser,
                    title: "Estado de Orden Actualizado",
                    message: $"El estado de tu orden #{idOrder} ha cambiado al estado {dto.ToStatusId}.",
                    module: "SalesOrder",
                    actionData: idOrder.ToString()
                );

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
