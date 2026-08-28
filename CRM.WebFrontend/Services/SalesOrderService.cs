using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CRM.WebFrontend.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(IHttpClientFactory httpClientFactory, ILogger<SalesOrderService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _logger = logger;
    }

    public async Task<IEnumerable<SalesOrderViewModel>> GetOrdersAsync(long? userId = null, long? statusId = null, long? campaignId = null)
    {
        try
        {
            var url = $"api/orders?userId={userId}&statusId={statusId}&campaignId={campaignId}";
            var pagedResult = await _httpClient.GetFromJsonAsync<CRM.WebFrontend.Client.Models.PagedResult<SalesOrderViewModel>>(url);
            return pagedResult?.Items ?? new List<SalesOrderViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes en GetOrdersAsync (userId={UserId}, statusId={StatusId})", userId, statusId);
            return new List<SalesOrderViewModel>();
        }
    }

    public async Task<SalesOrderViewModel?> GetOrderByIdAsync(long id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SalesOrderViewModel>($"api/orders/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden #{Id} en GetOrderByIdAsync", id);
            return null;
        }
    }

    public async Task<IEnumerable<TimelineItemViewModel>> GetOrderHistoryAsync(long id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TimelineItemViewModel>>($"api/orders/{id}/history");
            return result ?? new List<TimelineItemViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de la orden #{Id}", id);
            return new List<TimelineItemViewModel>();
        }
    }

    public async Task<IEnumerable<FormTemplateViewModel>> GetTemplatesAsync(long idCmpg, long idStage)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<FormTemplateViewModel>>($"api/forms/campaign/{idCmpg}/stage/{idStage}");
            return result ?? new List<FormTemplateViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener plantillas de formulario (campaign={Cmpg}, stage={Stage})", idCmpg, idStage);
            return new List<FormTemplateViewModel>();
        }
    }

    public async Task<IEnumerable<FormFieldViewModel>> GetFieldsAsync(long idForm)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<FormFieldViewModel>>($"api/forms/{idForm}/fields");
            return result ?? new List<FormFieldViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener campos del formulario #{FormId}", idForm);
            return new List<FormFieldViewModel>();
        }
    }

    public async Task<IEnumerable<OrderDataViewModel>> GetOrderDataAsync(long idOrder)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<OrderDataViewModel>>($"api/forms/order/{idOrder}/data");
            return result ?? new List<OrderDataViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos guardados de la orden #{OrderId}", idOrder);
            return new List<OrderDataViewModel>();
        }
    }

    public async Task<bool> SaveOrderDataAsync(long idOrder, long idForm, IEnumerable<OrderDataViewModel> data)
    {
        var (success, _) = await SaveOrderDataWithDetailsAsync(idOrder, idForm, data);
        return success;
    }

    public async Task<(bool Success, string Message)> SaveOrderDataWithDetailsAsync(long idOrder, long idForm, IEnumerable<OrderDataViewModel> data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/forms/order/{idOrder}/template/{idForm}", data);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Datos guardados exitosamente.");
            }

            var content = await response.Content.ReadAsStringAsync();
            string errorMessage = $"Error de servidor ({response.StatusCode})";
            try
            {
                using var jsonDoc = JsonDocument.Parse(content);
                if (jsonDoc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    errorMessage = msgProp.GetString() ?? errorMessage;
                }
                else if (jsonDoc.RootElement.TryGetProperty("title", out var titleProp))
                {
                    errorMessage = titleProp.GetString() ?? errorMessage;
                }
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(content)) errorMessage = content;
            }

            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar datos de formulario de la orden #{OrderId}", idOrder);
            return (false, ex.Message);
        }
    }

    public async Task<AlternateProfileViewModel?> GetAlternateProfileAsync(long idOrder)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AlternateProfileViewModel>($"api/orders/{idOrder}/alternate-profile");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil alterno de la orden #{OrderId}", idOrder);
            return null;
        }
    }

    public async Task<bool> SaveAlternateProfileAsync(long idOrder, AlternateProfileViewModel profile)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/orders/{idOrder}/alternate-profile", new
            {
                profile.AlternateType,
                profile.AlternateData,
                profile.OriginalData,
                profile.Reason
            });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar perfil alterno de la orden #{OrderId}", idOrder);
            return false;
        }
    }

    public async Task<IEnumerable<OrderDocumentViewModel>> GetDocumentsByOrderAsync(long idOrder)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<OrderDocumentViewModel>>($"api/orders/{idOrder}/documents");
            return result ?? new List<OrderDocumentViewModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos de la orden #{OrderId}", idOrder);
            return new List<OrderDocumentViewModel>();
        }
    }

    public async Task<bool> UploadDocumentAsync(long idOrder, string documentType, string fileName, byte[] fileBytes, string contentType)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            content.Add(fileContent, "File", fileName);
            content.Add(new StringContent(documentType), "DocumentType");

            var response = await _httpClient.PostAsync($"api/orders/{idOrder}/documents", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir documento para la orden #{OrderId}", idOrder);
            return false;
        }
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)?> DownloadDocumentAsync(long idDocument)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/documents/{idDocument}/download");
            if (!response.IsSuccessStatusCode) return null;

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar 
                        ?? response.Content.Headers.ContentDisposition?.FileName 
                        ?? $"documento_{idDocument}";
            
            fileName = fileName.Trim('"');
            return (bytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar documento #{DocId}", idDocument);
            return null;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(long idOrder, long toStatusId, long? toSubstatusId = null, string? comment = null)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/orders/{idOrder}/status", new
            {
                ToStatusId = toStatusId,
                ToSubstatusId = toSubstatusId,
                Comment = comment
            });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de la orden #{OrderId} a {StatusId}", idOrder, toStatusId);
            return false;
        }
    }

    public async Task<bool> CheckPermissionAsync(string permissionKey, long statusId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PermissionCheckResponse>($"api/auth/check-permission?permissionKey={permissionKey}&statusId={statusId}");
            return response?.HasPermission ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permisos para key '{PermissionKey}' y status #{StatusId}", permissionKey, statusId);
            return false;
        }
    }

    private class PermissionCheckResponse
    {
        public bool HasPermission { get; set; }
    }
}
