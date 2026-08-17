using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class LeadRepository : ILeadRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LeadRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Lead>> GetByAssignedUserAsync(long userId, LeadFilters? filters = null, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new StringBuilder("SELECT * FROM lead_service.lead WHERE assigned_user_id = @UserId AND is_active = true");
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (filters != null)
        {
            if (filters.StatusId.HasValue)
            {
                sql.Append(" AND current_status_id = @StatusId");
                parameters.Add("StatusId", filters.StatusId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                sql.Append(" AND (first_name ILIKE @SearchPattern OR last_name ILIKE @SearchPattern OR email ILIKE @SearchPattern OR phone ILIKE @SearchPattern)");
                parameters.Add("SearchPattern", $"%{filters.SearchTerm}%");
            }

            if (filters.Limit.HasValue)
            {
                sql.Append(" LIMIT @Limit");
                parameters.Add("Limit", filters.Limit.Value);

                if (filters.Page.HasValue && filters.Page.Value > 0)
                {
                    var offset = (filters.Page.Value - 1) * filters.Limit.Value;
                    sql.Append(" OFFSET @Offset");
                    parameters.Add("Offset", offset);
                }
            }
        }

        return await connection.QueryAsync<Lead>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );
    }

    public async Task<Lead?> GetByIdAsync(long idLead, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM lead_service.lead WHERE id_lead = @IdLead;";

        return await connection.QueryFirstOrDefaultAsync<Lead>(
            new CommandDefinition(sql, new { IdLead = idLead }, cancellationToken: ct)
        );
    }

    public async Task<long> CreateAsync(Lead lead, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // No asignamos ni insertamos FullName porque es una columna autogenerada en base de datos.

        const string sql = @"
            INSERT INTO lead_service.lead (
                id_cmpg, id_src, first_name, last_name, phone, email, 
                document_number, raw_data, current_status_id, assigned_user_id, 
                owner_user_id, custody_user_id, is_active, register, last_update
            )
            VALUES (
                @IdCmpg, @IdSrc, @FirstName, @LastName, @Phone, @Email, 
                @DocumentNumber, CAST(@RawData AS jsonb), @CurrentStatusId, @AssignedUserId, 
                @OwnerUserId, @CustodyUserId, @IsActive, @Register, @LastUpdate
            )
            RETURNING id_lead;";

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, lead, cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateStatusAsync(long idLead, int idStatus, string? comment, long actorId, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Obtener estado actual del lead con bloqueo de fila para evitar condiciones de carrera
            const string selectLeadSql = @"
                SELECT current_status_id 
                FROM lead_service.lead 
                WHERE id_lead = @IdLead 
                FOR UPDATE;";

            var currentStatusId = await connection.ExecuteScalarAsync<long?>(
                new CommandDefinition(selectLeadSql, new { IdLead = idLead }, transaction: transaction, cancellationToken: ct)
            );

            if (!currentStatusId.HasValue)
            {
                transaction.Rollback();
                return false;
            }

           // 2. Obtener el rol del usuario actor
            const string selectRoleSql = @"
                SELECT r.name 
                FROM user_service.users u
                LEFT JOIN access_control.user_role ur ON u.id_user = ur.id_user AND ur.is_active = true
                LEFT JOIN access_control.role r ON ur.id_role = r.id_role AND r.is_active = true
                WHERE u.id_user = @ActorId;";

            var roleName = await connection.ExecuteScalarAsync<string?>(
                new CommandDefinition(selectRoleSql, new { ActorId = actorId }, transaction: transaction, cancellationToken: ct)
            );

            if (string.IsNullOrEmpty(roleName))
            {
                throw new InvalidOperationException("No se encontró el rol del usuario actor.");
            }

            // 3. Validar transición usando la función PostgreSQL sales_service.validate_status_transition
            const string validateSql = "SELECT sales_service.validate_status_transition(@FromStatus, @ToStatus, @Role);";
            var isValid = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(validateSql, new 
                { 
                    FromStatus = currentStatusId.Value, 
                    ToStatus = (long)idStatus, 
                    Role = roleName 
                }, transaction: transaction, cancellationToken: ct)
            );

            if (!isValid)
            {
                throw new InvalidOperationException($"Transición de estado de {currentStatusId.Value} a {idStatus} no permitida para el rol '{roleName}'.");
            }

            // 4. Actualizar el estado del lead
            const string updateLeadSql = @"
                UPDATE lead_service.lead 
                SET current_status_id = @NewStatusId, last_update = NOW() 
                WHERE id_lead = @IdLead;";

            await connection.ExecuteAsync(
                new CommandDefinition(updateLeadSql, new { NewStatusId = (long)idStatus, IdLead = idLead }, transaction: transaction, cancellationToken: ct)
            );

            // 5. Registrar la transición en el historial de tracking
            const string insertTrackingSql = @"
                INSERT INTO lead_service.lead_tracking (
                    id_lead, previous_status_id, new_status_id, id_user, comments, register
                )
                VALUES (
                    @IdLead, @PreviousStatusId, @NewStatusId, @ActorId, @Comment, NOW()
                );";

            await connection.ExecuteAsync(
                new CommandDefinition(insertTrackingSql, new 
                { 
                    IdLead = idLead, 
                    PreviousStatusId = currentStatusId.Value, 
                    NewStatusId = (long)idStatus, 
                    ActorId = actorId, 
                    Comment = comment 
                }, transaction: transaction, cancellationToken: ct)
            );

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
