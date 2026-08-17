using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Dapper;

namespace CRM.ApiHub.Infrastructure.Persistence;

public class OrderDocumentRepository : IOrderDocumentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderDocumentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderDocument>> GetByOrderAsync(long idOrder, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.order_document WHERE id_order = @IdOrder AND is_active = true ORDER BY uploaded_at DESC;";

        return await connection.QueryAsync<OrderDocument>(
            new CommandDefinition(sql, new { IdOrder = idOrder }, cancellationToken: ct)
        );
    }

    public async Task<long> UploadAsync(OrderDocument document, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO sales_service.order_document (
                id_order, document_type, file_name, file_path, file_size_kb,
                mime_type, verified_by, verified_at, verification_status,
                verification_notes, uploaded_by, uploaded_at, is_active
            )
            VALUES (
                @IdOrder, @DocumentType, @FileName, @FilePath, @FileSizeKb,
                @MimeType, @VerifiedBy, @VerifiedAt, @VerificationStatus,
                @VerificationNotes, @UploadedBy, @UploadedAt, @IsActive
            )
            RETURNING id_document;";

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, document, cancellationToken: ct)
        );
    }

    public async Task<bool> UpdateVerificationAsync(long idDoc, string status, string? notes, long verifiedBy, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE sales_service.order_document 
            SET verification_status = @Status, 
                verification_notes = @Notes, 
                verified_by = @VerifiedBy, 
                verified_at = NOW()
            WHERE id_document = @IdDoc;";

        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Status = status, Notes = notes, VerifiedBy = verifiedBy, IdDoc = idDoc }, cancellationToken: ct)
        );

        return rowsAffected > 0;
    }

    public async Task<OrderDocument?> GetByIdAsync(long idDoc, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM sales_service.order_document WHERE id_document = @IdDoc;";

        return await connection.QueryFirstOrDefaultAsync<OrderDocument>(
            new CommandDefinition(sql, new { IdDoc = idDoc }, cancellationToken: ct)
        );
    }
}