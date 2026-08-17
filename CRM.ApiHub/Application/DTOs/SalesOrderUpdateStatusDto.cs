namespace CRM.ApiHub.Application.DTOs;

public class SalesOrderUpdateStatusDto
{
    public long ToStatusId { get; set; }
    public long? ToSubstatusId { get; set; }
    public string? Comment { get; set; }
    public bool IsBulk { get; set; } = false;
}
