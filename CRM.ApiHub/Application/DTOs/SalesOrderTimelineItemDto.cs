using System;
using System.Text.Json;

namespace CRM.ApiHub.Application.DTOs;

public class SalesOrderTimelineItemDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
    public JsonElement? Details { get; set; }
}
