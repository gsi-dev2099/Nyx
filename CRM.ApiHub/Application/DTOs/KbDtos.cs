using System;

namespace CRM.ApiHub.Application.DTOs;

public class KbArticleResponseDto
{
    public long IdArticle { get; set; }
    public long IdCategory { get; set; }
    public long? IdCmpg { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string[] TargetRoles { get; set; } = Array.Empty<string>();
    public string Version { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubmitFeedbackRequestDto
{
    public bool IsHelpful { get; set; }
    public string? Comment { get; set; }
}
