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
    Task<IEnumerable<FlowStage>> GetAllStagesAsync();
    Task<IEnumerable<FlowStage>> GetStagesAsync(long? flowId = null);
    Task<long> CreateStageAsync(FlowStage stage);
    Task<bool> MoveStageAsync(long stageId, string direction); // "up" | "down"
    Task<bool> SetStageOrderAsync(long stageId, short newOrderIndex);  // direct order set
    Task<bool> UpdateStageAsync(FlowStage stage); // edit name, description, sla, terminal
    
    Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId = null);
    Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCheckpointCatalogAsync(long? flowId = null);
    Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code);
    Task<long> CreateCheckpointCatalogAsync(CheckpointCatalog cp);
    Task ApproveCheckpointCatalogAsync(long checkpointId, string approvedByJson);
    Task<bool> UpdateCheckpointCampaignAsync(long checkpointId, string campaign);
    Task<bool> UpdateCheckpointPortfolioAsync(long checkpointId, string portfolio);
    Task<bool> UpdateCheckpointStageAsync(long checkpointId, long? stageId);
    Task<bool> UpdateCheckpointCatalogAsync(long id, CheckpointCatalog cp);

    Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId);
    Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps);
    
    Task<long> CreateFlowInstanceAsync(FlowInstance instance);
    Task<FlowInstance?> GetFlowInstanceAsync(string entityType, long entityId, long flowId);
    Task<FlowInstance?> GetFlowInstanceByIdAsync(long instanceId);
    Task<FlowInstance?> GetFlowInstanceByEntityAsync(string entityType, long entityId);
    Task UpdateFlowInstanceStageAsync(long instanceId, long currentStageId, int dayCounter, string status);
    Task UpdateFlowInstanceFactsAsync(long instanceId, string factsJson);
    
    Task<long> CreateCheckpointInstanceAsync(CheckpointInstance cpInst);
    Task<CheckpointInstance?> GetCheckpointInstanceByIdAsync(long cpInstanceId);
    Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId);
    Task UpdateCheckpointInstanceStatusAsync(long cpInstanceId, string status, long? resolvedBy);

    Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId);
    Task UpsertStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long? completedBy);
    Task ActivateDueScheduledCheckpointsAsync();
    
    Task RecordStageTransitionAsync(StageTransition transition);
    Task LogAuditAsync(long actorId, string action, long? instanceId, long? checkpointId, string detailJson);
    Task<IEnumerable<FlowAuditLog>> GetAuditLogsAsync(int limit = 50);
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
        const string sql = "SELECT id_stage AS IdStage, id_flow AS IdFlow, stage_code AS StageCode, name, description, order_index AS OrderIndex, is_terminal AS IsTerminal, sla_hours AS SlaHours, portfolio AS Portfolio, campaign AS Campaign, metadata FROM stage WHERE id_flow = @flowId ORDER BY order_index;";
        return await db.QueryAsync<FlowStage>(sql, new { flowId });
    }

    public async Task<IEnumerable<FlowStage>> GetAllStagesAsync()
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_stage AS IdStage, id_flow AS IdFlow, stage_code AS StageCode, name, description, order_index AS OrderIndex, is_terminal AS IsTerminal, sla_hours AS SlaHours, portfolio AS Portfolio, campaign AS Campaign, metadata FROM stage ORDER BY id_flow, order_index;";
        return await db.QueryAsync<FlowStage>(sql);
    }

    public async Task<IEnumerable<FlowStage>> GetStagesAsync(long? flowId = null)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_stage AS IdStage, id_flow AS IdFlow, stage_code AS StageCode, name, description, 
                   order_index AS OrderIndex, is_terminal AS IsTerminal, sla_hours AS SlaHours, 
                   portfolio AS Portfolio, campaign AS Campaign, metadata 
            FROM stage 
            WHERE (@flowId IS NULL OR id_flow = @flowId) 
            ORDER BY id_flow, order_index;";
        return await db.QueryAsync<FlowStage>(sql, new { flowId });
    }

    public async Task<long> CreateStageAsync(FlowStage stage)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO stage (id_flow, stage_code, name, description, order_index, is_terminal, sla_hours, portfolio, campaign, metadata)
            VALUES (@IdFlow, @StageCode, @Name, @Description, @OrderIndex, @IsTerminal, @SlaHours, @Portfolio, @Campaign, @Metadata::jsonb)
            RETURNING id_stage;";
        return await db.ExecuteScalarAsync<long>(sql, stage);
    }

    public async Task<bool> MoveStageAsync(long stageId, string direction)
    {
        using var db = CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();
        try
        {
            var current = await db.QueryFirstOrDefaultAsync<FlowStage>(
                "SELECT id_stage AS IdStage, id_flow AS IdFlow, order_index AS OrderIndex FROM stage WHERE id_stage = @stageId",
                new { stageId }, tx);
            if (current == null) return false;

            FlowStage? neighbor;
            if (direction == "up")
                neighbor = await db.QueryFirstOrDefaultAsync<FlowStage>(
                    "SELECT id_stage AS IdStage, order_index AS OrderIndex FROM stage WHERE id_flow = @IdFlow AND order_index < @OrderIndex ORDER BY order_index DESC LIMIT 1",
                    new { current.IdFlow, current.OrderIndex }, tx);
            else
                neighbor = await db.QueryFirstOrDefaultAsync<FlowStage>(
                    "SELECT id_stage AS IdStage, order_index AS OrderIndex FROM stage WHERE id_flow = @IdFlow AND order_index > @OrderIndex ORDER BY order_index ASC LIMIT 1",
                    new { current.IdFlow, current.OrderIndex }, tx);

            if (neighbor == null) return false;

            // Safe swap using temporary order value 9999 to prevent PostgreSQL unique constraint violations
            await db.ExecuteAsync("UPDATE stage SET order_index = 9999 WHERE id_stage = @id", new { id = current.IdStage }, tx);
            await db.ExecuteAsync("UPDATE stage SET order_index = @newOrder WHERE id_stage = @id", new { newOrder = current.OrderIndex, id = neighbor.IdStage }, tx);
            await db.ExecuteAsync("UPDATE stage SET order_index = @newOrder WHERE id_stage = @id", new { newOrder = neighbor.OrderIndex, id = current.IdStage }, tx);

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public async Task<bool> SetStageOrderAsync(long stageId, short newOrderIndex)
    {
        using var db = CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();
        try
        {
            var current = await db.QueryFirstOrDefaultAsync<FlowStage>(
                "SELECT id_stage AS IdStage, id_flow AS IdFlow, order_index AS OrderIndex FROM stage WHERE id_stage = @stageId",
                new { stageId }, tx);
            if (current == null) return false;

            if (current.OrderIndex == newOrderIndex) return true;

            var existingWithTarget = await db.QueryFirstOrDefaultAsync<FlowStage>(
                "SELECT id_stage AS IdStage, order_index AS OrderIndex FROM stage WHERE id_flow = @IdFlow AND order_index = @targetOrder AND id_stage != @stageId",
                new { current.IdFlow, targetOrder = newOrderIndex, stageId }, tx);

            if (existingWithTarget != null)
            {
                // Safe swap: Set current stage to 9999 temporary order to clear unique constraint
                await db.ExecuteAsync("UPDATE stage SET order_index = 9999 WHERE id_stage = @id", new { id = current.IdStage }, tx);
                // Assign current's old order to conflicting stage
                await db.ExecuteAsync("UPDATE stage SET order_index = @oldOrder WHERE id_stage = @id", new { oldOrder = current.OrderIndex, id = existingWithTarget.IdStage }, tx);
                // Assign target order to current stage
                await db.ExecuteAsync("UPDATE stage SET order_index = @newOrder WHERE id_stage = @id", new { newOrder = newOrderIndex, id = current.IdStage }, tx);
            }
            else
            {
                await db.ExecuteAsync("UPDATE stage SET order_index = @newOrder WHERE id_stage = @stageId", new { newOrder = newOrderIndex, stageId }, tx);
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            return false;
        }
    }

    public async Task<bool> UpdateStageAsync(FlowStage stage)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE stage SET
                name        = @Name,
                description = @Description,
                sla_hours   = @SlaHours,
                is_terminal = @IsTerminal,
                portfolio   = @Portfolio,
                campaign    = @Campaign
            WHERE id_stage  = @IdStage;";
        var rows = await db.ExecuteAsync(sql, stage);
        return rows > 0;
    }

    public async Task<IEnumerable<CheckpointCatalog>> GetCheckpointCatalogAsync(long? flowId = null)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_checkpoint AS IdCheckpoint, code, name, description, id_flow AS IdFlow, trigger_stage_id AS TriggerStageId, origin, scope, blocks, blocks_advance AS BlocksAdvance, rollback_to_stage AS RollbackToStage, triggered_by_ko AS TriggeredByKo, is_recurrent AS IsRecurrent, recurrence_days AS RecurrenceDays, max_occurrences AS MaxOccurrences, owner_dept AS OwnerDept, category AS Category, division AS Division, approval_job_title AS ApprovalJobTitle, satellites AS Satellites, execution_order AS ExecutionOrder, rollback_to_checkpoint_code AS RollbackToCheckpointCode, rollback_to_step_order AS RollbackToStepOrder, precondition_fact AS PreconditionFact, portfolio AS Portfolio, campaign AS Campaign, provider AS Provider, finalizes_cycle AS FinalizesCycle, target_roles AS TargetRoles, approval_status AS ApprovalStatus, approved_by AS ApprovedBy, is_active AS IsActive, version, created_by AS CreatedBy, created_at AS CreatedAt
            FROM checkpoint_catalog
            WHERE is_active = true AND (@flowId IS NULL OR id_flow = @flowId)
            ORDER BY execution_order ASC, id_checkpoint ASC;";
        return await db.QueryAsync<CheckpointCatalog>(sql, new { flowId });
    }

    public async Task<IEnumerable<CheckpointCatalogWithStepsDto>> GetFullCheckpointCatalogAsync(long? flowId = null)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT cc.id_checkpoint AS IdCheckpoint, cc.code, cc.name, cc.description, cc.id_flow AS IdFlow, 
                   cc.trigger_stage_id AS TriggerStageId, cc.origin, cc.scope, cc.blocks, cc.blocks_advance AS BlocksAdvance, 
                   cc.rollback_to_stage AS RollbackToStage, cc.triggered_by_ko AS TriggeredByKo, cc.is_recurrent AS IsRecurrent, 
                   cc.recurrence_days AS RecurrenceDays, cc.max_occurrences AS MaxOccurrences, cc.owner_dept AS OwnerDept, 
                   cc.category AS Category, cc.division AS Division, cc.approval_job_title AS ApprovalJobTitle, 
                   cc.satellites AS Satellites, cc.execution_order AS ExecutionOrder, 
                   cc.rollback_to_checkpoint_code AS RollbackToCheckpointCode, cc.rollback_to_step_order AS RollbackToStepOrder, 
                   cc.precondition_fact AS PreconditionFact, cc.portfolio AS Portfolio, cc.campaign AS Campaign, 
                   cc.provider AS Provider, cc.finalizes_cycle AS FinalizesCycle, cc.target_roles AS TargetRoles, 
                   cc.approval_status AS ApprovalStatus, cc.approved_by AS ApprovedBy, cc.is_active AS IsActive, 
                   cc.version, cc.created_by AS CreatedBy, cc.created_at AS CreatedAt,
                   cs.id_step AS IdStep, cs.id_checkpoint AS IdCheckpoint, cs.step_order AS StepOrder, 
                   cs.name AS Name, cs.is_required AS IsRequired
            FROM checkpoint_catalog cc
            LEFT JOIN checkpoint_step cs ON cs.id_checkpoint = cc.id_checkpoint
            WHERE cc.is_active = true AND (@flowId IS NULL OR cc.id_flow = @flowId)
            ORDER BY cc.execution_order ASC, cc.id_checkpoint ASC, cs.step_order ASC;";

        var lookup = new Dictionary<long, CheckpointCatalogWithStepsDto>();
        await db.QueryAsync<CheckpointCatalogWithStepsDto, CheckpointStep, CheckpointCatalogWithStepsDto>(
            sql,
            (cp, step) =>
            {
                if (!lookup.TryGetValue(cp.IdCheckpoint, out var entry))
                {
                    entry = cp;
                    entry.Steps = new List<CheckpointStep>();
                    lookup.Add(entry.IdCheckpoint, entry);
                }
                if (step != null && step.IdStep > 0)
                {
                    entry.Steps.Add(step);
                }
                return entry;
            },
            new { flowId },
            splitOn: "IdStep"
        );

        return lookup.Values;
    }

    public async Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_checkpoint AS IdCheckpoint, code, name, description, id_flow AS IdFlow, trigger_stage_id AS TriggerStageId, origin, scope, blocks, blocks_advance AS BlocksAdvance, rollback_to_stage AS RollbackToStage, triggered_by_ko AS TriggeredByKo, is_recurrent AS IsRecurrent, recurrence_days AS RecurrenceDays, max_occurrences AS MaxOccurrences, owner_dept AS OwnerDept, category AS Category, division AS Division, approval_job_title AS ApprovalJobTitle, satellites AS Satellites, execution_order AS ExecutionOrder, rollback_to_checkpoint_code AS RollbackToCheckpointCode, rollback_to_step_order AS RollbackToStepOrder, precondition_fact AS PreconditionFact, portfolio AS Portfolio, campaign AS Campaign, provider AS Provider, finalizes_cycle AS FinalizesCycle, target_roles AS TargetRoles, approval_status AS ApprovalStatus, approved_by AS ApprovedBy, is_active AS IsActive, version, created_by AS CreatedBy, created_at AS CreatedAt
            FROM checkpoint_catalog WHERE code = @code AND is_active = true;";
        return await db.QueryFirstOrDefaultAsync<CheckpointCatalog>(sql, new { code });
    }

    public async Task<long> CreateCheckpointCatalogAsync(CheckpointCatalog cp)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO checkpoint_catalog (code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, triggered_by_ko, is_recurrent, recurrence_days, max_occurrences, owner_dept, category, division, approval_job_title, satellites, execution_order, rollback_to_checkpoint_code, rollback_to_step_order, precondition_fact, portfolio, campaign, provider, finalizes_cycle, target_roles, approval_status, approved_by, is_active, version, created_by)
            VALUES (@Code, @Name, @Description, @IdFlow, @TriggerStageId, @Origin, @Scope, @Blocks, @BlocksAdvance, @RollbackToStage, @TriggeredByKo, @IsRecurrent, @RecurrenceDays, @MaxOccurrences, @OwnerDept, @Category, @Division, @ApprovalJobTitle, @Satellites, @ExecutionOrder, @RollbackToCheckpointCode, @RollbackToStepOrder, @PreconditionFact, @Portfolio, @Campaign, @Provider, @FinalizesCycle, @TargetRoles, @ApprovalStatus, @ApprovedBy::jsonb, true, 1, @CreatedBy)
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
            INSERT INTO flow_instance (id_flow, entity_type, entity_id, current_stage_id, day_counter, metadata, facts, status)
            VALUES (@IdFlow, @EntityType, @EntityId, @CurrentStageId, @DayCounter, @Metadata::jsonb, COALESCE(@Facts::jsonb, '{}'::jsonb), 'ACTIVE')
            RETURNING id_instance;";
        return await db.ExecuteScalarAsync<long>(sql, inst);
    }

    public async Task<bool> UpdateCheckpointCampaignAsync(long checkpointId, string campaign)
    {
        using var db = CreateConnection();
        const string sql = "UPDATE checkpoint_catalog SET campaign = @campaign WHERE id_checkpoint = @checkpointId;";
        var rows = await db.ExecuteAsync(sql, new { checkpointId, campaign });
        return rows > 0;
    }

    public async Task<bool> UpdateCheckpointPortfolioAsync(long checkpointId, string portfolio)
    {
        using var db = CreateConnection();
        const string sql = "UPDATE checkpoint_catalog SET portfolio = @portfolio WHERE id_checkpoint = @checkpointId;";
        var rows = await db.ExecuteAsync(sql, new { checkpointId, portfolio });
        return rows > 0;
    }

    public async Task<bool> UpdateCheckpointStageAsync(long checkpointId, long? stageId)
    {
        using var db = CreateConnection();
        const string sql = "UPDATE checkpoint_catalog SET trigger_stage_id = @stageId WHERE id_checkpoint = @checkpointId;";
        var rows = await db.ExecuteAsync(sql, new { checkpointId, stageId });
        return rows > 0;
    }

    public async Task<bool> UpdateCheckpointCatalogAsync(long id, CheckpointCatalog cp)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE checkpoint_catalog
            SET name = @Name,
                description = @Description,
                origin = @Origin,
                scope = @Scope,
                trigger_stage_id = COALESCE(NULLIF(@TriggerStageId, 0), trigger_stage_id),
                blocks_advance = @BlocksAdvance,
                execution_order = @ExecutionOrder,
                category = @Category,
                division = @Division,
                approval_job_title = @ApprovalJobTitle,
                satellites = @Satellites,
                rollback_to_checkpoint_code = @RollbackToCheckpointCode,
                rollback_to_step_order = @RollbackToStepOrder,
                precondition_fact = @PreconditionFact,
                portfolio = @Portfolio,
                campaign = @Campaign,
                provider = @Provider,
                finalizes_cycle = @FinalizesCycle,
                target_roles = @TargetRoles
            WHERE id_checkpoint = @id;";
        var rows = await db.ExecuteAsync(sql, new {
            id,
            cp.Name,
            cp.Description,
            cp.Origin,
            cp.Scope,
            cp.TriggerStageId,
            cp.BlocksAdvance,
            cp.ExecutionOrder,
            cp.Category,
            cp.Division,
            cp.ApprovalJobTitle,
            cp.Satellites,
            cp.RollbackToCheckpointCode,
            cp.RollbackToStepOrder,
            cp.PreconditionFact,
            cp.Portfolio,
            cp.Campaign,
            cp.Provider,
            cp.FinalizesCycle,
            cp.TargetRoles
        });
        return rows > 0;
    }

    public async Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_step AS IdStep, id_checkpoint AS IdCheckpoint, step_order AS StepOrder, name AS Name, is_required AS IsRequired
            FROM checkpoint_step
            WHERE id_checkpoint = @checkpointId
            ORDER BY step_order ASC;";
        return await db.QueryAsync<CheckpointStep>(sql, new { checkpointId });
    }

    public async Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps)
    {
        using var db = CreateConnection();
        const string deleteSql = "DELETE FROM checkpoint_step WHERE id_checkpoint = @checkpointId;";
        await db.ExecuteAsync(deleteSql, new { checkpointId });

        if (steps != null && steps.Any())
        {
            const string insertSql = @"
                INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
                VALUES (@checkpointId, @StepOrder, @Name, @IsRequired);";
            foreach (var step in steps)
            {
                if (!string.IsNullOrWhiteSpace(step.Name))
                {
                    await db.ExecuteAsync(insertSql, new { checkpointId, step.StepOrder, Name = step.Name.Trim(), step.IsRequired });
                }
            }
        }
    }

    public async Task<FlowInstance?> GetFlowInstanceAsync(string entityType, long entityId, long flowId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_instance AS IdInstance, id_flow AS IdFlow, entity_type AS EntityType, entity_id AS EntityId, 
                   current_stage_id AS CurrentStageId, day_counter AS DayCounter, metadata, facts, status, 
                   created_at AS CreatedAt, completed_at AS CompletedAt
            FROM flow_instance
            WHERE entity_type = @entityType AND entity_id = @entityId AND id_flow = @flowId;";
        return await db.QueryFirstOrDefaultAsync<FlowInstance>(sql, new { entityType, entityId, flowId });
    }

    public async Task<FlowInstance?> GetFlowInstanceByIdAsync(long instanceId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_instance AS IdInstance, id_flow AS IdFlow, entity_type AS EntityType, entity_id AS EntityId, 
                   current_stage_id AS CurrentStageId, day_counter AS DayCounter, metadata, facts, status, 
                   created_at AS CreatedAt, completed_at AS CompletedAt
            FROM flow_instance
            WHERE id_instance = @instanceId;";
        return await db.QueryFirstOrDefaultAsync<FlowInstance>(sql, new { instanceId });
    }

    public async Task<FlowInstance?> GetFlowInstanceByEntityAsync(string entityType, long entityId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_instance AS IdInstance, id_flow AS IdFlow, entity_type AS EntityType, entity_id AS EntityId, 
                   current_stage_id AS CurrentStageId, day_counter AS DayCounter, metadata, facts, status, 
                   created_at AS CreatedAt, completed_at AS CompletedAt
            FROM flow_instance
            WHERE entity_type = @entityType AND entity_id = @entityId
            ORDER BY id_instance DESC
            LIMIT 1;";
        return await db.QueryFirstOrDefaultAsync<FlowInstance>(sql, new { entityType, entityId });
    }

    public async Task UpdateFlowInstanceStageAsync(long instanceId, long currentStageId, int dayCounter, string status)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE flow_instance
            SET current_stage_id = @currentStageId, day_counter = @dayCounter, status = @status, 
                completed_at = CASE WHEN @status = 'COMPLETED' THEN CURRENT_TIMESTAMP ELSE completed_at END
            WHERE id_instance = @instanceId;";
        await db.ExecuteAsync(sql, new { instanceId, currentStageId, dayCounter, status });
    }

    public async Task UpdateFlowInstanceFactsAsync(long instanceId, string factsJson)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE flow_instance
            SET facts = COALESCE(@factsJson::jsonb, '{}'::jsonb)
            WHERE id_instance = @instanceId;";
        await db.ExecuteAsync(sql, new { instanceId, factsJson });
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

    public async Task<CheckpointInstance?> GetCheckpointInstanceByIdAsync(long cpInstanceId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_cp_instance AS IdCpInstance, id_instance AS IdInstance, id_checkpoint AS IdCheckpoint, 
                   status, opened_at_stage AS OpenedAtStage, is_retroactive AS IsRetroactive, 
                   occurrence_number AS OccurrenceNumber, scheduled_for AS ScheduledFor, 
                   resolved_by AS ResolvedBy, resolved_at AS ResolvedAt, created_at AS CreatedAt
            FROM checkpoint_instance
            WHERE id_cp_instance = @cpInstanceId;";
        return await db.QueryFirstOrDefaultAsync<CheckpointInstance>(sql, new { cpInstanceId });
    }

    public async Task<IEnumerable<CheckpointInstance>> GetCheckpointInstancesForFlowAsync(long instanceId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_cp_instance AS IdCpInstance, id_instance AS IdInstance, id_checkpoint AS IdCheckpoint, 
                   status, opened_at_stage AS OpenedAtStage, is_retroactive AS IsRetroactive, 
                   occurrence_number AS OccurrenceNumber, scheduled_for AS ScheduledFor, 
                   resolved_by AS ResolvedBy, resolved_at AS ResolvedAt, created_at AS CreatedAt
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

    public async Task<IEnumerable<CheckpointStepProgress>> GetStepProgressAsync(long cpInstanceId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_progress AS IdProgress, id_cp_instance AS IdCpInstance, id_step AS IdStep, 
                   is_completed AS IsCompleted, completed_by AS CompletedBy, completed_at AS CompletedAt
            FROM checkpoint_step_progress
            WHERE id_cp_instance = @cpInstanceId
            ORDER BY id_step;";
        return await db.QueryAsync<CheckpointStepProgress>(sql, new { cpInstanceId });
    }

    public async Task UpsertStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long? completedBy)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO checkpoint_step_progress (id_cp_instance, id_step, is_completed, completed_by, completed_at)
            VALUES (@cpInstanceId, @stepId, @isCompleted, @completedBy, CASE WHEN @isCompleted THEN CURRENT_TIMESTAMP ELSE NULL END)
            ON CONFLICT (id_cp_instance, id_step)
            DO UPDATE SET 
                is_completed = @isCompleted, 
                completed_by = @completedBy, 
                completed_at = CASE WHEN @isCompleted THEN CURRENT_TIMESTAMP ELSE NULL END;";
        await db.ExecuteAsync(sql, new { cpInstanceId, stepId, isCompleted, completedBy });
    }

    public async Task ActivateDueScheduledCheckpointsAsync()
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE checkpoint_instance ci
            SET status = 'PENDING',
                opened_at_stage = (
                    SELECT fi.current_stage_id 
                    FROM flow_instance fi 
                    WHERE fi.id_instance = ci.id_instance
                )
            WHERE ci.status = 'SCHEDULED' 
              AND ci.scheduled_for IS NOT NULL 
              AND ci.scheduled_for <= CURRENT_TIMESTAMP;";
        await db.ExecuteAsync(sql);
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

    public async Task<IEnumerable<FlowAuditLog>> GetAuditLogsAsync(int limit = 50)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_log AS IdLog, id_instance AS IdInstance, id_checkpoint AS IdCheckpoint, action, actor_id AS ActorId, detail::text AS Detail, checksum, created_at AS CreatedAt FROM audit_log ORDER BY id_log DESC LIMIT @limit;";
        return await db.QueryAsync<FlowAuditLog>(sql, new { limit });
    }
}
