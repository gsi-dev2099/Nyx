using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;
using Nyx.FlowEngine.Domain.Entities;

namespace Nyx.FlowEngine.Infrastructure;

public interface IFlowRepository
{
    Task<IEnumerable<FlowDefinition>> GetFlowDefinitionsAsync();
    Task<FlowDefinition?> GetFlowByCodeAsync(string code);
    Task<IEnumerable<FlowStage>> GetFlowStagesAsync(long flowId);
    
    Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId = null);
    Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code);
    Task<long> CreateCheckpointCatalogAsync(CheckpointCatalog cp);
    Task ApproveCheckpointCatalogAsync(long checkpointId, string approvedByJson);
    
    Task<long> CreateFlowInstanceAsync(FlowInstance instance);
    Task<FlowInstance?> GetFlowInstanceAsync(string entityType, long entityId, long flowId);
    Task UpdateFlowInstanceStageAsync(long instanceId, long currentStageId, int dayCounter, string status);
    
    Task<long> CreateCheckpointInstanceAsync(CheckpointInstance cpInst);
    Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId);
    Task UpdateCheckpointInstanceStatusAsync(long cpInstanceId, string status, long? resolvedBy);
    
    Task RecordStageTransitionAsync(StageTransition transition);
    Task LogAuditAsync(long actorId, string action, long? instanceId, long? checkpointId, string detailJson);
}

public class FlowRepository : IFlowRepository
{
    private readonly string _connectionString;

    public FlowRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection configuration is missing.");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<FlowDefinition>> GetFlowDefinitionsAsync()
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_flow AS IdFlow, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, is_active AS IsActive, current_version AS CurrentVersion, created_by AS CreatedBy, created_at AS CreatedAt FROM flow_definition WHERE is_active = true ORDER BY id_flow;";
        return await db.QueryAsync<FlowDefinition>(sql);
    }

    public async Task<FlowDefinition?> GetFlowByCodeAsync(string code)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_flow AS IdFlow, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, is_active AS IsActive, current_version AS CurrentVersion, created_by AS CreatedBy, created_at AS CreatedAt FROM flow_definition WHERE code = @code AND is_active = true;";
        return await db.QueryFirstOrDefaultAsync<FlowDefinition>(sql, new { code });
    }

    public async Task<IEnumerable<FlowStage>> GetFlowStagesAsync(long flowId)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_stage AS IdStage, id_flow AS IdFlow, stage_code AS StageCode, name, description, order_index AS OrderIndex, is_terminal AS IsTerminal, sla_hours AS SlaHours, metadata FROM stage WHERE id_flow = @flowId ORDER BY order_index;";
        return await db.QueryAsync<FlowStage>(sql, new { flowId });
    }

    public async Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId = null)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_checkpoint AS IdCheckpoint, code, name, description, id_flow AS IdFlow, trigger_stage_id AS TriggerStageId, origin, scope, blocks, blocks_advance AS BlocksAdvance, rollback_to_stage AS RollbackToStage, triggered_by_ko AS TriggeredByKo, is_recurrent AS IsRecurrent, recurrence_days AS RecurrenceDays, max_occurrences AS MaxOccurrences, owner_dept AS OwnerDept, approval_status AS ApprovalStatus, approved_by AS ApprovedBy, is_active AS IsActive, version, created_by AS CreatedBy, created_at AS CreatedAt
            FROM checkpoint_catalog
            WHERE is_active = true AND (@flowId IS NULL OR id_flow IS NULL OR id_flow = @flowId)
            ORDER BY id_checkpoint;";
        return await db.QueryAsync<CheckpointCatalog>(sql, new { flowId });
    }

    public async Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_checkpoint AS IdCheckpoint, code, name, description, id_flow AS IdFlow, trigger_stage_id AS TriggerStageId, origin, scope, blocks, blocks_advance AS BlocksAdvance, rollback_to_stage AS RollbackToStage, triggered_by_ko AS TriggeredByKo, is_recurrent AS IsRecurrent, recurrence_days AS RecurrenceDays, max_occurrences AS MaxOccurrences, owner_dept AS OwnerDept, approval_status AS ApprovalStatus, approved_by AS ApprovedBy, is_active AS IsActive, version, created_by AS CreatedBy, created_at AS CreatedAt
            FROM checkpoint_catalog WHERE code = @code AND is_active = true;";
        return await db.QueryFirstOrDefaultAsync<CheckpointCatalog>(sql, new { code });
    }

    public async Task<long> CreateCheckpointCatalogAsync(CheckpointCatalog cp)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO checkpoint_catalog (code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, triggered_by_ko, is_recurrent, recurrence_days, max_occurrences, owner_dept, approval_status, approved_by, is_active, version, created_by)
            VALUES (@Code, @Name, @Description, @IdFlow, @TriggerStageId, @Origin, @Scope, @Blocks, @BlocksAdvance, @RollbackToStage, @TriggeredByKo, @IsRecurrent, @RecurrenceDays, @MaxOccurrences, @OwnerDept, @ApprovalStatus, @ApprovedBy::jsonb, true, 1, @CreatedBy)
            RETURNING id_checkpoint;";
        return await db.ExecuteScalarAsync<long>(sql, cp);
    }

    public async Task ApproveCheckpointCatalogAsync(long checkpointId, string approvedByJson)
    {
        using var db = CreateConnection();
        const string sql = "UPDATE checkpoint_catalog SET approval_status = 'ACTIVE', approved_by = @approvedByJson::jsonb WHERE id_checkpoint = @checkpointId;";
        await db.ExecuteAsync(sql, new { checkpointId, approvedByJson });
    }

    public async Task<long> CreateFlowInstanceAsync(FlowInstance inst)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, day_counter, metadata, status)
            VALUES (@IdFlow, @EntityType, @EntityId, @CurrentStageId, @DayCounter, @Metadata::jsonb, 'ACTIVE')
            RETURNING id_instance;";
        return await db.ExecuteScalarAsync<long>(sql, inst);
    }

    public async Task<FlowInstance?> GetFlowInstanceAsync(string entityType, long entityId, long flowId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_instance AS IdInstance, id_flow AS IdFlow, entity_type AS EntityType, entity_id AS EntityId, current_stage_id AS CurrentStageId, day_counter AS DayCounter, metadata, status, created_at AS CreatedAt, completed_at AS CompletedAt
            FROM flow_instance
            WHERE entity_type = @entityType AND entity_id = @entityId AND id_flow = @flowId;";
        return await db.QueryFirstOrDefaultAsync<FlowInstance>(sql, new { entityType, entityId, flowId });
    }

    public async Task UpdateFlowInstanceStageAsync(long instanceId, long currentStageId, int dayCounter, string status)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE flow_instance
            SET current_stage_id = @currentStageId, day_counter = @dayCounter, status = @status, completed_at = CASE WHEN @status = 'COMPLETED' THEN CURRENT_TIMESTAMP ELSE completed_at END
            WHERE id_instance = @instanceId;";
        await db.ExecuteAsync(sql, new { instanceId, currentStageId, dayCounter, status });
    }

    public async Task<long> CreateCheckpointInstanceAsync(CheckpointInstance cpInst)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO checkpoint_instance (id_instance, id_checkpoint, status, opened_at_stage, is_retroactive, occurrence_number, scheduled_for)
            VALUES (@IdInstance, @IdCheckpoint, @Status, @OpenedAtStage, @IsRetroactive, @OccurrenceNumber, @ScheduledFor)
            RETURNING id_cp_instance;";
        return await db.ExecuteScalarAsync<long>(sql, cpInst);
    }

    public async Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_cp_instance AS IdCpInstance, id_instance AS IdInstance, id_checkpoint AS IdCheckpoint, status, opened_at_stage AS OpenedAtStage, is_retroactive AS IsRetroactive, occurrence_number AS OccurrenceNumber, scheduled_for AS ScheduledFor, resolved_by AS ResolvedBy, resolved_at AS ResolvedAt, created_at AS CreatedAt
            FROM checkpoint_instance
            WHERE id_instance = @instanceId ORDER BY id_cp_instance;";
        return await db.QueryAsync<CheckpointInstance>(sql, new { instanceId });
    }

    public async Task UpdateCheckpointInstanceStatusAsync(long cpInstanceId, string status, long? resolvedBy)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE checkpoint_instance
            SET status = @status, resolved_by = @resolvedBy, resolved_at = CURRENT_TIMESTAMP
            WHERE id_cp_instance = @cpInstanceId;";
        await db.ExecuteAsync(sql, new { cpInstanceId, status, resolvedBy });
    }

    public async Task RecordStageTransitionAsync(StageTransition t)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO stage_transition (id_instance, from_stage_id, to_stage_id, direction, triggered_by, actor_id)
            VALUES (@IdInstance, @FromStageId, @ToStageId, @Direction, @TriggeredBy, @ActorId);";
        await db.ExecuteAsync(sql, t);
    }

    public async Task LogAuditAsync(long actorId, string action, long? instanceId, long? checkpointId, string detailJson)
    {
        using var db = CreateConnection();
        var timestamp = DateTime.UtcNow.ToString("o");
        var rawData = $"{actorId}|{action}|{instanceId}|{checkpointId}|{detailJson}|{timestamp}";
        using var sha = SHA512.Create();
        var checksum = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawData)));

        const string sql = @"
            INSERT INTO audit_log (id_instance, id_checkpoint, action, actor_id, detail, checksum)
            VALUES (@instanceId, @checkpointId, @action, @actorId, @detailJson::jsonb, @checksum);";
        await db.ExecuteAsync(sql, new { instanceId, checkpointId, action, actorId, detailJson, checksum });
    }
}
