using Dapper;
using System.Data;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class PreSaleRepository : IPreSaleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PreSaleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LeadPreSale>> GetByUserAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.*, 
                   u_owner.username AS OwnerUserName, 
                   u_curr.username AS CurrentUserName,
                   u_adv1.username AS AssignedAdvisor1Name,
                   u_adv2.username AS AssignedAdvisor2Name,
                   u_adv3.username AS AssignedAdvisor3Name,
                   c.name AS CampaignName,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%1ª Llamada%' OR lcl.notes LIKE '%CP #12%')), false) AS Call1Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%2ª Llamada%' OR lcl.notes LIKE '%CP #13%')), false) AS Call2Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%3ª Llamada%' OR lcl.notes LIKE '%CP #14%')), false) AS Call3Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Retención%' OR lcl.notes LIKE '%Alerta Cambio%' OR lcl.notes LIKE '%CP #15%')), false) AS RetentionCompleted,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Base Botada%' OR lcl.notes LIKE '%Gestion Botada%' OR lcl.notes LIKE '%CP #75%')), false) AS BotadaCompleted,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Alternas%' OR lcl.notes LIKE '%CP #76%')), false) AS AlternasCompleted,
                   (SELECT data_obtained::text FROM lead_service.lead_call_log WHERE id_presale = p.id_presale ORDER BY id_call DESC LIMIT 1) AS LastCallDataObtained
            FROM lead_service.lead_pre_sale p
            LEFT JOIN user_service.users u_owner ON p.owner_user_id = u_owner.id_user
            LEFT JOIN user_service.users u_curr ON p.current_user_id = u_curr.id_user
            LEFT JOIN user_service.users u_adv1 ON p.assigned_advisor_1_id = u_adv1.id_user
            LEFT JOIN user_service.users u_adv2 ON p.assigned_advisor_2_id = u_adv2.id_user
            LEFT JOIN user_service.users u_adv3 ON p.assigned_advisor_3_id = u_adv3.id_user
            LEFT JOIN campaign_service.campaign c ON p.id_cmpg = c.id_cmpg
            WHERE p.current_user_id = @UserId 
               OR p.owner_user_id = @UserId
               OR p.assigned_advisor_1_id = @UserId
               OR p.assigned_advisor_2_id = @UserId
               OR p.assigned_advisor_3_id = @UserId
            ORDER BY COALESCE(p.last_activity_at, p.register) DESC;";
        
        return await connection.QueryAsync<LeadPreSale>(sql, new { UserId = userId });
    }

    public async Task<LeadPreSale?> GetByIdAsync(int idPresale)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT p.*, 
                   u_owner.username AS OwnerUserName, 
                   u_curr.username AS CurrentUserName,
                   u_adv1.username AS AssignedAdvisor1Name,
                   u_adv2.username AS AssignedAdvisor2Name,
                   u_adv3.username AS AssignedAdvisor3Name,
                   c.name AS CampaignName,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%1ª Llamada%' OR lcl.notes LIKE '%CP #12%')), false) AS Call1Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%2ª Llamada%' OR lcl.notes LIKE '%CP #13%')), false) AS Call2Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%3ª Llamada%' OR lcl.notes LIKE '%CP #14%')), false) AS Call3Completed,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Retención%' OR lcl.notes LIKE '%Alerta Cambio%' OR lcl.notes LIKE '%CP #15%')), false) AS RetentionCompleted,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Base Botada%' OR lcl.notes LIKE '%Gestion Botada%' OR lcl.notes LIKE '%CP #75%')), false) AS BotadaCompleted,
                   COALESCE((SELECT COUNT(1) > 0 FROM lead_service.lead_call_log lcl WHERE lcl.id_presale = p.id_presale AND (lcl.notes LIKE '%Alternas%' OR lcl.notes LIKE '%CP #76%')), false) AS AlternasCompleted
            FROM lead_service.lead_pre_sale p
            LEFT JOIN user_service.users u_owner ON p.owner_user_id = u_owner.id_user
            LEFT JOIN user_service.users u_curr ON p.current_user_id = u_curr.id_user
            LEFT JOIN user_service.users u_adv1 ON p.assigned_advisor_1_id = u_adv1.id_user
            LEFT JOIN user_service.users u_adv2 ON p.assigned_advisor_2_id = u_adv2.id_user
            LEFT JOIN user_service.users u_adv3 ON p.assigned_advisor_3_id = u_adv3.id_user
            LEFT JOIN campaign_service.campaign c ON p.id_cmpg = c.id_cmpg
            WHERE p.id_presale = @IdPresale;";

        return await connection.QuerySingleOrDefaultAsync<LeadPreSale>(sql, new { IdPresale = idPresale });
    }

    public async Task<int> CreateAsync(LeadPreSale preSale)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO lead_service.lead_pre_sale (
                id_cmpg, phone, operator, target_operator, first_name, last_name, address, province, dni,
                coverage_status, id_status, owner_user_id, current_user_id, 
                assigned_advisor_1_id, assigned_advisor_2_id, assigned_advisor_3_id,
                notes, register
            )
            VALUES (
                @IdCmpg, @Phone, @Operator, @TargetOperator, @FirstName, @LastName, @Address, @Province, @Dni,
                @CoverageStatus, @IdStatus, @OwnerUserId, @CurrentUserId, 
                @AssignedAdvisor1Id, @AssignedAdvisor2Id, @AssignedAdvisor3Id,
                @Notes, @Register
            )
            RETURNING id_presale;";

        return await connection.ExecuteScalarAsync<int>(sql, preSale);
    }

    public async Task<bool> UpdateAsync(LeadPreSale preSale, long userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale
            SET id_cmpg = @IdCmpg,
                phone = @Phone,
                operator = @Operator,
                target_operator = @TargetOperator,
                first_name = @FirstName,
                last_name = @LastName,
                address = @Address,
                province = @Province,
                dni = @Dni,
                notes = @Notes,
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;";

        var rows = await connection.ExecuteAsync(sql, new 
        {
            preSale.IdPresale,
            preSale.IdCmpg,
            preSale.Phone,
            preSale.Operator,
            preSale.TargetOperator,
            preSale.FirstName,
            preSale.LastName,
            preSale.Address,
            preSale.Province,
            preSale.Dni,
            preSale.Notes,
            UserId = userId
        });

        return rows > 0;
    }

    public async Task<bool> AddCallLogAsync(int idPresale, string callLog, long userId = 1, IEnumerable<string>? completedSteps = null, string? result = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string getNextCallNumSql = "SELECT COALESCE(MAX(call_number), 0) + 1 FROM lead_service.lead_call_log WHERE id_presale = @PreSaleId;";
        var nextCallNum = (short)await connection.ExecuteScalarAsync<int>(getNextCallNumSql, new { PreSaleId = idPresale });

        // Serializar pasos completados en data_obtained para trazabilidad visual
        var stepsArr = completedSteps?.ToArray() ?? Array.Empty<string>();
        var dataObtainedJson = System.Text.Json.JsonSerializer.Serialize(new 
        { 
            steps = stepsArr,
            result = result,
            recorded_at = DateTime.UtcNow.ToString("o")
        });

        const string sql = @"
            INSERT INTO lead_service.lead_call_log 
            (id_presale, call_number, id_user, call_type, result, data_obtained, notes, register)
            VALUES 
            (@PreSaleId, @CallNumber, @UserId, @CallType, @Result, @DataObtained::jsonb, @Notes, @Register);

            -- Al completar un checkpoint o llamada, la gestión regresa automáticamente al dueño para que lo pueda gestionar él o asignar el siguiente paso
            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            SELECT @PreSaleId, @UserId, owner_user_id, assigned_call_step, 'RETURNED_TO_OWNER', 'Checkpoint completado. Gestión retornada automáticamente al propietario.'
            FROM lead_service.lead_pre_sale 
            WHERE id_presale = @PreSaleId 
              AND assignment_status = 'ACCEPTED'
              AND owner_user_id <> @UserId;

            UPDATE lead_service.lead_pre_sale 
            SET current_user_id = owner_user_id,
                assignment_status = 'NONE',
                assignment_responded_at = NOW(),
                last_activity_at = NOW()
            WHERE id_presale = @PreSaleId;";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            PreSaleId = idPresale, 
            CallNumber = nextCallNum,
            UserId = userId,
            CallType = "OUTBOUND",
            Result = result,
            DataObtained = dataObtainedJson,
            Notes = callLog,
            Register = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> AssignAsync(int idPresale, int toUserId, string context)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET current_user_id = @ToUserId, 
                assigned_advisor_1_id = COALESCE(assigned_advisor_1_id, @ToUserId),
                notes = CASE WHEN @Context IS NOT NULL AND @Context <> '' THEN @Context ELSE notes END, 
                last_activity_at = @UpdatedAt
            WHERE id_presale = @IdPresale;";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            ToUserId = toUserId, 
            Context = context, 
            IdPresale = idPresale,
            UpdatedAt = DateTime.UtcNow
        });

        return rowsAffected > 0;
    }

    public async Task<bool> AssignMultiAsync(int idPresale, long? advisor1Id, long? advisor2Id, long? advisor3Id, string context)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET assigned_advisor_1_id = @Advisor1Id,
                assigned_advisor_2_id = @Advisor2Id,
                assigned_advisor_3_id = @Advisor3Id,
                current_user_id = COALESCE(@Advisor1Id, @Advisor2Id, @Advisor3Id, current_user_id),
                notes = CASE WHEN @Context IS NOT NULL AND @Context <> '' THEN @Context ELSE notes END,
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            IdPresale = idPresale,
            Advisor1Id = advisor1Id,
            Advisor2Id = advisor2Id,
            Advisor3Id = advisor3Id,
            Context = context
        });

        return rowsAffected > 0;
    }

    public async Task<bool> AssignStepWithHandshakeAsync(int idPresale, long toUserId, int callStep, long fromUserId, string context)
    {
        using var connection = _connectionFactory.CreateConnection();
        string advisorCol = callStep switch
        {
            1 => "assigned_advisor_1_id",
            2 => "assigned_advisor_2_id",
            3 => "assigned_advisor_3_id",
            _ => "assigned_advisor_1_id"
        };

        var sql = $@"
            UPDATE lead_service.lead_pre_sale 
            SET {advisorCol} = @ToUserId,
                current_user_id = @ToUserId,
                assignment_status = 'PENDING_ACCEPT',
                assigned_call_step = @CallStep,
                assignment_requested_at = NOW(),
                assignment_rejected_reason = NULL,
                notes = CASE WHEN @Context IS NOT NULL AND @Context <> '' THEN @Context ELSE notes END,
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;

            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            VALUES (@IdPresale, @FromUserId, @ToUserId, @CallStep, 'REQUESTED', @Context);";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            IdPresale = idPresale,
            ToUserId = toUserId,
            FromUserId = fromUserId,
            CallStep = callStep,
            Context = context
        });

        return rowsAffected > 0;
    }

    public async Task<bool> AcceptAssignmentAsync(int idPresale, long userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET assignment_status = 'ACCEPTED',
                assignment_responded_at = NOW(),
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;

            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            SELECT @IdPresale, owner_user_id, @UserId, assigned_call_step, 'ACCEPTED', 'Confirmación aceptada por el asesor'
            FROM lead_service.lead_pre_sale WHERE id_presale = @IdPresale LIMIT 1;";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, UserId = userId });
        if (rowsAffected > 0)
        {
            try
            {
                await AddCallLogAsync(idPresale, $"[HANDOVER ACEPTADO]: El asesor confirmó y aceptó la gestión de la llamada.", userId);
            }
            catch { }
        }
        return rowsAffected > 0;
    }

    public async Task<bool> RejectAssignmentAsync(int idPresale, long userId, string reason)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET assignment_status = 'REJECTED',
                current_user_id = owner_user_id,
                assigned_advisor_1_id = CASE WHEN assigned_call_step = 1 THEN NULL ELSE assigned_advisor_1_id END,
                assigned_advisor_2_id = CASE WHEN assigned_call_step = 2 THEN NULL ELSE assigned_advisor_2_id END,
                assigned_advisor_3_id = CASE WHEN assigned_call_step = 3 THEN NULL ELSE assigned_advisor_3_id END,
                assignment_rejected_reason = @Reason,
                assignment_responded_at = NOW(),
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;

            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            SELECT @IdPresale, @UserId, owner_user_id, assigned_call_step, 'REJECTED', @Reason
            FROM lead_service.lead_pre_sale WHERE id_presale = @IdPresale LIMIT 1;";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, UserId = userId, Reason = reason });
        if (rowsAffected > 0)
        {
            try
            {
                await AddCallLogAsync(idPresale, $"[HANDOVER RECHAZADO]: Derivación rechazada. Motivo: {reason}. Retornado al propietario original.", userId);
            }
            catch { }
        }
        return rowsAffected > 0;
    }

    public async Task<bool> CancelAssignmentAsync(int idPresale, long ownerUserId)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Verificar que la asignación esté en estado PENDING_ACCEPT y que sea el dueño o supervisor
        const string checkSql = @"
            SELECT id_presale, owner_user_id, assignment_status 
            FROM lead_service.lead_pre_sale 
            WHERE id_presale = @IdPresale;";

        var presale = await connection.QueryFirstOrDefaultAsync(checkSql, new { IdPresale = idPresale });
        if (presale == null) return false;

        string assignmentStatus = presale.assignment_status ?? "NONE";
        long presaleOwnerId = (long)presale.owner_user_id;

        if (assignmentStatus != "PENDING_ACCEPT")
        {
            return false;
        }

        bool isOwner = (presaleOwnerId == ownerUserId);
        bool isSupervisor = ownerUserId == 1 || ownerUserId == 9 || ownerUserId == 237;
        if (!isOwner && !isSupervisor)
        {
            return false;
        }

        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET assignment_status = 'CANCELLED',
                current_user_id = owner_user_id,
                assigned_advisor_1_id = CASE WHEN assigned_call_step = 1 THEN NULL ELSE assigned_advisor_1_id END,
                assigned_advisor_2_id = CASE WHEN assigned_call_step = 2 THEN NULL ELSE assigned_advisor_2_id END,
                assigned_advisor_3_id = CASE WHEN assigned_call_step = 3 THEN NULL ELSE assigned_advisor_3_id END,
                assignment_responded_at = NOW(),
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;

            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            SELECT @IdPresale, @OwnerUserId, @OwnerUserId, assigned_call_step, 'CANCELLED_BY_OWNER', 'Cancelado por el propietario antes de aceptación'
            FROM lead_service.lead_pre_sale WHERE id_presale = @IdPresale LIMIT 1;";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, OwnerUserId = ownerUserId });
        if (rowsAffected > 0)
        {
            try
            {
                await AddCallLogAsync(idPresale, $"[HANDOVER CANCELADO]: El propietario canceló la asignación pendiente antes de que fuera aceptada.", ownerUserId);
            }
            catch { }
        }
        return rowsAffected > 0;
    }

    public async Task<bool> RevertAssignmentAsync(int idPresale, long actorUserId, string context)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Regla: Sólo el asesor que recibió y aceptó la transferencia (current_user_id) o supervisor/admin puede revertirla.
        // Si el estado es ACCEPTED, el dueño NO puede revertirla una vez que ya fue aceptada.
        const string checkSql = @"
            SELECT id_presale, owner_user_id, current_user_id, assignment_status 
            FROM lead_service.lead_pre_sale 
            WHERE id_presale = @IdPresale;";

        var presale = await connection.QueryFirstOrDefaultAsync(checkSql, new { IdPresale = idPresale });
        if (presale == null) return false;

        string assignmentStatus = presale.assignment_status ?? "NONE";
        long currentUserId = (long)presale.current_user_id;
        long ownerUserId = (long)presale.owner_user_id;

        if (assignmentStatus != "ACCEPTED")
        {
            return false;
        }

        bool isReceptor = (currentUserId == actorUserId && actorUserId != ownerUserId);
        bool isSupervisor = actorUserId == 1 || actorUserId == 9 || actorUserId == 237;

        if (!isReceptor && !isSupervisor)
        {
            // El dueño NO puede revertir si la transferencia ya fue aceptada
            return false;
        }

        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET assignment_status = 'REVERTED',
                current_user_id = owner_user_id,
                assigned_advisor_1_id = CASE WHEN assigned_call_step = 1 THEN NULL ELSE assigned_advisor_1_id END,
                assigned_advisor_2_id = CASE WHEN assigned_call_step = 2 THEN NULL ELSE assigned_advisor_2_id END,
                assigned_advisor_3_id = CASE WHEN assigned_call_step = 3 THEN NULL ELSE assigned_advisor_3_id END,
                assignment_responded_at = NOW(),
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;

            INSERT INTO lead_service.lead_assignment_log (id_presale, from_user_id, to_user_id, call_step, status, notes)
            SELECT @IdPresale, @ActorUserId, owner_user_id, assigned_call_step, 'REVERTED_BY_RECEPTOR', @Context
            FROM lead_service.lead_pre_sale WHERE id_presale = @IdPresale LIMIT 1;";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, ActorUserId = actorUserId, Context = context });
        if (rowsAffected > 0)
        {
            try
            {
                const string getNextCallNumSql = "SELECT COALESCE(MAX(call_number), 0) + 1 FROM lead_service.lead_call_log WHERE id_presale = @PreSaleId;";
                var nextCallNum = (short)await connection.ExecuteScalarAsync<int>(getNextCallNumSql, new { PreSaleId = idPresale });

                const string logSql = @"
                    INSERT INTO lead_service.lead_call_log 
                    (id_presale, call_number, id_user, call_type, data_obtained, notes, register)
                    VALUES 
                    (@PreSaleId, @CallNumber, @UserId, 'OUTBOUND', '{}'::jsonb, @Notes, NOW());";

                await connection.ExecuteAsync(logSql, new
                {
                    PreSaleId = idPresale,
                    CallNumber = nextCallNum,
                    UserId = actorUserId,
                    Notes = $"[HANDOVER REVERTIDO]: El asesor receptor devolvió la gestión de la llamada al propietario original antes de completar el checkpoint. Contexto: {context}"
                });
            }
            catch { }
        }
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateTargetOperatorAsync(int idPresale, string targetOperator)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale 
            SET target_operator = @TargetOperator,
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;";
        var rows = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, TargetOperator = targetOperator });
        return rows > 0;
    }

    public async Task<IEnumerable<PreSaleAssignmentHistoryDto>> GetAssignmentHistoryAsync(int idPresale)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT l.id_log AS IdLog,
                   l.id_presale AS IdPresale,
                   l.from_user_id AS FromUserId,
                   COALESCE(u_from.username, CAST(l.from_user_id AS VARCHAR)) AS FromUserName,
                   l.to_user_id AS ToUserId,
                   COALESCE(u_to.username, CAST(l.to_user_id AS VARCHAR)) AS ToUserName,
                   l.call_step AS CallStep,
                   l.status AS Status,
                   l.notes AS Notes,
                   l.created_at AS CreatedAt
            FROM lead_service.lead_assignment_log l
            LEFT JOIN user_service.users u_from ON l.from_user_id = u_from.id_user
            LEFT JOIN user_service.users u_to ON l.to_user_id = u_to.id_user
            WHERE l.id_presale = @IdPresale

            UNION ALL

            SELECT (1000000 + c.id_call) AS IdLog,
                   c.id_presale AS IdPresale,
                   c.id_user AS FromUserId,
                   COALESCE(u_call.username, CAST(c.id_user AS VARCHAR)) AS FromUserName,
                   c.id_user AS ToUserId,
                   COALESCE(u_call.username, CAST(c.id_user AS VARCHAR)) AS ToUserName,
                   c.call_number AS CallStep,
                   'CALL_LOG' AS Status,
                   c.notes AS Notes,
                   c.register AS CreatedAt
            FROM lead_service.lead_call_log c
            LEFT JOIN user_service.users u_call ON c.id_user = u_call.id_user
            WHERE c.id_presale = @IdPresale
              AND (c.notes IS NULL OR (c.notes NOT LIKE '[HANDOVER %' AND c.notes NOT LIKE '[CONSOLIDACIÓN%'))

            ORDER BY CreatedAt DESC;";

        return await connection.QueryAsync<PreSaleAssignmentHistoryDto>(sql, new { IdPresale = idPresale });
    }

    public async Task<bool> DiscardPreSaleAsync(int idPresale, string reason, long userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE lead_service.lead_pre_sale
            SET id_status = 4,
                discard_reason = @Reason,
                discarded_at = NOW(),
                discarded_by = @UserId,
                last_activity_at = NOW()
            WHERE id_presale = @IdPresale;";

        var rowsAffected = await connection.ExecuteAsync(sql, new { IdPresale = idPresale, Reason = reason, UserId = userId });
        return rowsAffected > 0;
    }

    public async Task<long> ConvertAsync(int idPresale, dynamic paramsData)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            await connection.ExecuteAsync("UPDATE lead_service.lead_status SET category = 'OPEN' WHERE id_sts = 1;");
            
            const string getCmpgSql = "SELECT id_cmpg FROM lead_service.lead_pre_sale WHERE id_presale = @IdPresale;";
            var idCmpg = await connection.ExecuteScalarAsync<long?>(getCmpgSql, new { IdPresale = idPresale });
            if (!idCmpg.HasValue) return 0;

            const string sql = "SELECT lead_service.convert_presale_to_order(@IdPresale, @IdCmpg, @UserId);";
            var resultId = await connection.ExecuteScalarAsync<long>(sql, new 
            { 
                IdPresale = idPresale,
                IdCmpg = idCmpg.Value,
                UserId = (long)(paramsData?.UserId ?? 0)
            });
            return resultId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConvertAsync: {ex.Message}");
            return 0;
        }
    }
}