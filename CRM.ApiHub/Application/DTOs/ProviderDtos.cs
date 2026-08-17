namespace CRM.ApiHub.Application.DTOs;

public class ProviderSyncLogRequestDto
{
    public long IdOrder { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public class UpdateOrderProviderStatusRequestDto
{
    public long IdOrder { get; set; }
    public string ProviderStatusCode { get; set; } = string.Empty;
}

public class UpdateOrderProviderStatusResponseDto
{
    public bool Success { get; set; }
    public bool Mapped { get; set; }
    public long? InternalStatusBefore { get; set; }
    public long? InternalStatusAfter { get; set; }
    public bool IncidentCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}
