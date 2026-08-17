using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("kb_feedback", Schema = "knowledge_base")]
public class KbFeedback
{
    [Key]
    [Column("id_feedback")]
    public long IdFeedback { get; set; }

    [Column("id_article")]
    public long IdArticle { get; set; }

    [Column("feedback_date")]
    public DateTime FeedbackDate { get; set; }

    [Column("id_user")]
    public long IdUser { get; set; }

    [Column("is_helpful")]
    public bool IsHelpful { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("context")]
    public string Context { get; set; } = "{}";

    [Column("register")]
    public DateTime Register { get; set; } = DateTime.UtcNow;
}
