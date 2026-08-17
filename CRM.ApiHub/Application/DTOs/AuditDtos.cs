using System.Collections.Generic;

namespace CRM.ApiHub.Application.DTOs;

public class AuditChecklistResponseDto
{
    public long IdChecklist { get; set; }
    public long IdCmpg { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<AuditChecklistItemDto> Items { get; set; } = new();
}

public class AuditChecklistItemDto
{
    public long IdItem { get; set; }
    public short OrderIndex { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExpectedText { get; set; }
    public string? CrmFieldKey { get; set; }
    public bool IsCritical { get; set; }
}

public class CreateAuditRequestDto
{
    public long IdChecklist { get; set; }
}

public class SaveAuditItemRequestDto
{
    public long IdItem { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public string? AudioTimestamp { get; set; }
}

public class CloseAuditRequestDto
{
    public string Status { get; set; } = string.Empty;
}
