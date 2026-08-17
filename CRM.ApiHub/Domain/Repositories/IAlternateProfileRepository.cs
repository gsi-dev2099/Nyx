using CRM.ApiHub.Domain.Entities;
using System.Threading.Tasks; 

namespace CRM.ApiHub.Domain.Repositories
{
    public interface IAlternateProfileRepository
    {
        Task<long> CreateAsync(AlternateProfile entity);
        Task<AlternateProfile?> GetByOrderIdAsync(long idOrder);
    }
}