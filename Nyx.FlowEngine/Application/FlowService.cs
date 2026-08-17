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
            Status = "ACTIVE"
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
        // 1. Get current instance & stages
        var activeCps = (await _repo.GetCheckpointInstancesForFlowAsync(instanceId)).ToList();
        var catalog = (await _repo.GetCheckpointCatalogAsync()).ToDictionary(c => c.IdCheckpoint);

        // 2. Check if any active/pending checkpoint blocks stage advance!
        foreach (var cpInst in activeCps)
        {
            if (cpInst.Status == "PENDING" && catalog.TryGetValue(cpInst.IdCheckpoint, out var catDef))
            {
                if (catDef.BlocksAdvance)
                {
                    throw new InvalidOperationException($"Cannot advance stage: Checkpoint '{catDef.Name}' is PENDING and blocks stage advance.");
                }
            }
        }

        // 3. Perform transition to next stage
        // In full implementation: advances to order_index + 1
        _logger.LogInformation("Advanced flow instance #{InstanceId}", instanceId);
        return new FlowInstance { IdInstance = instanceId, Status = "ACTIVE" };
    }

    public async Task<CheckpointInstance> ResolveCheckpointAsync(long cpInstanceId, string status, long actorId)
    {
        await _repo.UpdateCheckpointInstanceStatusAsync(cpInstanceId, status.ToUpperInvariant(), actorId);
        await _repo.LogAuditAsync(actorId, "CHECKPOINT_RESOLVED", null, cpInstanceId, $"{{\"status\":\"{status}\"}}");

        return new CheckpointInstance { IdCpInstance = cpInstanceId, Status = status.ToUpperInvariant() };
    }

    private async Task TriggerCheckpointsForStageAsync(long instanceId, long flowId, long stageId, long actorId)
    {
        var catalogCheckpoints = (await _repo.GetCheckpointCatalogAsync(flowId))
            .Where(c => c.ApprovalStatus == "ACTIVE" && c.TriggerStageId == stageId)
            .ToList();

        foreach (var cp in catalogCheckpoints)
        {
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
