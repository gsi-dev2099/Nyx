using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.KB;

public class SubmitKbFeedbackUseCase
{
    private readonly IKnowledgeBaseRepository _kbRepository;

    public SubmitKbFeedbackUseCase(IKnowledgeBaseRepository kbRepository)
    {
        _kbRepository = kbRepository;
    }

    public async Task<bool> ExecuteAsync(long idArticle, long userId, bool isHelpful, string? comment, CancellationToken ct = default)
    {
        return await _kbRepository.SaveFeedbackAsync(idArticle, userId, isHelpful, comment, ct);
    }
}
