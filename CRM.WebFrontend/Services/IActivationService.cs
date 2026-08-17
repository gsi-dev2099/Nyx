using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class ProductActivationViewModel
{
    [System.Text.Json.Serialization.JsonPropertyName("idTracking")]
    public long IdItem { get; set; }
    public long IdOrder { get; set; }
    public long IdProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public long IdProvider { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("activationStatus")]
    public string Status { get; set; } = "PENDING"; // PENDING, IN_PROCESS, ACTIVATED, DELAYED, FAILED, CANCELLED
    
    [System.Text.Json.Serialization.JsonPropertyName("orderLoadedAt")]
    public DateTime RequestedDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("expectedActivationDate")]
    public DateTime? PromisedDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("actualActivationDate")]
    public DateTime? ActualDate { get; set; }
    public string? Notes { get; set; }
    
    public int DaysElapsed => (int)(DateTime.UtcNow - RequestedDate).TotalDays;
    public bool IsDelayed => Status == "DELAYED" || (Status != "ACTIVATED" && PromisedDate.HasValue && DateTime.UtcNow > PromisedDate.Value);
}

public interface IActivationService
{
    Task<List<ProductActivationViewModel>> GetPendingActivationsAsync(long idProvider);
    Task<List<ProductActivationViewModel>> GetDelayedActivationsAsync();
    Task<List<ProductActivationViewModel>> GetActivationsByOrderAsync(long idOrder);
    Task<bool> UpdateActivationAsync(long idItem, string status, DateTime? actualDate, string? notes = null);
}
