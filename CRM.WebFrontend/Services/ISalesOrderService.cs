using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM.WebFrontend.Services;

public class SalesOrderViewModel
{
    public long IdOrder { get; set; }
    public long IdLead { get; set; }
    public long IdCmpg { get; set; }
    public long IdUser { get; set; }
    public long? OwnerUserId { get; set; }
    public long? CustodyUserId { get; set; }
    public long? IdStatus { get; set; }
    public long? IdSubstatus { get; set; }
    public string? Status { get; set; }
    public DateTime SalesDate { get; set; }
    public int TotalProducts { get; set; }
    public decimal? TotalValue { get; set; }
    public bool IsAlternate { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime Register { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class FormTemplateViewModel
{
    public long IdForm { get; set; }
    public long IdCmpg { get; set; }
    public long IdStage { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class FormFieldViewModel
{
    public long IdFld { get; set; }
    public long IdForm { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? ValidationRegex { get; set; }
    public string? Options { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public string? ValidationType { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? GroupName { get; set; }
    public long? DependsOnField { get; set; }
    public string? DependsOnValue { get; set; }
}

public class OrderDataViewModel
{
    public long IdOrddata { get; set; }
    public long IdOrder { get; set; }
    public long IdFld { get; set; }
    public string? ValueText { get; set; }
    public string? ValueJson { get; set; }
    public string? FieldStatus { get; set; }
    public long? ValidatedBy { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public short Version { get; set; }
    public long SourceFormId { get; set; }
}

public class AlternateProfileViewModel
{
    public long IdAlternate { get; set; }
    public long IdOrder { get; set; }
    public string AlternateType { get; set; } = string.Empty;
    public string AlternateData { get; set; } = string.Empty;
    public string OriginalData { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public long CreatedBy { get; set; }
    public long? AuthorizedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TimelineItemViewModel
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
}

public class OrderDocumentViewModel
{
    public long IdDocument { get; set; }
    public long IdOrder { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
}

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderViewModel>> GetOrdersAsync(long? userId = null, long? statusId = null, long? campaignId = null);
    Task<SalesOrderViewModel?> GetOrderByIdAsync(long id);
    Task<IEnumerable<TimelineItemViewModel>> GetOrderHistoryAsync(long id);
    Task<IEnumerable<FormTemplateViewModel>> GetTemplatesAsync(long idCmpg, long idStage);
    Task<IEnumerable<FormFieldViewModel>> GetFieldsAsync(long idForm);
    Task<IEnumerable<OrderDataViewModel>> GetOrderDataAsync(long idOrder);
    Task<bool> SaveOrderDataAsync(long idOrder, long idForm, IEnumerable<OrderDataViewModel> data);
    Task<(bool Success, string Message)> SaveOrderDataWithDetailsAsync(long idOrder, long idForm, IEnumerable<OrderDataViewModel> data);
    Task<AlternateProfileViewModel?> GetAlternateProfileAsync(long idOrder);
    Task<bool> SaveAlternateProfileAsync(long idOrder, AlternateProfileViewModel profile);
    Task<IEnumerable<OrderDocumentViewModel>> GetDocumentsByOrderAsync(long idOrder);
    Task<bool> UploadDocumentAsync(long idOrder, string documentType, string fileName, byte[] fileBytes, string contentType);
    Task<(byte[] Bytes, string ContentType, string FileName)?> DownloadDocumentAsync(long idDocument);
    Task<bool> UpdateOrderStatusAsync(long idOrder, long toStatusId, long? toSubstatusId = null, string? comment = null);
    Task<bool> CheckPermissionAsync(string permissionKey, long statusId);
}
