using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Providers;

public class UpdateOrderProviderStatusUseCase
{
    private readonly IProviderRepository _providerRepository;

    public UpdateOrderProviderStatusUseCase(IProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<UpdateOrderProviderStatusResponseDto> ExecuteAsync(
        long idProvider, 
        UpdateOrderProviderStatusRequestDto request, 
        long actorUserId,
        CancellationToken ct = default)
    {
        // 1. Get mappings for the provider
        var mappings = await _providerRepository.GetStatusMappingAsync(idProvider, ct);
        var mapping = mappings.FirstOrDefault(m => string.Equals(m.ProviderStatusCode, request.ProviderStatusCode, StringComparison.OrdinalIgnoreCase));

        // Get current order status for logs
        var currentStatusId = await _providerRepository.GetOrderCurrentStatusAsync(request.IdOrder, ct);

        if (mapping == null)
        {
            // Log sync failure
            var errorMsg = $"No se encontró mapeo de estado para el código de proveedor: {request.ProviderStatusCode}";
            await _providerRepository.LogSyncAsync(
                idProvider: idProvider,
                idOrder: request.IdOrder,
                statusCode: request.ProviderStatusCode,
                result: $"ERROR: {errorMsg}",
                internalStatusBefore: currentStatusId,
                internalStatusAfter: currentStatusId,
                success: false,
                ct: ct
            );

            return new UpdateOrderProviderStatusResponseDto
            {
                Success = false,
                Mapped = false,
                InternalStatusBefore = currentStatusId,
                InternalStatusAfter = currentStatusId,
                IncidentCreated = false,
                Message = errorMsg
            };
        }

        bool statusUpdated = false;
        bool incidentCreated = false;
        long? finalStatusId = currentStatusId;

        // 2. Apply auto update if enabled
        if (mapping.AutoUpdate)
        {
            statusUpdated = await _providerRepository.UpdateOrderStatusAsync(
                request.IdOrder, 
                mapping.InternalStatusId, 
                mapping.InternalSubstatusId, 
                actorUserId,
                ct
            );
            finalStatusId = mapping.InternalStatusId;
        }

        // 3. Create incident automatically if set
        if (mapping.CreatesIncidentId.HasValue)
        {
            var title = $"Incidencia Automática Proveedor: {mapping.ProviderStatusName ?? request.ProviderStatusCode}";
            var desc = $"Generada automáticamente al recibir estado de proveedor '{request.ProviderStatusCode}' ({mapping.ProviderStatusName}) mapeado al estado interno {mapping.InternalStatusId}.";
            
            incidentCreated = await _providerRepository.CreateOrderIncidentAsync(
                request.IdOrder, 
                mapping.CreatesIncidentId.Value, 
                title, 
                desc, 
                ct
            );
        }

        // 4. Log sync success
        var successDetail = mapping.AutoUpdate 
            ? $"Mapeo automático aplicado exitosamente. Estado de la orden cambiado de {currentStatusId} a {finalStatusId}."
            : $"Estado de proveedor recibido. No se actualiza de forma automática (auto_update=false).";

        if (incidentCreated)
        {
            successDetail += " Se generó incidencia automática vinculada.";
        }

        await _providerRepository.LogSyncAsync(
            idProvider: idProvider,
            idOrder: request.IdOrder,
            statusCode: request.ProviderStatusCode,
            result: successDetail,
            internalStatusBefore: currentStatusId,
            internalStatusAfter: finalStatusId,
            success: true,
            ct: ct
        );

        return new UpdateOrderProviderStatusResponseDto
        {
            Success = true,
            Mapped = true,
            InternalStatusBefore = currentStatusId,
            InternalStatusAfter = finalStatusId,
            IncidentCreated = incidentCreated,
            Message = successDetail
        };
    }
}
