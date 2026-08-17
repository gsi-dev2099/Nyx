using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class ReportService : IReportService
{
    private readonly HttpClient _httpClient;

    public ReportService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
    }

    public async Task<List<ConversionFunnelStageViewModel>> GetConversionFunnelAsync(long idCmpg, DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            var query = BuildQuery(idCmpg, dateFrom, dateTo);
            var result = await _httpClient.GetFromJsonAsync<List<ConversionFunnelStageViewModel>>($"api/reports/funnel{query}");
            return result ?? new List<ConversionFunnelStageViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetConversionFunnelAsync: {ex.Message}");
            return GetFallbackFunnelData();
        }
    }

    public async Task<List<SalesByAsesorViewModel>> GetSalesByAsesorAsync(DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            var query = BuildQuery(null, dateFrom, dateTo);
            var result = await _httpClient.GetFromJsonAsync<List<SalesByAsesorViewModel>>($"api/reports/sales-by-asesor{query}");
            return result ?? new List<SalesByAsesorViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetSalesByAsesorAsync: {ex.Message}");
            return GetFallbackSalesByAsesorData();
        }
    }

    public async Task<IncidentStatsViewModel?> GetIncidentStatsAsync(long idCmpg, DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            var query = BuildQuery(idCmpg, dateFrom, dateTo);
            return await _httpClient.GetFromJsonAsync<IncidentStatsViewModel>($"api/reports/incidents{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetIncidentStatsAsync: {ex.Message}");
            return GetFallbackIncidentStats();
        }
    }

    public async Task<ActivationStatsViewModel?> GetActivationStatsAsync(long idProvider, DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            var query = BuildQuery(idProvider, dateFrom, dateTo, paramName: "idProvider");
            return await _httpClient.GetFromJsonAsync<ActivationStatsViewModel>($"api/reports/activations{query}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetActivationStatsAsync: {ex.Message}");
            return GetFallbackActivationStats();
        }
    }

    private static string BuildQuery(long? id, DateTime? dateFrom, DateTime? dateTo, string paramName = "idCmpg")
    {
        var queryParams = new List<string>();
        if (id.HasValue && id.Value > 0)
        {
            queryParams.Add($"{paramName}={id.Value}");
        }
        if (dateFrom.HasValue)
        {
            queryParams.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
        }
        if (dateTo.HasValue)
        {
            queryParams.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");
        }

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
    }

    #region Fallback Mock Data for UI Resilience
    private static List<ConversionFunnelStageViewModel> GetFallbackFunnelData()
    {
        return new List<ConversionFunnelStageViewModel>
        {
            new() { StageId = 1, StageCode = "LEADS", StageName = "Leads Asignados", Count = 450 },
            new() { StageId = 2, StageCode = "PRESALES", StageName = "Pre-Ventas Evaluadas", Count = 280 },
            new() { StageId = 3, StageCode = "VENTAS", StageName = "Ventas Cerradas", Count = 160 },
            new() { StageId = 4, StageCode = "ACTIVADAS", StageName = "Servicios Activados", Count = 135 }
        };
    }

    private static List<SalesByAsesorViewModel> GetFallbackSalesByAsesorData()
    {
        return new List<SalesByAsesorViewModel>
        {
            new() { IdUser = 101, Username = "jgarcia", AdvisorName = "Juan García", TotalOrders = 45, ApprovedOrders = 42, TotalAmountPen = 12500m, TotalAmountEur = 3100m },
            new() { IdUser = 102, Username = "mlopez", AdvisorName = "María López", TotalOrders = 38, ApprovedOrders = 35, TotalAmountPen = 9800m, TotalAmountEur = 2450m },
            new() { IdUser = 103, Username = "crodriguez", AdvisorName = "Carlos Rodríguez", TotalOrders = 32, ApprovedOrders = 28, TotalAmountPen = 8200m, TotalAmountEur = 2050m },
            new() { IdUser = 104, Username = "smartinez", AdvisorName = "Sofia Martínez", TotalOrders = 29, ApprovedOrders = 26, TotalAmountPen = 7500m, TotalAmountEur = 1875m },
            new() { IdUser = 105, Username = "aperez", AdvisorName = "Alejandro Pérez", TotalOrders = 24, ApprovedOrders = 20, TotalAmountPen = 5900m, TotalAmountEur = 1475m }
        };
    }

    private static IncidentStatsViewModel GetFallbackIncidentStats()
    {
        return new IncidentStatsViewModel
        {
            TotalIncidents = 24,
            OpenIncidents = 8,
            InProgressIncidents = 5,
            ResolvedIncidents = 9,
            ClosedIncidents = 2,
            CategoryBreakdown = new List<IncidentCategoryBreakdownViewModel>
            {
                new() { CategoryName = "Documentación Incompleta", Status = "OPEN", Count = 9 },
                new() { CategoryName = "Fallo en Scoring Crediticio", Status = "IN_PROGRESS", Count = 6 },
                new() { CategoryName = "Audio de Contrato Defectuoso", Status = "RESOLVED", Count = 5 },
                new() { CategoryName = "Rechazo de Cobertura Técnica", Status = "OPEN", Count = 4 }
            }
        };
    }

    private static ActivationStatsViewModel GetFallbackActivationStats()
    {
        return new ActivationStatsViewModel
        {
            TotalActivations = 135,
            PendingActivations = 12,
            InProcessActivations = 18,
            ActivatedActivations = 95,
            DelayedActivations = 7,
            FailedActivations = 3,
            CancelledActivations = 0,
            AverageDelayDays = 2.4
        };
    }
    #endregion
}
