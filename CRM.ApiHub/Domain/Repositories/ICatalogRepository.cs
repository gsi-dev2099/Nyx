using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface ICatalogRepository
{
    Task<IEnumerable<OrderStatus>> GetOrderStatusesAsync();
    Task<IEnumerable<OrderSubstatus>> GetOrderSubstatusesAsync(int idStatus);
    Task<IEnumerable<Product>> GetProductsAsync(int idCmpg);
    
    Task<IEnumerable<Currency>> GetCurrenciesAsync();
}