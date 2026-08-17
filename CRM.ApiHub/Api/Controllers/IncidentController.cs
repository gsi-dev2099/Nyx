using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Infrastructure.Services;
using System.Security.Claims;


namespace CRM.ApiHub.Api.Controllers;

// DTOs para evitar el colapso de la URL por longitud
public record IncidentResponseRequest(string ResponseText, string ResponseType);
public record ResolveIncidentRequest(string Notes);
public record UpdateIncidentRequest(string CustomName, string CustomDescription, string? CustomSolution, string? AssignedToRole, DateTime? DueAt);

[ApiController]
[Route("api/incidents")]
[Authorize]
public class IncidentController : ControllerBase
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly INotificationService _notificationService;
    private readonly ISlaEngineClient _slaEngineClient;

    public IncidentController(
        IIncidentRepository incidentRepository,
        ISalesOrderRepository salesOrderRepository,
        INotificationService notificationService,
        ISlaEngineClient slaEngineClient)
    {
        _incidentRepository = incidentRepository;
        _salesOrderRepository = salesOrderRepository;
        _notificationService = notificationService;
        _slaEngineClient = slaEngineClient;
    }


    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] long idCmpg, [FromQuery] long idStatus)
    {
        var catalog = await _incidentRepository.GetCatalogAsync(idCmpg, idStatus);
        return Ok(catalog);
    }

    [HttpGet("order/{idOrder}")]
    public async Task<IActionResult> GetByOrder(long idOrder)
    {
        var incidents = await _incidentRepository.GetByOrderAsync(idOrder);
        return Ok(incidents);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null) return NotFound(new { message = "Incidencia no encontrada." });
        return Ok(incident);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderIncident incident)
    {
        if (incident == null) return BadRequest("Datos de incidencia inválidos.");

        var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out var actorId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }
        incident.DetectedBy = actorId;

        var newIncidentId = await _incidentRepository.CreateAsync(incident);
        var suggestedArticles = await _incidentRepository.GetKbSuggestionsAsync(incident.IdIncident);

        // Disparar reloj SLA de incidencias en Nyx.SlaEngine
        await _slaEngineClient.StartMeasurementAsync("incident", newIncidentId, "SLA_INCIDENT_CRITICAL", null, actorId);

        // Send alert / notification
        var order = await _salesOrderRepository.GetByIdAsync(incident.IdOrder);
        if (order != null)
        {
            await _notificationService.SendNotificationAsync(
                userId: order.IdUser,
                title: "Nueva Incidencia Creada",
                message: $"Se ha registrado una incidencia '{incident.CustomName}' en tu orden #{incident.IdOrder}.",
                module: "Incidents",
                actionData: newIncidentId.ToString()
            );
        }

        return Created($"/api/incidents/{newIncidentId}", new 
        { 
            message = "Incidencia creada correctamente.",
            idOrderIncident = newIncidentId,
            kbSuggestions = suggestedArticles 
        });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateIncidentRequest request)
    {
        if (request == null) return BadRequest("Datos de actualización inválidos.");

        var success = await _incidentRepository.UpdateAsync(
            id, 
            request.CustomName, 
            request.CustomDescription, 
            request.CustomSolution, 
            request.AssignedToRole, 
            request.DueAt
        );

        if (!success) return NotFound(new { message = "Incidencia no encontrada o no actualizada." });

        return Ok(new { message = "Datos generales de la incidencia actualizados correctamente." });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _incidentRepository.DeleteAsync(id);
        if (!success) return NotFound(new { message = "Incidencia no encontrada para eliminar." });
        return Ok(new { message = "Incidencia eliminada correctamente." });
    }

    [HttpPost("{id}/responses")]
    public async Task<IActionResult> CreateResponse(long id, [FromBody] IncidentResponseRequest request)
    {
        if (request == null) return BadRequest("Datos de respuesta inválidos.");

        var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out var actorId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        await _incidentRepository.CreateResponseAsync(id, request.ResponseText, request.ResponseType, actorId);

        // Send alert / notification
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident != null)
        {
            var order = await _salesOrderRepository.GetByIdAsync(incident.IdOrder);
            if (order != null)
            {
                await _notificationService.SendNotificationAsync(
                    userId: order.IdUser,
                    title: "Nueva Respuesta a Incidencia",
                    message: $"Se agregó una respuesta a la incidencia #{id} en tu orden #{incident.IdOrder}.",
                    module: "Incidents",
                    actionData: id.ToString()
                );
            }
        }

        return Ok(new { message = "Respuesta registrada." });
    }

    [HttpPatch("{id}/resolve")]
    public async Task<IActionResult> Resolve(long id, [FromBody] ResolveIncidentRequest request)
    {
        if (request == null) return BadRequest("Datos de resolución inválidos.");

        var actorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (actorIdClaim == null || !long.TryParse(actorIdClaim.Value, out var actorId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o no identificado." });
        }

        await _incidentRepository.ResolveAsync(id, request.Notes, actorId);

        // Resolver reloj SLA de incidencias en Nyx.SlaEngine
        await _slaEngineClient.ResolveMeasurementAsync("incident", id, "SLA_INCIDENT_CRITICAL", actorId);

        // Send alert / notification
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident != null)
        {
            var order = await _salesOrderRepository.GetByIdAsync(incident.IdOrder);
            if (order != null)
            {
                await _notificationService.SendNotificationAsync(
                    userId: order.IdUser,
                    title: "Incidencia Resuelta",
                    message: $"La incidencia #{id} en tu orden #{incident.IdOrder} ha sido resuelta.",
                    module: "Incidents",
                    actionData: id.ToString()
                );
            }
        }

        return Ok(new { message = "Incidencia resuelta correctamente." });
    }

    [HttpGet]
    public async Task<IActionResult> GetIncidents([FromQuery] string? assignedToRole, [FromQuery] string? status, CancellationToken ct)
    {
        var incidents = await _incidentRepository.GetFilteredAsync(assignedToRole, status, ct);
        return Ok(incidents);
    }

    [HttpGet("{id:long}/kb-suggestions")]
    public async Task<IActionResult> GetKbSuggestions(long id, CancellationToken ct)
    {
        var incident = await _incidentRepository.GetByIdAsync(id);
        if (incident == null) return NotFound(new { message = "Incidencia no encontrada." });
        var suggestions = await _incidentRepository.GetKbSuggestionsAsync(incident.IdIncident);
        return Ok(suggestions);
    }
}