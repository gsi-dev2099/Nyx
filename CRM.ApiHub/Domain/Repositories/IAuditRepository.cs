using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IAuditRepository
{
    Task<AuditChecklistTemplate?> GetChecklistTemplateByCampaignAsync(long idCmpg, CancellationToken ct = default);
    Task<IEnumerable<AuditChecklistItem>> GetChecklistItemsAsync(long idChecklist, CancellationToken ct = default);
    Task<long> CreateAuditAsync(long idOrder, long idChecklist, long auditedBy, CancellationToken ct = default);
    Task<bool> SaveItemAsync(long idAudit, long idItem, string result, string? obs, string? audioTimestamp, CancellationToken ct = default);
    Task<bool> CloseAuditAsync(long idAudit, string status, CancellationToken ct = default);
    Task<SalesOrderAudit?> GetAuditByIdAsync(long idAudit, CancellationToken ct = default);
}
