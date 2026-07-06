using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IPreSaleRepository
{
    Task<IEnumerable<LeadPreSale>> GetByUserAsync(int userId);
    Task<int> CreateAsync(LeadPreSale preSale);
    Task<bool> AddCallLogAsync(int idPresale, string callLog);
    Task<bool> AssignAsync(int idPresale, int toUserId, string context);
    Task<bool> ConvertAsync(int idPresale, dynamic paramsData);
}