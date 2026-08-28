using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Application;

public interface ICycleService
{
    // Ciclos
    Task<IEnumerable<CycleDefinition>> GetCyclesAsync(bool includeInactive = false);
    Task<CycleDefinitionDetailDto?> GetCycleDetailAsync(long cycleId, bool includeInactive = false);
    Task<CycleDefinition> CreateCycleAsync(CycleDefinition cycle);
    Task<bool> UpdateCycleAsync(long id, CycleDefinition cycle);
    Task<bool> SoftDeleteCycleAsync(long cycleId);

    // Etapas
    Task<IEnumerable<CycleStage>> GetStagesByCycleAsync(long cycleId);
    Task<CycleStage> CreateStageAsync(CycleStage stage);
    Task<bool> ReorderStagesAsync(long cycleId, List<long> stageIdsInOrder);
    Task<bool> UpdateStageAsync(long id, CycleStage stage);
    Task<bool> DeleteStageAsync(long stageId);

    // Checkpoints
    Task<IEnumerable<CheckpointCatalog>> GetCheckpointsByCycleAsync(long cycleId, bool includeInactive = false);
    Task<IEnumerable<CheckpointCatalogDetailDto>> GetFullCheckpointsByCycleAsync(long cycleId, bool includeInactive = false);
    Task<CheckpointCatalog?> GetCheckpointByIdAsync(long id);
    Task<CheckpointCatalogDetailDto?> GetFullCheckpointByIdAsync(long id);
    Task<CheckpointCatalog> CreateCheckpointAsync(SaveCheckpointDto cp);
    Task<bool> UpdateCheckpointAsync(long id, SaveCheckpointDto cp);
    Task<bool> SoftDeleteCheckpointAsync(long cpId);
    Task<bool> ToggleCheckpointActiveAsync(long cpId);
    Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps);
    Task SaveCheckpointCanvasSchemaAsync(long checkpointId, string canvasSchemaJson);

    // Metadatos y Conciliación (Roles y Carteras)
    Task<IEnumerable<MetaRole>> GetMetaRolesAsync();
    Task<MetaRole> CreateMetaRoleAsync(MetaRole role);
    Task<IEnumerable<MetaPortfolio>> GetMetaPortfoliosAsync();
    Task<MetaPortfolio> CreateMetaPortfolioAsync(MetaPortfolio portfolio);

    // Políticas
    Task<IEnumerable<CyclePolicyRule>> GetPoliciesAsync(long? cycleId);
    Task<CyclePolicyRule> SavePolicyRuleAsync(CyclePolicyRule rule);

    // Instancias de Ciclo y Ejecución
    Task<CycleInstanceDetailDto> StartCycleInstanceAsync(string cycleCode, string entityType, long entityId, long actorId);
    Task<CycleInstanceDetailDto?> GetInstanceDetailByEntityAsync(string entityType, long entityId);
    Task<CycleInstanceDetailDto?> GetInstanceDetailByIdAsync(long instanceId);
    Task<CycleValidationResultDto> ValidateStageAdvanceAsync(long instanceId);
    Task<CycleInstanceDetailDto> AdvanceStageAsync(long instanceId, long actorId);
    Task<ResolveCheckpointResultDto> ResolveCheckpointAsync(long cpInstanceId, string status, string answersJson, long actorId);

    // Contrato Zero-Logic UI & Ejecución de Acciones
    Task<UiContextDto> GetUiContextAsync(long instanceId, long actorId);
    Task<ExecuteActionResultDto> ExecuteActionAsync(long instanceId, ExecuteActionRequest req);

    // Importación / Exportación JSON (GSI Backup)
    Task<GsiImportResultDto> ImportGsiBackupJsonAsync(long cycleId, string jsonContent);
    Task<string> ExportGsiBackupJsonAsync(long cycleId);

    // Simulación Temporal (D+X)
    Task<int> SimulateTimeAdvanceAsync(long instanceId, int days);

    // Handshake de Telefonía & Ownership
    Task<(bool Success, string Message)> RequestHandshakeAsync(long instanceId, long targetActorId, long actorId, string? context);
    Task<(bool Success, string Message)> AcceptHandshakeAsync(long instanceId, long actorId);
    Task<(bool Success, string Message)> CancelHandshakeAsync(long instanceId, long actorId);
    Task<(bool Success, string Message)> RejectHandshakeAsync(long instanceId, long actorId, string reason);
    Task<(bool Success, string Message)> RevertHandshakeAsync(long instanceId, long actorId, string reason);

    // Auditoría
    Task<IEnumerable<CycleAuditLog>> GetAuditLogsAsync(int limit = 50);
}
