using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IPreSaleRepository
{
    Task<IEnumerable<LeadPreSale>> GetByUserAsync(int userId);
    Task<LeadPreSale?> GetByIdAsync(int idPresale);
    Task<int> CreateAsync(LeadPreSale preSale);
    Task<bool> UpdateAsync(LeadPreSale preSale, long userId);
    Task<bool> AddCallLogAsync(int idPresale, string callLog, long userId = 1, IEnumerable<string>? completedSteps = null, string? result = null);
    Task<bool> AssignAsync(int idPresale, int toUserId, string context);
    Task<bool> AssignMultiAsync(int idPresale, long? advisor1Id, long? advisor2Id, long? advisor3Id, string context);
    Task<bool> AssignStepWithHandshakeAsync(int idPresale, long toUserId, int callStep, long fromUserId, string context);
    Task<bool> AcceptAssignmentAsync(int idPresale, long userId);
    Task<bool> RejectAssignmentAsync(int idPresale, long userId, string reason);
    Task<bool> CancelAssignmentAsync(int idPresale, long ownerUserId);
    Task<bool> RevertAssignmentAsync(int idPresale, long actorUserId, string context);
    Task<bool> UpdateTargetOperatorAsync(int idPresale, string targetOperator);
    Task<bool> DiscardPreSaleAsync(int idPresale, string reason, long userId);
    Task<IEnumerable<PreSaleAssignmentHistoryDto>> GetAssignmentHistoryAsync(int idPresale);
    Task<long> ConvertAsync(int idPresale, dynamic paramsData);
}