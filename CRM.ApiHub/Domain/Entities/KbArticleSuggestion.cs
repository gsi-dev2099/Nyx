namespace CRM.ApiHub.Domain.Entities;

public class KbArticleSuggestion
{
    public long IdArticle { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Slug { get; set; }
}