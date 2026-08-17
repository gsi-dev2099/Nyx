using System;
using System.Collections.Generic;

namespace CRM.WebFrontend.Client.Models;

// Matches CRM.ApiHub.Domain.Entities.Currency exactly
public class CurrencyDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public short DecimalPlaces { get; set; }
    public bool IsActive { get; set; }
    public bool IsBase { get; set; }
    public bool IsLocal { get; set; }
}

// Matches CRM.ApiHub.Application.DTOs.ConvertAmountResponseDto exactly
public class ConvertAmountResponseDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime Date { get; set; }
}

// Matches CRM.ApiHub.Application.DTOs.SettlementResponseDto exactly
public class SettlementResponseDto
{
    public long IdSettlement { get; set; }
    public long IdUser { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime SettlementDate { get; set; }
    public decimal TotalEur { get; set; }
    public decimal TotalPen { get; set; }
    public long? ExchangeRateId { get; set; }
    public decimal? ExchangeRateApplied { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public string Status { get; set; } = "DRAFT";
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Matches CRM.ApiHub.Application.DTOs.SettlementItemResponseDto exactly
public class SettlementItemResponseDto
{
    public long IdItem { get; set; }
    public long IdSettlement { get; set; }
    public long IdOrder { get; set; }
    public long? IdOrderItem { get; set; }
    public decimal CommissionEur { get; set; }
    public decimal CommissionPen { get; set; }
    public string? ProductName { get; set; }
    public string? Notes { get; set; }
}

// Wrapper for GET /api/commissions/settlements/{id} response
public class SettlementDetailResponseDto
{
    public SettlementResponseDto Settlement { get; set; } = new();
    public List<SettlementItemResponseDto> Items { get; set; } = new();
}
