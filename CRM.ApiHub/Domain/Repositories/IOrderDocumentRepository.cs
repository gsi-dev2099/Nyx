using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IOrderDocumentRepository
{
    Task<IEnumerable<OrderDocument>> GetByOrderAsync(long idOrder, CancellationToken ct = default);
    Task<long> UploadAsync(OrderDocument document, CancellationToken ct = default);
    Task<bool> UpdateVerificationAsync(long idDoc, string status, string? notes, long verifiedBy, CancellationToken ct = default);
    Task<OrderDocument?> GetByIdAsync(long idDoc, CancellationToken ct = default);
}
