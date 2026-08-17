using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IOrderDataRepository
{
    Task<IEnumerable<OrderData>> GetByOrderAsync(long idOrder);
    Task UpdateFieldStatusAsync(long idData, string status, long validatedBy);
}