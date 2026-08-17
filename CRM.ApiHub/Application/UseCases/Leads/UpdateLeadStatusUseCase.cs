using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class UpdateLeadStatusUseCase
{
    private readonly ILeadRepository _leadRepository;
    private readonly INotificationService _notificationService;

    public UpdateLeadStatusUseCase(
        ILeadRepository leadRepository,
        INotificationService notificationService)
    {
        _leadRepository = leadRepository;
        _notificationService = notificationService;
    }

    public async Task<bool> ExecuteAsync(long idLead, LeadUpdateStatusDto dto, long actorId, CancellationToken ct = default)
    {
        var success = await _leadRepository.UpdateStatusAsync(idLead, dto.IdStatus, dto.Comment, actorId, ct);
        if (success)
        {
            var lead = await _leadRepository.GetByIdAsync(idLead, ct);
            if (lead != null)
            {
                long recipientId = lead.AssignedUserId ?? lead.OwnerUserId ?? actorId;
                await _notificationService.SendNotificationAsync(
                    userId: recipientId,
                    title: "Estado de Lead Actualizado",
                    message: $"El lead '{lead.FirstName} {lead.LastName}' ha cambiado al estado {dto.IdStatus}.",
                    module: "Lead",
                    actionData: idLead.ToString()
                );
            }
        }
        return success;
    }
}
