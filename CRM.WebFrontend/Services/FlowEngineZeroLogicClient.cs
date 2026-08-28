using System.Net.Http.Json;
using Nyx.FlowEngine.Domain.Entities;

namespace CRM.WebFrontend.Services;

public interface IFlowEngineZeroLogicClient
{
    Task<UiContextDto?> GetUiContextByEntityAsync(string entityType, long entityId, long actorId);
    Task<UiContextDto?> GetUiContextByIdAsync(long instanceId, long actorId);
    Task<ExecuteActionResultDto?> ExecuteActionAsync(long instanceId, ExecuteActionRequest request);
    Task<bool> ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId);
    Task<UiContextDto?> StartFlowInstanceAsync(string cycleCode, string entityType, long entityId, long actorId);
}

public class FlowEngineZeroLogicClient : IFlowEngineZeroLogicClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlowEngineZeroLogicClient> _logger;

    public FlowEngineZeroLogicClient(IHttpClientFactory httpClientFactory, ILogger<FlowEngineZeroLogicClient> logger)
    {
        // Conecta con el BackendApi (o directamente con el endpoint de ciclo del motor de flujo)
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _logger = logger;
    }

    public async Task<UiContextDto?> GetUiContextByEntityAsync(string entityType, long entityId, long actorId)
    {
        try
        {
            // Intenta obtener la instancia por entidad
            var instanceDetail = await _httpClient.GetFromJsonAsync<CycleInstanceDetailDto>(
                $"api/cycles/instances/entity/{entityType}/{entityId}");

            if (instanceDetail != null && instanceDetail.IdInstance > 0)
            {
                return await GetUiContextByIdAsync(instanceDetail.IdInstance, actorId);
            }

            // Si no existe, inicia una instancia por defecto y retorna el contexto
            return await StartFlowInstanceAsync("FLOW_TELCO_SALES_001", entityType, entityId, actorId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener el contexto UI para {EntityType} #{EntityId}.", entityType, entityId);
            return null;
        }
    }

    public async Task<UiContextDto?> GetUiContextByIdAsync(long instanceId, long actorId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UiContextDto>(
                $"api/cycles/instances/{instanceId}/ui-context?actorId={actorId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al consultar ui-context para la instancia #{InstanceId}", instanceId);
            return null;
        }
    }

    public async Task<ExecuteActionResultDto?> ExecuteActionAsync(long instanceId, ExecuteActionRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/cycles/instances/{instanceId}/execute-action", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ExecuteActionResultDto>();
            }
            var errJson = await response.Content.ReadFromJsonAsync<ExecuteActionResultDto>();
            return errJson ?? new ExecuteActionResultDto { Success = false, Message = $"Error HTTP {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar la acción {ActionCode} para la instancia #{InstanceId}", request.ActionCode, instanceId);
            return new ExecuteActionResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> ToggleStepProgressAsync(long cpInstanceId, long stepId, bool isCompleted, long actorId)
    {
        try
        {
            var payload = new { isCompleted, actorId };
            var response = await _httpClient.PostAsJsonAsync($"api/cycles/checkpoints/instances/{cpInstanceId}/steps/{stepId}/toggle", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al alternar paso #{StepId} en CP #{CpInstanceId}", stepId, cpInstanceId);
            return false;
        }
    }

    public async Task<UiContextDto?> StartFlowInstanceAsync(string cycleCode, string entityType, long entityId, long actorId)
    {
        try
        {
            var payload = new { cycleCode, entityType, entityId, actorId };
            var response = await _httpClient.PostAsJsonAsync("api/cycles/instances/start", payload);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<CycleInstanceDetailDto>();
                if (created != null)
                {
                    return await GetUiContextByIdAsync(created.IdInstance, actorId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar instancia de flujo para {EntityType} #{EntityId}", entityType, entityId);
        }
        return null;
    }
}
