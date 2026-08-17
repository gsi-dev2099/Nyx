using System.Collections.Generic;

namespace CRM.WebFrontend.Client.Models;

public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class OrderStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class FormFieldDto
{
    public long IdFld { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? ValidationRegex { get; set; }
    public string? Options { get; set; } // JSON string
    public int OrderIndex { get; set; }
    public string? ValidationType { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? GroupName { get; set; }
    public long? DependsOnField { get; set; }
    public string? DependsOnValue { get; set; }
    
    // For tracking the current value in the UI
    public string Value { get; set; } = string.Empty;
}

public class FormSchemaDto
{
    public long IdForm { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<FormFieldDto> Fields { get; set; } = new();
}

/// <summary>
/// Matches the backend entity FormTemplate exactly.
/// The backend returns: IdForm, IdCmpg, Name, Description, Register, IsActive, 
/// IdStage, FormOrder, IsPrimary, AllowsPartial, MinCompletionPct, UpdatedAt
/// </summary>
public class FormTemplateDto
{
    public long IdForm { get; set; }
    public long IdCmpg { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long IdStage { get; set; }
    public short FormOrder { get; set; }
    public bool IsPrimary { get; set; }
    public bool AllowsPartial { get; set; }
    public short MinCompletionPct { get; set; }
    public bool IsActive { get; set; }
}
