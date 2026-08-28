using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Infrastructure;

public interface ICycleRepository
{
    // Ciclos
    Task<IEnumerable<CycleDefinition>> GetCyclesAsync(bool includeInactive = false);
    Task<CycleDefinition?> GetCycleByIdAsync(long id);
    Task<CycleDefinition?> GetCycleByCodeAsync(string code);
    Task<long> CreateCycleAsync(CycleDefinition cycle);
    Task<bool> UpdateCycleAsync(CycleDefinition cycle);
    Task<bool> SoftDeleteCycleAsync(long cycleId);

    // Etapas
    Task<IEnumerable<CycleStage>> GetStagesByCycleAsync(long cycleId);
    Task<CycleStage?> GetStageByIdAsync(long stageId);
    Task<long> CreateStageAsync(CycleStage stage);
    Task<bool> UpdateStageAsync(CycleStage stage);
    Task<bool> UpdateStageOrderAsync(long stageId, short orderIndex);
    Task<bool> DeleteStageAsync(long stageId);

    // Checkpoints
    Task<IEnumerable<CheckpointCatalog>> GetCheckpointsByCycleAsync(long cycleId, bool includeInactive = false);
    Task<IEnumerable<CheckpointCatalogDetailDto>> GetFullCheckpointsByCycleAsync(long cycleId, bool includeInactive = false);
    Task<CheckpointCatalog?> GetCheckpointByIdAsync(long id);
    Task<CheckpointCatalogDetailDto?> GetFullCheckpointByIdAsync(long id);
    Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code);
    Task<long> CreateCheckpointAsync(CheckpointCatalog cp);
    Task<bool> UpdateCheckpointAsync(CheckpointCatalog cp);
    Task<bool> SoftDeleteCheckpointAsync(long cpId);
    Task<bool> ToggleCheckpointActiveAsync(long cpId);
    Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId);
    Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps);
    Task<bool> UpdateCheckpointCanvasSchemaAsync(long checkpointId, string canvasSchemaJson);
    Task BulkUpsertCheckpointsAsync(long cycleId, IEnumerable<CheckpointCatalog> checkpoints);

    // Metadatos y Conciliación (Roles y Carteras)
    Task<IEnumerable<MetaRole>> GetMetaRolesAsync();
    Task<long> CreateMetaRoleAsync(MetaRole role);
    Task<IEnumerable<MetaPortfolio>> GetMetaPortfoliosAsync();
    Task<long> CreateMetaPortfolioAsync(MetaPortfolio portfolio);

    // Políticas
    Task<IEnumerable<CyclePolicyRule>> GetPoliciesAsync(long? cycleId);
    Task<CyclePolicyRule?> GetPolicyByCodeAsync(string code);
    Task<long> SavePolicyRuleAsync(CyclePolicyRule rule);

    // Instancias
    Task<long> CreateInstanceAsync(CycleInstance instance);
    Task<CycleInstance?> GetInstanceByIdAsync(long instanceId);
    Task<CycleInstance?> GetInstanceByEntityAsync(string entityType, long entityId);
    Task<bool> UpdateInstanceAsync(CycleInstance instance);
    Task<IEnumerable<CheckpointInstanceDetailDto>> GetCheckpointInstancesForInstanceAsync(long instanceId);
    Task<long> CreateCheckpointInstanceAsync(CheckpointInstance cpInst);
    Task<CheckpointInstance?> GetCheckpointInstanceByIdAsync(long cpInstanceId);
    Task<bool> UpdateCheckpointInstanceAsync(CheckpointInstance cpInst);
    Task<long> CreateTransitionAsync(StageTransition transition);
    Task<IEnumerable<StageTransition>> GetTransitionsForInstanceAsync(long instanceId);

    // Activación programada y simulación temporal
    Task ActivateDueScheduledCheckpointsAsync();
    Task<int> FastForwardTimeAsync(long instanceId, int days);
    Task<long> LogAuditAsync(long actorId, string action, long? instanceId, long? checkpointId, string detail);
    Task<IEnumerable<CycleAuditLog>> GetAuditLogsAsync(int limit = 50);
}
