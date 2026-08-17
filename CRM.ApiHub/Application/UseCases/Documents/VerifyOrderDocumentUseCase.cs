using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Documents;

public class VerifyOrderDocumentUseCase
{
    private readonly IOrderDocumentRepository _repository;

    public VerifyOrderDocumentUseCase(IOrderDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(long idDoc, string status, string? notes, long verifiedBy, CancellationToken ct = default)
    {
        return await _repository.UpdateVerificationAsync(idDoc, status, notes, verifiedBy, ct);
    }
}
