using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("kb_article", Schema = "knowledge_base")]
public class KbArticle
{
    [Key]
    [Column("id_article")]
    public long IdArticle { get; set; }

    [Column("id_category")]
    public long IdCategory { get; set; }

    [Column("id_cmpg")]
    public long? IdCmpg { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("summary")]
    public string? Summary { get; set; }

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [Column("target_roles")]
    public string[] TargetRoles { get; set; } = Array.Empty<string>();

    [Column("version")]
    public string Version { get; set; } = string.Empty;

    [Column("is_published")]
    public bool IsPublished { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    [Column("view_count")]
    public int ViewCount { get; set; }

    [Column("helpful_count")]
    public int HelpfulCount { get; set; }

    [Column("not_helpful_count")]
    public int NotHelpfulCount { get; set; }

    [Column("created_by")]
    public long CreatedBy { get; set; }

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("published_by")]
    public long? PublishedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("deprecated_at")]
    public DateTime? DeprecatedAt { get; set; }
}
