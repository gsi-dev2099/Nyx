using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface ICampaignRepository
{
    Task<IEnumerable<Campaign>> GetAllActiveAsync();
    Task<Campaign?> GetByIdAsync(int id);
}