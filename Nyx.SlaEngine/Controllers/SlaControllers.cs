using Microsoft.AspNetCore.Mvc;
using Nyx.SlaEngine.Application;
using Nyx.SlaEngine.Domain.Entities;

namespace Nyx.SlaEngine.Controllers;

[ApiController]
[Route("api/sla")]
public class SlaController : ControllerBase
{
    private readonly ISlaService _service;

    public SlaController(ISlaService service)
    {
        _service = service;
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _service.GetPoliciesAsync();
        return Ok(policies);
    }

    [HttpPost("policies")]
    public async Task<IActionResult> CreatePolicy([FromBody] SlaPolicy policy)
    {
        var created = await _service.CreatePolicyAsync(policy);
        return CreatedAtAction(nameof(GetPolicies), new { id = created.IdPolicy }, created);
    }

    [HttpPost("measurements/start")]
    public async Task<IActionResult> StartMeasurement([FromBody] StartSlaRequest request)
    {
        var result = await _service.StartMeasurementAsync(
            request.EntityType, 
            request.EntityId, 
            request.PolicyCode, 
            request.OwnerUserId, 
            request.ActorId ?? 1
        );
        return Ok(result);
    }

    [HttpPost("measurements/resolve")]
    public async Task<IActionResult> ResolveMeasurement([FromBody] ResolveSlaRequest request)
    {
        var result = await _service.ResolveMeasurementAsync(
            request.EntityType, 
            request.EntityId, 
            request.PolicyCode, 
            request.ActorId ?? 1
        );
        if (result == null) return NotFound("Measurement or policy not found");
        return Ok(result);
    }

    [HttpPost("measurements/pause")]
    public async Task<IActionResult> PauseMeasurement([FromBody] ResolveSlaRequest request)
    {
        var result = await _service.PauseMeasurementAsync(
            request.EntityType, 
            request.EntityId, 
            request.PolicyCode, 
            request.ActorId ?? 1
        );
        if (result == null) return NotFound("Measurement or policy not found");
        return Ok(result);
    }

    [HttpGet("measurements/status")]
    public async Task<IActionResult> GetStatus([FromQuery] string entityType, [FromQuery] long entityId, [FromQuery] string policyCode)
    {
        var result = await _service.GetStatusAsync(entityType, entityId, policyCode);
        if (result == null) return NotFound("Measurement not found");
        return Ok(result);
    }
}

public record StartSlaRequest(string EntityType, long EntityId, string PolicyCode, long? OwnerUserId, long? ActorId);
public record ResolveSlaRequest(string EntityType, long EntityId, string PolicyCode, long? ActorId);
