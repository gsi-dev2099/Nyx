using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.Exceptions;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Infrastructure.Services;
using System;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class UpdateLeadStatusUseCase
{
    private readonly ILeadRepository _leadRepository;
    private readonly INotificationService _notificationService;
    private readonly IFlowEngineClient _flowEngineClient;
    private readonly ISlaEngineClient _slaEngineClient;
    private readonly ILogger<UpdateLeadStatusUseCase> _logger;

    public UpdateLeadStatusUseCase(
        ILeadRepository leadRepository,
        INotificationService notificationService,
        IFlowEngineClient flowEngineClient,
        ISlaEngineClient slaEngineClient,
        ILogger<UpdateLeadStatusUseCase> logger)
    {
        _leadRepository = leadRepository;
        _notificationService = notificationService;
        _flowEngineClient = flowEngineClient;
        _slaEngineClient = slaEngineClient;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(long idLead, LeadUpdateStatusDto dto, long actorId, CancellationToken ct = default)
    {
        var lead = await _leadRepository.GetByIdAsync(idLead, ct);
        if (lead == null)
            return false;

        // Validar transición con FlowEngine
        try
        {
            bool isValid = await _flowEngineClient.ValidateTransitionAsync("LEAD", (int)lead.CurrentStatusId, dto.IdStatus);
            if (!isValid)
            {
                throw new InvalidTransitionException("LEAD", (int)lead.CurrentStatusId, dto.IdStatus, $"La transición de estado {lead.CurrentStatusId} a {dto.IdStatus} no es válida según el motor de flujos.");
            }
        }
        catch (InvalidTransitionException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Circuit Breaker abierto o error de red persistente
            throw new InvalidTransitionException("LEAD", (int)lead.CurrentStatusId, dto.IdStatus, $"No se pudo validar la transición con el motor de flujos debido a un error de red: {ex.Message}");
        }

        var success = await _leadRepository.UpdateStatusAsync(idLead, dto.IdStatus, dto.Comment, actorId, ct);
        if (success)
        {
            long recipientId = lead.AssignedUserId ?? lead.OwnerUserId ?? actorId;
            await _notificationService.SendNotificationAsync(
                userId: recipientId,
                title: "Estado de Lead Actualizado",
                message: $"El lead '{lead.FirstName} {lead.LastName}' ha cambiado al estado {dto.IdStatus}.",
                module: "Lead",
                actionData: idLead.ToString()
            );

            // 5. Fire-and-Forget controlado hacia el SlaEngine
            try
            {
                await _slaEngineClient.TrackStateChangeAsync("LEAD", idLead, dto.IdStatus, lead.CustodyUserId);
            }
            catch (Exception ex)
            {
                // Regla Estricta: Falla asilada, no bloquea el flujo principal
                _logger.LogError(ex, "Failed to notify SLA Engine for Lead {LeadId}. CustodyUserId: {CustodyUserId}, TargetStatus: {TargetStatus}", 
                                 idLead, lead.CustodyUserId, dto.IdStatus);
            }
        }
        return success;
    }
}
