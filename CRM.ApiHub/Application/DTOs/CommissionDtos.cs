using System;

namespace CRM.ApiHub.Application.DTOs;

public class ConvertAmountRequestDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
}

public class ConvertAmountResponseDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime Date { get; set; }
}

public class CreateSettlementRequestDto
{
    public long UserId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class AddSettlementItemsRequestDto
{
    public long[] OrderIds { get; set; } = Array.Empty<long>();
}

public class UpdateSettlementRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

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
