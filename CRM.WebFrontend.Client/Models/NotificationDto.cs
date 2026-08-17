using System.Text.Json.Serialization;

namespace CRM.WebFrontend.Client.Models;

public class NotificationDto
{
    [JsonPropertyName("idAlert")]
    public int IdAlert { get; set; }

    [JsonPropertyName("idUser")]
    public long IdUser { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("module")]
    public string? Module { get; set; }

    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }

    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("actionData")]
    public string? ActionData { get; set; }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            if (diff.TotalMinutes < 1) return "ahora";
            if (diff.TotalMinutes < 60) return $"hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"hace {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"hace {(int)diff.TotalDays}d";
            return CreatedAt.ToString("dd/MM/yyyy");
        }
    }

    public string ModuleIcon => Module?.ToUpperInvariant() switch
    {
        "ORDER" or "SALES" => "📦",
        "BACKOFFICE" or "BAC" => "🏢",
        "INCIDENT" or "INCIDENCIA" => "⚠️",
        "SUPERVISOR" => "👁️",
        "AUDIT" or "AUDITORIA" => "📋",
        "TRANSFER" or "TRANSFERENCIA" => "🔄",
        _ => "🔔"
    };
}
