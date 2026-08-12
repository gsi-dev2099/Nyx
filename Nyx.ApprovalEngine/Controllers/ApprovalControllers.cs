using Microsoft.AspNetCore.Mvc;
using Nyx.ApprovalEngine.Application;
using Nyx.ApprovalEngine.Domain.Entities;

namespace Nyx.ApprovalEngine.Controllers;

[ApiController]
[Route("api/approval")]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _service;

    public ApprovalController(IApprovalService service)
    {
        _service = service;
    }

    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _service.GetPoliciesAsync();
        return Ok(policies);
    }

    [HttpPost("policies")]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyRequest req)
    {
        var policy = new ApprovalPolicy
        {
            Code = req.Code,
            Name = req.Name,
            Description = req.Description,
            ScopeType = req.ScopeType,
            ScopeId = req.ScopeId,
            CreatedBy = req.CreatedBy ?? 1
        };

        var chain = new ApprovalChain
        {
            ChainMode = req.ChainMode,
            MaxSlaHours = req.MaxSlaHours,
            OnTimeoutAction = req.OnTimeoutAction
        };

        var created = await _service.CreatePolicyWithChainAsync(policy, chain, req.Steps);
        return Ok(created);
    }

    [HttpPost("requests/submit")]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitApprovalRequest req)
    {
        var result = await _service.SubmitRequestAsync(
            req.PolicyCode,
            req.EntityType,
            req.EntityId,
            req.RequestedBy,
            req.EntityContextJson ?? "{}",
            req.CallbackUrl
        );
        return Ok(result);
    }

    [HttpPost("requests/{id:long}/decide")]
    public async Task<IActionResult> DecideRequest(long id, [FromBody] DecideApprovalRequest req)
    {
        try
        {
            var result = await _service.DecideRequestAsync(id, req.DecidedBy, req.Decision, req.Reason, req.EvidencePath);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPending([FromQuery] long approverId, [FromQuery] string approverRole)
    {
        var pending = await _service.GetPendingApprovalsAsync(approverId, approverRole);
        return Ok(pending);
    }

    [HttpGet("requests/{id:long}")]
    public async Task<IActionResult> GetRequest(long id)
    {
        var req = await _service.GetRequestAsync(id);
        if (req == null) return NotFound();
        return Ok(req);
    }

    [HttpPost("delegations")]
    public async Task<IActionResult> CreateDelegation([FromBody] CreateDelegationRequest req)
    {
        var result = await _service.CreateDelegationAsync(
            req.DelegatorId,
            req.DelegateId,
            req.PolicyId,
            req.Reason,
            req.ValidFrom,
            req.ValidUntil
        );
        return Ok(result);
    }
}

public record CreatePolicyRequest(
    string Code, 
    string Name, 
    string? Description, 
    string ScopeType, 
    long? ScopeId, 
    string ChainMode, 
    short? MaxSlaHours, 
    string OnTimeoutAction, 
    IEnumerable<ApprovalChainStep> Steps, 
    long? CreatedBy
);

public record SubmitApprovalRequest(
    string PolicyCode, 
    string EntityType, 
    long EntityId, 
    long RequestedBy, 
    string? EntityContextJson, 
    string? CallbackUrl
);

public record DecideApprovalRequest(
    long DecidedBy, 
    string Decision, 
    string? Reason, 
    string? EvidencePath
);

public record CreateDelegationRequest(
    long DelegatorId, 
    long DelegateId, 
    long? PolicyId, 
    string Reason, 
    DateTime ValidFrom, 
    DateTime ValidUntil
);
