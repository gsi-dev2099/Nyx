using System;

namespace CRM.ApiHub.Application.DTOs;

public class BulkTransferRequestDto
{
    public long[] OrderIds { get; set; } = Array.Empty<long>();
    public long BackofficeUserId { get; set; }
    public string? Comment { get; set; }
}
