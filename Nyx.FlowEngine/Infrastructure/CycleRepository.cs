using Dapper;
using Npgsql;
using Nyx.FlowEngine.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Nyx.FlowEngine.Infrastructure;

public class CycleRepository : ICycleRepository
{
    private readonly string _connectionString;

    public CycleRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") 
            ?? "Host=crm_postgres;Port=5432;Database=nyx_flow;Username=usr_flow;Password=Flow$$Nyx2026!Engine#Key";
    }

    private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    // ==========================================
    // CICLOS
    // ==========================================
    public async Task<IEnumerable<CycleDefinition>> GetCyclesAsync(bool includeInactive = false)
    {
        using var conn = CreateConnection();
        string sql = includeInactive 
            ? "SELECT * FROM nyx_flow.cycle_definition ORDER BY id_cycle ASC;"
            : "SELECT * FROM nyx_flow.cycle_definition WHERE is_active = true ORDER BY id_cycle ASC;";
        return await conn.QueryAsync<CycleDefinition>(sql);
    }

    public async Task<CycleDefinition?> GetCycleByIdAsync(long id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_definition WHERE id_cycle = @id;";
        return await conn.QueryFirstOrDefaultAsync<CycleDefinition>(sql, new { id });
    }

    public async Task<CycleDefinition?> GetCycleByCodeAsync(string code)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_definition WHERE cycle_code = @code AND is_active = true;";
        return await conn.QueryFirstOrDefaultAsync<CycleDefinition>(sql, new { code });
    }

    public async Task<long> CreateCycleAsync(CycleDefinition cycle)
    {
        cycle.EntryPolicyJson = string.IsNullOrWhiteSpace(cycle.EntryPolicyJson) ? "{}" : cycle.EntryPolicyJson;
        cycle.ExitPolicyJson = string.IsNullOrWhiteSpace(cycle.ExitPolicyJson) ? "{}" : cycle.ExitPolicyJson;
        cycle.CreatedAt = cycle.CreatedAt == default ? DateTime.UtcNow : cycle.CreatedAt;

        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.cycle_definition (cycle_code, name, description, scope_type, is_active, current_version, entry_policy_json, exit_policy_json, created_by, created_at)
            VALUES (@CycleCode, @Name, @Description, @ScopeType, @IsActive, @CurrentVersion, CAST(@EntryPolicyJson AS jsonb), CAST(@ExitPolicyJson AS jsonb), @CreatedBy, @CreatedAt)
            RETURNING id_cycle;";
        return await conn.ExecuteScalarAsync<long>(sql, cycle);
    }

    public async Task<bool> UpdateCycleAsync(CycleDefinition cycle)
    {
        cycle.EntryPolicyJson = string.IsNullOrWhiteSpace(cycle.EntryPolicyJson) ? "{}" : cycle.EntryPolicyJson;
        cycle.ExitPolicyJson = string.IsNullOrWhiteSpace(cycle.ExitPolicyJson) ? "{}" : cycle.ExitPolicyJson;

        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.cycle_definition 
            SET name = @Name, description = @Description, scope_type = @ScopeType, 
                entry_policy_json = CAST(@EntryPolicyJson AS jsonb), exit_policy_json = CAST(@ExitPolicyJson AS jsonb), is_active = @IsActive
            WHERE id_cycle = @IdCycle;";
        var rows = await conn.ExecuteAsync(sql, cycle);
        return rows > 0;
    }

    public async Task<bool> SoftDeleteCycleAsync(long cycleId)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE nyx_flow.cycle_definition SET is_active = false WHERE id_cycle = @cycleId;";
        var rows = await conn.ExecuteAsync(sql, new { cycleId });
        return rows > 0;
    }

    // ==========================================
    // ETAPAS
    // ==========================================
    public async Task<IEnumerable<CycleStage>> GetStagesByCycleAsync(long cycleId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_stage WHERE id_cycle = @cycleId ORDER BY order_index ASC;";
        return await conn.QueryAsync<CycleStage>(sql, new { cycleId });
    }

    public async Task<CycleStage?> GetStageByIdAsync(long stageId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_stage WHERE id_stage = @stageId;";
        return await conn.QueryFirstOrDefaultAsync<CycleStage>(sql, new { stageId });
    }

    public async Task<long> CreateStageAsync(CycleStage stage)
    {
        stage.PoliciesJson = string.IsNullOrWhiteSpace(stage.PoliciesJson) ? "{}" : stage.PoliciesJson;

        using var conn = CreateConnection();
        if (stage.OrderIndex <= 0)
        {
            var maxOrder = await conn.ExecuteScalarAsync<short?>("SELECT MAX(order_index) FROM nyx_flow.cycle_stage WHERE id_cycle = @id_cycle;", new { id_cycle = stage.IdCycle });
            stage.OrderIndex = (short)((maxOrder ?? 0) + 1);
        }

        const string sql = @"
            INSERT INTO nyx_flow.cycle_stage (id_cycle, stage_code, name, description, order_index, is_terminal, sla_hours, policies_json)
            VALUES (@IdCycle, @StageCode, @Name, @Description, @OrderIndex, @IsTerminal, @SlaHours, CAST(@PoliciesJson AS jsonb))
            RETURNING id_stage;";
        return await conn.ExecuteScalarAsync<long>(sql, stage);
    }

    public async Task<bool> UpdateStageAsync(CycleStage stage)
    {
        stage.PoliciesJson = string.IsNullOrWhiteSpace(stage.PoliciesJson) ? "{}" : stage.PoliciesJson;

        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.cycle_stage 
            SET name = @Name, description = @Description, is_terminal = @IsTerminal, sla_hours = @SlaHours, policies_json = CAST(@PoliciesJson AS jsonb)
            WHERE id_stage = @IdStage;";
        var rows = await conn.ExecuteAsync(sql, stage);
        return rows > 0;
    }

    public async Task<bool> UpdateStageOrderAsync(long stageId, short orderIndex)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE nyx_flow.cycle_stage SET order_index = @orderIndex WHERE id_stage = @stageId;";
        var rows = await conn.ExecuteAsync(sql, new { stageId, orderIndex });
        return rows > 0;
    }

    public async Task<bool> DeleteStageAsync(long stageId)
    {
        using var conn = CreateConnection();
        // Desasociar checkpoints de esta etapa
        await conn.ExecuteAsync("UPDATE nyx_flow.checkpoint_catalog SET trigger_stage_id = NULL WHERE trigger_stage_id = @stageId;", new { stageId });
        const string sql = "DELETE FROM nyx_flow.cycle_stage WHERE id_stage = @stageId;";
        var rows = await conn.ExecuteAsync(sql, new { stageId });
        return rows > 0;
    }

    // ==========================================
    // CHECKPOINTS
    // ==========================================
    public async Task<IEnumerable<CheckpointCatalog>> GetCheckpointsByCycleAsync(long cycleId, bool includeInactive = false)
    {
        using var conn = CreateConnection();
        string sql = includeInactive
            ? "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_cycle = @cycleId ORDER BY execution_order ASC, id_checkpoint ASC;"
            : "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_cycle = @cycleId AND is_active = true ORDER BY execution_order ASC, id_checkpoint ASC;";
        return await conn.QueryAsync<CheckpointCatalog>(sql, new { cycleId });
    }

    public async Task<IEnumerable<CheckpointCatalogDetailDto>> GetFullCheckpointsByCycleAsync(long cycleId, bool includeInactive = false)
    {
        using var conn = CreateConnection();
        string cpSql = includeInactive
            ? "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_cycle = @cycleId ORDER BY execution_order ASC, id_checkpoint ASC;"
            : "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_cycle = @cycleId AND is_active = true ORDER BY execution_order ASC, id_checkpoint ASC;";
        var cps = (await conn.QueryAsync<CheckpointCatalogDetailDto>(cpSql, new { cycleId })).ToList();

        if (!cps.Any()) return cps;

        const string stepSql = "SELECT * FROM nyx_flow.checkpoint_step WHERE id_checkpoint = ANY(@cpIds) ORDER BY step_order ASC;";
        var cpIds = cps.Select(c => c.IdCheckpoint).ToArray();
        var steps = await conn.QueryAsync<CheckpointStep>(stepSql, new { cpIds });

        foreach (var cp in cps)
        {
            cp.Steps = steps.Where(s => s.IdCheckpoint == cp.IdCheckpoint).ToList();
        }

        return cps;
    }

    public async Task<CheckpointCatalog?> GetCheckpointByIdAsync(long id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_checkpoint = @id;";
        return await conn.QueryFirstOrDefaultAsync<CheckpointCatalog>(sql, new { id });
    }

    public async Task<CheckpointCatalogDetailDto?> GetFullCheckpointByIdAsync(long id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.checkpoint_catalog WHERE id_checkpoint = @id;";
        var cp = await conn.QueryFirstOrDefaultAsync<CheckpointCatalogDetailDto>(sql, new { id });
        if (cp == null) return null;

        const string stepSql = "SELECT * FROM nyx_flow.checkpoint_step WHERE id_checkpoint = @id ORDER BY step_order ASC;";
        var steps = await conn.QueryAsync<CheckpointStep>(stepSql, new { id });
        cp.Steps = steps.ToList();
        return cp;
    }

    public async Task<CheckpointCatalog?> GetCheckpointByCodeAsync(string code)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.checkpoint_catalog WHERE code = @code AND is_active = true;";
        return await conn.QueryFirstOrDefaultAsync<CheckpointCatalog>(sql, new { code });
    }

    public async Task<long> CreateCheckpointAsync(CheckpointCatalog cp)
    {
        cp.TemplateSchemaJson = string.IsNullOrWhiteSpace(cp.TemplateSchemaJson) ? "{}" : cp.TemplateSchemaJson;
        cp.PoliciesJson = string.IsNullOrWhiteSpace(cp.PoliciesJson) ? "{}" : cp.PoliciesJson;
        cp.ProvidersJson = string.IsNullOrWhiteSpace(cp.ProvidersJson) ? "[]" : cp.ProvidersJson;
        cp.AllowedActionsJson = string.IsNullOrWhiteSpace(cp.AllowedActionsJson) ? "[]" : cp.AllowedActionsJson;
        cp.BranchingRulesJson = string.IsNullOrWhiteSpace(cp.BranchingRulesJson) ? "{}" : cp.BranchingRulesJson;

        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.checkpoint_catalog (
                id_cycle, trigger_stage_id, code, name, description, origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko,
                rollback_to_stage, is_recurrent, recurrence_days, activation_trigger, delay_days, 
                precondition_fact, template_schema_json, policies_json, providers_json, allowed_actions_json, branching_rules_json,
                category, owner_dept, execution_order, is_active, version, created_at
            ) VALUES (
                @IdCycle, @TriggerStageId, @Code, @Name, @Description, @Origin, @Scope, @BlocksAdvance, @FinalizesCycle, @TriggeredByKo,
                @RollbackToStage, @IsRecurrent, @RecurrenceDays, @ActivationTrigger, @DelayDays,
                @PreconditionFact, CAST(@TemplateSchemaJson AS jsonb), CAST(@PoliciesJson AS jsonb), CAST(@ProvidersJson AS jsonb),
                CAST(@AllowedActionsJson AS jsonb), CAST(@BranchingRulesJson AS jsonb),
                @Category, @OwnerDept, @ExecutionOrder, @IsActive, @Version, @CreatedAt
            ) RETURNING id_checkpoint;";
        return await conn.ExecuteScalarAsync<long>(sql, cp);
    }

    public async Task<bool> UpdateCheckpointAsync(CheckpointCatalog cp)
    {
        cp.TemplateSchemaJson = string.IsNullOrWhiteSpace(cp.TemplateSchemaJson) ? "{}" : cp.TemplateSchemaJson;
        cp.PoliciesJson = string.IsNullOrWhiteSpace(cp.PoliciesJson) ? "{}" : cp.PoliciesJson;
        cp.ProvidersJson = string.IsNullOrWhiteSpace(cp.ProvidersJson) ? "[]" : cp.ProvidersJson;
        cp.AllowedActionsJson = string.IsNullOrWhiteSpace(cp.AllowedActionsJson) ? "[]" : cp.AllowedActionsJson;
        cp.BranchingRulesJson = string.IsNullOrWhiteSpace(cp.BranchingRulesJson) ? "{}" : cp.BranchingRulesJson;

        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.checkpoint_catalog SET 
                name = @Name, description = @Description, trigger_stage_id = @TriggerStageId, blocks_advance = @BlocksAdvance,
                finalizes_cycle = @FinalizesCycle, triggered_by_ko = @TriggeredByKo,
                rollback_to_stage = @RollbackToStage, is_recurrent = @IsRecurrent, recurrence_days = @RecurrenceDays,
                activation_trigger = @ActivationTrigger, delay_days = @DelayDays, precondition_fact = @PreconditionFact,
                template_schema_json = CAST(@TemplateSchemaJson AS jsonb), policies_json = CAST(@PoliciesJson AS jsonb),
                providers_json = CAST(@ProvidersJson AS jsonb), allowed_actions_json = CAST(@AllowedActionsJson AS jsonb),
                branching_rules_json = CAST(@BranchingRulesJson AS jsonb),
                category = @Category, owner_dept = @OwnerDept, execution_order = @ExecutionOrder, is_active = @IsActive
            WHERE id_checkpoint = @IdCheckpoint;";
        var rows = await conn.ExecuteAsync(sql, cp);
        return rows > 0;
    }

    public async Task<bool> SoftDeleteCheckpointAsync(long cpId)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE nyx_flow.checkpoint_catalog SET is_active = false WHERE id_checkpoint = @cpId;";
        var rows = await conn.ExecuteAsync(sql, new { cpId });
        return rows > 0;
    }

    public async Task<bool> ToggleCheckpointActiveAsync(long cpId)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE nyx_flow.checkpoint_catalog SET is_active = NOT is_active WHERE id_checkpoint = @cpId RETURNING is_active;";
        return await conn.ExecuteScalarAsync<bool>(sql, new { cpId });
    }

    public async Task BulkUpsertCheckpointsAsync(long cycleId, IEnumerable<CheckpointCatalog> checkpoints)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();

        const string sql = @"
            INSERT INTO nyx_flow.checkpoint_catalog (
                id_checkpoint, id_cycle, trigger_stage_id, code, name, description, origin, scope,
                blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
                activation_trigger, delay_days, precondition_fact, template_schema_json,
                policies_json, providers_json, category, owner_dept, execution_order, is_active, version, created_at
            ) VALUES (
                @IdCheckpoint, @IdCycle, @TriggerStageId, @Code, @Name, @Description, @Origin, @Scope,
                @BlocksAdvance, @FinalizesCycle, @TriggeredByKo, @RollbackToStage,
                @ActivationTrigger, @DelayDays, @PreconditionFact, CAST(COALESCE(@TemplateSchemaJson, '{}') AS jsonb),
                CAST(COALESCE(@PoliciesJson, '{}') AS jsonb), CAST(COALESCE(@ProvidersJson, '[]') AS jsonb), @Category, @OwnerDept, @ExecutionOrder, @IsActive, @Version, @CreatedAt
            ) ON CONFLICT (code) DO UPDATE SET
                id_cycle = EXCLUDED.id_cycle,
                trigger_stage_id = EXCLUDED.trigger_stage_id,
                name = EXCLUDED.name,
                description = EXCLUDED.description,
                origin = EXCLUDED.origin,
                scope = EXCLUDED.scope,
                blocks_advance = EXCLUDED.blocks_advance,
                finalizes_cycle = EXCLUDED.finalizes_cycle,
                triggered_by_ko = EXCLUDED.triggered_by_ko,
                rollback_to_stage = EXCLUDED.rollback_to_stage,
                providers_json = EXCLUDED.providers_json,
                policies_json = EXCLUDED.policies_json,
                owner_dept = EXCLUDED.owner_dept,
                execution_order = EXCLUDED.execution_order,
                is_active = EXCLUDED.is_active;";

        foreach (var cp in checkpoints)
        {
            cp.IdCycle = cycleId;
            await conn.ExecuteAsync(sql, cp, tx);
        }

        await tx.CommitAsync();
    }

    public async Task<IEnumerable<CheckpointStep>> GetCheckpointStepsAsync(long checkpointId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.checkpoint_step WHERE id_checkpoint = @checkpointId ORDER BY step_order ASC;";
        return await conn.QueryAsync<CheckpointStep>(sql, new { checkpointId });
    }

    public async Task SaveCheckpointStepsAsync(long checkpointId, IEnumerable<CheckpointStep> steps)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();

        await conn.ExecuteAsync("DELETE FROM nyx_flow.checkpoint_step WHERE id_checkpoint = @checkpointId;", new { checkpointId }, tx);
        
        const string insertSql = @"
            INSERT INTO nyx_flow.checkpoint_step (id_checkpoint, step_order, name, is_required)
            VALUES (@IdCheckpoint, @StepOrder, @Name, @IsRequired);";

        foreach (var s in steps)
        {
            s.IdCheckpoint = checkpointId;
            await conn.ExecuteAsync(insertSql, s, tx);
        }

        await tx.CommitAsync();
    }

    public async Task<bool> UpdateCheckpointCanvasSchemaAsync(long checkpointId, string canvasSchemaJson)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE nyx_flow.checkpoint_catalog SET template_schema_json = CAST(@canvasSchemaJson AS jsonb) WHERE id_checkpoint = @checkpointId;";
        var rows = await conn.ExecuteAsync(sql, new { checkpointId, canvasSchemaJson });
        return rows > 0;
    }

    // ==========================================
    // METADATOS Y CONCILIACIÓN (ROLES Y CARTERAS)
    // ==========================================
    public async Task<IEnumerable<MetaRole>> GetMetaRolesAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.meta_role WHERE is_active = true ORDER BY name ASC;";
        return await conn.QueryAsync<MetaRole>(sql);
    }

    public async Task<long> CreateMetaRoleAsync(MetaRole role)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.meta_role (role_code, name, description, external_system_code, is_active, created_at)
            VALUES (@RoleCode, @Name, @Description, @ExternalSystemCode, @IsActive, @CreatedAt)
            ON CONFLICT (role_code) DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description, external_system_code = EXCLUDED.external_system_code, is_active = EXCLUDED.is_active
            RETURNING id_role;";
        return await conn.ExecuteScalarAsync<long>(sql, role);
    }

    public async Task<IEnumerable<MetaPortfolio>> GetMetaPortfoliosAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.meta_portfolio WHERE is_active = true ORDER BY name ASC;";
        return await conn.QueryAsync<MetaPortfolio>(sql);
    }

    public async Task<long> CreateMetaPortfolioAsync(MetaPortfolio portfolio)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.meta_portfolio (portfolio_code, name, description, external_system_code, is_active, created_at)
            VALUES (@PortfolioCode, @Name, @Description, @ExternalSystemCode, @IsActive, @CreatedAt)
            ON CONFLICT (portfolio_code) DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description, external_system_code = EXCLUDED.external_system_code, is_active = EXCLUDED.is_active
            RETURNING id_portfolio;";
        return await conn.ExecuteScalarAsync<long>(sql, portfolio);
    }

    public async Task<IEnumerable<MetaCampaign>> GetMetaCampaignsAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.meta_campaign WHERE is_active = true ORDER BY name ASC;";
        return await conn.QueryAsync<MetaCampaign>(sql);
    }

    public async Task<long> CreateMetaCampaignAsync(MetaCampaign campaign)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.meta_campaign (campaign_code, name, description, external_system_code, is_active, created_at)
            VALUES (@CampaignCode, @Name, @Description, @ExternalSystemCode, @IsActive, @CreatedAt)
            ON CONFLICT (campaign_code) DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description, external_system_code = EXCLUDED.external_system_code, is_active = EXCLUDED.is_active
            RETURNING id_campaign;";
        return await conn.ExecuteScalarAsync<long>(sql, campaign);
    }

    // ==========================================
    // POLÍTICAS
    // ==========================================
    public async Task<IEnumerable<CyclePolicyRule>> GetPoliciesAsync(long? cycleId)
    {
        using var conn = CreateConnection();
        string sql = "SELECT * FROM nyx_flow.cycle_policy_rule WHERE is_active = true ";
        if (cycleId.HasValue) sql += " AND (id_cycle = @cycleId OR id_cycle IS NULL) ";
        sql += " ORDER BY id_rule ASC;";
        return await conn.QueryAsync<CyclePolicyRule>(sql, new { cycleId });
    }

    public async Task<CyclePolicyRule?> GetPolicyByCodeAsync(string code)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_policy_rule WHERE rule_code = @code AND is_active = true;";
        return await conn.QueryFirstOrDefaultAsync<CyclePolicyRule>(sql, new { code });
    }

    public async Task<long> SavePolicyRuleAsync(CyclePolicyRule rule)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.cycle_policy_rule (rule_code, id_cycle, name, description, entity_type, action_trigger, rule_definition_json, is_active, created_at)
            VALUES (@RuleCode, @IdCycle, @Name, @Description, @EntityType, @ActionTrigger, CAST(@RuleDefinitionJson AS jsonb), @IsActive, @CreatedAt)
            ON CONFLICT (rule_code) DO UPDATE SET 
                name = EXCLUDED.name, description = EXCLUDED.description, rule_definition_json = EXCLUDED.rule_definition_json, is_active = EXCLUDED.is_active
            RETURNING id_rule;";
        return await conn.ExecuteScalarAsync<long>(sql, rule);
    }

    // ==========================================
    // INSTANCIAS DE CICLO
    // ==========================================
    public async Task<long> CreateInstanceAsync(CycleInstance instance)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.cycle_instance (
                id_cycle, entity_type, entity_id, current_stage_id, owner_actor_id, current_actor_id, 
                handshake_status, handshake_target_actor_id, handshake_requested_at, day_counter, metadata, facts, status, created_at
            ) VALUES (
                @IdCycle, @EntityType, @EntityId, @CurrentStageId, @OwnerActorId, @CurrentActorId,
                @HandshakeStatus, @HandshakeTargetActorId, @HandshakeRequestedAt, @DayCounter, CAST(@Metadata AS jsonb), CAST(@Facts AS jsonb), @Status, @CreatedAt
            ) RETURNING id_instance;";
        return await conn.ExecuteScalarAsync<long>(sql, instance);
    }

    public async Task<CycleInstance?> GetInstanceByIdAsync(long instanceId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_instance WHERE id_instance = @instanceId;";
        return await conn.QueryFirstOrDefaultAsync<CycleInstance>(sql, new { instanceId });
    }

    public async Task<CycleInstance?> GetInstanceByEntityAsync(string entityType, long entityId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_instance WHERE entity_type = @entityType AND entity_id = @entityId ORDER BY id_instance DESC LIMIT 1;";
        return await conn.QueryFirstOrDefaultAsync<CycleInstance>(sql, new { entityType, entityId });
    }

    public async Task<bool> UpdateInstanceAsync(CycleInstance instance)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.cycle_instance SET 
                current_stage_id = @CurrentStageId, owner_actor_id = @OwnerActorId, current_actor_id = @CurrentActorId,
                handshake_status = @HandshakeStatus, handshake_target_actor_id = @HandshakeTargetActorId, handshake_requested_at = @HandshakeRequestedAt,
                day_counter = @DayCounter, metadata = CAST(@Metadata AS jsonb), facts = CAST(@Facts AS jsonb), status = @Status, completed_at = @CompletedAt
            WHERE id_instance = @IdInstance;";
        var rows = await conn.ExecuteAsync(sql, instance);
        return rows > 0;
    }

    public async Task<IEnumerable<CheckpointInstanceDetailDto>> GetCheckpointInstancesForInstanceAsync(long instanceId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT 
                ci.id_cp_instance AS IdCpInstance,
                ci.id_instance AS IdInstance,
                ci.id_checkpoint AS IdCheckpoint,
                c.code AS Code,
                c.name AS Name,
                c.description AS Description,
                ci.status AS Status,
                c.blocks_advance AS BlocksAdvance,
                c.finalizes_cycle AS FinalizesCycle,
                c.triggered_by_ko AS TriggeredByKo,
                c.owner_dept AS OwnerDept,
                c.providers_json AS ProvidersJson,
                c.policies_json AS PoliciesJson,
                ci.opened_at_stage AS OpenedAtStage,
                s.name AS OpenedAtStageName,
                ci.scheduled_for AS ScheduledFor,
                ci.resolved_by AS ResolvedBy,
                ci.resolved_at AS ResolvedAt,
                c.template_schema_json AS TemplateSchemaJson,
                ci.answers_json AS AnswersJson
            FROM nyx_flow.checkpoint_instance ci
            JOIN nyx_flow.checkpoint_catalog c ON ci.id_checkpoint = c.id_checkpoint
            LEFT JOIN nyx_flow.cycle_stage s ON ci.opened_at_stage = s.id_stage
            WHERE ci.id_instance = @instanceId
            ORDER BY c.execution_order ASC, ci.id_cp_instance ASC;";
        return await conn.QueryAsync<CheckpointInstanceDetailDto>(sql, new { instanceId });
    }

    public async Task<long> CreateCheckpointInstanceAsync(CheckpointInstance cpInst)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.checkpoint_instance (
                id_instance, id_checkpoint, status, opened_at_stage, scheduled_for, resolved_by, resolved_at, answers_json, created_at
            ) VALUES (
                @IdInstance, @IdCheckpoint, @Status, @OpenedAtStage, @ScheduledFor, @ResolvedBy, @ResolvedAt, CAST(@AnswersJson AS jsonb), @CreatedAt
            ) RETURNING id_cp_instance;";
        return await conn.ExecuteScalarAsync<long>(sql, cpInst);
    }

    public async Task<CheckpointInstance?> GetCheckpointInstanceByIdAsync(long cpInstanceId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.checkpoint_instance WHERE id_cp_instance = @cpInstanceId;";
        return await conn.QueryFirstOrDefaultAsync<CheckpointInstance>(sql, new { cpInstanceId });
    }

    public async Task<bool> UpdateCheckpointInstanceAsync(CheckpointInstance cpInst)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.checkpoint_instance SET 
                status = @Status, scheduled_for = @ScheduledFor, resolved_by = @ResolvedBy, resolved_at = @ResolvedAt, answers_json = CAST(@AnswersJson AS jsonb)
            WHERE id_cp_instance = @IdCpInstance;";
        var rows = await conn.ExecuteAsync(sql, cpInst);
        return rows > 0;
    }

    public async Task<long> CreateTransitionAsync(StageTransition transition)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO nyx_flow.stage_transition (id_instance, from_stage_id, to_stage_id, direction, triggered_by, actor_id, transitioned_at)
            VALUES (@IdInstance, @FromStageId, @ToStageId, @Direction, @TriggeredBy, @ActorId, @TransitionedAt)
            RETURNING id_transition;";
        return await conn.ExecuteScalarAsync<long>(sql, transition);
    }

    public async Task<IEnumerable<StageTransition>> GetTransitionsForInstanceAsync(long instanceId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.stage_transition WHERE id_instance = @instanceId ORDER BY id_transition DESC;";
        return await conn.QueryAsync<StageTransition>(sql, new { instanceId });
    }

    // ==========================================
    // ACTIVACIÓN DE PROGRAMADOS Y SIMULACIÓN TEMPORAL
    // ==========================================
    public async Task ActivateDueScheduledCheckpointsAsync()
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE nyx_flow.checkpoint_instance 
            SET status = 'PENDING' 
            WHERE status = 'SCHEDULED' AND scheduled_for <= NOW();";
        await conn.ExecuteAsync(sql);
    }

    public async Task<int> FastForwardTimeAsync(long instanceId, int days)
    {
        using var conn = CreateConnection();
        string sql = @"
            UPDATE nyx_flow.checkpoint_instance 
            SET scheduled_for = scheduled_for - (@days || ' days')::interval
            WHERE id_instance = @instanceId AND status = 'SCHEDULED';";
        var updated = await conn.ExecuteAsync(sql, new { instanceId, days });
        await ActivateDueScheduledCheckpointsAsync();
        return updated;
    }

    public async Task<long> LogAuditAsync(long actorId, string action, long? instanceId, long? checkpointId, string detail)
    {
        using var conn = CreateConnection();
        string rawToHash = $"{actorId}|{action}|{instanceId}|{checkpointId}|{detail}|{DateTime.UtcNow.Ticks}";
        string checksum;
        using (var sha = SHA512.Create())
        {
            checksum = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawToHash)));
        }

        const string sql = @"
            INSERT INTO nyx_flow.cycle_audit_log (id_instance, id_checkpoint, action, actor_id, detail, checksum, created_at)
            VALUES (@instanceId, @checkpointId, @action, @actorId, CAST(@detail AS jsonb), @checksum, NOW())
            RETURNING id_log;";
        return await conn.ExecuteScalarAsync<long>(sql, new { instanceId, checkpointId, action, actorId, detail, checksum });
    }

    public async Task<IEnumerable<CycleAuditLog>> GetAuditLogsAsync(int limit = 50)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM nyx_flow.cycle_audit_log ORDER BY id_log DESC LIMIT @limit;";
        return await conn.QueryAsync<CycleAuditLog>(sql, new { limit });
    }
}
