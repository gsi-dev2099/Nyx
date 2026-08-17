using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.KB;

public class SearchKbArticlesUseCase
{
    private readonly IKnowledgeBaseRepository _kbRepository;

    public SearchKbArticlesUseCase(IKnowledgeBaseRepository kbRepository)
    {
        _kbRepository = kbRepository;
    }

    public async Task<IEnumerable<KbArticleResponseDto>> ExecuteAsync(
        string? query, 
        long? idCmpg, 
        string? contentType, 
        long? incidentId, 
        long? statusId, 
        string? userRole, 
        CancellationToken ct = default)
    {
        // 1. If incidentId is provided, get by incident (sugerencias contextuales)
        if (incidentId.HasValue)
        {
            var articles = await _kbRepository.GetByIncidentAsync(incidentId.Value, ct);
            return articles.Select(MapToDto);
        }

        // 2. If statusId is provided, get by status and role (sugerencias por estado/rol)
        if (statusId.HasValue)
        {
            var articles = await _kbRepository.GetByStatusAsync(statusId.Value, userRole, ct);
            return articles.Select(MapToDto);
        }

        // 3. Otherwise, perform a standard search
        var searchResults = await _kbRepository.SearchAsync(query, idCmpg, contentType, ct);
        return searchResults.Select(MapToDto);
    }

    private static KbArticleResponseDto MapToDto(Domain.Entities.KbArticle a)
    {
        return new KbArticleResponseDto
        {
            IdArticle = a.IdArticle,
            IdCategory = a.IdCategory,
            IdCmpg = a.IdCmpg,
            Title = a.Title,
            Slug = a.Slug,
            Summary = a.Summary,
            Content = a.Content,
            ContentType = a.ContentType,
            TargetRoles = a.TargetRoles,
            Version = a.Version,
            IsPinned = a.IsPinned,
            ViewCount = a.ViewCount,
            HelpfulCount = a.HelpfulCount,
            NotHelpfulCount = a.NotHelpfulCount,
            CreatedAt = a.CreatedAt
        };
    }
}
