using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Leads;

public class CreateLeadUseCase
{
    private readonly ILeadRepository _leadRepository;

    public CreateLeadUseCase(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<Lead> ExecuteAsync(LeadCreateDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            IdCmpg = dto.IdCmpg,
            IdSrc = dto.IdSrc,
            DocumentNumber = dto.DocumentNumber,
            RawData = string.IsNullOrWhiteSpace(dto.RawData) ? "{}" : dto.RawData,
            AssignedUserId = dto.AssignedUserId,
            OwnerUserId = dto.AssignedUserId,
            CustodyUserId = dto.AssignedUserId,
            CurrentStatusId = 1, // Default status (NUEVO)
            IsActive = true,
            Register = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow
        };

        var id = await _leadRepository.CreateAsync(lead, ct);
        lead.IdLead = id;
        return lead;
    }
}
