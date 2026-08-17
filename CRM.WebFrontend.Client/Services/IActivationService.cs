using CRM.WebFrontend.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Client.Services;

public interface IActivationService
{
    Task<List<ProviderDto>> GetProvidersAsync();
    Task<List<ProductActivationTrackingDto>> GetPendingActivationsAsync(long idProvider);
    Task<bool> UpdateActivationAsync(long idItem, UpdateActivationRequestDto dto);
}
