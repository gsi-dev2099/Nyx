using Microsoft.AspNetCore.Mvc;
using Nyx.FlowEngine.Application;
using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Controllers;

[ApiController]
[Route("api/flow")]
public class FlowController : ControllerBase
{
    private readonly IFlowService _service;

    public FlowController(IFlowService service)
    {
        _service = service;
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions()
    {
        var flows = await _service.GetFlowDefinitionsAsync();
        return Ok(flows);
    }

    [HttpGet("checkpoints/catalog")]
    public async Task<IActionResult> GetCheckpointCatalog([FromQuery] long? flowId)
    {
        var catalog = await _service.GetCheckpointCatalogAsync(flowId);
        return Ok(catalog);
    }

    [HttpPost("checkpoints/catalog")]
    public async Task<IActionResult> CreateCheckpoint([FromBody] CheckpointCatalog cp)
    {
        var created = await _service.CreateCheckpointCatalogAsync(cp);
        return Ok(created);
    }

    [HttpPost("checkpoints/catalog/{id:long}/approve")]
    public async Task<IActionResult> ApproveCheckpoint(long id, [FromBody] ApproveCheckpointRequest req)
    {
        await _service.ApproveCheckpointCatalogAsync(id, req.ApprovedByJson, req.ActorId ?? 1);
        return Ok(new { message = "Checkpoint activated and approved with triple signoff" });
    }

    [HttpPost("instances/start")]
    public async Task<IActionResult> StartInstance([FromBody] StartFlowRequest req)
    {
        var instance = await _service.StartFlowInstanceAsync(req.FlowCode, req.EntityType, req.EntityId, req.ActorId ?? 1);
        return Ok(instance);
    }

    [HttpPost("instances/{id:long}/advance")]
    public async Task<IActionResult> AdvanceStage(long id, [FromBody] AdvanceStageRequest req)
    {
        try
        {
            var result = await _service.AdvanceStageAsync(id, req.ActorId ?? 1);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("checkpoints/instances/{id:long}/resolve")]
    public async Task<IActionResult> ResolveCheckpoint(long id, [FromBody] ResolveCheckpointRequest req)
    {
        var result = await _service.ResolveCheckpointAsync(id, req.Status, req.ActorId ?? 1);
        return Ok(result);
    }
}

public record ApproveCheckpointRequest(string ApprovedByJson, long? ActorId);
public record StartFlowRequest(string FlowCode, string EntityType, long EntityId, long? ActorId);
public record AdvanceStageRequest(long? ActorId);
public record ResolveCheckpointRequest(string Status, long? ActorId);
