using Nyx.ApprovalEngine.Domain.Entities;
using Nyx.ApprovalEngine.Infrastructure;

namespace Nyx.ApprovalEngine.Application;

public interface IApprovalService
{
    Task<IEnumerable<ApprovalPolicy>> GetPoliciesAsync();
    Task<ApprovalPolicy> CreatePolicyWithChainAsync(ApprovalPolicy policy, ApprovalChain chain, IEnumerable<ApprovalChainStep> steps);
    Task<ApprovalRequest> SubmitRequestAsync(string policyCode, string entityType, long entityId, long requestedBy, string entityContextJson, string? callbackUrl);
    Task<ApprovalRequest> DecideRequestAsync(long requestId, long decidedBy, string decision, string? reason, string? evidencePath);
    Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsAsync(long approverId, string approverRole);
    Task<ApprovalRequest?> GetRequestAsync(long requestId);
    Task<ApprovalDelegation> CreateDelegationAsync(long delegatorId, long delegateId, long? policyId, string reason, DateTime validFrom, DateTime validUntil);
}

public class ApprovalService : IApprovalService
{
    private readonly IApprovalRepository _repo;
    private readonly ILogger<ApprovalService> _logger;

    public ApprovalService(IApprovalRepository repo, ILogger<ApprovalService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<ApprovalPolicy>> GetPoliciesAsync() => await _repo.GetPoliciesAsync();

    public async Task<ApprovalPolicy> CreatePolicyWithChainAsync(ApprovalPolicy policy, ApprovalChain chain, IEnumerable<ApprovalChainStep> steps)
    {
        var policyId = await _repo.CreatePolicyAsync(policy);
        policy.IdPolicy = policyId;

        chain.IdPolicy = policyId;
        var chainId = await _repo.CreateChainAsync(chain);

        foreach (var step in steps)
        {
            step.IdChain = chainId;
            await _repo.CreateChainStepAsync(step);
        }

        await _repo.LogAuditAsync(policy.CreatedBy, "POLICY_CREATED", null, policyId, $"{{\"code\":\"{policy.Code}\",\"steps\":{steps.Count()}}}");
        return policy;
    }

    public async Task<ApprovalRequest> SubmitRequestAsync(string policyCode, string entityType, long entityId, long requestedBy, string entityContextJson, string? callbackUrl)
    {
        var policy = await _repo.GetPolicyByCodeAsync(policyCode) 
            ?? throw new KeyNotFoundException($"Approval policy '{policyCode}' not found.");

        var request = new ApprovalRequest
        {
            IdPolicy = policy.IdPolicy,
            PolicyVersion = policy.CurrentVersion,
            EntityType = entityType.ToLowerInvariant(),
            EntityId = entityId,
            EntityContext = entityContextJson,
            Status = "PENDING",
            CurrentStep = 1,
            RequestedBy = requestedBy,
            CallbackUrl = callbackUrl,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var requestId = await _repo.CreateRequestAsync(request);
        request.IdRequest = requestId;

        await _repo.LogAuditAsync(requestedBy, "REQUEST_SUBMITTED", requestId, policy.IdPolicy, $"{{\"entityType\":\"{entityType}\",\"entityId\":{entityId}}}");
        _logger.LogInformation("Submitted approval request #{RequestId} for Policy {PolicyCode}", requestId, policyCode);

        return request;
    }

    public async Task<ApprovalRequest> DecideRequestAsync(long requestId, long decidedBy, string decision, string? reason, string? evidencePath)
    {
        var req = await _repo.GetRequestByIdAsync(requestId) 
            ?? throw new KeyNotFoundException($"Approval request #{requestId} not found.");

        if (req.Status != "PENDING" && req.Status != "IN_PROGRESS")
        {
            throw new InvalidOperationException($"Request #{requestId} is already in state '{req.Status}'.");
        }

        // ISO 27001 / SOX: Requester cannot approve their own request!
        if (req.RequestedBy == decidedBy)
        {
            throw new InvalidOperationException("Segregation of duties rule: Requester cannot decide their own approval request.");
        }

        var steps = (await _repo.GetChainStepsAsync(req.IdPolicy)).ToList();
        var currentStepObj = steps.FirstOrDefault(s => s.StepOrder == req.CurrentStep);

        var decisionObj = new ApprovalDecision
        {
            IdRequest = requestId,
            StepOrder = req.CurrentStep,
            DecidedBy = decidedBy,
            OriginalApprover = currentStepObj?.ApproverType == "USER" ? long.Parse(currentStepObj.ApproverRef) : null,
            DecisionType = decision.ToUpperInvariant(),
            Reason = reason,
            EvidencePath = evidencePath
        };

        await _repo.RecordDecisionAsync(decisionObj);

        if (decision.Equals("REJECTED", StringComparison.OrdinalIgnoreCase))
        {
            req.Status = "REJECTED";
            await _repo.UpdateRequestStatusAsync(requestId, "REJECTED", req.CurrentStep);
        }
        else if (decision.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            if (req.CurrentStep >= steps.Count)
            {
                req.Status = "APPROVED";
                await _repo.UpdateRequestStatusAsync(requestId, "APPROVED", req.CurrentStep);
            }
            else
            {
                req.CurrentStep++;
                req.Status = "IN_PROGRESS";
                await _repo.UpdateRequestStatusAsync(requestId, "IN_PROGRESS", req.CurrentStep);
            }
        }

        await _repo.LogAuditAsync(decidedBy, $"REQUEST_{decision.ToUpperInvariant()}", requestId, req.IdPolicy, $"{{\"step\":{req.CurrentStep},\"reason\":\"{reason}\"}}");
        return req;
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsAsync(long approverId, string approverRole)
    {
        return await _repo.GetPendingRequestsForApproverAsync(approverId, approverRole);
    }

    public async Task<ApprovalRequest?> GetRequestAsync(long requestId) => await _repo.GetRequestByIdAsync(requestId);

    public async Task<ApprovalDelegation> CreateDelegationAsync(long delegatorId, long delegateId, long? policyId, string reason, DateTime validFrom, DateTime validUntil)
    {
        var d = new ApprovalDelegation
        {
            DelegatorId = delegatorId,
            DelegateId = delegateId,
            IdPolicy = policyId,
            Reason = reason,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            IsActive = true
        };

        var id = await _repo.CreateDelegationAsync(d);
        d.IdDelegation = id;

        await _repo.LogAuditAsync(delegatorId, "DELEGATION_CREATED", null, policyId, $"{{\"delegateId\":{delegateId},\"until\":\"{validUntil:o}\"}}");
        return d;
    }
}
