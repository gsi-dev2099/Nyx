using CRM.WebFrontend.Models;

namespace CRM.WebFrontend.Services;

public interface IBackofficeService
{
    Task<IEnumerable<SalesQueueItem>> GetPendingQueueAsync();
    Task<DocumentVerificationData?> GetVerificationDataAsync(long idOrder);
    Task<IEnumerable<OpenIncidentItem>> GetOpenIncidentsAsync(long idOrder);
    Task SubmitVerificationDecisionAsync(long idOrder, string decision, string? observation);

    // BACKOFFICE Incidents Screen Methods
    Task<IEnumerable<BackofficeIncidentDto>> GetIncidentsAsync(string? role = "BACKOFFICE", string? status = "OPEN");
    Task<IEnumerable<KbArticleSuggestionDto>> GetKbSuggestionsAsync(long idIncident);
    Task AddIncidentResponseAsync(long idIncident, string responseText, string responseType, long respondedBy);
    Task ResolveIncidentAsync(long idIncident, string notes, long resolvedBy);
}