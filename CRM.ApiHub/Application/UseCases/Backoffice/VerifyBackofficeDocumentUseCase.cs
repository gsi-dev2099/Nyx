using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Backoffice;

public class VerifyBackofficeDocumentUseCase
{
    private readonly IBackofficeRepository _repository;

    public VerifyBackofficeDocumentUseCase(IBackofficeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(
        long idDoc,
        string status,
        string? notes,
        long verifiedBy,
        CancellationToken ct = default)
    {
        return await _repository.VerifyDocumentAsync(idDoc, status, notes, verifiedBy, ct);
    }
}
