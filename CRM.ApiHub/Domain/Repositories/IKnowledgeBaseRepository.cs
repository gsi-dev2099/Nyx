using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IKnowledgeBaseRepository
{
    Task<IEnumerable<KbArticle>> SearchAsync(string? query, long? idCmpg, string? contentType, CancellationToken ct = default);
    Task<KbArticle?> GetByIdAsync(long idArticle, CancellationToken ct = default);
    Task<IEnumerable<KbArticle>> GetByIncidentAsync(long idIncident, CancellationToken ct = default);
    Task<IEnumerable<KbArticle>> GetByStatusAsync(long idStatus, string? role, CancellationToken ct = default);
    Task<bool> TrackViewAsync(long idArticle, long? userId, CancellationToken ct = default);
    Task<bool> SaveFeedbackAsync(long idArticle, long userId, bool isHelpful, string? comment, CancellationToken ct = default);
}
