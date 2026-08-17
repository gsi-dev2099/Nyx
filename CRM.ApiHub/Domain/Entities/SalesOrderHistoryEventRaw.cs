using System;

namespace CRM.ApiHub.Domain.Entities;

public class SalesOrderHistoryEventRaw
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
}
