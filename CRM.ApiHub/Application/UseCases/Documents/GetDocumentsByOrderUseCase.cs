using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Documents;

public class GetDocumentsByOrderUseCase
{
    private readonly IOrderDocumentRepository _repository;

    public GetDocumentsByOrderUseCase(IOrderDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderDocument>> ExecuteAsync(long idOrder, CancellationToken ct = default)
    {
        return await _repository.GetByOrderAsync(idOrder, ct);
    }
}
