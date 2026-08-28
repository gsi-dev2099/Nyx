using System;

namespace CRM.ApiHub.Application.DTOs;

public class SalesOrderCreateDto
{
    public long IdLead { get; set; }
    public long IdCmpg { get; set; }
    public long IdUser { get; set; }
    public long? OwnerUserId { get; set; }
    public long? CustodyUserId { get; set; }
    public long? IdStatus { get; set; } = 1;
    public long? IdSubstatus { get; set; }
    public string? CurrencyCode { get; set; } = "EUR";
    public string? CommissionCurrency { get; set; } = "PEN";
    public string? Status { get; set; } = "BORRADOR";
    public DateTime? SalesDate { get; set; }
    public int TotalProducts { get; set; } = 0;
    public decimal? TotalValue { get; set; }
    public bool IsAlternate { get; set; } = false;
    public decimal DiscountPercentage { get; set; } = 0;
}
