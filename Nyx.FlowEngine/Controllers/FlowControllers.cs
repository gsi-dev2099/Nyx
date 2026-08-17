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

    [HttpGet("stages")]
    public async Task<IActionResult> GetStages([FromQuery] long? flowId = null)
    {
        var stages = await _service.GetStagesAsync(flowId);
        return Ok(stages);
    }

    [HttpPost("stages")]
    public async Task<IActionResult> CreateStage([FromBody] FlowStage stage)
    {
        var created = await _service.CreateStageAsync(stage);
        return Ok(created);
    }

    [HttpPatch("stages/{id:long}/move")]
    public async Task<IActionResult> MoveStage(long id, [FromQuery] string direction)
    {
        if (direction != "up" && direction != "down")
            return BadRequest(new { error = "direction must be 'up' or 'down'" });
        var moved = await _service.MoveStageAsync(id, direction);
        return moved ? Ok(new { moved = true }) : BadRequest(new { error = "Cannot move stage in that direction (already at boundary)." });
    }

    [HttpPatch("stages/{id:long}/order")]
    public async Task<IActionResult> SetStageOrder(long id, [FromBody] SetOrderRequest req)
    {
        var ok = await _service.SetStageOrderAsync(id, req.OrderIndex);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Stage not found." });
    }

    [HttpPatch("stages/{id:long}")]
    public async Task<IActionResult> UpdateStage(long id, [FromBody] FlowStage stage)
    {
        stage.IdStage = id;
        var ok = await _service.UpdateStageAsync(stage);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Stage not found." });
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 50)
    {
        var logs = await _service.GetAuditLogsAsync(limit);
        return Ok(logs);
    }

    [HttpGet("checkpoints/catalog")]
    [HttpGet("catalogs")]
    public async Task<IActionResult> GetCheckpointCatalog([FromQuery] long? flowId)
    {
        var catalog = await _service.GetCheckpointCatalogAsync(flowId);
        return Ok(catalog);
    }

    [HttpGet("checkpoints/catalog/full")]
    [HttpGet("catalogs/full")]
    public async Task<IActionResult> GetFullCheckpointCatalog([FromQuery] long? flowId)
    {
        var fullCatalog = await _service.GetFullCheckpointCatalogAsync(flowId);
        return Ok(fullCatalog);
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

    [HttpPatch("checkpoints/catalog/{id:long}/campaign")]
    public async Task<IActionResult> UpdateCheckpointCampaign(long id, [FromBody] UpdateCampaignRequest req)
    {
        var ok = await _service.UpdateCheckpointCampaignAsync(id, req.Campaign);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Checkpoint not found." });
    }

    [HttpPatch("checkpoints/catalog/{id:long}/portfolio")]
    public async Task<IActionResult> UpdateCheckpointPortfolio(long id, [FromBody] UpdatePortfolioRequest req)
    {
        var ok = await _service.UpdateCheckpointPortfolioAsync(id, req.Portfolio);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Checkpoint not found." });
    }

    [HttpPatch("checkpoints/catalog/{id:long}/stage")]
    public async Task<IActionResult> UpdateCheckpointStage(long id, [FromBody] UpdateStageIdRequest req)
    {
        var ok = await _service.UpdateCheckpointStageAsync(id, req.StageId);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Checkpoint not found." });
    }

    [HttpPut("checkpoints/catalog/{id:long}")]
    public async Task<IActionResult> UpdateCheckpointCatalog(long id, [FromBody] CheckpointCatalog cp)
    {
        var ok = await _service.UpdateCheckpointCatalogAsync(id, cp);
        return ok ? Ok(new { updated = true }) : NotFound(new { error = "Checkpoint not found." });
    }

    [HttpGet("checkpoints/catalog/{id:long}/steps")]
    public async Task<IActionResult> GetCheckpointSteps(long id)
    {
        var steps = await _service.GetCheckpointStepsAsync(id);
        return Ok(steps);
    }

    [HttpPost("checkpoints/catalog/{id:long}/steps")]
    public async Task<IActionResult> SaveCheckpointSteps(long id, [FromBody] List<CheckpointStep> steps)
    {
        await _service.SaveCheckpointStepsAsync(id, steps);
        return Ok(new { saved = true });
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("instances/{id:long}")]
    public async Task<IActionResult> GetInstance(long id)
    {
        var instance = await _service.GetFlowInstanceByIdAsync(id);
        if (instance == null) return NotFound(new { error = $"Flow instance #{id} not found." });
        return Ok(instance);
    }

    [HttpGet("instances/{id:long}/checkpoints")]
    public async Task<IActionResult> GetInstanceCheckpoints(long id)
    {
        var checkpoints = await _service.GetCheckpointInstancesForFlowAsync(id);
        return Ok(checkpoints);
    }

    [HttpGet("instances/by-entity/{entityType}/{entityId:long}")]
    public async Task<IActionResult> GetByEntity(string entityType, long entityId)
    {
        var instanceWithCps = await _service.GetFlowInstanceWithCheckpointsByEntityAsync(entityType, entityId);
        if (instanceWithCps == null) return NotFound(new { error = $"No flow instance found for {entityType} #{entityId}." });
        return Ok(instanceWithCps);
    }

    [HttpPost("instances/{id:long}/facts")]
    public async Task<IActionResult> SetFacts(long id, [FromBody] SetFactsRequest req)
    {
        await _service.SetFlowInstanceFactsAsync(id, req.FactsJson, req.ActorId ?? 1);
        return Ok(new { updated = true });
    }

    [HttpPost("checkpoints/instances/{id:long}/resolve")]
    public async Task<IActionResult> ResolveCheckpoint(long id, [FromBody] ResolveCheckpointRequest req)
    {
        var result = await _service.ResolveCheckpointAsync(id, req.Status, req.ActorId ?? 1);
        return Ok(result);
    }

    [HttpGet("checkpoints/instances/{id:long}/steps")]
    public async Task<IActionResult> GetStepProgress(long id)
    {
        var steps = await _service.GetStepProgressAsync(id);
        return Ok(steps);
    }

    [HttpPost("checkpoints/instances/{cpInstanceId:long}/steps/{stepId:long}/toggle")]
    public async Task<IActionResult> ToggleStepProgress(long cpInstanceId, long stepId, [FromBody] ToggleStepRequest req)
    {
        await _service.ToggleStepProgressAsync(cpInstanceId, stepId, req.IsCompleted, req.ActorId ?? 1);
        return Ok(new { cpInstanceId, stepId, isCompleted = req.IsCompleted });
    }
}

public record ApproveCheckpointRequest(string ApprovedByJson, long? ActorId);
public record StartFlowRequest(string FlowCode, string EntityType, long EntityId, long? ActorId);
public record AdvanceStageRequest(long? ActorId);
public record ResolveCheckpointRequest(string Status, long? ActorId);
public record SetOrderRequest(short OrderIndex);
public record UpdateCampaignRequest(string Campaign);
public record UpdatePortfolioRequest(string Portfolio);
public record UpdateStageIdRequest(long? StageId);
public record SetFactsRequest(string FactsJson, long? ActorId);
public record ToggleStepRequest(bool IsCompleted, long? ActorId);
