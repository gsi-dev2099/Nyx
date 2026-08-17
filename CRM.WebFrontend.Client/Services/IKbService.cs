using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

public interface IKbService
{
    Task<List<KbArticleResponseDto>> SearchAsync(string? query, string? contentType);
    Task<KbArticleResponseDto?> GetByIdAsync(long idArticle);
    Task<bool> SubmitFeedbackAsync(long idArticle, bool isHelpful, string? comment);
}
