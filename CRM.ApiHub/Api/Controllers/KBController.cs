using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.KB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Dapper;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class KBController : ControllerBase
{
    private readonly SearchKbArticlesUseCase _searchKbArticlesUseCase;
    private readonly GetKbArticleByIdUseCase _getKbArticleByIdUseCase;
    private readonly SubmitKbFeedbackUseCase _submitKbFeedbackUseCase;
    private readonly CRM.ApiHub.Infrastructure.Persistence.IDbConnectionFactory _connectionFactory;

    public KBController(
        SearchKbArticlesUseCase searchKbArticlesUseCase,
        GetKbArticleByIdUseCase getKbArticleByIdUseCase,
        SubmitKbFeedbackUseCase submitKbFeedbackUseCase,
        CRM.ApiHub.Infrastructure.Persistence.IDbConnectionFactory connectionFactory)
    {
        _searchKbArticlesUseCase = searchKbArticlesUseCase;
        _getKbArticleByIdUseCase = getKbArticleByIdUseCase;
        _submitKbFeedbackUseCase = submitKbFeedbackUseCase;
        _connectionFactory = connectionFactory;
    }

    [HttpGet("api/kb/categories")]
    public async Task<IActionResult> GetCategories()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT id_category AS IdCategory, name AS Name, description AS Description FROM knowledge_base.kb_category WHERE is_active = true ORDER BY order_index;";
        var categories = await connection.QueryAsync(sql);
        return Ok(categories);
    }

    [HttpGet("api/kb/articles")]
    public async Task<IActionResult> GetArticles()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.id_article AS Id, a.title AS Title, c.name AS Category, COALESCE(cmp.name, 'General') AS Campaign,
                   a.summary AS Summary, a.content AS Content, a.helpful_count AS HelpfulVotes, a.view_count AS Views
            FROM knowledge_base.kb_article a
            LEFT JOIN knowledge_base.kb_category c ON a.id_category = c.id_category
            LEFT JOIN campaign_service.campaign cmp ON a.id_cmpg = cmp.id_cmpg
            WHERE a.is_published = true
            ORDER BY a.created_at DESC;";
        var articles = await connection.QueryAsync(sql);
        return Ok(articles);
    }

    [HttpGet("api/kb/search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] long? idCmpg,
        [FromQuery] string? contentType,
        [FromQuery] long? incidentId,
        [FromQuery] long? statusId,
        CancellationToken ct)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            var results = await _searchKbArticlesUseCase.ExecuteAsync(
                query, idCmpg, contentType, incidentId, statusId, userRole, ct);

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al realizar la búsqueda en la base de conocimiento.", details = ex.Message });
        }
    }

    [HttpGet("api/kb/{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            long? userId = null;
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId))
            {
                userId = parsedId;
            }

            var article = await _getKbArticleByIdUseCase.ExecuteAsync(id, userId, ct);
            if (article == null)
            {
                return NotFound(new { message = $"No se encontró ningún artículo de conocimiento con ID {id}." });
            }

            return Ok(article);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el artículo de conocimiento.", details = ex.Message });
        }
    }

    [HttpPost("api/kb/{id:long}/feedback")]
    public async Task<IActionResult> SubmitFeedback(long id, [FromBody] SubmitFeedbackRequestDto dto, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            long userId = 1; // Fallback
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long parsedId))
            {
                userId = parsedId;
            }

            var success = await _submitKbFeedbackUseCase.ExecuteAsync(id, userId, dto.IsHelpful, dto.Comment, ct);
            if (!success)
            {
                return BadRequest(new { message = "No se pudo registrar el feedback para este artículo." });
            }

            return Ok(new { message = "Feedback registrado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al enviar el feedback para el artículo.", details = ex.Message });
        }
    }
}
