using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class AuditChecklistViewModel
{
    public long IdChecklist { get; set; }
    public long IdCmpg { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<AuditChecklistItemViewModel> Items { get; set; } = new();
}

public class AuditChecklistItemViewModel
{
    public long IdItem { get; set; }
    public short OrderIndex { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExpectedText { get; set; }
    public string? CrmFieldKey { get; set; }
    public bool IsCritical { get; set; }

    // Evaluated state properties for the UI
    public string Result { get; set; } = string.Empty; // "OK", "KO", "N/A"
    public string Timestamp { get; set; } = string.Empty;
    public string Observation { get; set; } = string.Empty;
}

public interface IAuditService
{
    Task<AuditChecklistViewModel?> GetChecklistAsync(long idCmpg);
    Task<long> CreateAuditAsync(long idOrder, long idChecklist);
    Task<bool> SaveItemAsync(long idAudit, long idItem, string result, string? observation, string? audioTimestamp);
    Task<bool> CloseAuditAsync(long idAudit, string status);
}
