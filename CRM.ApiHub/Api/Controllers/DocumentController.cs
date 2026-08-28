using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Application.UseCases.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class DocumentController : ControllerBase
{
    private readonly GetDocumentsByOrderUseCase _getDocumentsByOrderUseCase;
    private readonly UploadOrderDocumentUseCase _uploadOrderDocumentUseCase;
    private readonly VerifyOrderDocumentUseCase _verifyOrderDocumentUseCase;
    private readonly GetDocumentByIdUseCase _getDocumentByIdUseCase;
    private readonly IFileStorageService _fileStorageService;
    private readonly string _bucketName;

    public DocumentController(
        GetDocumentsByOrderUseCase getDocumentsByOrderUseCase,
        UploadOrderDocumentUseCase uploadOrderDocumentUseCase,
        VerifyOrderDocumentUseCase verifyOrderDocumentUseCase,
        GetDocumentByIdUseCase getDocumentByIdUseCase,
        IFileStorageService fileStorageService,
        IConfiguration config)
    {
        _getDocumentsByOrderUseCase = getDocumentsByOrderUseCase;
        _uploadOrderDocumentUseCase = uploadOrderDocumentUseCase;
        _verifyOrderDocumentUseCase = verifyOrderDocumentUseCase;
        _getDocumentByIdUseCase = getDocumentByIdUseCase;
        _fileStorageService = fileStorageService;
        _bucketName = config["MinioSettings:BucketName"] ?? "nyx-crm-documents";
    }

    [HttpGet("api/orders/{id:long}/documents")]
    public async Task<IActionResult> GetDocumentsByOrder(long id, CancellationToken ct)
    {
        var docs = await _getDocumentsByOrderUseCase.ExecuteAsync(id, ct);
        return Ok(docs);
    }

    [HttpPost("api/orders/{id:long}/documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(
        long id,
        [FromForm] DocumentUploadRequestDto dto,
        CancellationToken ct)
    {
        if (dto == null || dto.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { message = "No se ha proporcionado ningún archivo o el archivo está vacío." });
        }

        if (string.IsNullOrWhiteSpace(dto.DocumentType))
        {
            return BadRequest(new { message = "El tipo de documento es requerido." });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long uploadedBy))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        var originalFileName = dto.File.FileName;
        var mimeType = string.IsNullOrWhiteSpace(dto.File.ContentType) ? "application/octet-stream" : dto.File.ContentType;
        var fileSizeKb = dto.File.Length / 1024;

        using var fileStream = dto.File.OpenReadStream();
        var createdDoc = await _uploadOrderDocumentUseCase.ExecuteAsync(
            idOrder: id,
            documentType: dto.DocumentType,
            originalFileName: originalFileName,
            mimeType: mimeType,
            fileSizeKb: fileSizeKb,
            fileStream: fileStream,
            uploadedBy: uploadedBy,
            ct: ct
        );

        return Created($"api/documents/{createdDoc.IdDocument}", createdDoc);
    }

    [HttpPatch("api/documents/{id:long}/verify")]
    public async Task<IActionResult> VerifyDocument(long id, [FromBody] DocumentVerifyRequestDto dto, CancellationToken ct)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "El estado de verificación es requerido." });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("id_user");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long verifiedBy))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        var success = await _verifyOrderDocumentUseCase.ExecuteAsync(
            idDoc: id,
            status: dto.Status,
            notes: dto.Notes,
            verifiedBy: verifiedBy,
            ct: ct
        );

        if (!success)
        {
            return NotFound(new { message = "Documento no encontrado." });
        }

        return Ok(new { message = "Verificación de documento actualizada correctamente." });
    }

    [HttpGet("api/documents/{id:long}/download")]
    public async Task<IActionResult> DownloadDocument(long id, CancellationToken ct)
    {
        var doc = await _getDocumentByIdUseCase.ExecuteAsync(id, ct);
        if (doc == null)
        {
            return NotFound(new { message = "Documento no encontrado." });
        }

        var stream = await _fileStorageService.DownloadFileAsync(_bucketName, doc.FilePath, ct);
        var mimeType = doc.MimeType ?? "application/octet-stream";
        var fileName = doc.FileName ?? "documento";

        return File(stream, mimeType, fileName);
    }

    [HttpGet("api/documents/{id:long}/presigned-url")]
    public async Task<IActionResult> GetPresignedUrl(long id, [FromQuery] int expiryMinutes = 15, CancellationToken ct = default)
    {
        var doc = await _getDocumentByIdUseCase.ExecuteAsync(id, ct);
        if (doc == null)
        {
            return NotFound(new { message = "Documento no encontrado." });
        }

        var presignedUrl = await _fileStorageService.GetPresignedUrlAsync(
            bucketName: _bucketName,
            objectKey: doc.FilePath,
            expiry: TimeSpan.FromMinutes(Math.Clamp(expiryMinutes, 1, 60)),
            ct: ct
        );

        return Ok(new { url = presignedUrl, expiresInMinutes = expiryMinutes });
    }
}
