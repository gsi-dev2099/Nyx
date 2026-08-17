using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace CRM.ApiHub.Application.UseCases.Documents;

public class UploadOrderDocumentUseCase
{
    private readonly IOrderDocumentRepository _repository;
    private readonly IConfiguration _config;

    public UploadOrderDocumentUseCase(IOrderDocumentRepository repository, IConfiguration config)
    {
        _repository = repository;
        _config = config;
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
        // 1. Obtener la ruta configurada de manera portable
        var storagePathConfig = _config["DocumentSettings:StoragePath"] ?? "Storage/Documents";
        var absoluteStoragePath = Path.IsPathRooted(storagePathConfig)
            ? storagePathConfig
            : Path.Combine(Directory.GetCurrentDirectory(), storagePathConfig);

        // 2. Asegurar que el directorio exista
        if (!Directory.Exists(absoluteStoragePath))
        {
            Directory.CreateDirectory(absoluteStoragePath);
        }

        // 3. Generar nombre de archivo único para evitar sobreescritura y guardar en disco
        var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
        var physicalFilePath = Path.Combine(absoluteStoragePath, uniqueFileName);

        using (var destStream = new FileStream(physicalFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destStream, ct);
        }

        // 4. Crear registro en la tabla order_document
        var doc = new OrderDocument
        {
            IdOrder = idOrder,
            DocumentType = documentType,
            FileName = originalFileName,
            FilePath = physicalFilePath,
            FileSizeKb = (int)fileSizeKb,
            MimeType = mimeType,
            VerificationStatus = "PENDING",
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            IsActive = true
        };

        var docId = await _repository.UploadAsync(doc, ct);
        doc.IdDocument = docId;

        return doc;
    }
}
