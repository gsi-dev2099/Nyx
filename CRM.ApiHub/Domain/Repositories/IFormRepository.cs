using CRM.ApiHub.Domain.Entities;

namespace CRM.ApiHub.Domain.Repositories;

public interface IFormRepository
{
    Task<IEnumerable<FormTemplate>> GetTemplatesByCampaignStageAsync(long idCmpg, long idStage);
    Task<IEnumerable<FormField>> GetFieldsByTemplateAsync(long idForm);
    Task SaveOrderDataAsync(long idOrder, long idForm, IEnumerable<OrderData> fields);
    Task SeedDefaultFormsAsync();
}