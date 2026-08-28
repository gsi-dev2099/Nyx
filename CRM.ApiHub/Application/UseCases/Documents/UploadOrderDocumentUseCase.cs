using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace CRM.ApiHub.Application.UseCases.Documents;

public class UploadOrderDocumentUseCase
{
    private readonly IOrderDocumentRepository _repository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFlowEngineClient _flowEngineClient;
    private readonly string _bucketName;

    public UploadOrderDocumentUseCase(
        IOrderDocumentRepository repository, 
        IFileStorageService fileStorageService,
        IFlowEngineClient flowEngineClient,
        IConfiguration config)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
        _flowEngineClient = flowEngineClient;
        _bucketName = config["MinioSettings:BucketName"] ?? "nyx-crm-documents";
    }

    public async Task<OrderDocument> ExecuteAsync(
        long idOrder,
        string documentType,
        string originalFileName,
        string mimeType,
        long fileSizeKb,
        Stream fileStream,
        long uploadedBy,
        CancellationToken ct = default)
    {
        // 1. Generar clave de objeto única estructurada por orden
        var sanitizedFileName = Path.GetFileName(originalFileName);
        var objectKey = $"orders/{idOrder}/{Guid.NewGuid():N}_{sanitizedFileName}";

        // 2. Subir archivo a MinIO / S3
        var storedKey = await _fileStorageService.UploadFileAsync(
            bucketName: _bucketName,
            objectKey: objectKey,
            content: fileStream,
            contentType: mimeType,
            ct: ct
        );

        // 3. Crear registro en la tabla order_document
        var doc = new OrderDocument
        {
            IdOrder = idOrder,
            DocumentType = documentType,
            FileName = originalFileName,
            FilePath = storedKey, // Ahora almacena la clave de objeto MinIO/S3
            FileSizeKb = (int)fileSizeKb,
            MimeType = mimeType,
            VerificationStatus = "PENDING",
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            IsActive = true
        };

        var docId = await _repository.UploadAsync(doc, ct);
        doc.IdDocument = docId;

        // 4. Emitir Facts automáticos a Nyx.FlowEngine para auto-evaluar checkpoints
        try
        {
            var facts = new System.Collections.Generic.Dictionary<string, object>
            {
                ["doc_uploaded"] = true,
                ["last_doc_type"] = documentType
            };

            var upperType = documentType.ToUpperInvariant();
            if (upperType.Contains("DNI")) facts["doc_dni_uploaded"] = true;
            if (upperType.Contains("CONTRATO")) facts["doc_contrato_uploaded"] = true;
            if (upperType.Contains("NOMINA")) facts["doc_nomina_uploaded"] = true;
            if (upperType.Contains("RECIBO") || upperType.Contains("FACTURA")) facts["doc_factura_uploaded"] = true;

            var factsJson = System.Text.Json.JsonSerializer.Serialize(facts);
            await _flowEngineClient.SetEntityFactsAsync("order", idOrder, factsJson, uploadedBy);
        }
        catch
        {
            // Logging / fallback seguro: no romper subida de documento si el motor de flujos tiene latencia
        }

        return doc;
    }
}
