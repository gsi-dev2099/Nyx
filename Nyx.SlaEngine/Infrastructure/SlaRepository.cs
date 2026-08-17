using System.Data;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;
using Nyx.SlaEngine.Domain.Entities;

namespace Nyx.SlaEngine.Infrastructure;

public interface ISlaRepository
{
    Task<IEnumerable<SlaPolicy>> GetPoliciesAsync();
    Task<SlaPolicy?> GetPolicyByCodeAsync(string code);
    Task<long> CreatePolicyAsync(SlaPolicy policy);
    
    Task<SlaMeasurement?> GetMeasurementAsync(string entityType, long entityId, long policyId);
    Task<IEnumerable<SlaMeasurement>> GetActiveMeasurementsAsync();
    Task<long> StartMeasurementAsync(SlaMeasurement measurement);
    Task UpdateMeasurementAsync(SlaMeasurement measurement);
    
    Task<IEnumerable<WorkSchedule>> GetCalendarScheduleAsync(long calendarId);
    Task<IEnumerable<Holiday>> GetCalendarHolidaysAsync(long calendarId);
    
    Task LogAuditAsync(long actorId, string action, long? measurementId, long? policyId, string detailJson);
}

public class SlaRepository : ISlaRepository
{
    private readonly string _connectionString;

    public SlaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection configuration is missing.");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<SlaPolicy>> GetPoliciesAsync()
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_policy AS IdPolicy, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, target_minutes AS TargetMinutes, warning_pct AS WarningPct, critical_pct AS CriticalPct, escalation_pct AS EscalationPct, applies_to AS AppliesTo, is_active AS IsActive, created_by AS CreatedBy, created_at AS CreatedAt FROM sla_policy WHERE is_active = true ORDER BY id_policy;";
        return await db.QueryAsync<SlaPolicy>(sql);
    }

    public async Task<SlaPolicy?> GetPolicyByCodeAsync(string code)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_policy AS IdPolicy, code, name, description, scope_type AS ScopeType, scope_id AS ScopeId, target_minutes AS TargetMinutes, warning_pct AS WarningPct, critical_pct AS CriticalPct, escalation_pct AS EscalationPct, applies_to AS AppliesTo, is_active AS IsActive, created_by AS CreatedBy, created_at AS CreatedAt FROM sla_policy WHERE code = @code AND is_active = true;";
        return await db.QueryFirstOrDefaultAsync<SlaPolicy>(sql, new { code });
    }

    public async Task<long> CreatePolicyAsync(SlaPolicy policy)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO sla_policy (code, name, description, scope_type, scope_id, target_minutes, warning_pct, critical_pct, escalation_pct, applies_to, is_active, created_by)
            VALUES (@Code, @Name, @Description, @ScopeType, @ScopeId, @TargetMinutes, @WarningPct, @CriticalPct, @EscalationPct, @AppliesTo, true, @CreatedBy)
            RETURNING id_policy;";
        return await db.ExecuteScalarAsync<long>(sql, policy);
    }

    public async Task<SlaMeasurement?> GetMeasurementAsync(string entityType, long entityId, long policyId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_measurement AS IdMeasurement, id_policy AS IdPolicy, entity_type AS EntityType, entity_id AS EntityId, owner_user_id AS OwnerUserId, started_at AS StartedAt, paused_at AS PausedAt, resolved_at AS ResolvedAt, elapsed_minutes AS ElapsedMinutes, status, breach_at AS BreachAt, metadata
            FROM sla_measurement
            WHERE entity_type = @entityType AND entity_id = @entityId AND id_policy = @policyId;";
        return await db.QueryFirstOrDefaultAsync<SlaMeasurement>(sql, new { entityType, entityId, policyId });
    }

    public async Task<IEnumerable<SlaMeasurement>> GetActiveMeasurementsAsync()
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT id_measurement AS IdMeasurement, id_policy AS IdPolicy, entity_type AS EntityType, entity_id AS EntityId, owner_user_id AS OwnerUserId, started_at AS StartedAt, paused_at AS PausedAt, resolved_at AS ResolvedAt, elapsed_minutes AS ElapsedMinutes, status, breach_at AS BreachAt, metadata
            FROM sla_measurement
            WHERE status IN ('RUNNING', 'PAUSED', 'WARNING');";
        return await db.QueryAsync<SlaMeasurement>(sql);
    }

    public async Task<long> StartMeasurementAsync(SlaMeasurement m)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO sla_measurement (id_policy, entity_type, entity_id, owner_user_id, started_at, elapsed_minutes, status, breach_at, metadata)
            VALUES (@IdPolicy, @EntityType, @EntityId, @OwnerUserId, @StartedAt, @ElapsedMinutes, @Status, @BreachAt, @Metadata::jsonb)
            RETURNING id_measurement;";
        return await db.ExecuteScalarAsync<long>(sql, m);
    }

    public async Task UpdateMeasurementAsync(SlaMeasurement m)
    {
        using var db = CreateConnection();
        const string sql = @"
            UPDATE sla_measurement
            SET paused_at = @PausedAt, resolved_at = @ResolvedAt, elapsed_minutes = @ElapsedMinutes, status = @Status, metadata = @Metadata::jsonb
            WHERE id_measurement = @IdMeasurement;";
        await db.ExecuteAsync(sql, m);
    }

    public async Task<IEnumerable<WorkSchedule>> GetCalendarScheduleAsync(long calendarId)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_schedule AS IdSchedule, id_calendar AS IdCalendar, day_of_week AS DayOfWeek, start_time AS StartTime, end_time AS EndTime FROM work_schedule WHERE id_calendar = @calendarId;";
        return await db.QueryAsync<WorkSchedule>(sql, new { calendarId });
    }

    public async Task<IEnumerable<Holiday>> GetCalendarHolidaysAsync(long calendarId)
    {
        using var db = CreateConnection();
        const string sql = "SELECT id_holiday AS IdHoliday, id_calendar AS IdCalendar, holiday_date AS HolidayDate, name, is_half_day AS IsHalfDay FROM holiday WHERE id_calendar = @calendarId;";
        return await db.QueryAsync<Holiday>(sql, new { calendarId });
    }

    public async Task LogAuditAsync(long actorId, string action, long? measurementId, long? policyId, string detailJson)
    {
        using var db = CreateConnection();
        var timestamp = DateTime.UtcNow.ToString("o");
        var rawData = $"{actorId}|{action}|{measurementId}|{policyId}|{detailJson}|{timestamp}";
        using var sha = SHA512.Create();
        var checksum = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawData)));

        const string sql = @"
            INSERT INTO sla_audit_log (id_measurement, id_policy, action, actor_id, detail, checksum)
            VALUES (@measurementId, @policyId, @action, @actorId, @detailJson::jsonb, @checksum);";
        await db.ExecuteAsync(sql, new { measurementId, policyId, action, actorId, detailJson, checksum });
    }
}
