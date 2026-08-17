using System.Collections.Generic;

namespace CRM.ApiHub.Application.DTOs;

public class SupervisorStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalValue { get; set; }
    public int TotalProducts { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
}
