using System.Text.Json;
using Nyx.FlowEngine.Domain.Entities;
using Nyx.FlowEngine.Infrastructure;

namespace Nyx.FlowEngine.Application;

public interface IFlowService
{
    Task<IEnumerable<FlowDefinition>> GetFlowDefinitionsAsync();
    Task<IEnumerable<FlowStage>> GetStagesAsync(long? flowId = null);
    Task<FlowStage> CreateStageAsync(FlowStage stage);
    Task<bool> MoveStageAsync(long stageId, string direction);
    Task<bool> SetStageOrderAsync(long stageId, short newOrderIndex);
    Task<bool> UpdateStageAsync(FlowStage stage);
    Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId);
    Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCheckpointCatalogAsync(long? flowId);
    Task<CheckpointCatalog> CreateCheckpointCatalogAsync(CheckpointCatalog cp);
    Task ApproveCheckpointCatalogAsync(long checkpointId, string approvedByJson, long actorId);
    Task<bool> UpdateCheckpointCampaignAsync(long checkpointId, string campaign);
    Task<bool> UpdateCheckpointPortfolioAsync(long checkpointId, string portfolio);
    Task<bool> UpdateCheckpointStageAsync(long checkpointId, long? stageId);
    Task<bool> UpdateCheckpointCatalogAsync(long id, CheckpointCatalog cp);
    Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId);
    Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps);
    Task<IEnumerable<FlowAuditLog>> GetAuditLogsAsync(int limit = 50);

    Task<FlowInstanceWithCheckpointsDto> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId);
    Task<FlowInstanceWithCheckpointsDto> AdvanceStageAsync(long instanceId, long actorId);
    Task<ResolveCheckpointResultDto> ResolveCheckpointAsync(long cpInstanceId, string status, long actorId);
    Task<FlowInstance?> GetFlowInstanceByIdAsync(long instanceId);
    Task<FlowInstance?> GetFlowInstanceByEntityAsync(string entityType, long entityId);
    Task<FlowInstanceWithCheckpointsDto?> GetFlowInstanceWithCheckpointsByEntityAsync(string entityType, long entityId);
    Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByIdAsync(long instanceId);
    Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByEntityAsync(string entityType, long entityId);
    Task<FlowValidationResultDto> ValidateStageAdvanceAsync(long instanceId);
    Task<FlowInstanceDetailDto> ResetTestFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId);
    Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId);
    Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId);
    Task ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId);
    Task SetFlowInstanceFactsAsync(long instanceId, string factsJson, long actorId);
    Task SetEntityFactsAsync(string entityType, long entityId, string factsJson, long actorId);
    Task<FlowInstanceWithCheckpointsDto> SyncStageByStatusAsync(string entityType, long entityId, int statusId, long actorId);
}

public class FlowService : IFlowService
{
    private readonly IFlowRepository _repo;
    private readonly ILogger<FlowService> _logger;

    public FlowService(IFlowRepository repo, ILogger<FlowService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<FlowDefinition>> GetFlowDefinitionsAsync() => await _repo.GetFlowDefinitionsAsync();
    public async Task<IEnumerable<FlowStage>> GetStagesAsync(long? flowId = null) => await _repo.GetStagesAsync(flowId);
    
    public async Task<FlowStage> CreateStageAsync(FlowStage stage)
    {
        var id = await _repo.CreateStageAsync(stage);
        stage.IdStage = id;
        await _repo.LogAuditAsync(stage.IdFlow > 0 ? stage.IdFlow : 1, "STAGE_CREATED", null, null, $"{{\"code\":\"{stage.StageCode}\",\"orderIndex\":{stage.OrderIndex}}}");
        return stage;
    }

    public async Task<bool> SetStageOrderAsync(long stageId, short newOrderIndex)
    {
        var ok = await _repo.SetStageOrderAsync(stageId, newOrderIndex);
        if (ok) await _repo.LogAuditAsync(1, "STAGE_ORDER_SET", null, null, $"{{\"stageId\":{stageId},\"orderIndex\":{newOrderIndex}}}");
        return ok;
    }

    public async Task<bool> UpdateStageAsync(FlowStage stage)
    {
        var ok = await _repo.UpdateStageAsync(stage);
        if (ok) await _repo.LogAuditAsync(1, "STAGE_UPDATED", null, null, $"{{\"stageId\":{stage.IdStage},\"name\":\"{stage.Name}\"}}");
        return ok;
    }

    public async Task<bool> MoveStageAsync(long stageId, string direction)
    {
        var moved = await _repo.MoveStageAsync(stageId, direction);
        if (moved) await _repo.LogAuditAsync(1, "STAGE_REORDERED", null, null, $"{{\"stageId\":{stageId},\"direction\":\"{direction}\"}}");
        return moved;
    }

    public async Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId) => await _repo.GetCheckpointCatalogAsync(flowId);
    public async Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCheckpointCatalogAsync(long? flowId) => await _repo.GetFullCheckpointCatalogAsync(flowId);
    public async Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId) => await _repo.GetCheckpointStepsAsync(checkpointId);
    public async Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps) => await _repo.SaveCheckpointStepsAsync(checkpointId, steps);
    public async Task<IEnumerable<FlowAuditLog>> GetAuditLogsAsync(int limit = 50) => await _repo.GetAuditLogsAsync(limit);

    public async Task<CheckpointCatalog> CreateCheckpointCatalogAsync(CheckpointCatalog cp)
    {
        var id = await _repo.CreateCheckpointCatalogAsync(cp);
        cp.IdCheckpoint = id;
        await _repo.LogAuditAsync(cp.CreatedBy, "CHECKPOINT_CREATED", null, id, $"{{\"code\":\"{cp.Code}\",\"status\":\"{cp.ApprovalStatus}\"}}");
        return cp;
    }

    public async Task ApproveCheckpointCatalogAsync(long checkpointId, string approvedByJson, long actorId)
    {
        await _repo.ApproveCheckpointCatalogAsync(checkpointId, approvedByJson);
        await _repo.LogAuditAsync(actorId, "CHECKPOINT_APPROVED", null, checkpointId, $"{{\"approvedBy\":{approvedByJson}}}");
    }

    public async Task<bool> UpdateCheckpointCampaignAsync(long checkpointId, string campaign)
    {
        return await _repo.UpdateCheckpointCampaignAsync(checkpointId, campaign);
    }

    public async Task<bool> UpdateCheckpointPortfolioAsync(long checkpointId, string portfolio)
    {
        return await _repo.UpdateCheckpointPortfolioAsync(checkpointId, portfolio);
    }

    public async Task<bool> UpdateCheckpointStageAsync(long checkpointId, long? stageId)
    {
        return await _repo.UpdateCheckpointStageAsync(checkpointId, stageId);
    }

    public async Task<bool> UpdateCheckpointCatalogAsync(long id, CheckpointCatalog cp)
    {
        return await _repo.UpdateCheckpointCatalogAsync(id, cp);
    }

    public async Task<FlowInstance?> GetFlowInstanceByIdAsync(long instanceId) => await _repo.GetFlowInstanceByIdAsync(instanceId);

    public async Task<FlowInstance?> GetFlowInstanceByEntityAsync(string entityType, long entityId) => 
        await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);

    public async Task<FlowInstanceWithCheckpointsDto?> GetFlowInstanceWithCheckpointsByEntityAsync(string entityType, long entityId)
    {
        var inst = await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);
        if (inst == null) return null;

        var cps = (await _repo.GetCheckpointInstancesForFlowAsync(inst.IdInstance)).ToList();
        return new FlowInstanceWithCheckpointsDto
        {
            IdInstance = inst.IdInstance,
            IdFlow = inst.IdFlow,
            EntityType = inst.EntityType,
            EntityId = inst.EntityId,
            CurrentStageId = inst.CurrentStageId,
            DayCounter = inst.DayCounter,
            Metadata = inst.Metadata,
            Facts = inst.Facts,
            Status = inst.Status,
            CreatedAt = inst.CreatedAt,
            CompletedAt = inst.CompletedAt,
            CheckpointInstances = cps
        };
    }

    public async Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByIdAsync(long instanceId)
    {
        var inst = await _repo.GetFlowInstanceByIdAsync(instanceId);
        if (inst == null) return null;
        return await BuildFlowInstanceDetailAsync(inst);
    }

    public async Task<FlowInstanceDetailDto?> GetFlowInstanceDetailByEntityAsync(string entityType, long entityId)
    {
        var inst = await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);
        if (inst == null) return null;
        return await BuildFlowInstanceDetailAsync(inst);
    }

    public async Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId) =>
        await _repo.GetCheckpointInstancesForFlowAsync(instanceId);

    public async Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId) =>
        await _repo.GetStepProgressAsync(cpInstanceId);

    public async Task ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId)
    {
        await _repo.UpsertStepProgressAsync(cpInstanceId, stepId, isCompleted, actorId);
        await _repo.LogAuditAsync(actorId, "STEP_TOGGLED", null, cpInstanceId, $"{{\"stepId\":{stepId},\"isCompleted\":{isCompleted.ToString().ToLower()}}}");

        if (isCompleted)
        {
            var allCompleted = await _repo.AreAllStepsCompletedAsync(cpInstanceId);
            if (allCompleted)
            {
                var cp = await _repo.GetCheckpointInstanceByIdAsync(cpInstanceId);
                if (cp != null && (cp.Status == "PENDING" || cp.Status == "KO"))
                {
                    await ResolveCheckpointAsync(cpInstanceId, "APPROVED", actorId);
                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_AUTO_APPROVED_BY_STEPS", cp.IdInstance, cp.IdCheckpoint, "{\"reason\":\"ALL_STEPS_COMPLETED\"}");
                }
            }
        }
    }

    public async Task SetEntityFactsAsync(string entityType, long entityId, string factsJson, long actorId)
    {
        var inst = await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);
        if (inst != null)
        {
            await SetFlowInstanceFactsAsync(inst.IdInstance, factsJson, actorId);
        }
    }

    public async Task SetFlowInstanceFactsAsync(long instanceId, string factsJson, long actorId)
    {
        var instance = await _repo.GetFlowInstanceByIdAsync(instanceId);
        if (instance == null) return;

        var currentFacts = new Dictionary<string, object>();
        try
        {
            if (!string.IsNullOrWhiteSpace(instance.Facts) && instance.Facts != "{}")
            {
                currentFacts = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Facts) ?? new();
            }
        }
        catch { currentFacts = new(); }

        try
        {
            var incomingFacts = JsonSerializer.Deserialize<Dictionary<string, object>>(factsJson) ?? new();
            foreach (var kvp in incomingFacts)
            {
                currentFacts[kvp.Key] = kvp.Value;
            }
        }
        catch { }

        var mergedFactsJson = JsonSerializer.Serialize(currentFacts);
        await _repo.UpdateFlowInstanceFactsAsync(instanceId, mergedFactsJson);
        await _repo.LogAuditAsync(actorId, "FLOW_FACTS_UPDATED", instanceId, null, mergedFactsJson);

        // Auto-evaluación reactiva de checkpoints pendientes según los hechos
        var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).Where(c => c.Status == "PENDING").ToList();
        var catalog = (await _repo.GetCheckpointCatalogAsync(null)).ToDictionary(c => c.IdCheckpoint);

        foreach (var cpInst in activeCps)
        {
            if (catalog.TryGetValue(cpInst.IdCheckpoint, out var catDef) && !string.IsNullOrEmpty(catDef.PreconditionFact))
            {
                if (currentFacts.TryGetValue(catDef.PreconditionFact, out var val) &&
                    (val?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true || val?.ToString() == "1"))
                {
                    await ResolveCheckpointAsync(cpInst.IdCpInstance, "APPROVED", actorId);
                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_AUTO_APPROVED_BY_FACT", instanceId, catDef.IdCheckpoint, 
                        $"{{\"fact\":\"{catDef.PreconditionFact}\",\"value\":{JsonSerializer.Serialize(val)}}}");
                }
            }
        }
    }

    public async Task<FlowInstanceWithCheckpointsDto> SyncStageByStatusAsync(string entityType, long entityId, int statusId, long actorId)
    {
        var instance = await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);
        if (instance == null)
        {
            var flowCode = "PIPELINE_TELECOM";
            return await StartFlowInstanceAsync(flowCode, entityType, entityId, actorId);
        }

        var targetStage = await _repo.GetStageByStatusAsync(statusId, instance.IdFlow);
        if (targetStage == null || targetStage.IdStage == instance.CurrentStageId)
        {
            return (await GetFlowInstanceWithCheckpointsByEntityAsync(entityType, entityId))!;
        }

        var allStages = (await _repo.GetFlowStagesAsync(instance.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
        var currentStage = allStages.FirstOrDefault(s => s.IdStage == instance.CurrentStageId);

        bool isMovingForward = currentStage == null || targetStage.OrderIndex > currentStage.OrderIndex;

        if (isMovingForward)
        {
            var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance)).ToList();
            var catalog = (await _repo.GetCheckpointCatalogAsync(null)).ToDictionary(c => c.IdCheckpoint);

            foreach (var cpInst in activeCps)
            {
                if (cpInst.Status == "PENDING" && catalog.TryGetValue(cpInst.IdCheckpoint, out var catDef))
                {
                    if (catDef.BlocksAdvance)
                    {
                        throw new InvalidOperationException(
                            $"Transición bloqueada: Checkpoint obligatorio '{catDef.Name}' ({catDef.Code}) está PENDIENTE.");
                    }
                }
            }
        }

        var fromStageId = instance.CurrentStageId;
        instance.CurrentStageId = targetStage.IdStage;
        instance.DayCounter++;
        instance.Status = targetStage.IsTerminal ? "COMPLETED" : "ACTIVE";

        await _repo.UpdateFlowInstanceStageAsync(instance.IdInstance, targetStage.IdStage, instance.DayCounter, instance.Status);

        await _repo.RecordStageTransitionAsync(new StageTransition
        {
            IdInstance = instance.IdInstance,
            FromStageId = fromStageId,
            ToStageId = targetStage.IdStage,
            Direction = isMovingForward ? "FORWARD" : "BACKWARD",
            TriggeredBy = $"STATUS_SYNC:{statusId}",
            ActorId = actorId
        });

        await TriggerCheckpointsForStageAsync(instance.IdInstance, instance.IdFlow, targetStage.IdStage, actorId);

        await _repo.LogAuditAsync(actorId, "STAGE_STATUS_SYNCED", instance.IdInstance, null,
            $"{{\"statusId\":{statusId},\"fromStage\":{fromStageId},\"toStage\":{targetStage.IdStage},\"stageCode\":\"{targetStage.StageCode}\"}}");

        return (await GetFlowInstanceWithCheckpointsByEntityAsync(entityType, entityId))!;
    }

    public async Task<FlowInstanceWithCheckpointsDto> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId)
    {
        var flow = await _repo.GetFlowByCodeAsync(flowCode) 
            ?? throw new KeyNotFoundException($"Flow definition '{flowCode}' not found.");

        var stages = (await _repo.GetFlowStagesAsync(flow.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
        if (stages.Count == 0) throw new InvalidOperationException($"Flow '{flowCode}' has no defined stages.");

        var firstStage = stages.First();

        var existing = await _repo.GetFlowInstanceAsync(entityType, entityId, flow.IdFlow);
        if (existing != null)
        {
            return new FlowInstanceWithCheckpointsDto
            {
                IdInstance = existing.IdInstance, IdFlow = existing.IdFlow, EntityType = existing.EntityType,
                EntityId = existing.EntityId, CurrentStageId = existing.CurrentStageId, DayCounter = existing.DayCounter,
                Status = existing.Status, Facts = existing.Facts, Metadata = existing.Metadata, CreatedAt = existing.CreatedAt,
                CheckpointInstances = (await _repo.GetCheckpointInstancesForFlowAsync(existing.IdInstance)).ToList()
            };
        }

        var instance = new FlowInstance
        {
            IdFlow = flow.IdFlow,
            EntityType = entityType.ToLowerInvariant(),
            EntityId = entityId,
            CurrentStageId = firstStage.IdStage,
            DayCounter = 1,
            Status = "ACTIVE",
            Facts = "{}"
        };

        var instanceId = await _repo.CreateFlowInstanceAsync(instance);
        instance.IdInstance = instanceId;

        await _repo.RecordStageTransitionAsync(new StageTransition
        {
            IdInstance = instanceId,
            FromStageId = null,
            ToStageId = firstStage.IdStage,
            Direction = "FORWARD",
            TriggeredBy = "INITIALIZATION",
            ActorId = actorId
        });

        await TriggerCheckpointsForStageAsync(instanceId, flow.IdFlow, firstStage.IdStage, actorId);

        await _repo.LogAuditAsync(actorId, "FLOW_STARTED", instanceId, null, $"{{\"flowCode\":\"{flowCode}\",\"stage\":\"{firstStage.StageCode}\"}}");

        return new FlowInstanceWithCheckpointsDto
        {
            IdInstance = instance.IdInstance,
            IdFlow = instance.IdFlow,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            CurrentStageId = instance.CurrentStageId,
            DayCounter = instance.DayCounter,
            Status = instance.Status,
            Facts = instance.Facts,
            Metadata = instance.Metadata,
            CreatedAt = instance.CreatedAt,
            CheckpointInstances = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList()
        };
    }

    public async Task<FlowInstanceDetailDto> ResetTestFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId)
    {
        var existing = await _repo.GetFlowInstanceByEntityAsync(entityType, entityId);
        if (existing != null)
        {
            await _repo.DeleteFlowInstanceDataAsync(existing.IdInstance);
        }

        await StartFlowInstanceAsync(flowCode, entityType, entityId, actorId);
        var detail = await GetFlowInstanceDetailByEntityAsync(entityType, entityId);
        return detail!;
    }

    public async Task<FlowValidationResultDto> ValidateStageAdvanceAsync(long instanceId)
    {
        var instance = await _repo.GetFlowInstanceByIdAsync(instanceId)
            ?? throw new KeyNotFoundException($"Flow instance #{instanceId} not found.");

        var allStages = (await _repo.GetFlowStagesAsync(instance.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
        var currentStage = allStages.FirstOrDefault(s => s.IdStage == instance.CurrentStageId);

        var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList();
        var catalog = (await _repo.GetCheckpointCatalogAsync(null)).ToDictionary(c => c.IdCheckpoint);

        var blockingPending = new List<CheckpointInstanceDetailDto>();
        var reasons = new List<string>();

        foreach (var cpInst in activeCps)
        {
            if (cpInst.Status == "PENDING" && catalog.TryGetValue(cpInst.IdCheckpoint, out var catDef) && catDef.BlocksAdvance)
            {
                blockingPending.Add(new CheckpointInstanceDetailDto
                {
                    IdCpInstance = cpInst.IdCpInstance,
                    IdCheckpoint = cpInst.IdCheckpoint,
                    Code = catDef.Code,
                    Name = catDef.Name,
                    BlocksAdvance = true,
                    Status = "PENDING"
                });
                reasons.Add($"Checkpoint obligatorio '{catDef.Name}' ({catDef.Code}) está pendiente de resolución.");
            }
        }

        return new FlowValidationResultDto
        {
            InstanceId = instanceId,
            CurrentStageId = instance.CurrentStageId,
            CurrentStageName = currentStage?.Name ?? $"Stage #{instance.CurrentStageId}",
            CanAdvance = blockingPending.Count == 0,
            PendingBlockingCount = blockingPending.Count,
            BlockingPendingCheckpoints = blockingPending,
            BlockingReasons = reasons
        };
    }

    public async Task<FlowInstanceWithCheckpointsDto> AdvanceStageAsync(long instanceId, long actorId)
    {
        var instance = await _repo.GetFlowInstanceByIdAsync(instanceId) 
            ?? throw new KeyNotFoundException($"Flow instance #{instanceId} not found.");

        var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList();
        var catalog = (await _repo.GetCheckpointCatalogAsync(null)).ToDictionary(c => c.IdCheckpoint);

        foreach (var cpInst in activeCps)
        {
            if (cpInst.Status == "PENDING" && catalog.TryGetValue(cpInst.IdCheckpoint, out var catDef))
            {
                if (catDef.BlocksAdvance)
                {
                    throw new InvalidOperationException(
                        $"Cannot advance stage: Checkpoint '{catDef.Name}' ({catDef.Code}) is PENDING and blocks stage advance.");
                }
            }
        }

        var stages = (await _repo.GetFlowStagesAsync(instance.IdFlow))
            .OrderBy(s => s.OrderIndex).ToList();

        var currentIdx = stages.FindIndex(s => s.IdStage == instance.CurrentStageId);
        if (currentIdx < 0) 
            throw new InvalidOperationException($"Current stage #{instance.CurrentStageId} not found in flow definition.");

        if (currentIdx >= stages.Count - 1) 
            throw new InvalidOperationException("Flow is already at the last stage. Use FinalizesCycle to complete.");

        var nextStage = stages[currentIdx + 1];
        var fromStageId = instance.CurrentStageId;

        instance.CurrentStageId = nextStage.IdStage;
        instance.DayCounter++;
        instance.Status = nextStage.IsTerminal ? "COMPLETED" : "ACTIVE";

        await _repo.UpdateFlowInstanceStageAsync(instanceId, nextStage.IdStage, instance.DayCounter, instance.Status);

        await _repo.RecordStageTransitionAsync(new StageTransition
        {
            IdInstance = instanceId,
            FromStageId = fromStageId,
            ToStageId = nextStage.IdStage,
            Direction = "FORWARD",
            TriggeredBy = "MANUAL",
            ActorId = actorId
        });

        await TriggerCheckpointsForStageAsync(instanceId, instance.IdFlow, nextStage.IdStage, actorId);

        await _repo.LogAuditAsync(actorId, "STAGE_ADVANCED", instanceId, null, 
            $"{{\"from\":{fromStageId},\"to\":{nextStage.IdStage},\"stage\":\"{nextStage.StageCode}\"}}");

        var reloaded = await _repo.GetFlowInstanceByIdAsync(instanceId) ?? instance;
        return new FlowInstanceWithCheckpointsDto
        {
            IdInstance = reloaded.IdInstance,
            IdFlow = reloaded.IdFlow,
            EntityType = reloaded.EntityType,
            EntityId = reloaded.EntityId,
            CurrentStageId = reloaded.CurrentStageId,
            DayCounter = reloaded.DayCounter,
            Status = reloaded.Status,
            Facts = reloaded.Facts,
            Metadata = reloaded.Metadata,
            CreatedAt = reloaded.CreatedAt,
            CheckpointInstances = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList()
        };
    }

    public async Task<ResolveCheckpointResultDto> ResolveCheckpointAsync(long cpInstanceId, string status, long actorId)
    {
        var normalizedStatus = status.ToUpperInvariant();
        await _repo.UpdateCheckpointInstanceStatusAsync(cpInstanceId, normalizedStatus, actorId);

        var cpInstance = await _repo.GetCheckpointInstanceByIdAsync(cpInstanceId)
            ?? throw new KeyNotFoundException($"Checkpoint instance #{cpInstanceId} not found.");

        var catDef = (await _repo.GetCheckpointCatalogAsync())
            .FirstOrDefault(c => c.IdCheckpoint == cpInstance.IdCheckpoint)
            ?? throw new KeyNotFoundException($"Checkpoint definition #{cpInstance.IdCheckpoint} not found in catalog.");

        var instance = await _repo.GetFlowInstanceByIdAsync(cpInstance.IdInstance)
            ?? throw new KeyNotFoundException($"Flow instance #{cpInstance.IdInstance} not found.");

        var result = new ResolveCheckpointResultDto
        {
            CheckpointInstanceId = cpInstanceId,
            IdCheckpoint = catDef.IdCheckpoint,
            Code = catDef.Code,
            Name = catDef.Name,
            ResolvedStatus = normalizedStatus,
            CurrentStageId = instance.CurrentStageId,
            FlowStatus = instance.Status,
            NextAction = "NONE"
        };

        var newlyTriggered = new List<CheckpointInstanceDetailDto>();

        // ══════════════════════════════════════════════════════════════════════
        // 1. CASO KO: EVALUACIÓN ESTRICTA (DISPARADORES -> ROLLBACK -> FIN CICLO -> BLOQUEO)
        // ══════════════════════════════════════════════════════════════════════
        if (normalizedStatus == "KO")
        {
            bool chainedTriggered = false;

            // ── A. Disparo Encadenado (TriggeredByKo / disparaSiKoDe) ───────
            var allActiveCps = (await _repo.GetCheckpointCatalogAsync(instance.IdFlow))
                .Where(c => c.ApprovalStatus == "ACTIVE")
                .ToList();

            var chainedCps = allActiveCps
                .Where(c => c.TriggeredByKo == catDef.IdCheckpoint)
                .ToList();

            // Mapeos canónicos explícitos:
            // CP 15 KO -> dispara CP 75
            // CP 75 KO -> dispara CP 76
            // CP 18 / CP 79 KO -> dispara CP 80
            // CP 80 / CP 77 / CP 78 KO -> dispara CP 76
            // CP 20 / CP 21 KO -> dispara CP 74
            var targetCpIds = new List<long>();
            if (catDef.IdCheckpoint == 15) targetCpIds.Add(75);
            else if (catDef.IdCheckpoint == 75) targetCpIds.Add(76);
            else if (catDef.IdCheckpoint == 79 || catDef.IdCheckpoint == 18) targetCpIds.Add(80);
            else if (catDef.IdCheckpoint == 80 || catDef.IdCheckpoint == 77 || catDef.IdCheckpoint == 78) targetCpIds.Add(76);
            else if (catDef.IdCheckpoint == 20 || catDef.IdCheckpoint == 21) targetCpIds.Add(74);

            foreach (var tid in targetCpIds)
            {
                var targetCp = allActiveCps.FirstOrDefault(c => c.IdCheckpoint == tid);
                if (targetCp != null && !chainedCps.Any(c => c.IdCheckpoint == tid))
                {
                    chainedCps.Add(targetCp);
                }
            }

            foreach (var chained in chainedCps)
            {
                var alreadyOpen = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance))
                    .Any(ci => ci.IdCheckpoint == chained.IdCheckpoint && ci.Status == "PENDING");

                if (!alreadyOpen)
                {
                    var newInstId = await _repo.CreateCheckpointInstanceAsync(new CheckpointInstance
                    {
                        IdInstance = instance.IdInstance,
                        IdCheckpoint = chained.IdCheckpoint,
                        Status = "PENDING",
                        OpenedAtStage = instance.CurrentStageId,
                        IsRetroactive = false,
                        OccurrenceNumber = 1
                    });

                    chainedTriggered = true;
                    newlyTriggered.Add(new CheckpointInstanceDetailDto
                    {
                        IdCpInstance = newInstId,
                        IdCheckpoint = chained.IdCheckpoint,
                        Code = chained.Code,
                        Name = chained.Name,
                        Status = "PENDING",
                        BlocksAdvance = chained.BlocksAdvance,
                        FinalizesCycle = chained.FinalizesCycle,
                        TriggeredByKo = catDef.IdCheckpoint,
                        TriggeredByKoName = catDef.Name
                    });

                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_CHAINED_TRIGGERED", instance.IdInstance, chained.IdCheckpoint, 
                        $"{{\"triggeredByKo\":{catDef.IdCheckpoint},\"code\":\"{chained.Code}\"}}");
                }
            }

            if (chainedTriggered)
            {
                result.NextAction = "CHAINED_TRIGGERED";
                result.Message = $"Resultado KO disparó automáticamente el/los checkpoint(s) de gestión: {string.Join(", ", newlyTriggered.Select(t => t.Name))}.";
            }
            // ── B. Retroceso de Etapa (RollbackToStage / retrocede) ─────────
            else if (catDef.RollbackToStage.HasValue && catDef.RollbackToStage > 0)
            {
                var rollbackStageId = catDef.RollbackToStage.Value;
                var stages = (await _repo.GetFlowStagesAsync(instance.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
                var targetStage = stages.FirstOrDefault(s => s.IdStage == rollbackStageId);
                if (targetStage != null)
                {
                    var fromStageId = instance.CurrentStageId;
                    await _repo.UpdateFlowInstanceStageAsync(instance.IdInstance, rollbackStageId, instance.DayCounter + 1, "ACTIVE");
                    await _repo.RecordStageTransitionAsync(new StageTransition
                    {
                        IdInstance = instance.IdInstance,
                        FromStageId = fromStageId,
                        ToStageId = rollbackStageId,
                        Direction = "BACKWARD",
                        TriggeredBy = $"CHECKPOINT_KO:{catDef.Code}",
                        ActorId = actorId
                    });

                    await TriggerCheckpointsForStageAsync(instance.IdInstance, instance.IdFlow, rollbackStageId, actorId);

                    await _repo.LogAuditAsync(actorId, "STAGE_ROLLBACK", instance.IdInstance, catDef.IdCheckpoint, 
                        $"{{\"from\":{fromStageId},\"to\":{rollbackStageId},\"stage\":\"{targetStage.Name}\"}}");

                    result.NextAction = "STAGE_ROLLBACK";
                    result.CurrentStageId = rollbackStageId;
                    result.CurrentStageName = targetStage.Name;
                    result.Message = $"Resultado KO ejecutó el retroceso de etapa hacia: {targetStage.Name}.";
                }
            }
            // ── C. Fin de Ciclo Irreversible (FinalizesCycle: true) ──────────
            else if (catDef.FinalizesCycle)
            {
                await _repo.UpdateFlowInstanceStageAsync(instance.IdInstance, instance.CurrentStageId, instance.DayCounter, "CLOSED");
                await _repo.LogAuditAsync(actorId, "FLOW_CYCLE_FINALIZED", instance.IdInstance, catDef.IdCheckpoint, 
                    $"{{\"reason\":\"CHECKPOINT_FINALIZES_CYCLE\",\"code\":\"{catDef.Code}\"}}");

                // Cerrar cualquier otro checkpoint pendiente
                var pendingCps = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance))
                    .Where(ci => ci.Status == "PENDING" && ci.IdCpInstance != cpInstanceId)
                    .ToList();
                foreach (var pending in pendingCps)
                {
                    await _repo.UpdateCheckpointInstanceStatusAsync(pending.IdCpInstance, "KO", actorId);
                }

                result.NextAction = "CYCLE_FINALIZED";
                result.FlowStatus = "CLOSED";
                result.Message = "Resultado KO en checkpoint terminal finalizó el ciclo de forma irreversible (Expediente Descartado/Cerrado).";
            }
            // ── D. Bloqueo Simple ───────────────────────────────────────────
            else if (catDef.BlocksAdvance)
            {
                result.NextAction = "BLOCKED";
                result.Message = $"Resultado KO registrado en '{catDef.Name}'. Impide el avance a la siguiente etapa hasta que se resuelva.";
            }
            else
            {
                result.NextAction = "NONE";
                result.Message = $"Resultado KO registrado en '{catDef.Name}' (No bloqueante).";
            }
        }
        // ══════════════════════════════════════════════════════════════════════
        // 2. CASO APPROVED (OK): PROGRESIÓN SECUENCIAL O AUTO-AVANCE DE ETAPA
        // ══════════════════════════════════════════════════════════════════════
        else if (normalizedStatus == "APPROVED")
        {
            // Buscar el siguiente checkpoint secuencial activo dentro de la misma etapa (no dependiente de KO)
            var nextSequentialCp = (await _repo.GetCheckpointCatalogAsync(instance.IdFlow))
                .Where(c => c.ApprovalStatus == "ACTIVE" 
                         && c.TriggerStageId == instance.CurrentStageId
                         && c.TriggeredByKo == null
                         && c.ExecutionOrder > catDef.ExecutionOrder)
                .OrderBy(c => c.ExecutionOrder)
                .FirstOrDefault();

            if (nextSequentialCp != null)
            {
                var alreadyOpen = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance))
                    .Any(ci => ci.IdCheckpoint == nextSequentialCp.IdCheckpoint && ci.Status == "PENDING");

                if (!alreadyOpen)
                {
                    var newInstId = await _repo.CreateCheckpointInstanceAsync(new CheckpointInstance
                    {
                        IdInstance = instance.IdInstance,
                        IdCheckpoint = nextSequentialCp.IdCheckpoint,
                        Status = "PENDING",
                        OpenedAtStage = instance.CurrentStageId,
                        IsRetroactive = false,
                        OccurrenceNumber = 1
                    });

                    newlyTriggered.Add(new CheckpointInstanceDetailDto
                    {
                        IdCpInstance = newInstId,
                        IdCheckpoint = nextSequentialCp.IdCheckpoint,
                        Code = nextSequentialCp.Code,
                        Name = nextSequentialCp.Name,
                        Status = "PENDING",
                        BlocksAdvance = nextSequentialCp.BlocksAdvance,
                        FinalizesCycle = nextSequentialCp.FinalizesCycle,
                        ExecutionOrder = nextSequentialCp.ExecutionOrder,
                        OwnerDept = nextSequentialCp.OwnerDept ?? "Asesor"
                    });

                    result.NextAction = "SEQUENTIAL_TRIGGERED";
                    result.Message = $"Checkpoint '{catDef.Name}' aprobado. Se activó el siguiente hito: '{nextSequentialCp.Name}'.";
                }
                else
                {
                    result.NextAction = "NONE";
                    result.Message = $"Checkpoint '{catDef.Name}' aprobado. El siguiente hito '{nextSequentialCp.Name}' ya se encuentra activo.";
                }
            }
            else
            {
                // Si no restan checkpoints secuenciales en esta etapa, verificar si quedan checkpoints bloqueantes pendientes:
                var remainingPendingInStage = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance))
                    .Where(ci => ci.Status == "PENDING" && ci.IdCpInstance != cpInstanceId)
                    .ToList();

                if (!remainingPendingInStage.Any())
                {
                    try
                    {
                        await AdvanceStageAsync(instance.IdInstance, actorId);
                        result.NextAction = "STAGE_ADVANCED";
                        result.Message = $"Todos los checkpoints requeridos de la etapa han sido aprobados. El expediente avanzó automáticamente a la siguiente etapa.";
                    }
                    catch (Exception ex)
                    {
                        result.NextAction = "NONE";
                        result.Message = $"Checkpoint '{catDef.Name}' aprobado. Intento de auto-avance: {ex.Message}";
                    }
                }
                else
                {
                    result.NextAction = "NONE";
                    result.Message = $"Checkpoint '{catDef.Name}' aprobado. Quedan {remainingPendingInStage.Count} checkpoints pendientes en la etapa.";
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 3. RECURRENCIA (Si aplica)
        // ══════════════════════════════════════════════════════════════════════
        if (catDef.IsRecurrent)
        {
            var currentOccurrence = cpInstance.OccurrenceNumber;
            bool canRepeat = !catDef.MaxOccurrences.HasValue || currentOccurrence < catDef.MaxOccurrences.Value;

            if (canRepeat)
            {
                var nextScheduled = DateTime.UtcNow.AddDays(catDef.RecurrenceDays ?? 30);
                await _repo.CreateCheckpointInstanceAsync(new CheckpointInstance
                {
                    IdInstance = instance.IdInstance,
                    IdCheckpoint = catDef.IdCheckpoint,
                    Status = "SCHEDULED",
                    OpenedAtStage = instance.CurrentStageId,
                    IsRetroactive = false,
                    OccurrenceNumber = (short)(currentOccurrence + 1),
                    ScheduledFor = nextScheduled
                });
            }
        }

        // Recargar detalle completo para devolver al cliente
        var reloadedInstance = await _repo.GetFlowInstanceByIdAsync(instance.IdInstance) ?? instance;
        var detailDto = await BuildFlowInstanceDetailAsync(reloadedInstance);

        result.TriggeredCheckpoints = newlyTriggered;
        result.FlowInstance = detailDto;
        result.CurrentStageId = detailDto.CurrentStageId;
        result.CurrentStageName = detailDto.CurrentStage?.Name ?? $"Stage #{detailDto.CurrentStageId}";
        result.FlowStatus = detailDto.Status;
        result.CanAdvanceStage = detailDto.CanAdvanceStage;

        await _repo.LogAuditAsync(actorId, "CHECKPOINT_RESOLVED", cpInstance.IdInstance, catDef.IdCheckpoint, 
            $"{{\"status\":\"{normalizedStatus}\",\"nextAction\":\"{result.NextAction}\"}}");

        return result;
    }

    private async Task TriggerCheckpointsForStageAsync(long instanceId, long flowId, long stageId, long actorId)
    {
        var instance = await _repo.GetFlowInstanceByIdAsync(instanceId);
        Dictionary<string, object>? facts = null;
        try
        {
            var rawFacts = !string.IsNullOrWhiteSpace(instance?.Facts) && instance.Facts != "{}" 
                ? instance.Facts 
                : instance?.Metadata ?? "{}";
            facts = JsonSerializer.Deserialize<Dictionary<string, object>>(rawFacts);
        }
        catch { facts = new(); }

        var allStageCheckpoints = (await _repo.GetCheckpointCatalogAsync(flowId))
            .Where(c => c.ApprovalStatus == "ACTIVE" 
                     && c.TriggerStageId == stageId 
                     && c.TriggeredByKo == null
                     && (c.IdFlow == flowId || c.IdFlow == null))
            .OrderBy(c => c.ExecutionOrder)
            .ToList();

        if (!allStageCheckpoints.Any()) return;

        // Abrir los checkpoints de entrada (menor orden de ejecución en la etapa)
        var minOrder = allStageCheckpoints.Min(c => c.ExecutionOrder);
        var initialCheckpoints = allStageCheckpoints.Where(c => c.ExecutionOrder == minOrder).ToList();

        foreach (var cp in initialCheckpoints)
        {
            if (!string.IsNullOrEmpty(cp.PreconditionFact))
            {
                bool factMet = facts != null && facts.TryGetValue(cp.PreconditionFact, out var val) 
                    && (val?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true || val?.ToString() == "1");

                if (!factMet)
                {
                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_SKIPPED_PRECONDITION", instanceId, cp.IdCheckpoint, 
                        $"{{\"fact\":\"{cp.PreconditionFact}\",\"met\":false}}");
                    continue;
                }
            }

            var alreadyOpen = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId))
                .Any(ci => ci.IdCheckpoint == cp.IdCheckpoint && (ci.Status == "PENDING" || ci.Status == "APPROVED"));

            if (!alreadyOpen)
            {
                await _repo.CreateCheckpointInstanceAsync(new CheckpointInstance
                {
                    IdInstance = instanceId,
                    IdCheckpoint = cp.IdCheckpoint,
                    Status = "PENDING",
                    OpenedAtStage = stageId,
                    IsRetroactive = false,
                    OccurrenceNumber = 1
                });
            }
        }
    }

    private async Task<FlowInstanceDetailDto> BuildFlowInstanceDetailAsync(FlowInstance inst)
    {
        var stages = (await _repo.GetFlowStagesAsync(inst.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
        var currentStageEntity = stages.FirstOrDefault(s => s.IdStage == inst.CurrentStageId);

        var stageDetailDto = currentStageEntity != null ? new FlowStageDetailDto
        {
            IdStage = currentStageEntity.IdStage,
            IdFlow = currentStageEntity.IdFlow,
            StageCode = currentStageEntity.StageCode,
            Name = currentStageEntity.Name,
            Description = currentStageEntity.Description,
            OrderIndex = currentStageEntity.OrderIndex,
            IsTerminal = currentStageEntity.IsTerminal,
            SlaHours = currentStageEntity.SlaHours,
            Portfolio = currentStageEntity.Portfolio,
            Campaign = currentStageEntity.Campaign
        } : null;

        var catalog = (await _repo.GetCheckpointCatalogAsync(null)).ToDictionary(c => c.IdCheckpoint);
        var cpInstances = (await _repo.GetCheckpointInstancesForFlowAsync(inst.IdInstance)).ToList();
        var transitions = (await _repo.GetStageTransitionsForInstanceAsync(inst.IdInstance)).ToList();

        var stageMap = stages.ToDictionary(s => s.IdStage, s => s.Name);

        var enrichedCheckpoints = new List<CheckpointInstanceDetailDto>();

        foreach (var ci in cpInstances)
        {
            catalog.TryGetValue(ci.IdCheckpoint, out var cat);

            var steps = (await _repo.GetCheckpointStepsAsync(ci.IdCheckpoint)).OrderBy(s => s.StepOrder).ToList();
            var progress = (await _repo.GetStepProgressAsync(ci.IdCpInstance)).ToDictionary(p => p.IdStep);

            var stepDtos = steps.Select(s =>
            {
                progress.TryGetValue(s.IdStep, out var p);
                return new CheckpointStepDetailDto
                {
                    IdStep = s.IdStep,
                    IdCheckpoint = s.IdCheckpoint,
                    StepOrder = s.StepOrder,
                    Name = s.Name,
                    IsRequired = s.IsRequired,
                    IsCompleted = p?.IsCompleted ?? false,
                    CompletedBy = p?.CompletedBy,
                    CompletedAt = p?.CompletedAt
                };
            }).ToList();

            string? rollbackStageName = null;
            if (cat?.RollbackToStage.HasValue == true && stageMap.TryGetValue(cat.RollbackToStage.Value, out var rName))
            {
                rollbackStageName = rName;
            }

            string? triggeredByKoName = null;
            if (cat?.TriggeredByKo.HasValue == true && catalog.TryGetValue(cat.TriggeredByKo.Value, out var tCat))
            {
                triggeredByKoName = tCat.Name;
            }

            string? openedStageName = null;
            if (ci.OpenedAtStage.HasValue && stageMap.TryGetValue(ci.OpenedAtStage.Value, out var oName))
            {
                openedStageName = oName;
            }

            enrichedCheckpoints.Add(new CheckpointInstanceDetailDto
            {
                IdCpInstance = ci.IdCpInstance,
                IdInstance = ci.IdInstance,
                IdCheckpoint = ci.IdCheckpoint,
                Code = cat?.Code ?? $"CP_{ci.IdCheckpoint}",
                Name = cat?.Name ?? $"Checkpoint #{ci.IdCheckpoint}",
                Description = cat?.Description,
                Status = ci.Status,
                OpenedAtStage = ci.OpenedAtStage,
                OpenedAtStageName = openedStageName,
                IsRetroactive = ci.IsRetroactive,
                OccurrenceNumber = ci.OccurrenceNumber,
                ScheduledFor = ci.ScheduledFor,
                ResolvedBy = ci.ResolvedBy,
                ResolvedAt = ci.ResolvedAt,
                CreatedAt = ci.CreatedAt,
                OwnerDept = cat?.OwnerDept ?? "Asesor",
                Category = cat?.Category ?? "GENERAL",
                Division = cat?.Division ?? "OPERACIONES",
                BlocksAdvance = cat?.BlocksAdvance ?? true,
                FinalizesCycle = cat?.FinalizesCycle ?? false,
                RollbackToStage = cat?.RollbackToStage,
                RollbackToStageName = rollbackStageName,
                TriggeredByKo = cat?.TriggeredByKo,
                TriggeredByKoName = triggeredByKoName,
                ExecutionOrder = cat?.ExecutionOrder ?? 1,
                Campaign = cat?.Campaign ?? "GENERAL",
                Portfolio = cat?.Portfolio ?? "GENERAL",
                Provider = cat?.Provider ?? "INTERNO",
                ApprovalStatus = cat?.ApprovalStatus ?? "ACTIVE",
                Steps = stepDtos
            });
        }

        var flows = await _repo.GetFlowDefinitionsAsync();
        var flowDef = flows.FirstOrDefault(f => f.IdFlow == inst.IdFlow);

        return new FlowInstanceDetailDto
        {
            IdInstance = inst.IdInstance,
            IdFlow = inst.IdFlow,
            FlowCode = flowDef?.Code ?? "PIPELINE_TELECOM",
            FlowName = flowDef?.Name ?? "Pipeline Telecom",
            EntityType = inst.EntityType,
            EntityId = inst.EntityId,
            CurrentStageId = inst.CurrentStageId,
            CurrentStage = stageDetailDto,
            DayCounter = inst.DayCounter,
            Status = inst.Status,
            Facts = inst.Facts,
            Metadata = inst.Metadata,
            CreatedAt = inst.CreatedAt,
            CompletedAt = inst.CompletedAt,
            Checkpoints = enrichedCheckpoints,
            RecentTransitions = transitions
        };
    }
}
