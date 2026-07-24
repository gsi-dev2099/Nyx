using CRM.WebFrontend.Client.Models;

namespace CRM.WebFrontend.Client.Services;

public interface IMaintenanceService
{
    // Statuses
    Task<List<OrderStatusMaintenanceDto>> GetAllStatusesAsync();
    Task<bool> ToggleStatusAsync(int id, bool isActive);

    // Products
    Task<List<ProductMaintenanceDto>> GetAllProductsAsync();
    Task<bool> ToggleProductAsync(int id, bool isActive);

    // Incident Catalog
    Task<List<IncidentCatalogMaintenanceDto>> GetAllIncidentCatalogAsync();
    Task<bool> CreateIncidentCatalogAsync(CreateIncidentCatalogDto dto);
    Task<bool> UpdateIncidentCatalogAsync(long id, UpdateIncidentCatalogDto dto);
    Task<bool> DeleteIncidentCatalogAsync(long id);

    // Exchange Rates
    Task<List<ExchangeRateMaintenanceDto>> GetExchangeRatesAsync();
    Task<bool> CreateExchangeRateAsync(CreateExchangeRateDto dto);

    // Campaigns (aux)
    Task<List<CampaignSimpleDto>> GetCampaignsAsync();
}
