namespace CRM.ApiHub.Domain.DTOs;

public class BulkTransferResultDto
{
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public System.Collections.Generic.List<long> FailedOrderIds { get; set; } = new();
}