using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.KB;

public class GetKbArticleByIdUseCase
{
    private readonly IKnowledgeBaseRepository _kbRepository;

    public GetKbArticleByIdUseCase(IKnowledgeBaseRepository kbRepository)
    {
        _kbRepository = kbRepository;
    }

    public async Task<KbArticleResponseDto?> ExecuteAsync(long idArticle, long? userId, CancellationToken ct = default)
    {
        var article = await _kbRepository.GetByIdAsync(idArticle, ct);
        if (article == null)
        {
            return null;
        }

        // Track view asynchronously
        await _kbRepository.TrackViewAsync(idArticle, userId, ct);

        return new KbArticleResponseDto
        {
            IdArticle = article.IdArticle,
            IdCategory = article.IdCategory,
            IdCmpg = article.IdCmpg,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            ContentType = article.ContentType,
            TargetRoles = article.TargetRoles,
            Version = article.Version,
            IsPinned = article.IsPinned,
            ViewCount = article.ViewCount + 1, // Add 1 because we just tracked it
            HelpfulCount = article.HelpfulCount,
            NotHelpfulCount = article.NotHelpfulCount,
            CreatedAt = article.CreatedAt
        };
    }
}
