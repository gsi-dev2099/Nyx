using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Documents;

public class GetDocumentByIdUseCase
{
    private readonly IOrderDocumentRepository _repository;

    public GetDocumentByIdUseCase(IOrderDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderDocument?> ExecuteAsync(long idDoc, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(idDoc, ct);
    }
}
