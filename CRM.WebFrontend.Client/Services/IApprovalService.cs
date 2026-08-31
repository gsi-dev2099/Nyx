using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Client.Services;

public interface IApprovalService
{
    // The ApiHub proxy endpoint for fetching pending isn't implemented? Wait.
    // The ApprovalController in ApiHub only has GET /api/approvals/{id}, POST /api/orders/{id}/approvals, PATCH /api/approvals/{id}
    // But it does NOT have GET /api/approvals/pending.
    // If it doesn't, we will hit ApprovalEngine directly if we can, or we need to add it to ApiHub.
    // Wait, let's implement the service first.
    Task<IEnumerable<ApprovalRequestDto>> GetPendingApprovalsAsync(long approverId, string approverRole);
    Task<bool> DecideRequestAsync(long requestId, ApprovalPatchDto dto);
}

public class ApprovalRequestDto
{
    public long IdRequest { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string EntityContext { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public long RequestedBy { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

public class ApprovalPatchDto
{
    public string Status { get; set; } = null!; // APPROVED or REJECTED
    public string Comments { get; set; } = null!;
    public long AuthorizedBy { get; set; }
}
