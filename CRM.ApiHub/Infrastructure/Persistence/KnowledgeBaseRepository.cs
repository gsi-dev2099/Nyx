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

public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public KnowledgeBaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<KbArticle>> SearchAsync(string? query, long? idCmpg, string? contentType, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = new StringBuilder(@"
            SELECT 
                id_article AS IdArticle,
                id_category AS IdCategory,
                id_cmpg AS IdCmpg,
                title AS Title,
                slug AS Slug,
                summary AS Summary,
                content AS Content,
                content_type AS ContentType,
                target_roles AS TargetRoles,
                version AS Version,
                is_published AS IsPublished,
                is_pinned AS IsPinned,
                view_count AS ViewCount,
                helpful_count AS HelpfulCount,
                not_helpful_count AS NotHelpfulCount,
                created_by AS CreatedBy,
                created_at AS CreatedAt
            FROM knowledge_base.kb_article
            WHERE is_published = true");

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql.Append(@" AND to_tsvector('spanish', title || ' ' || COALESCE(summary, '') || ' ' || content) @@ plainto_tsquery('spanish', @Query)");
            parameters.Add("Query", query);
        }

        if (idCmpg.HasValue)
        {
            sql.Append(" AND (id_cmpg = @IdCmpg OR id_cmpg IS NULL)");
            parameters.Add("IdCmpg", idCmpg.Value);
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            sql.Append(" AND content_type = @ContentType");
            parameters.Add("ContentType", contentType);
        }

        sql.Append(" ORDER BY is_pinned DESC, view_count DESC, created_at DESC;");

        return await connection.QueryAsync<KbArticle>(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
        );
    }

    public async Task<KbArticle?> GetByIdAsync(long idArticle, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                id_article AS IdArticle,
                id_category AS IdCategory,
                id_cmpg AS IdCmpg,
                title AS Title,
                slug AS Slug,
                summary AS Summary,
                content AS Content,
                content_type AS ContentType,
                target_roles AS TargetRoles,
                version AS Version,
                is_published AS IsPublished,
                is_pinned AS IsPinned,
                view_count AS ViewCount,
                helpful_count AS HelpfulCount,
                not_helpful_count AS NotHelpfulCount,
                created_by AS CreatedBy,
                created_at AS CreatedAt
            FROM knowledge_base.kb_article
            WHERE id_article = @IdArticle;";

        return await connection.QueryFirstOrDefaultAsync<KbArticle>(
            new CommandDefinition(sql, new { IdArticle = idArticle }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<KbArticle>> GetByIncidentAsync(long idIncident, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                a.id_article AS IdArticle,
                a.id_category AS IdCategory,
                a.id_cmpg AS IdCmpg,
                a.title AS Title,
                a.slug AS Slug,
                a.summary AS Summary,
                a.content AS Content,
                a.content_type AS ContentType,
                a.target_roles AS TargetRoles,
                a.version AS Version,
                a.is_published AS IsPublished,
                a.is_pinned AS IsPinned,
                a.view_count AS ViewCount,
                a.helpful_count AS HelpfulCount,
                a.not_helpful_count AS NotHelpfulCount,
                a.created_by AS CreatedBy,
                a.created_at AS CreatedAt
            FROM knowledge_base.kb_article a
            INNER JOIN knowledge_base.kb_article_incident_link l ON a.id_article = l.id_article
            WHERE l.id_incident = @IdIncident AND a.is_published = true
            ORDER BY l.is_primary DESC, a.view_count DESC;";

        return await connection.QueryAsync<KbArticle>(
            new CommandDefinition(sql, new { IdIncident = idIncident }, cancellationToken: ct)
        );
    }

    public async Task<IEnumerable<KbArticle>> GetByStatusAsync(long idStatus, string? role, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                a.id_article AS IdArticle,
                a.id_category AS IdCategory,
                a.id_cmpg AS IdCmpg,
                a.title AS Title,
                a.slug AS Slug,
                a.summary AS Summary,
                a.content AS Content,
                a.content_type AS ContentType,
                a.target_roles AS TargetRoles,
                a.version AS Version,
                a.is_published AS IsPublished,
                a.is_pinned AS IsPinned,
                a.view_count AS ViewCount,
                a.helpful_count AS HelpfulCount,
                a.not_helpful_count AS NotHelpfulCount,
                a.created_by AS CreatedBy,
                a.created_at AS CreatedAt
            FROM knowledge_base.kb_article a
            INNER JOIN knowledge_base.kb_article_status_link l ON a.id_article = l.id_article
            WHERE l.id_status = @IdStatus 
              AND (l.for_role = @Role OR l.for_role = 'ALL' OR @Role IS NULL)
              AND a.is_published = true
            ORDER BY a.is_pinned DESC, a.view_count DESC;";

        return await connection.QueryAsync<KbArticle>(
            new CommandDefinition(sql, new { IdStatus = idStatus, Role = role }, cancellationToken: ct)
        );
    }

    public async Task<bool> TrackViewAsync(long idArticle, long? userId, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            const string updateSql = @"
                UPDATE knowledge_base.kb_article 
                SET view_count = view_count + 1 
                WHERE id_article = @IdArticle;";

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(updateSql, new { IdArticle = idArticle }, transaction: transaction, cancellationToken: ct)
            );

            const string insertLogSql = @"
                INSERT INTO knowledge_base.kb_search_log 
                    (search_date, id_user, query, results_count, clicked_article_id, register)
                VALUES 
                    (CURRENT_DATE, @UserId, 'DIRECT_VIEW', 1, @IdArticle, NOW());";

            await connection.ExecuteAsync(
                new CommandDefinition(insertLogSql, new { UserId = userId, IdArticle = idArticle }, transaction: transaction, cancellationToken: ct)
            );

            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> SaveFeedbackAsync(long idArticle, long userId, bool isHelpful, string? comment, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var transaction = connection.BeginTransaction();
        try
        {
            const string insertFeedbackSql = @"
                INSERT INTO knowledge_base.kb_feedback 
                    (id_article, feedback_date, id_user, is_helpful, comment, context, register)
                VALUES 
                    (@IdArticle, CURRENT_DATE, @UserId, @IsHelpful, @Comment, '{}'::jsonb, NOW());";

            await connection.ExecuteAsync(
                new CommandDefinition(insertFeedbackSql, new 
                { 
                    IdArticle = idArticle, 
                    UserId = userId, 
                    IsHelpful = isHelpful, 
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
