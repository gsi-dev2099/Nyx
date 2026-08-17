using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IActivationRepository
{
    Task<IEnumerable<ProductActivationTracking>> GetPendingActivationsAsync(long idProvider, CancellationToken ct = default);
    Task<IEnumerable<ProductActivationTracking>> GetByOrderAsync(long idOrder, CancellationToken ct = default);
    Task<bool> UpdateActivationAsync(long idTracking, string status, DateTime? actualDate, CancellationToken ct = default);
    Task<IEnumerable<ProductActivationTracking>> GetDelayedAsync(CancellationToken ct = default);
    Task<ProductActivationTracking?> GetByIdAsync(long idTracking, CancellationToken ct = default);
}
