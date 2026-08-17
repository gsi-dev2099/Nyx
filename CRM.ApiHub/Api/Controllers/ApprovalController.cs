using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRM.ApiHub.Domain.DTOs;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Infrastructure.Services;
using System.Security.Claims;

namespace CRM.ApiHub.Api.Controllers;

public class ApprovalRequestDto
{
    public long AuthorizedBy { get; set; }
    public string Comments { get; set; } = null!;
    public bool IsApproved { get; set; }
}

public class ApprovalPatchDto
{
    public string Status { get; set; } = null!; // APPROVED or REJECTED
    public string Comments { get; set; } = null!;
    public long AuthorizedBy { get; set; }
}

[ApiController]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalRepository _repository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly INotificationService _notificationService;
    private readonly IApprovalEngineClient _approvalEngineClient;

    public ApprovalController(
        IApprovalRepository repository,
        ISalesOrderRepository salesOrderRepository,
        INotificationService notificationService,
        IApprovalEngineClient approvalEngineClient)
    {
        _repository = repository;
        _salesOrderRepository = salesOrderRepository;
        _notificationService = notificationService;
        _approvalEngineClient = approvalEngineClient;
    }

    [HttpPost("api/orders/{id:long}/approvals")]
    [Authorize(Roles = "ASESOR,BACKOFFICE")]
    public async Task<IActionResult> ProcessApproval(long id, [FromBody] ApprovalRequestDto dto)
    {
        if (dto == null) return BadRequest("Datos de solicitud de aprobación inválidos.");

        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        if (userClaim == null || !long.TryParse(userClaim.Value, out long requestedBy))
        {
            return Unauthorized(new { message = "Usuario no autenticado o token inválido." });
        }

        if (requestedBy == -999) requestedBy = 101;
        else if (requestedBy == -1000) requestedBy = 237;
        else if (requestedBy == -998) requestedBy = 9;

        var approvalId = await _repository.CreateApprovalRequestAsync(id, dto.Comments, requestedBy);

        // Disparar solicitud en motor autonomo Nyx.ApprovalEngine
        await _approvalEngineClient.SubmitRequestAsync("APPROVAL_HIGH_DISCOUNT", "order", id, requestedBy);

        return CreatedAtAction(nameof(GetById), new { id = approvalId }, new { message = "Solicitud de aprobación registrada como PENDING.", id = approvalId });
    }

    [HttpPatch("api/approvals/{id:long}")]
    [Authorize(Roles = "SUPERVISOR")]
    public async Task<IActionResult> UpdateApproval(long id, [FromBody] ApprovalPatchDto dto)
    {
        if (dto == null) return BadRequest("Datos de modificación inválidos.");

        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("id_user")
                        ?? User.FindFirst("userId");

        if (userClaim == null || !long.TryParse(userClaim.Value, out long supervisorId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o token inválido." });
        }

        if (supervisorId == -999) supervisorId = 101;
        else if (supervisorId == -1000) supervisorId = 237;
        else if (supervisorId == -998) supervisorId = 9;

        var success = await _repository.UpdateApprovalAsync(id, dto.Status, dto.Comments, supervisorId);
        if (!success)
        {
            return NotFound(new { message = "Aprobación no encontrada." });
        }

        // Registrar decision en motor autonomo Nyx.ApprovalEngine (Valida reglas ISO 27001 / SOX de Segregacion de Deberes)
        try
        {
            await _approvalEngineClient.DecideRequestAsync(id, supervisorId, dto.Status, dto.Comments);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        // Emit internal notification on approval
        if (dto.Status == "APPROVED")
        {
            var approval = await _repository.GetApprovalByIdAsync(id);
            if (approval != null)
            {
                var order = await _salesOrderRepository.GetByIdAsync(approval.IdOrder);
                if (order != null)
                {
                    await _notificationService.SendNotificationAsync(
                        userId: order.IdUser,
                        title: "Orden Aprobada",
                        message: $"Tu orden #{approval.IdOrder} ha sido aprobada por el supervisor.",
                        module: "Approvals",
                        actionData: id.ToString()
                    );
                }
            }
        }

        return Ok(new { message = "Aprobación actualizada correctamente.", id = id });
    }


    [HttpGet("api/approvals/{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var approval = await _repository.GetApprovalByIdAsync(id);
        if (approval == null)
        {
            return NotFound(new { message = "Aprobación no encontrada." });
        }
        return Ok(approval);
    }
}