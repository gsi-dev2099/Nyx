using Dapper;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities; 
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.DTOs;

namespace CRM.ApiHub.Infrastructure.Persistence
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ApprovalRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<long> RegisterApprovalAsync(ApprovalDto approval)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            // 1. Obtener el id_user de la orden para usarlo como requested_by
            const string getOrderUserSql = "SELECT id_user FROM sales_service.sales_order WHERE id_order = @IdOrder;";
            var requestedBy = await connection.ExecuteScalarAsync<long?>(getOrderUserSql, new { IdOrder = approval.IdOrder });

            // Si no se encuentra la orden en sales_order, usamos authorized_by como fallback
            var finalRequestedBy = requestedBy ?? approval.AuthorizedBy;

            string statusValue = approval.IsApproved ? "APPROVED" : "REJECTED";
            string typeValue = "MANUAL";

            var sql = @"INSERT INTO sales_service.sales_order_approval 
                        (id_order, approved_by, approval_reason, rejection_reason, status, register, approval_type, requested_by, resolved_at) 
                        VALUES (@IdOrder, @AuthorizedBy, @ApprovalReason, @RejectionReason, @Status, NOW(), @Type, @RequestedBy, NOW()) 
                        RETURNING id_approval;";
            
            return await connection.ExecuteScalarAsync<long>(sql, new 
            {
                approval.IdOrder,
                approval.AuthorizedBy,
                ApprovalReason = approval.IsApproved ? approval.Comments : null,
                RejectionReason = !approval.IsApproved ? approval.Comments : null,
                Status = statusValue,
                Type = typeValue,
                RequestedBy = finalRequestedBy
            });
        }

        public async Task<long> CreateApprovalRequestAsync(long idOrder, string reason, long requestedBy)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var sql = @"INSERT INTO sales_service.sales_order_approval 
                        (id_order, approval_type, status, requested_by, approval_reason, register) 
                        VALUES (@IdOrder, 'MANUAL', 'PENDING', @RequestedBy, @Reason, NOW()) 
                        RETURNING id_approval;";
            return await connection.ExecuteScalarAsync<long>(sql, new { IdOrder = idOrder, RequestedBy = requestedBy, Reason = reason });
        }

        public async Task<bool> UpdateApprovalAsync(long idApproval, string status, string comments, long authorizedBy)
        {
            var sql = @"UPDATE sales_service.sales_order_approval 
                        SET status = @Status, 
                            approved_by = @AuthorizedBy,
                            approval_reason = CASE WHEN @Status = 'APPROVED' THEN @Comments ELSE approval_reason END,
                            rejection_reason = CASE WHEN @Status = 'REJECTED' THEN @Comments ELSE rejection_reason END,
                            resolved_at = NOW() 
                        WHERE id_approval = @IdApproval;";
            
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                IdApproval = idApproval, 
                Status = status, 
                Comments = comments, 
                AuthorizedBy = authorizedBy 
            });
            return rowsAffected > 0;
        }

        public async Task<ApprovalDto?> GetApprovalByIdAsync(long idApproval)
        {
            var sql = @"SELECT id_order AS ""IdOrder"", approved_by AS ""AuthorizedBy"", status AS ""Status"", 
                               approval_reason AS ""ApprovalReason"", rejection_reason AS ""RejectionReason""
                        FROM sales_service.sales_order_approval 
                        WHERE id_approval = @IdApproval;";
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { IdApproval = idApproval });
            if (result == null) return null;
            
            return new ApprovalDto
            {
                IdOrder = result.IdOrder,
                AuthorizedBy = result.AuthorizedBy,
                IsApproved = result.Status == "APPROVED",
                Comments = result.Status == "APPROVED" ? (result.ApprovalReason ?? "") : (result.RejectionReason ?? "")
            };
        }
    }
}