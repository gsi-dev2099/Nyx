namespace CRM.WebFrontend.Models;

public record SalesQueueItem(
    long IdOrder, 
    string CustomerName, 
    string Priority, 
    DateTime SubmittedAt, 
    string Status
);

public class BackofficeValidationFieldDto
{
    public string FieldKey { get; set; } = "";
    public string Label { get; set; } = "";
    public string Category { get; set; } = "IDENTIDAD"; // IDENTIDAD, COBERTURA, CREDITO, DOCUMENTO
    public string FormValue { get; set; } = "";
    public string SystemVerifiedValue { get; set; } = "";
    public string Status { get; set; } = "VALID"; // VALID, OBSERVED, REJECTED
    public string RejectReason { get; set; } = "";

    public BackofficeValidationFieldDto() { }

    public BackofficeValidationFieldDto(string fieldKey, string label, string category, string formValue, string systemVerifiedValue, string status = "VALID", string rejectReason = "")
    {
        FieldKey = fieldKey;
        Label = label;
        Category = category;
        FormValue = formValue;
        SystemVerifiedValue = systemVerifiedValue;
        Status = status;
        RejectReason = rejectReason;
    }
}

public record DocumentVerificationData(
    long IdOrder, 
    string DocumentImageUrl, 
    string FormFullName, 
    string FormDocumentNumber,
    string ScannedFullName, 
    string ScannedDocumentNumber,
    List<BackofficeValidationFieldDto>? ValidationFields = null
);

public record OpenIncidentItem(
    long IdIncident, 
    string Description, 
    string Status, 
    DateTime CreatedAt
);

public record BackofficeIncidentDto(
    long IdOrderIncident,
    long IdOrder,
    long IdIncident,
    string? CustomName,
    string? CustomDescription,
    string? IncidentStatus,
    string? AssignedToRole,
    DateTime? DueAt,
    DateTime Register,
    short? Priority = null
);

public record KbArticleSuggestionDto(
    long IdArticle,
    string Title,
    string Summary,
    string Slug
);