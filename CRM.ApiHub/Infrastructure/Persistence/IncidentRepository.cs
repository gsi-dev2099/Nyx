using Dapper;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class IncidentRepository : IIncidentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public IncidentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<IncidentCatalog>> GetCatalogAsync(long idCmpg, long idStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM sales_service.incident_catalog 
            WHERE id_cmpg = @IdCmpg AND id_status = @IdStatus AND is_active = true;";
        return await connection.QueryAsync<IncidentCatalog>(sql, new { IdCmpg = idCmpg, IdStatus = idStatus });
    }

    public async Task<IEnumerable<OrderIncident>> GetByOrderAsync(long idOrder)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.order_incident WHERE id_order = @IdOrder ORDER BY register DESC;";
        return await connection.QueryAsync<OrderIncident>(sql, new { IdOrder = idOrder });
    }

    public async Task<long> CreateAsync(OrderIncident incident)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.order_incident 
            (id_order, id_incident, custom_name, custom_description, incident_status, detected_by, assigned_to_role, due_at, register) 
            VALUES 
            (@IdOrder, @IdIncident, @CustomName, @CustomDescription, 'OPEN', @DetectedBy, @AssignedToRole, @DueAt, NOW())
            RETURNING id_order_incident;";

        return await connection.ExecuteScalarAsync<long>(sql, incident);
    }

    public async Task CreateResponseAsync(long idOrderIncident, string responseText, string responseType, long respondedBy)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.incident_response 
            (id_order_incident, response_type, response_text, responded_by, register) 
            VALUES 
            (@IdOrderIncident, @ResponseType, @ResponseText, @RespondedBy, NOW());";

        await connection.ExecuteAsync(sql, new 
        { 
            IdOrderIncident = idOrderIncident, 
            ResponseType = responseType, 
            ResponseText = responseText, 
            RespondedBy = respondedBy 
        });
    }

    public async Task ResolveAsync(long idOrderIncident, string notes, long resolvedBy)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_incident 
            SET incident_status = 'RESOLVED', 
                resolution_notes = @Notes, 
                resolved_by = @ResolvedBy, 
                resolved_at = NOW() 
            WHERE id_order_incident = @IdOrderIncident;";

        await connection.ExecuteAsync(sql, new { Notes = notes, ResolvedBy = resolvedBy, IdOrderIncident = idOrderIncident });
    }

    public async Task<IEnumerable<KbArticleSuggestion>> GetKbSuggestionsAsync(long idIncident)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.id_article, a.title, a.summary, a.slug 
            FROM knowledge_base.kb_article a
            INNER JOIN knowledge_base.kb_article_incident_link l ON a.id_article = l.id_article
            WHERE l.id_incident = @IdIncident AND a.is_published = true;";

        return await connection.QueryAsync<KbArticleSuggestion>(sql, new { IdIncident = idIncident });
    }

    public async Task<OrderIncident?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.order_incident WHERE id_order_incident = @Id;";
        return await connection.QueryFirstOrDefaultAsync<OrderIncident>(sql, new { Id = id });
    }

    public async Task<bool> UpdateAsync(long id, string customName, string customDescription, string? customSolution, string? assignedToRole, DateTime? dueAt)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_incident 
            SET custom_name = @CustomName, 
                custom_description = @CustomDescription, 
                custom_solution = @CustomSolution, 
                assigned_to_role = @AssignedToRole, 
                due_at = @DueAt 
            WHERE id_order_incident = @Id;";
        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            CustomName = customName, 
            CustomDescription = customDescription, 
            CustomSolution = customSolution, 
            AssignedToRole = assignedToRole, 
            DueAt = dueAt 
        });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM sales_service.order_incident WHERE id_order_incident = @Id;";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<OrderIncident>> GetFilteredAsync(string? assignedToRole, string? status, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new System.Text.StringBuilder(@"
            SELECT o.*, c.priority AS Priority 
            FROM sales_service.order_incident o
            LEFT JOIN sales_service.incident_catalog c ON o.id_incident = c.id_incident
            WHERE 1=1");
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(assignedToRole))
        {
            sql.Append(" AND o.assigned_to_role = @AssignedToRole");
            parameters.Add("AssignedToRole", assignedToRole);
        }
        
        if (!string.IsNullOrEmpty(status))
        {
            sql.Append(" AND o.incident_status = @Status");
            parameters.Add("Status", status);
        }
        
        sql.Append(" ORDER BY o.register DESC;");
        
        return await connection.QueryAsync<OrderIncident>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );
    }
}