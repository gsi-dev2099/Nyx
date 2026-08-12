using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;
using Nyx.ApprovalEngine.Domain.Entities;

namespace Nyx.ApprovalEngine.Infrastructure;

public interface IApprovalRepository
{
    Task<IEnumerable<ApprovalPolicy>> GetPoliciesAsync();
    Task<ApprovalPolicy?> GetPolicyByCodeAsync(string code);
    Task<long> CreatePolicyAsync(ApprovalPolicy policy);
    
    Task<long> CreateChainAsync(ApprovalChain chain);
    Task CreateChainStepAsync(ApprovalChainStep step);
    Task<IEnumerable<ApprovalChainStep>> GetChainStepsAsync(long policyId);
    
    Task<long> CreateRequestAsync(ApprovalRequest request);
    Task<ApprovalRequest?> GetRequestByIdAsync(long requestId);
    Task<IEnumerable<ApprovalRequest>> GetPendingRequestsForApproverAsync(long approverId, string approverRole);
    Task UpdateRequestStatusAsync(long requestId, string status, short currentStep);
    
    Task<long> RecordDecisionAsync(ApprovalDecision decision);
    Task<IEnumerable<ApprovalDecision>> GetDecisionsForRequestAsync(long requestId);
    
    Task<long> CreateDelegationAsync(ApprovalDelegation delegation);
    Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsAsync(long delegateId);
    
    Task LogAuditAsync(long actorId, string action, long? requestId, long? policyId, string detailJson, string? actorIp = null);
}

public class ApprovalRepository : IApprovalRepository
{
    private readonly string _connectionString;

    public ApprovalRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection configuration is missing.");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<ApprovalPolicy>> GetPoliciesAsync()
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_policy AS IdPolicy, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, is_active AS IsActive, current_version AS CurrentVersion, created_by AS CreatedBy, created_at AS CreatedAt FROM policy WHERE is_active = true ORDER BY id_policy;";
        return await db.QueryAsync<ApprovalPolicy>(sql);
    }

    public async Task<ApprovalPolicy?> GetPolicyByCodeAsync(string code)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_policy AS IdPolicy, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, is_active AS IsActive, current_version AS CurrentVersion, created_by AS CreatedBy, created_at AS CreatedAt FROM policy WHERE code = @code AND is_active = true;";
        return await db.QueryFirstOrDefaultAsync<ApprovalPolicy>(sql, new { code });
    }

    public async Task<long> CreatePolicyAsync(ApprovalPolicy policy)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO policy (code, name, description, scope_type, scope_id, is_active, current_version, created_by)
            VALUES (@Code, @Name, @Description, @ScopeType, @ScopeId, true, 1, @CreatedBy)
            RETURNING id_policy;";
        return await db.ExecuteScalarAsync<long>(sql, policy);
    }

    public async Task<long> CreateChainAsync(ApprovalChain chain)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO chain (id_policy, chain_mode, max_sla_hours, on_timeout_action)
            VALUES (@IdPolicy, @ChainMode, @MaxSlaHours, @OnTimeoutAction)
            RETURNING id_chain;";
        return await db.ExecuteScalarAsync<long>(sql, chain);
    }

    public async Task CreateChainStepAsync(ApprovalChainStep step)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO chain_step (id_chain, step_order, approver_type, approver_ref, condition_expr, can_delegate, sla_hours, is_optional)
            VALUES (@IdChain, @StepOrder, @ApproverType, @ApproverRef, @ConditionExpr::jsonb, @CanDelegate, @SlaHours, @IsOptional);";
        await db.ExecuteAsync(sql, step);
    }

    public async Task<IEnumerable<ApprovalChainStep>> GetChainStepsAsync(long policyId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT cs.id_step AS IdStep, cs.id_chain AS IdChain, cs.step_order AS StepOrder, cs.approver_type AS ApproverType, cs.approver_ref AS ApproverRef, cs.condition_expr AS ConditionExpr, cs.can_delegate AS CanDelegate, cs.sla_hours AS SlaHours, cs.is_optional AS IsOptional
            FROM chain_step cs
            JOIN chain c ON c.id_chain = cs.id_chain
            WHERE c.id_policy = @policyId
            ORDER BY cs.step_order;";
        return await db.QueryAsync<ApprovalChainStep>(sql, new { policyId });
    }

    public async Task<long> CreateRequestAsync(ApprovalRequest req)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO request (id_policy, policy_version, entity_type, entity_id, entity_context, status, current_step, requested_by, callback_url, expires_at)
            VALUES (@IdPolicy, @PolicyVersion, @EntityType, @EntityId, @EntityContext::jsonb, 'PENDING', 1, @RequestedBy, @CallbackUrl, @ExpiresAt)
            RETURNING id_request;";
        return await db.ExecuteScalarAsync<long>(sql, req);
    }

    public async Task<ApprovalRequest?> GetRequestByIdAsync(long requestId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_request AS IdRequest, id_policy AS IdPolicy, policy_version AS PolicyVersion, entity_type AS EntityType, entity_id AS EntityId, entity_context AS EntityContext, status, current_step AS CurrentStep, requested_by AS RequestedBy, callback_url AS CallbackUrl, expires_at AS ExpiresAt, created_at AS CreatedAt, resolved_at AS ResolvedAt
            FROM request WHERE id_request = @requestId;";
        return await db.QueryFirstOrDefaultAsync<ApprovalRequest>(sql, new { requestId });
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingRequestsForApproverAsync(long approverId, string approverRole)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT r.id_request AS IdRequest, r.id_policy AS IdPolicy, r.policy_version AS PolicyVersion, r.entity_type AS EntityType, r.entity_id AS EntityId, r.entity_context AS EntityContext, r.status, r.current_step AS CurrentStep, r.requested_by AS RequestedBy, r.callback_url AS CallbackUrl, r.expires_at AS ExpiresAt, r.created_at AS CreatedAt
            FROM request r
            JOIN chain c ON c.id_policy = r.id_policy
            JOIN chain_step cs ON cs.id_chain = c.id_chain AND cs.step_order = r.current_step
            WHERE r.status IN ('PENDING', 'IN_PROGRESS')
              AND (
                (cs.approver_type = 'USER' AND cs.approver_ref = @approverIdStr) OR
                (cs.approver_type = 'ROLE' AND cs.approver_ref = @approverRole)
              )
            ORDER BY r.created_at DESC;";
        return await db.QueryAsync<ApprovalRequest>(sql, new { approverIdStr = approverId.ToString(), approverRole });
    }

    public async Task UpdateRequestStatusAsync(long requestId, string status, short currentStep)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE request
            SET status = @status, current_step = @currentStep, resolved_at = CASE WHEN @status IN ('APPROVED','REJECTED') THEN CURRENT_TIMESTAMP ELSE resolved_at END
            WHERE id_request = @requestId;";
        await db.ExecuteAsync(sql, new { requestId, status, currentStep });
    }

    public async Task<long> RecordDecisionAsync(ApprovalDecision d)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO decision (id_request, step_order, decided_by, original_approver, decision, reason, evidence_path)
            VALUES (@IdRequest, @StepOrder, @DecidedBy, @OriginalApprover, @DecisionType, @Reason, @EvidencePath)
            RETURNING id_decision;";
        return await db.ExecuteScalarAsync<long>(sql, d);
    }

    public async Task<IEnumerable<ApprovalDecision>> GetDecisionsForRequestAsync(long requestId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_decision AS IdDecision, id_request AS IdRequest, step_order AS StepOrder, decided_by AS DecidedBy, original_approver AS OriginalApprover, decision AS DecisionType, reason, evidence_path AS EvidencePath, decided_at AS DecidedAt
            FROM decision WHERE id_request = @requestId ORDER BY step_order;";
        return await db.QueryAsync<ApprovalDecision>(sql, new { requestId });
    }

    public async Task<long> CreateDelegationAsync(ApprovalDelegation d)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO delegation (delegator_id, delegate_id, id_policy, reason, valid_from, valid_until, is_active)
            VALUES (@DelegatorId, @DelegateId, @IdPolicy, @Reason, @ValidFrom, @ValidUntil, true)
            RETURNING id_delegation;";
        return await db.ExecuteScalarAsync<long>(sql, d);
    }

    public async Task<IEnumerable<ApprovalDelegation>> GetActiveDelegationsAsync(long delegateId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_delegation AS IdDelegation, delegator_id AS DelegatorId, delegate_id AS DelegateId, id_policy AS IdPolicy, reason, valid_from AS ValidFrom, valid_until AS ValidUntil, is_active AS IsActive
            FROM delegation WHERE delegate_id = @delegateId AND is_active = true AND CURRENT_TIMESTAMP BETWEEN valid_from AND valid_until;";
        return await db.QueryAsync<ApprovalDelegation>(sql, new { delegateId });
    }

    public async Task LogAuditAsync(long actorId, string action, long? requestId, long? policyId, string detailJson, string? actorIp = null)
    {
        using var db = CreateConnection();
        var timestamp = DateTime.UtcNow.ToString("o");
        var rawData = $"{actorId}|{action}|{requestId}|{policyId}|{detailJson}|{timestamp}";
        using var sha = SHA512.Create();
        var checksum = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawData)));

        const string sql = @"
            INSERT INTO audit_log (id_request, id_policy, action, actor_id, actor_ip, detail, checksum)
            VALUES (@requestId, @policyId, @action, @actorId, @actorIp::inet, @detailJson::jsonb, @checksum);";
        await db.ExecuteAsync(sql, new { requestId, policyId, action, actorId, actorIp = actorIp ?? "127.0.0.1", detailJson, checksum });
    }
}
