using System.Collections.Generic;

namespace CRM.ApiHub.Application.DTOs;

public class ConversionFunnelStageDto
{
    public long StageId { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public string StageName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SalesByAsesorDto
{
    public long IdUser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AdvisorName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public decimal TotalAmountPen { get; set; }
    public decimal TotalAmountEur { get; set; }
}

public class IncidentCategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class IncidentStatsDto
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int InProgressIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int ClosedIncidents { get; set; }
    public List<IncidentCategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
}

public class ActivationStatsDto
{
    public int TotalActivations { get; set; }
    public int PendingActivations { get; set; }
    public int InProcessActivations { get; set; }
    public int ActivatedActivations { get; set; }
    public int DelayedActivations { get; set; }
    public int FailedActivations { get; set; }
    public int CancelledActivations { get; set; }
    public double AverageDelayDays { get; set; }
}
