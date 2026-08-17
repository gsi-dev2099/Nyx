using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IProviderRepository
{
    Task<IEnumerable<ProviderCatalog>> GetCatalogAsync(CancellationToken ct = default);
    Task<IEnumerable<ProviderStatusMapping>> GetStatusMappingAsync(long idProvider, CancellationToken ct = default);
    Task<bool> LogSyncAsync(long idProvider, long? idOrder, string? statusCode, string result, long? internalStatusBefore = null, long? internalStatusAfter = null, bool? success = null, CancellationToken ct = default);
    Task<bool> UpdateOrderStatusAsync(long idOrder, long statusId, long? substatusId, long actorUserId, CancellationToken ct = default);
    Task<bool> CreateOrderIncidentAsync(long idOrder, long incidentId, string customName, string customDescription, CancellationToken ct = default);
    Task<long?> GetOrderCurrentStatusAsync(long idOrder, CancellationToken ct = default);
}
