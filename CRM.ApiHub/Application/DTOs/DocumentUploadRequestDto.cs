using Microsoft.AspNetCore.Http;

namespace CRM.ApiHub.Application.DTOs;

public class DocumentUploadRequestDto
{
    public IFormFile File { get; set; } = null!;
    public string DocumentType { get; set; } = string.Empty;
}
