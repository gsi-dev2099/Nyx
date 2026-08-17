using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Infrastructure.Services;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/engines")]
[Authorize]
public class EngineManagementController : ControllerBase
{
    private readonly ISlaEngineClient _slaClient;
    private readonly IFlowEngineClient _flowClient;
    private readonly IApprovalEngineClient _approvalClient;
    private readonly HttpClient _rawHttpClient;
    private readonly ILogger<EngineManagementController> _logger;

    public EngineManagementController(
        ISlaEngineClient slaClient,
        IFlowEngineClient flowClient,
        IApprovalEngineClient approvalClient,
        IHttpClientFactory httpClientFactory,
        ILogger<EngineManagementController> logger)
    {
        _slaClient = slaClient;
        _flowClient = flowClient;
        _approvalClient = approvalClient;
        _rawHttpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var slaStatus = await CheckHealthAsync("http://sla_engine_api:5070/health");
        var approvalStatus = await CheckHealthAsync("http://approval_engine_api:5071/health");
        var flowStatus = await CheckHealthAsync("http://flow_engine_api:5072/health");

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            engines = new[]
            {
                new { name = "Nyx.SlaEngine", port = 5070, database = "nyx_sla", isHealthy = slaStatus, endpoint = "/api/sla" },
                new { name = "Nyx.ApprovalEngine", port = 5071, database = "nyx_approval", isHealthy = approvalStatus, endpoint = "/api/approval" },
                new { name = "Nyx.FlowEngine", port = 5072, database = "nyx_flow", isHealthy = flowStatus, endpoint = "/api/flow" }
            }
        });
    }

    [HttpGet("flow/stages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlowStages()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.GetAsync("http://flow_engine_api:5072/api/flow/stages", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error fetching stages from FlowEngine");
        }
        return StatusCode(503, new { error = "Nyx.FlowEngine no disponible. Los datos de etapas provienen exclusivamente de nyx_flow.stage." });
    }

    [HttpPost("flow/stages")]
    public async Task<IActionResult> CreateFlowStage([FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PostAsync("http://flow_engine_api:5072/api/flow/stages", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error creating stage in FlowEngine");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}/move")]
    public async Task<IActionResult> MoveFlowStage(long id, [FromQuery] string direction)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.PatchAsync(
                $"http://flow_engine_api:5072/api/flow/stages/{id}/move?direction={direction}",
                null, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error moving stage");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}/order")]
    public async Task<IActionResult> SetFlowStageOrder(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"http://flow_engine_api:5072/api/flow/stages/{id}/order", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error setting stage order");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}")]
    public async Task<IActionResult> UpdateFlowStage(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"http://flow_engine_api:5072/api/flow/stages/{id}", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error updating stage");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/catalogs/{id:long}/campaign")]
    public async Task<IActionResult> UpdateCheckpointCampaign(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"http://flow_engine_api:5072/api/flow/checkpoints/catalog/{id}/campaign", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error updating checkpoint campaign");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/catalogs/{id:long}/portfolio")]
    public async Task<IActionResult> UpdateCheckpointPortfolio(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"http://flow_engine_api:5072/api/flow/checkpoints/catalog/{id}/portfolio", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error updating checkpoint portfolio");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/catalogs/{id:long}/stage")]
    public async Task<IActionResult> UpdateCheckpointStage(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"http://flow_engine_api:5072/api/flow/checkpoints/catalog/{id}/stage", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error updating checkpoint stage");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpGet("flow/audit")]
    public async Task<IActionResult> GetFlowAuditLogs([FromQuery] int limit = 50)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.GetAsync($"http://flow_engine_api:5072/api/flow/audit?limit={limit}", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error fetching audit logs from FlowEngine");
        }
        return Ok(Array.Empty<object>());
    }

    [HttpGet("flow/catalogs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlowCatalogs([FromQuery] long? flowId)
    {
        var catalog = await _flowClient.GetCheckpointCatalogAsync(flowId);
        return Ok(catalog);
    }

    [HttpPost("flow/catalogs")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFlowCatalog([FromBody] System.Text.Json.JsonElement payload)
    {
        var created = await _flowClient.CreateCheckpointCatalogAsync(payload);
        if (created != null)
        {
            return Ok(created);
        }
        return BadRequest(new { error = "No se pudo registrar el checkpoint en Nyx.FlowEngine." });
    }

    [HttpPut("flow/catalogs/{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateFlowCatalog(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        var ok = await _flowClient.UpdateCheckpointCatalogAsync(id, payload);
        if (ok) return Ok(new { updated = true });
        return BadRequest(new { error = "No se pudo actualizar el checkpoint en Nyx.FlowEngine." });
    }

    [HttpGet("flow/catalogs/{id:long}/steps")]
    [HttpGet("flow/checkpoints/catalog/{id:long}/steps")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCheckpointSteps(long id)
    {
        var steps = await _flowClient.GetCheckpointStepsAsync(id);
        return Ok(steps);
    }

    [HttpPost("flow/catalogs/{id:long}/steps")]
    [HttpPost("flow/checkpoints/catalog/{id:long}/steps")]
    [AllowAnonymous]
    public async Task<IActionResult> SaveCheckpointSteps(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        var ok = await _flowClient.SaveCheckpointStepsAsync(id, payload);
        if (ok) return Ok(new { saved = true });
        return BadRequest(new { error = "No se pudieron guardar los pasos del checkpoint." });
    }

    [HttpGet("approval/pending")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] long approverId = 9, [FromQuery] string approverRole = "SUPERVISOR")
    {
        var pending = await _approvalClient.GetPendingApprovalsAsync(approverId, approverRole);
        return Ok(pending);
    }

    [HttpGet("flow/instances/{id:long}")]
    public async Task<IActionResult> GetFlowInstance(long id)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.GetAsync($"http://flow_engine_api:5072/api/flow/instances/{id}", cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, new { error = "Instance not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { error = $"FlowEngine unreachable: {ex.Message}" });
        }
    }

    [HttpGet("flow/catalogs/full")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFullFlowCatalogs([FromQuery] long? flowId)
    {
        var fullCatalog = await _flowClient.GetFullCatalogAsync(flowId);
        return Ok(fullCatalog);
    }

    [HttpGet("flow/instances/by-entity/{entityType}/{entityId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlowInstanceByEntity(string entityType, long entityId)
    {
        var instance = await _flowClient.GetInstanceWithCheckpointsByEntityAsync(entityType, entityId);
        if (instance == null) return NotFound(new { error = $"No flow instance found for {entityType} #{entityId}." });
        return Ok(instance);
    }

    [HttpPost("flow/instances/{cpInstanceId:long}/resolve")]
    [HttpPost("flow/checkpoints/instances/{cpInstanceId:long}/resolve")]
    [AllowAnonymous]
    public async Task<IActionResult> ResolveCheckpoint(long cpInstanceId, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            var status = payload.TryGetProperty("status", out var stProp) ? stProp.GetString() ?? "SUBSANADO" : "SUBSANADO";
            var actorId = payload.TryGetProperty("actorId", out var actProp) && actProp.TryGetInt64(out var act) ? act : 1;
            var result = await _flowClient.ResolveCheckpointAsync(cpInstanceId, status, actorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving checkpoint instance #{CpInstanceId}", cpInstanceId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("flow/checkpoints/instances/{cpInstanceId:long}/steps/{stepId:long}/toggle")]
    [AllowAnonymous]
    public async Task<IActionResult> ToggleStepProgress(long cpInstanceId, long stepId, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            var isCompleted = payload.TryGetProperty("isCompleted", out var icProp) && icProp.GetBoolean();
            var actorId = payload.TryGetProperty("actorId", out var actProp) && actProp.TryGetInt64(out var act) ? act : 1;
            var ok = await _flowClient.ToggleStepProgressAsync(cpInstanceId, stepId, isCompleted, actorId);
            return ok ? Ok(new { cpInstanceId, stepId, isCompleted }) : BadRequest(new { error = "No se pudo actualizar el paso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling step progress for CP #{CpInstanceId}, Step #{StepId}", cpInstanceId, stepId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("flow/checkpoints/instances/{cpInstanceId:long}/steps")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStepProgress(long cpInstanceId)
    {
        var steps = await _flowClient.GetStepProgressAsync(cpInstanceId);
        return Ok(steps);
    }

    [HttpPost("flow/instances/{id:long}/advance")]
    [AllowAnonymous]
    public async Task<IActionResult> AdvanceFlowInstance(long id, [FromBody] System.Text.Json.JsonElement payload)
    {
        try
        {
            var actorId = payload.TryGetProperty("actorId", out var actProp) && actProp.TryGetInt64(out var act) ? act : 1;
            var result = await _flowClient.AdvanceStageAsync(id, actorId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<bool> CheckHealthAsync(string healthUrl)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = await _rawHttpClient.GetAsync(healthUrl, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
