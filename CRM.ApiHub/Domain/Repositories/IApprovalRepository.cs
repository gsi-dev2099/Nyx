using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.DTOs;

namespace CRM.ApiHub.Domain.Repositories
{
    public interface IApprovalRepository
    {
        Task<long> RegisterApprovalAsync(ApprovalDto approval);
        Task<long> CreateApprovalRequestAsync(long idOrder, string reason, long requestedBy);
        Task<bool> UpdateApprovalAsync(long idApproval, string status, string comments, long authorizedBy);
        Task<ApprovalDto?> GetApprovalByIdAsync(long idApproval);
    }
}