using Nyx.SlaEngine.Domain.Entities;
using Nyx.SlaEngine.Infrastructure;

namespace Nyx.SlaEngine.Application;

public interface ISlaService
{
    Task<IEnumerable<SlaPolicy>> GetPoliciesAsync();
    Task<SlaPolicy> CreatePolicyAsync(SlaPolicy policy);
    Task<SlaMeasurement> StartMeasurementAsync(string entityType, long entityId, string policyCode, long? ownerUserId, long actorId);
    Task<SlaMeasurement?> ResolveMeasurementAsync(string entityType, long entityId, string policyCode, long actorId);
    Task<SlaMeasurement?> PauseMeasurementAsync(string entityType, long entityId, string policyCode, long actorId);
    Task<SlaMeasurement?> GetStatusAsync(string entityType, long entityId, string policyCode);
}

public class SlaService : ISlaService
{
    private readonly ISlaRepository _repo;
    private readonly ILogger<SlaService> _logger;

    public SlaService(ISlaRepository repo, ILogger<SlaService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<SlaPolicy>> GetPoliciesAsync() => await _repo.GetPoliciesAsync();

    public async Task<SlaPolicy> CreatePolicyAsync(SlaPolicy policy)
    {
        var id = await _repo.CreatePolicyAsync(policy);
        policy.IdPolicy = id;
        await _repo.LogAuditAsync(policy.CreatedBy, "POLICY_CREATED", null, id, $"{{\"code\":\"{policy.Code}\",\"targetMinutes\":{policy.TargetMinutes}}}");
        return policy;
    }

    public async Task<SlaMeasurement> StartMeasurementAsync(string entityType, long entityId, string policyCode, long? ownerUserId, long actorId)
    {
        var policy = await _repo.GetPolicyByCodeAsync(policyCode) 
            ?? throw new KeyNotFoundException($"SLA Policy with code '{policyCode}' was not found.");

        var existing = await _repo.GetMeasurementAsync(entityType, entityId, policy.IdPolicy);
        if (existing != null)
        {
            return existing;
        }

        var startedAt = DateTime.UtcNow;
        var breachAt = startedAt.AddMinutes(policy.TargetMinutes);

        var measurement = new SlaMeasurement
        {
            IdPolicy = policy.IdPolicy,
            EntityType = entityType.ToLowerInvariant(),
            EntityId = entityId,
            OwnerUserId = ownerUserId,
            StartedAt = startedAt,
            BreachAt = breachAt,
            Status = "RUNNING",
            ElapsedMinutes = 0,
            Metadata = "{}"
        };

        var id = await _repo.StartMeasurementAsync(measurement);
        measurement.IdMeasurement = id;

        await _repo.LogAuditAsync(actorId, "MEASUREMENT_STARTED", id, policy.IdPolicy, $"{{\"entityType\":\"{entityType}\",\"entityId\":{entityId},\"breachAt\":\"{breachAt:o}\"}}");
        _logger.LogInformation("Started SLA measurement #{Id} for {EntityType} #{EntityId} with Policy {PolicyCode}", id, entityType, entityId, policyCode);

        return measurement;
    }

    public async Task<SlaMeasurement?> ResolveMeasurementAsync(string entityType, long entityId, string policyCode, long actorId)
    {
        var policy = await _repo.GetPolicyByCodeAsync(policyCode);
        if (policy == null) return null;

        var m = await _repo.GetMeasurementAsync(entityType, entityId, policy.IdPolicy);
        if (m == null || m.Status == "COMPLETED") return m;

        var now = DateTime.UtcNow;
        m.ResolvedAt = now;
        m.ElapsedMinutes = (int)Math.Max(0, (now - m.StartedAt).TotalMinutes);
        m.Status = m.ElapsedMinutes > policy.TargetMinutes ? "BREACHED" : "COMPLETED";

        await _repo.UpdateMeasurementAsync(m);
        await _repo.LogAuditAsync(actorId, "MEASUREMENT_RESOLVED", m.IdMeasurement, policy.IdPolicy, $"{{\"status\":\"{m.Status}\",\"elapsedMinutes\":{m.ElapsedMinutes}}}");
        return m;
    }

    public async Task<SlaMeasurement?> PauseMeasurementAsync(string entityType, long entityId, string policyCode, long actorId)
    {
        var policy = await _repo.GetPolicyByCodeAsync(policyCode);
        if (policy == null) return null;

        var m = await _repo.GetMeasurementAsync(entityType, entityId, policy.IdPolicy);
        if (m == null || m.Status != "RUNNING") return m;

        m.PausedAt = DateTime.UtcNow;
        m.Status = "PAUSED";

        await _repo.UpdateMeasurementAsync(m);
        await _repo.LogAuditAsync(actorId, "MEASUREMENT_PAUSED", m.IdMeasurement, policy.IdPolicy, "{}");
        return m;
    }

    public async Task<SlaMeasurement?> GetStatusAsync(string entityType, long entityId, string policyCode)
    {
        var policy = await _repo.GetPolicyByCodeAsync(policyCode);
        if (policy == null) return null;
        return await _repo.GetMeasurementAsync(entityType, entityId, policy.IdPolicy);
    }
}
