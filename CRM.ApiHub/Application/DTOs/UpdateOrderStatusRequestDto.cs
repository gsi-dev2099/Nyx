namespace CRM.ApiHub.Application.DTOs;

public class UpdateOrderStatusRequestDto
{
    public long ToStatusId { get; set; }
    public long? ToSubstatusId { get; set; }
    public string? Comment { get; set; }
}
