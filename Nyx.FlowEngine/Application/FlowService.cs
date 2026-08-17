using System.Text.Json;
using Nyx.FlowEngine.Domain.Entities;
using Nyx.FlowEngine.Infrastructure;

namespace Nyx.FlowEngine.Application;

public interface IFlowService
{
    Task<IEnumerable<FlowDefinition>> GetFlowDefinitionsAsync();
    Task<IEnumerable<FlowStage>> GetStagesAsync();
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
    Task<FlowInstance> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId);
    Task<FlowInstance> AdvanceStageAsync(long instanceId, long actorId);
    Task<CheckpointInstance> ResolveCheckpointAsync(long cpInstanceId, string status, long actorId);
    Task<FlowInstance?> GetFlowInstanceByIdAsync(long instanceId);
    Task<FlowInstance?> GetFlowInstanceByEntityAsync(string entityType, long entityId);
    Task<FlowInstanceWithCheckpointsDto?> GetFlowInstanceWithCheckpointsByEntityAsync(string entityType, long entityId);
    Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId);
    Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId);
    Task ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId);
    Task SetFlowInstanceFactsAsync(long instanceId, string factsJson, long actorId);
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
    public async Task<IEnumerable<FlowStage>> GetStagesAsync() => await _repo.GetAllStagesAsync();
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

    public async Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId) =>
        await _repo.GetCheckpointInstancesForFlowAsync(instanceId);

    public async Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId) =>
        await _repo.GetStepProgressAsync(cpInstanceId);

    public async Task ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId)
    {
        await _repo.UpsertStepProgressAsync(cpInstanceId, stepId, isCompleted, actorId);
        await _repo.LogAuditAsync(actorId, "STEP_TOGGLED", null, cpInstanceId, $"{{\"stepId\":{stepId},\"isCompleted\":{isCompleted.ToString().ToLower()}}}");
    }

    public async Task SetFlowInstanceFactsAsync(long instanceId, string factsJson, long actorId)
    {
        await _repo.UpdateFlowInstanceFactsAsync(instanceId, factsJson);
        await _repo.LogAuditAsync(actorId, "FLOW_FACTS_UPDATED", instanceId, null, factsJson);
    }

    public async Task<FlowInstance> StartFlowInstanceAsync(string flowCode, string entityType, long entityId, long actorId)
    {
        var flow = await _repo.GetFlowByCodeAsync(flowCode) 
            ?? throw new KeyNotFoundException($"Flow definition '{flowCode}' not found.");

        var stages = (await _repo.GetFlowStagesAsync(flow.IdFlow)).OrderBy(s => s.OrderIndex).ToList();
        if (stages.Count == 0) throw new InvalidOperationException($"Flow '{flowCode}' has no defined stages.");

        var firstStage = stages.First();

        var existing = await _repo.GetFlowInstanceAsync(entityType, entityId, flow.IdFlow);
        if (existing != null) return existing;

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

        // Trigger active checkpoints defined for this first stage
        await TriggerCheckpointsForStageAsync(instanceId, flow.IdFlow, firstStage.IdStage, actorId);

        await _repo.LogAuditAsync(actorId, "FLOW_STARTED", instanceId, null, $"{{\"flowCode\":\"{flowCode}\",\"stage\":\"{firstStage.StageCode}\"}}");
        return instance;
    }

    public async Task<FlowInstance> AdvanceStageAsync(long instanceId, long actorId)
    {
        // 1. Cargar la instancia actual
        var instance = await _repo.GetFlowInstanceByIdAsync(instanceId) 
            ?? throw new KeyNotFoundException($"Flow instance #{instanceId} not found.");

        // 2. Validar checkpoints bloqueantes
        var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList();
        var catalog = (await _repo.GetCheckpointCatalogAsync(instance.IdFlow)).ToDictionary(c => c.IdCheckpoint);

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

        // 3. Obtener las etapas ordenadas del flow y calcular la siguiente
        var stages = (await _repo.GetFlowStagesAsync(instance.IdFlow))
            .OrderBy(s => s.OrderIndex).ToList();

        var currentIdx = stages.FindIndex(s => s.IdStage == instance.CurrentStageId);
        if (currentIdx < 0) 
            throw new InvalidOperationException($"Current stage #{instance.CurrentStageId} not found in flow definition.");

        if (currentIdx >= stages.Count - 1) 
            throw new InvalidOperationException("Flow is already at the last stage. Use FinalizesCycle to complete.");

        var nextStage = stages[currentIdx + 1];
        var fromStageId = instance.CurrentStageId;

        // 4. Persistir el cambio de etapa
        instance.CurrentStageId = nextStage.IdStage;
        instance.DayCounter++;
        instance.Status = nextStage.IsTerminal ? "COMPLETED" : "ACTIVE";

        await _repo.UpdateFlowInstanceStageAsync(instanceId, nextStage.IdStage, instance.DayCounter, instance.Status);

        // 5. Registrar la transición
        await _repo.RecordStageTransitionAsync(new StageTransition
        {
            IdInstance = instanceId,
            FromStageId = fromStageId,
            ToStageId = nextStage.IdStage,
            Direction = "FORWARD",
            TriggeredBy = "MANUAL",
            ActorId = actorId
        });

        // 6. Disparar los checkpoints de la nueva etapa
        await TriggerCheckpointsForStageAsync(instanceId, instance.IdFlow, nextStage.IdStage, actorId);

        await _repo.LogAuditAsync(actorId, "STAGE_ADVANCED", instanceId, null, 
            $"{{\"from\":{fromStageId},\"to\":{nextStage.IdStage},\"stage\":\"{nextStage.StageCode}\"}}");

        return instance;
    }

    public async Task<CheckpointInstance> ResolveCheckpointAsync(long cpInstanceId, string status, long actorId)
    {
        var normalizedStatus = status.ToUpperInvariant();
        await _repo.UpdateCheckpointInstanceStatusAsync(cpInstanceId, normalizedStatus, actorId);

        var cpInstance = await _repo.GetCheckpointInstanceByIdAsync(cpInstanceId);
        CheckpointCatalog? catDef = null;
        FlowInstance? instance = null;

        if (cpInstance == null) goto LogAndReturn;

        catDef = (await _repo.GetCheckpointCatalogAsync())
            .FirstOrDefault(c => c.IdCheckpoint == cpInstance.IdCheckpoint);
        if (catDef == null) goto LogAndReturn;

        instance = await _repo.GetFlowInstanceByIdAsync(cpInstance.IdInstance);
        if (instance == null) goto LogAndReturn;

        // ── A. KO: Evaluar retroceso de etapa (Rollback) ───────────────────
        if (normalizedStatus == "KO" && catDef.RollbackToStage.HasValue && catDef.RollbackToStage > 0)
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
                await _repo.LogAuditAsync(actorId, "STAGE_ROLLBACK", instance.IdInstance, catDef.IdCheckpoint, 
                    $"{{\"from\":{fromStageId},\"to\":{rollbackStageId},\"reason\":\"CHECKPOINT_KO\"}}");
            }
        }

        // ── B. KO: Disparar checkpoints encadenados (TriggeredByKo) ────────────
        if (normalizedStatus == "KO")
        {
            var chainedCps = (await _repo.GetCheckpointCatalogAsync(instance.IdFlow))
                .Where(c => c.ApprovalStatus == "ACTIVE" && c.TriggeredByKo == catDef.IdCheckpoint)
                .ToList();

            foreach (var chained in chainedCps)
            {
                var alreadyOpen = (await _repo.GetCheckpointInstancesForFlowAsync(instance.IdInstance))
                    .Any(ci => ci.IdCheckpoint == chained.IdCheckpoint && ci.Status == "PENDING");

                if (!alreadyOpen)
                {
                    await _repo.CreateCheckpointInstanceAsync(new CheckpointInstance
                    {
                        IdInstance = instance.IdInstance,
                        IdCheckpoint = chained.IdCheckpoint,
                        Status = "PENDING",
                        OpenedAtStage = instance.CurrentStageId,
                        IsRetroactive = false,
                        OccurrenceNumber = 1
                    });
                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_CHAINED_TRIGGERED", instance.IdInstance, chained.IdCheckpoint, 
                        $"{{\"triggeredByKo\":{catDef.IdCheckpoint},\"code\":\"{chained.Code}\"}}");
                }
            }
        }

        // ── C. Recurrencia: programar la siguiente ocurrencia ──────────────────
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
            else if (catDef.FinalizesCycle)
            {
                // Agotó ocurrencias y FinalizesCycle: cerrar la instancia
                await _repo.UpdateFlowInstanceStageAsync(instance.IdInstance, instance.CurrentStageId, instance.DayCounter, "COMPLETED");
                await _repo.LogAuditAsync(actorId, "FLOW_CLOSED_BY_RECURRENCE_EXHAUSTION", instance.IdInstance, catDef.IdCheckpoint, 
                    $"{{\"maxOccurrences\":{catDef.MaxOccurrences},\"code\":\"{catDef.Code}\"}}");
            }
        }

        // ── D. FinalizesCycle (cierre inmediato por KO) ────────────────────────
        if (normalizedStatus == "KO" && catDef.FinalizesCycle && !catDef.IsRecurrent)
        {
            await _repo.UpdateFlowInstanceStageAsync(instance.IdInstance, instance.CurrentStageId, instance.DayCounter, "COMPLETED");
            await _repo.LogAuditAsync(actorId, "FLOW_CLOSED_BY_CHECKPOINT_KO", instance.IdInstance, catDef.IdCheckpoint, 
                $"{{\"code\":\"{catDef.Code}\",\"reason\":\"FINALIZES_CYCLE\"}}");
        }

    LogAndReturn:
        await _repo.LogAuditAsync(actorId, "CHECKPOINT_RESOLVED", cpInstance?.IdInstance, catDef?.IdCheckpoint, $"{{\"status\":\"{normalizedStatus}\"}}");
        return new CheckpointInstance { IdCpInstance = cpInstanceId, Status = normalizedStatus };
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
        catch
        {
            facts = new();
        }

        var catalogCheckpoints = (await _repo.GetCheckpointCatalogAsync(flowId))
            .Where(c => c.ApprovalStatus == "ACTIVE" && c.TriggerStageId == stageId && c.TriggeredByKo == null)
            .ToList();

        foreach (var cp in catalogCheckpoints)
        {
            // Evaluar precondición condicional si existe
            if (!string.IsNullOrEmpty(cp.PreconditionFact))
            {
                bool factMet = facts != null && facts.TryGetValue(cp.PreconditionFact, out var val) 
                    && (val?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true || val?.ToString() == "1");

                if (!factMet)
                {
                    await _repo.LogAuditAsync(actorId, "CHECKPOINT_SKIPPED_PRECONDITION", instanceId, cp.IdCheckpoint, 
                        $"{{\"fact\":\"{cp.PreconditionFact}\",\"met\":false}}");
                    continue; // Omitir checkpoint condicional no satisfecho
                }
            }

            var cpInst = new CheckpointInstance
            {
                IdInstance = instanceId,
                IdCheckpoint = cp.IdCheckpoint,
                Status = "PENDING",
                OpenedAtStage = stageId,
                IsRetroactive = false,
                OccurrenceNumber = 1
            };
            await _repo.CreateCheckpointInstanceAsync(cpInst);
        }
    }
}
