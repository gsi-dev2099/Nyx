using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.ApiHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Nyx.FlowEngine.Domain.Entities;

namespace CRM.ApiHub.Api.Controllers;

[ApiController]
[Route("api/engines")]
[Authorize]
public class EngineManagementController : ControllerBase
{
    private readonly ISlaEngineClient _slaClient;
    private readonly IFlowEngineClient _flowClient;
    private readonly IApprovalEngineClient _approvalClient;
    private readonly Nyx.FlowEngine.Application.IFlowService? _inProcessFlowService;
    private readonly HttpClient _rawHttpClient;
    private readonly ILogger<EngineManagementController> _logger;
    private readonly string _slaBaseUrl;
    private readonly string _flowBaseUrl;
    private readonly string _approvalBaseUrl;

    public EngineManagementController(
        ISlaEngineClient slaClient,
        IFlowEngineClient flowClient,
        IApprovalEngineClient approvalClient,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<EngineManagementController> logger,
        IServiceProvider serviceProvider)
    {
        _slaClient = slaClient;
        _flowClient = flowClient;
        _approvalClient = approvalClient;
        _rawHttpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _inProcessFlowService = serviceProvider.GetService<Nyx.FlowEngine.Application.IFlowService>();

        _slaBaseUrl = config["SlaEngineSettings:BaseUrl"] ?? "http://sla_engine_api:5070";
        _flowBaseUrl = config["FlowEngineSettings:BaseUrl"] ?? "http://flow_engine_api:5072";
        _approvalBaseUrl = config["ApprovalEngineSettings:BaseUrl"] ?? "http://approval_engine_api:5071";
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var slaStatus = await CheckHealthAsync($"{_slaBaseUrl}/health");
        var approvalStatus = await CheckHealthAsync($"{_approvalBaseUrl}/health");
        var flowStatus = await CheckHealthAsync($"{_flowBaseUrl}/health");

        // Si los servicios in-process están registrados, reportar saludable
        if (_inProcessFlowService != null) flowStatus = true;

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            engines = new[]
            {
                new { name = "Nyx.SlaEngine", baseUrl = _slaBaseUrl, database = "nyx_sla", isHealthy = slaStatus, endpoint = "/api/sla" },
                new { name = "Nyx.ApprovalEngine", baseUrl = _approvalBaseUrl, database = "nyx_approval", isHealthy = approvalStatus, endpoint = "/api/approval" },
                new { name = "Nyx.FlowEngine", baseUrl = _flowBaseUrl, database = "nyx_flow", isHealthy = flowStatus, endpoint = "/api/flow" }
            }
        });
    }

    [HttpGet("flow/stages")]
    public async Task<IActionResult> GetFlowStages([FromQuery] long? flowId = null)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var stages = await _inProcessFlowService.GetStagesAsync(flowId);
                return Ok(stages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process GetStagesAsync failed, attempting HTTP fallback.");
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var url = flowId.HasValue 
                ? $"{_flowBaseUrl}/api/flow/stages?flowId={flowId.Value}" 
                : $"{_flowBaseUrl}/api/flow/stages";
            var response = await _rawHttpClient.GetAsync(url, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
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
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> CreateFlowStage([FromBody] JsonElement payload)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var stage = new FlowStage
                {
                    IdFlow = payload.TryGetProperty("IdFlow", out var idf) && idf.TryGetInt64(out var idfl) ? idfl : (payload.TryGetProperty("idFlow", out var idf2) && idf2.TryGetInt64(out var idfl2) ? idfl2 : 1L),
                    StageCode = (payload.TryGetProperty("StageCode", out var sc) ? sc.GetString() : (payload.TryGetProperty("stageCode", out var sc2) ? sc2.GetString() : "STAGE")) ?? "STAGE",
                    Name = (payload.TryGetProperty("Name", out var nm) ? nm.GetString() : (payload.TryGetProperty("name", out var nm2) ? nm2.GetString() : "Nueva Etapa")) ?? "Nueva Etapa",
                    Description = payload.TryGetProperty("Description", out var ds) ? ds.GetString() : (payload.TryGetProperty("description", out var ds2) ? ds2.GetString() : null),
                    OrderIndex = payload.TryGetProperty("OrderIndex", out var oi) && oi.TryGetInt16(out var oiv) ? oiv : (payload.TryGetProperty("orderIndex", out var oi2) && oi2.TryGetInt16(out var oiv2) ? oiv2 : (short)1),
                    IsTerminal = payload.TryGetProperty("IsTerminal", out var it) && it.GetBoolean() || (payload.TryGetProperty("isTerminal", out var it2) && it2.GetBoolean()),
                    SlaHours = payload.TryGetProperty("SlaHours", out var sh) && sh.TryGetInt16(out var shv) ? shv : (payload.TryGetProperty("slaHours", out var sh2) && sh2.TryGetInt16(out var shv2) ? shv2 : (short?)null),
                    Portfolio = (payload.TryGetProperty("Portfolio", out var pf) ? pf.GetString() : (payload.TryGetProperty("portfolio", out var pf2) ? pf2.GetString() : "GENERAL")) ?? "GENERAL",
                    Campaign = (payload.TryGetProperty("Campaign", out var cp) ? cp.GetString() : (payload.TryGetProperty("campaign", out var cp2) ? cp2.GetString() : "GENERAL")) ?? "GENERAL",
                    Metadata = "{}"
                };
                var created = await _inProcessFlowService.CreateStageAsync(stage);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process CreateStageAsync failed, attempting HTTP fallback.");
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PostAsync($"{_flowBaseUrl}/api/flow/stages", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error creating stage in FlowEngine");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}/move")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> MoveFlowStage(long id, [FromQuery] string direction)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.PatchAsync(
                $"{_flowBaseUrl}/api/flow/stages/{id}/move?direction={direction}",
                null, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error moving stage");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}/order")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> SetFlowStageOrder(long id, [FromBody] JsonElement payload)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                short orderIndex = payload.TryGetProperty("OrderIndex", out var oi) && oi.TryGetInt16(out var oiv) ? oiv : (payload.TryGetProperty("orderIndex", out var oi2) && oi2.TryGetInt16(out var oiv2) ? oiv2 : (short)1);
                var ok = await _inProcessFlowService.SetStageOrderAsync(id, orderIndex);
                if (ok) return Ok(new { updated = true, idStage = id, orderIndex });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process SetStageOrderAsync failed, attempting HTTP fallback.");
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"{_flowBaseUrl}/api/flow/stages/{id}/order", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error setting stage order");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpPatch("flow/stages/{id:long}")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> UpdateFlowStage(long id, [FromBody] JsonElement payload)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var stage = new FlowStage
                {
                    IdStage = id,
                    Name = (payload.TryGetProperty("Name", out var nm) ? nm.GetString() : (payload.TryGetProperty("name", out var nm2) ? nm2.GetString() : "")) ?? "",
                    Description = payload.TryGetProperty("Description", out var ds) ? ds.GetString() : (payload.TryGetProperty("description", out var ds2) ? ds2.GetString() : null),
                    SlaHours = payload.TryGetProperty("SlaHours", out var sh) && sh.TryGetInt16(out var shv) ? shv : (payload.TryGetProperty("slaHours", out var sh2) && sh2.TryGetInt16(out var shv2) ? shv2 : (short?)null),
                    IsTerminal = payload.TryGetProperty("IsTerminal", out var it) && it.GetBoolean() || (payload.TryGetProperty("isTerminal", out var it2) && it2.GetBoolean()),
                    Portfolio = (payload.TryGetProperty("Portfolio", out var pf) ? pf.GetString() : (payload.TryGetProperty("portfolio", out var pf2) ? pf2.GetString() : "GENERAL")) ?? "GENERAL",
                    Campaign = (payload.TryGetProperty("Campaign", out var cp) ? cp.GetString() : (payload.TryGetProperty("campaign", out var cp2) ? cp2.GetString() : "GENERAL")) ?? "GENERAL"
                };
                var ok = await _inProcessFlowService.UpdateStageAsync(stage);
                if (ok) return Ok(new { updated = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process UpdateStageAsync failed, attempting HTTP fallback.");
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"{_flowBaseUrl}/api/flow/stages/{id}", content, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            return response.IsSuccessStatusCode ? Content(body, "application/json") : StatusCode((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EngineManagementController] Error updating stage");
            return StatusCode(503, new { error = "Nyx.FlowEngine no disponible." });
        }
    }

    [HttpGet("flow/catalogs")]
    public async Task<IActionResult> GetFlowCatalogs([FromQuery] long? flowId)
    {
        var catalog = await _flowClient.GetFullCatalogAsync(flowId);
        return Ok(catalog);
    }

    [HttpGet("flow/catalogs/full")]
    public async Task<IActionResult> GetFlowCatalogsFull([FromQuery] long? flowId)
    {
        var catalog = await _flowClient.GetFullCatalogAsync(flowId);
        return Ok(catalog);
    }

    [HttpPost("flow/catalogs")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> CreateFlowCatalog([FromBody] JsonElement payload)
    {
        var created = await _flowClient.CreateCheckpointCatalogAsync(payload);
        if (created != null)
        {
            return Ok(created);
        }
        return BadRequest(new { error = "No se pudo registrar el checkpoint en Nyx.FlowEngine." });
    }

    [HttpPut("flow/catalogs/{id:long}")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> UpdateFlowCatalog(long id, [FromBody] JsonElement payload)
    {
        var ok = await _flowClient.UpdateCheckpointCatalogAsync(id, payload);
        if (ok) return Ok(new { updated = true });
        return BadRequest(new { error = "No se pudo actualizar el checkpoint en Nyx.FlowEngine." });
    }

    [HttpPatch("flow/catalogs/{id:long}/stage")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> UpdateCatalogStage(long id, [FromBody] JsonElement payload)
    {
        try
        {
            long? stageId = null;
            if (payload.TryGetProperty("stageId", out var stProp) && stProp.ValueKind == JsonValueKind.Number)
            {
                stageId = stProp.GetInt64();
            }
            else if (payload.TryGetProperty("StageId", out var stProp2) && stProp2.ValueKind == JsonValueKind.Number)
            {
                stageId = stProp2.GetInt64();
            }

            if (_inProcessFlowService != null)
            {
                var ok = await _inProcessFlowService.UpdateCheckpointStageAsync(id, stageId);
                return ok ? Ok(new { updated = true }) : BadRequest(new { error = "No se pudo asignar etapa." });
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"{_flowBaseUrl}/api/flow/checkpoints/catalog/{id}/stage", content);
            return response.IsSuccessStatusCode ? Ok(new { updated = true }) : StatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating catalog stage");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPatch("flow/catalogs/{id:long}/campaign")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> UpdateCatalogCampaign(long id, [FromBody] JsonElement payload)
    {
        try
        {
            var campaign = (payload.TryGetProperty("Campaign", out var cProp) ? cProp.GetString() : (payload.TryGetProperty("campaign", out var cProp2) ? cProp2.GetString() : "GENERAL")) ?? "GENERAL";
            if (_inProcessFlowService != null)
            {
                var ok = await _inProcessFlowService.UpdateCheckpointCampaignAsync(id, campaign);
                return ok ? Ok(new { updated = true }) : BadRequest(new { error = "No se pudo actualizar campaña." });
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"{_flowBaseUrl}/api/flow/checkpoints/catalog/{id}/campaign", content);
            return response.IsSuccessStatusCode ? Ok(new { updated = true }) : StatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating catalog campaign");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPatch("flow/catalogs/{id:long}/portfolio")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> UpdateCatalogPortfolio(long id, [FromBody] JsonElement payload)
    {
        try
        {
            var portfolio = (payload.TryGetProperty("Portfolio", out var pProp) ? pProp.GetString() : (payload.TryGetProperty("portfolio", out var pProp2) ? pProp2.GetString() : "GENERAL")) ?? "GENERAL";
            if (_inProcessFlowService != null)
            {
                var ok = await _inProcessFlowService.UpdateCheckpointPortfolioAsync(id, portfolio);
                return ok ? Ok(new { updated = true }) : BadRequest(new { error = "No se pudo actualizar cartera." });
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _rawHttpClient.PatchAsync($"{_flowBaseUrl}/api/flow/checkpoints/catalog/{id}/portfolio", content);
            return response.IsSuccessStatusCode ? Ok(new { updated = true }) : StatusCode((int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating catalog portfolio");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("flow/catalogs/{id:long}/steps")]
    [HttpGet("flow/checkpoints/catalog/{id:long}/steps")]
    public async Task<IActionResult> GetCheckpointSteps(long id)
    {
        if (_inProcessFlowService != null)
        {
            var steps = await _inProcessFlowService.GetCheckpointStepsAsync(id);
            return Ok(steps);
        }
        var stepsRemote = await _flowClient.GetCheckpointStepsAsync(id);
        return Ok(stepsRemote);
    }

    [HttpPost("flow/catalogs/{id:long}/steps")]
    [HttpPost("flow/checkpoints/catalog/{id:long}/steps")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> SaveCheckpointSteps(long id, [FromBody] JsonElement payload)
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
        if (_inProcessFlowService != null)
        {
            try
            {
                var inst = await _inProcessFlowService.GetFlowInstanceByIdAsync(id);
                if (inst != null) return Ok(inst);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process GetFlowInstanceByIdAsync failed, attempting HTTP fallback.");
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _rawHttpClient.GetAsync($"{_flowBaseUrl}/api/flow/instances/{id}", cts.Token);
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


    [HttpPost("flow/instances/start")]
    [Authorize(Roles = "ASESOR,SUPERVISOR,BACKOFFICE,ADMINISTRADOR,ADMIN_CRM,COORDINADOR,CALIDAD")]
    public async Task<IActionResult> StartFlowInstance([FromBody] JsonElement payload)
    {
        try
        {
            var flowCode = payload.TryGetProperty("flowCode", out var fc) ? fc.GetString() ?? "FLOW_TELCO_SALES_001" : "FLOW_TELCO_SALES_001";
            var entityType = payload.TryGetProperty("entityType", out var et) ? et.GetString() ?? "lead_presale" : "lead_presale";
            long entityId = 0;
            if (payload.TryGetProperty("entityId", out var ei))
            {
                if (ei.ValueKind == JsonValueKind.Number) entityId = ei.GetInt64();
                else if (ei.ValueKind == JsonValueKind.String && long.TryParse(ei.GetString(), out var parsedEid)) entityId = parsedEid;
            }
            long actorId = payload.TryGetProperty("actorId", out var act) && act.TryGetInt64(out var actv) ? actv : 1;

            if (_inProcessFlowService != null)
            {
                var inst = await _inProcessFlowService.StartFlowInstanceAsync(flowCode, entityType, entityId, actorId);
                var fullInst = await _inProcessFlowService.GetFlowInstanceWithCheckpointsByEntityAsync(entityType, entityId);
                return Ok(fullInst ?? inst);
            }

            var res = await _flowClient.StartFlowInstanceAsync(flowCode, entityType, entityId, actorId);
            var instWithCp = await _flowClient.GetInstanceWithCheckpointsByEntityAsync(entityType, entityId);
            return Ok(instWithCp ?? (object?)res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting flow instance");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("flow/instances/{id:long}/detail")]
    public async Task<IActionResult> GetFlowInstanceDetail(long id)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var inst = await _inProcessFlowService.GetFlowInstanceDetailByIdAsync(id);
                if (inst != null) return Ok(inst);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process GetFlowInstanceDetailByIdAsync failed, attempting HTTP fallback.");
            }
        }

        var detail = await _flowClient.GetFlowInstanceDetailByIdAsync(id);
        if (detail == null) return NotFound(new { error = $"Flow instance #{id} not found or detail unavailable." });
        return Ok(detail);
    }

    [HttpGet("flow/instances/by-entity/{entityType}/{entityId:long}/detail")]
    public async Task<IActionResult> GetFlowInstanceDetailByEntity(string entityType, long entityId)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var inst = await _inProcessFlowService.GetFlowInstanceDetailByEntityAsync(entityType, entityId);
                if (inst != null) return Ok(inst);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process GetFlowInstanceDetailByEntityAsync failed, attempting HTTP fallback.");
            }
        }

        var detail = await _flowClient.GetFlowInstanceDetailByEntityAsync(entityType, entityId);
        if (detail == null) return NotFound(new { error = $"No flow instance detail found for {entityType} #{entityId}." });
        return Ok(detail);
    }

    [HttpGet("flow/instances/{id:long}/validate-advance")]
    public async Task<IActionResult> ValidateFlowAdvance(long id)
    {
        if (_inProcessFlowService != null)
        {
            try
            {
                var val = await _inProcessFlowService.ValidateStageAdvanceAsync(id);
                return Ok(val);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EngineManagementController] In-process ValidateStageAdvanceAsync failed, attempting HTTP fallback.");
            }
        }

        var result = await _flowClient.ValidateStageAdvanceAsync(id);
        if (result == null) return NotFound(new { error = $"Validation unavailable for flow instance #{id}." });
        return Ok(result);
    }

    [HttpGet("flow/instances/by-entity/{entityType}/{entityId:long}")]
    public async Task<IActionResult> GetFlowInstanceByEntity(string entityType, long entityId)
    {
        if (_inProcessFlowService != null)
        {
            var inst = await _inProcessFlowService.GetFlowInstanceWithCheckpointsByEntityAsync(entityType, entityId);
            if (inst != null) return Ok(inst);
        }
        var instance = await _flowClient.GetInstanceWithCheckpointsByEntityAsync(entityType, entityId);
        if (instance == null) return NotFound(new { error = $"No flow instance found for {entityType} #{entityId}." });
        return Ok(instance);
    }

    [HttpPost("flow/instances/{cpInstanceId:long}/resolve")]
    [HttpPost("flow/checkpoints/instances/{cpInstanceId:long}/resolve")]
    [Authorize(Roles = "ASESOR,SUPERVISOR,BACKOFFICE,ADMINISTRADOR,ADMIN_CRM,COORDINADOR,CALIDAD")]
    public async Task<IActionResult> ResolveCheckpoint(long cpInstanceId, [FromBody] JsonElement payload)
    {
        try
        {
            var status = payload.TryGetProperty("status", out var stProp) ? stProp.GetString() ?? "APPROVED" : "APPROVED";
            var actorId = payload.TryGetProperty("actorId", out var actProp) && actProp.TryGetInt64(out var act) ? act : 1;
            if (_inProcessFlowService != null)
            {
                var res = await _inProcessFlowService.ResolveCheckpointAsync(cpInstanceId, status, actorId);
                return Ok(res);
            }
            var result = await _flowClient.ResolveCheckpointDetailedAsync(cpInstanceId, status, actorId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving checkpoint instance #{CpInstanceId}", cpInstanceId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("flow/checkpoints/instances/{cpInstanceId:long}/steps/{stepId:long}/toggle")]
    [Authorize(Roles = "ASESOR,SUPERVISOR,BACKOFFICE,ADMINISTRADOR,ADMIN_CRM,COORDINADOR,CALIDAD")]
    public async Task<IActionResult> ToggleStepProgress(long cpInstanceId, long stepId, [FromBody] JsonElement payload)
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
    public async Task<IActionResult> GetStepProgress(long cpInstanceId)
    {
        var steps = await _flowClient.GetStepProgressAsync(cpInstanceId);
        return Ok(steps);
    }

    [HttpPost("flow/instances/{id:long}/advance")]
    [Authorize(Roles = "ADMIN_CRM,ADMINISTRADOR,SUPERVISOR,BACKOFFICE,COORDINADOR")]
    public async Task<IActionResult> AdvanceFlowInstance(long id, [FromBody] JsonElement payload)
    {
        var actorId = payload.TryGetProperty("actorId", out var actProp) && actProp.TryGetInt64(out var act) ? act : 1;
        var result = await _flowClient.AdvanceStageAsync(id, actorId);
        return Ok(result);
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
