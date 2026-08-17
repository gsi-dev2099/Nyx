using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.ApiHub.Domain.Entities;

[Table("user_alerts", Schema = "notification_service")]
public class UserAlert
{
    [Key]
    [Column("id_alert")]
    public int IdAlert { get; set; }

    [Column("id_user")]
    public long IdUser { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("module")]
    public string? Module { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("action_data")]
    public string? ActionData { get; set; } 
}