using Microsoft.AspNetCore.Mvc;
using Nyx.FlowEngine.Application;
using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Controllers;

[ApiController]
[Route("api/cycles")]
public class CycleController : ControllerBase
{
    private readonly ICycleService _service;

    public CycleController(ICycleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetCycles([FromQuery] bool includeInactive = false)
    {
        var cycles = await _service.GetCyclesAsync(includeInactive);
        return Ok(cycles);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCycleDetail(long id, [FromQuery] bool includeInactive = false)
    {
        var detail = await _service.GetCycleDetailAsync(id, includeInactive);
        return detail != null ? Ok(detail) : NotFound(new { error = "Ciclo no encontrado." });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCycle([FromBody] CycleDefinition cycle)
    {
        var created = await _service.CreateCycleAsync(cycle);
        return CreatedAtAction(nameof(GetCycleDetail), new { id = created.IdCycle }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateCycle(long id, [FromBody] CycleDefinition cycle)
    {
        var ok = await _service.UpdateCycleAsync(id, cycle);
        return ok ? Ok(new { updated = true }) : NotFound();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> SoftDeleteCycle(long id)
    {
        var ok = await _service.SoftDeleteCycleAsync(id);
        return ok ? Ok(new { deleted = true }) : NotFound();
    }

    // ==========================================
    // ETAPAS DEL CICLO
    // ==========================================
    [HttpGet("{id:long}/stages")]
    public async Task<IActionResult> GetStages(long id)
    {
        var stages = await _service.GetStagesByCycleAsync(id);
        return Ok(stages);
    }

    [HttpPost("{id:long}/stages")]
    public async Task<IActionResult> CreateStage(long id, [FromBody] CycleStage stage)
    {
        stage.IdCycle = id;
        var created = await _service.CreateStageAsync(stage);
        return Ok(created);
    }

    [HttpPatch("{id:long}/stages/reorder")]
    public async Task<IActionResult> ReorderStages(long id, [FromBody] List<long> stageIdsInOrder)
    {
        var ok = await _service.ReorderStagesAsync(id, stageIdsInOrder);
        return Ok(new { reordered = ok });
    }

    [HttpPut("stages/{stageId:long}")]
    public async Task<IActionResult> UpdateStage(long stageId, [FromBody] CycleStage stage)
    {
        var ok = await _service.UpdateStageAsync(stageId, stage);
        return ok ? Ok(new { updated = true }) : NotFound();
    }

    [HttpDelete("stages/{stageId:long}")]
    public async Task<IActionResult> DeleteStage(long stageId)
    {
        var ok = await _service.DeleteStageAsync(stageId);
        return ok ? Ok(new { deleted = true }) : NotFound();
    }

    // ==========================================
    // CHECKPOINTS
    // ==========================================
    [HttpGet("{id:long}/checkpoints")]
    public async Task<IActionResult> GetCheckpoints(long id, [FromQuery] bool includeInactive = false)
    {
        var cps = await _service.GetFullCheckpointsByCycleAsync(id, includeInactive);
        return Ok(cps);
    }

    [HttpGet("checkpoints/{cpId:long}")]
    public async Task<IActionResult> GetCheckpointById(long cpId)
    {
        var cp = await _service.GetFullCheckpointByIdAsync(cpId);
        return cp != null ? Ok(cp) : NotFound();
    }

    [HttpPost("{id:long}/checkpoints")]
    public async Task<IActionResult> CreateCheckpoint(long id, [FromBody] SaveCheckpointDto cp)
    {
        cp.IdCycle = id;
        var created = await _service.CreateCheckpointAsync(cp);
        return Ok(created);
    }

    [HttpPut("checkpoints/{cpId:long}")]
    public async Task<IActionResult> UpdateCheckpoint(long cpId, [FromBody] SaveCheckpointDto cp)
    {
        var ok = await _service.UpdateCheckpointAsync(cpId, cp);
        return ok ? Ok(new { updated = true }) : NotFound();
    }

    [HttpDelete("checkpoints/{cpId:long}")]
    public async Task<IActionResult> SoftDeleteCheckpoint(long cpId)
    {
        var ok = await _service.SoftDeleteCheckpointAsync(cpId);
        return ok ? Ok(new { deleted = true }) : NotFound();
    }

    [HttpPatch("checkpoints/{cpId:long}/toggle-active")]
    public async Task<IActionResult> ToggleCheckpointActive(long cpId)
    {
        var active = await _service.ToggleCheckpointActiveAsync(cpId);
        return Ok(new { isActive = active });
    }

    [HttpPost("checkpoints/{cpId:long}/steps")]
    public async Task<IActionResult> SaveSteps(long cpId, [FromBody] List<CheckpointStep> steps)
    {
        await _service.SaveCheckpointStepsAsync(cpId, steps);
        return Ok(new { saved = true });
    }

    // ==========================================
    // METADATOS Y CONCILIACIÓN (ROLES Y CARTERAS)
    // ==========================================
    [HttpGet("meta/roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _service.GetMetaRolesAsync();
        return Ok(roles);
    }

    [HttpPost("meta/roles")]
    public async Task<IActionResult> CreateRole([FromBody] MetaRole role)
    {
        var created = await _service.CreateMetaRoleAsync(role);
        return Ok(created);
    }

    [HttpGet("meta/portfolios")]
    public async Task<IActionResult> GetPortfolios()
    {
        var portfolios = await _service.GetMetaPortfoliosAsync();
        return Ok(portfolios);
    }

    [HttpPost("meta/portfolios")]
    public async Task<IActionResult> CreatePortfolio([FromBody] MetaPortfolio portfolio)
    {
        var created = await _service.CreateMetaPortfolioAsync(portfolio);
        return Ok(created);
    }

    [HttpGet("meta/campaigns")]
    public async Task<IActionResult> GetCampaigns()
    {
        var campaigns = await _service.GetMetaCampaignsAsync();
        return Ok(campaigns);
    }

    [HttpPost("meta/campaigns")]
    public async Task<IActionResult> CreateCampaign([FromBody] MetaCampaign campaign)
    {
        var created = await _service.CreateMetaCampaignAsync(campaign);
        return Ok(created);
    }

    [HttpPost("checkpoints/{cpId:long}/canvas")]
    public async Task<IActionResult> SaveCanvasSchema(long cpId, [FromBody] CanvasSchemaPayload payload)
    {
        await _service.SaveCheckpointCanvasSchemaAsync(cpId, payload.SchemaJson);
        return Ok(new { saved = true });
    }

    // ==========================================
    // IMPORTACIÓN Y EXPORTACIÓN JSON (GSI BACKUP)
    // ==========================================
    [HttpPost("{id:long}/import-gsi")]
    public async Task<IActionResult> ImportGsi(long id, [FromBody] ImportGsiRequest req)
    {
        try
        {
            var res = await _service.ImportGsiBackupJsonAsync(id, req.JsonContent);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:long}/export-gsi")]
    public async Task<IActionResult> ExportGsi(long id)
    {
        var json = await _service.ExportGsiBackupJsonAsync(id);
        return Content(json, "application/json");
    }

    // ==========================================
    // POLÍTICAS
    // ==========================================
    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies([FromQuery] long? cycleId = null)
    {
        var policies = await _service.GetPoliciesAsync(cycleId);
        return Ok(policies);
    }

    [HttpPost("policies")]
    public async Task<IActionResult> SavePolicy([FromBody] CyclePolicyRule rule)
    {
        var saved = await _service.SavePolicyRuleAsync(rule);
        return Ok(saved);
    }

    // ==========================================
    // INSTANCIAS Y CONTRATO ZERO-LOGIC UI
    // ==========================================
    [HttpPost("instances/start")]
    public async Task<IActionResult> StartInstance([FromBody] StartInstanceRequest req)
    {
        var instance = await _service.StartCycleInstanceAsync(req.CycleCode, req.EntityType, req.EntityId, req.ActorId);
        return Ok(instance);
    }

    [HttpGet("instances/{id:long}/ui-context")]
    public async Task<IActionResult> GetUiContext(long id, [FromQuery] long actorId = 101)
    {
        try
        {
            var ctx = await _service.GetUiContextAsync(id, actorId);
            return Ok(ctx);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("instances/{id:long}/execute-action")]
    public async Task<IActionResult> ExecuteAction(long id, [FromBody] ExecuteActionRequest req)
    {
        try
        {
            var result = await _service.ExecuteActionAsync(id, req);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("instances/{id:long}/simulate-time-advance")]
    public async Task<IActionResult> SimulateTimeAdvance(long id, [FromQuery] int days = 1)
    {
        var activated = await _service.SimulateTimeAdvanceAsync(id, days);
        return Ok(new { daysAdvanced = days, newlyActivatedCount = activated });
    }

    [HttpGet("instances/entity/{entityType}/{entityId:long}")]
    public async Task<IActionResult> GetInstanceByEntity(string entityType, long entityId)
    {
        var inst = await _service.GetInstanceDetailByEntityAsync(entityType, entityId);
        return inst != null ? Ok(inst) : NotFound(new { error = "Instancia de ciclo no encontrada para la entidad." });
    }

    [HttpGet("instances/{id:long}")]
    public async Task<IActionResult> GetInstanceById(long id)
    {
        var inst = await _service.GetInstanceDetailByIdAsync(id);
        return inst != null ? Ok(inst) : NotFound(new { error = "Instancia de ciclo no encontrada." });
    }

    [HttpPost("instances/{id:long}/advance")]
    public async Task<IActionResult> AdvanceStage(long id, [FromQuery] long actorId = 1)
    {
        try
        {
            var updated = await _service.AdvanceStageAsync(id, actorId);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("instances/checkpoints/{cpInstanceId:long}/resolve")]
    public async Task<IActionResult> ResolveCheckpoint(long cpInstanceId, [FromBody] ResolveCheckpointRequest req)
    {
        var result = await _service.ResolveCheckpointAsync(cpInstanceId, req.Status, req.AnswersJson ?? "{}", req.ActorId);
        return Ok(result);
    }

    // ==========================================
    // HANDSHAKE TELEFONÍA & OWNERSHIP
    // ==========================================
    [HttpPost("instances/{id:long}/handshake/request")]
    public async Task<IActionResult> RequestHandshake(long id, [FromBody] HandshakeActionRequest req)
    {
        var result = await _service.RequestHandshakeAsync(id, req.TargetActorId ?? 0, req.ActorId, req.Context);
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { error = result.Message });
    }

    [HttpPost("instances/{id:long}/handshake/accept")]
    public async Task<IActionResult> AcceptHandshake(long id, [FromBody] HandshakeActionRequest req)
    {
        var result = await _service.AcceptHandshakeAsync(id, req.ActorId);
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { error = result.Message });
    }

    [HttpPost("instances/{id:long}/handshake/cancel")]
    public async Task<IActionResult> CancelHandshake(long id, [FromBody] HandshakeActionRequest req)
    {
        var result = await _service.CancelHandshakeAsync(id, req.ActorId);
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { error = result.Message });
    }

    [HttpPost("instances/{id:long}/handshake/reject")]
    public async Task<IActionResult> RejectHandshake(long id, [FromBody] HandshakeActionRequest req)
    {
        var result = await _service.RejectHandshakeAsync(id, req.ActorId, req.Reason ?? "Rechazado");
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { error = result.Message });
    }

    [HttpPost("instances/{id:long}/handshake/revert")]
    public async Task<IActionResult> RevertHandshake(long id, [FromBody] HandshakeActionRequest req)
    {
        var result = await _service.RevertHandshakeAsync(id, req.ActorId, req.Reason ?? "Reversión al titular");
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { error = result.Message });
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 50)
    {
        var logs = await _service.GetAuditLogsAsync(limit);
        return Ok(logs);
    }
}

public record StartInstanceRequest(string CycleCode, string EntityType, long EntityId, long ActorId);
public record ResolveCheckpointRequest(string Status, string? AnswersJson, long ActorId);
public record CanvasSchemaPayload(string SchemaJson);
public record ImportGsiRequest(string JsonContent);
