using System;
using System.Text.Json.Serialization;

namespace CRM.WebFrontend.Client.Models;

public class ProviderDto
{
    [JsonPropertyName("idProvider")]
    public long IdProvider { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("integrationType")]
    public string IntegrationType { get; set; } = string.Empty;
}

public class ProductActivationTrackingDto
{
    [JsonPropertyName("idTracking")]
    public long IdTracking { get; set; }
    
    [JsonPropertyName("idOrder")]
    public long IdOrder { get; set; }
    
    [JsonPropertyName("idOrderItem")]
    public long IdOrderItem { get; set; }
    
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;
    
    [JsonPropertyName("idProvider")]
    public long? IdProvider { get; set; }
    
    [JsonPropertyName("providerRef")]
    public string? ProviderRef { get; set; }
    
    [JsonPropertyName("orderLoadedAt")]
    public DateTime? OrderLoadedAt { get; set; }
    
    [JsonPropertyName("expectedActivationDate")]
    public DateTime? ExpectedActivationDate { get; set; }
    
    [JsonPropertyName("actualActivationDate")]
    public DateTime? ActualActivationDate { get; set; }
    
    [JsonPropertyName("activationStatus")]
    public string ActivationStatus { get; set; } = string.Empty;
    
    [JsonPropertyName("delayDays")]
    public int? DelayDays { get; set; }
    
    [JsonPropertyName("delayReason")]
    public string? DelayReason { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class UpdateActivationRequestDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }
}
