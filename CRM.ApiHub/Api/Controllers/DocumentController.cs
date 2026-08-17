using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.ApiHub.Api.Controllers;

[Authorize]
[ApiController]
public class DocumentController : ControllerBase
{
    private readonly GetDocumentsByOrderUseCase _getDocumentsByOrderUseCase;
    private readonly UploadOrderDocumentUseCase _uploadOrderDocumentUseCase;
    private readonly VerifyOrderDocumentUseCase _verifyOrderDocumentUseCase;
    private readonly GetDocumentByIdUseCase _getDocumentByIdUseCase;

    public DocumentController(
        GetDocumentsByOrderUseCase getDocumentsByOrderUseCase,
        UploadOrderDocumentUseCase uploadOrderDocumentUseCase,
        VerifyOrderDocumentUseCase verifyOrderDocumentUseCase,
        GetDocumentByIdUseCase getDocumentByIdUseCase)
    {
        _getDocumentsByOrderUseCase = getDocumentsByOrderUseCase;
        _uploadOrderDocumentUseCase = uploadOrderDocumentUseCase;
        _verifyOrderDocumentUseCase = verifyOrderDocumentUseCase;
        _getDocumentByIdUseCase = getDocumentByIdUseCase;
    }

    [HttpGet("api/orders/{id:long}/documents")]
    public async Task<IActionResult> GetDocumentsByOrder(long id, CancellationToken ct)
    {
        try
        {
            var docs = await _getDocumentsByOrderUseCase.ExecuteAsync(id, ct);
            return Ok(docs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los documentos de la orden.", details = ex.Message });
        }
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

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long uploadedBy))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (uploadedBy == -999) uploadedBy = 101;
        else if (uploadedBy == -1000) uploadedBy = 237;
        else if (uploadedBy == -998) uploadedBy = 9;

        try
        {
            var originalFileName = dto.File.FileName;
            var mimeType = dto.File.ContentType;
            var fileSizeKb = dto.File.Length / 1024;

            using (var fileStream = dto.File.OpenReadStream())
            {
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
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al subir el documento.", details = ex.Message });
        }
    }

    [HttpPatch("api/documents/{id:long}/verify")]
    public async Task<IActionResult> VerifyDocument(long id, [FromBody] DocumentVerifyRequestDto dto, CancellationToken ct)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new { message = "El estado de verificación es requerido." });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long verifiedBy))
        {
            return Unauthorized(new { message = "Usuario no autorizado." });
        }

        if (verifiedBy == -999) verifiedBy = 101;
        else if (verifiedBy == -1000) verifiedBy = 237;
        else if (verifiedBy == -998) verifiedBy = 9;

        try
        {
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la verificación del documento.", details = ex.Message });
        }
    }

    [HttpGet("api/documents/{id:long}/download")]
    public async Task<IActionResult> DownloadDocument(long id, CancellationToken ct)
    {
        try
        {
            var doc = await _getDocumentByIdUseCase.ExecuteAsync(id, ct);
            if (doc == null)
            {
                return NotFound(new { message = "Documento no encontrado." });
            }

            if (doc.FilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                doc.FilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(doc.FilePath);
            }

            if (!System.IO.File.Exists(doc.FilePath))
            {
                return NotFound(new { message = "El archivo físico no existe en el servidor." });
            }

            var mimeType = doc.MimeType ?? "application/octet-stream";
            return PhysicalFile(doc.FilePath, mimeType, System.IO.Path.GetFileName(doc.FilePath));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al descargar el documento.", details = ex.Message });
        }
    }
}
