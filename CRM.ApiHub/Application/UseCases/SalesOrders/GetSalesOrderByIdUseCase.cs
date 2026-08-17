using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.SalesOrders;

public class GetSalesOrderByIdUseCase
{
    private readonly ISalesOrderRepository _salesOrderRepository;

    public GetSalesOrderByIdUseCase(ISalesOrderRepository salesOrderRepository)
    {
        _salesOrderRepository = salesOrderRepository;
    }

    public async Task<SalesOrder?> ExecuteAsync(long idOrder, CancellationToken ct = default)
    {
        return await _salesOrderRepository.GetByIdAsync(idOrder, ct);
    }
}
