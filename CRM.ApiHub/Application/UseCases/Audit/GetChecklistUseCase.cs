using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Audit;

public class GetChecklistUseCase
{
    private readonly IAuditRepository _auditRepository;

    public GetChecklistUseCase(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<AuditChecklistResponseDto?> ExecuteAsync(long idCmpg, CancellationToken ct = default)
    {
        var template = await _auditRepository.GetChecklistTemplateByCampaignAsync(idCmpg, ct);
        if (template == null)
        {
            return null;
        }

        var items = await _auditRepository.GetChecklistItemsAsync(template.IdChecklist, ct);

        return new AuditChecklistResponseDto
        {
            IdChecklist = template.IdChecklist,
            IdCmpg = template.IdCmpg,
            Name = template.Name,
            Version = template.Version,
            Items = items.Select(item => new AuditChecklistItemDto
            {
                IdItem = item.IdItem,
                OrderIndex = item.OrderIndex,
                ItemType = item.ItemType,
                Description = item.Description,
                ExpectedText = item.ExpectedText,
                CrmFieldKey = item.CrmFieldKey,
                IsCritical = item.IsCritical
            }).ToList()
        };
    }
}
