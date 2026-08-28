using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using CRM.ApiHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.ApiHub.Infrastructure.Services;

public class MinioStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<MinioStorageService> _logger;
    private readonly string _defaultBucket;
    private readonly string _localStorageFallbackPath;
    private readonly bool _useLocalStorageFallback;

    public MinioStorageService(IConfiguration config, ILogger<MinioStorageService> logger)
    {
        _logger = logger;
        _defaultBucket = config["MinioSettings:BucketName"] ?? "nyx-crm-documents";
        _localStorageFallbackPath = Path.Combine(Path.GetTempPath(), "NyxStorage", "Documents");

        var endpoint = config["MinioSettings:Endpoint"] ?? "http://crm_minio:9000";
        var accessKey = config["MinioSettings:AccessKey"] ?? config["MINIO_ROOT_USER"] ?? "nyx_admin";
        var secretKey = config["MinioSettings:SecretKey"] ?? config["MINIO_ROOT_PASSWORD"] ?? "NyxMinio$$2026StorageKey!";

        try
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true, // Requerido para MinIO
                UseHttp = endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase),
                AuthenticationRegion = "us-east-1",
                Timeout = TimeSpan.FromSeconds(15)
            };

            _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
            _useLocalStorageFallback = false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo inicializar AmazonS3Client para MinIO en {Endpoint}. Se activará fallback a almacenamiento local.", endpoint);
            _useLocalStorageFallback = true;
            _s3Client = null!;
        }
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct = default)
    {
        if (_useLocalStorageFallback || _s3Client == null) return;

        var targetBucket = string.IsNullOrWhiteSpace(bucketName) ? _defaultBucket : bucketName;
        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = targetBucket,
                UseClientRegion = true
            };
            await _s3Client.PutBucketAsync(putBucketRequest, ct);
            _logger.LogInformation("Bucket MinIO '{BucketName}' verificado/creado exitosamente.", targetBucket);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketAlreadyOwnedByYou" || ex.ErrorCode == "BucketAlreadyExists" || ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // El bucket ya existe
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Aviso al asegurar bucket MinIO '{BucketName}': {Message}", targetBucket, ex.Message);
        }
    }

    public async Task<string> UploadFileAsync(
        string bucketName, 
        string objectKey, 
        Stream content, 
        string contentType, 
        CancellationToken ct = default)
    {
        var targetBucket = string.IsNullOrWhiteSpace(bucketName) ? _defaultBucket : bucketName;

        if (!_useLocalStorageFallback && _s3Client != null)
        {
            try
            {
                await EnsureBucketExistsAsync(targetBucket, ct);

                if (content.CanSeek && content.Position != 0)
                {
                    content.Position = 0;
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = targetBucket,
                    Key = objectKey,
                    InputStream = content,
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                    AutoCloseStream = false
                };

                var response = await _s3Client.PutObjectAsync(putRequest, ct);
                _logger.LogInformation("Archivo subido exitosamente a MinIO: bucket={Bucket}, key={Key}, status={Status}", targetBucket, objectKey, response.HttpStatusCode);
                return objectKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al subir archivo a MinIO ({Key}): {Message}. Intentando fallback local temporal.", objectKey, ex.Message);
            }
        }

        // Fallback a almacenamiento local temporal seguro
        try
        {
            if (!Directory.Exists(_localStorageFallbackPath))
            {
                Directory.CreateDirectory(_localStorageFallbackPath);
            }

            var sanitizedKey = objectKey.Replace('/', '_').Replace('\\', '_');
            var localFilePath = Path.Combine(_localStorageFallbackPath, sanitizedKey);

            using (var destStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                if (content.CanSeek) content.Position = 0;
                await content.CopyToAsync(destStream, ct);
            }

            return objectKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al guardar en fallback local");
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        var targetBucket = string.IsNullOrWhiteSpace(bucketName) ? _defaultBucket : bucketName;

        if (!_useLocalStorageFallback && _s3Client != null)
        {
            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = targetBucket,
                    Key = objectKey
                };

                var response = await _s3Client.GetObjectAsync(getRequest, ct);
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, ct);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo descargar de MinIO ({Key}). Buscando en fallback local.", objectKey);
            }
        }

        // Buscar en fallback local
        var sanitizedKey = objectKey.Replace('/', '_').Replace('\\', '_');
        var localFilePath = Path.Combine(_localStorageFallbackPath, sanitizedKey);

        if (File.Exists(localFilePath))
        {
            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fileStream.CopyToAsync(memoryStream, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }

        throw new FileNotFoundException($"El archivo con clave '{objectKey}' no fue encontrado en MinIO ni en almacenamiento local.");
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string objectKey, CancellationToken ct = default)
    {
        var targetBucket = string.IsNullOrWhiteSpace(bucketName) ? _defaultBucket : bucketName;
        bool deletedFromMinio = false;

        if (!_useLocalStorageFallback && _s3Client != null)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = targetBucket,
                    Key = objectKey
                };
                await _s3Client.DeleteObjectAsync(deleteRequest, ct);
                deletedFromMinio = true;
                _logger.LogInformation("Archivo eliminado de MinIO: bucket={Bucket}, key={Key}", targetBucket, objectKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al eliminar archivo de MinIO ({Key}).", objectKey);
            }
        }

        var sanitizedKey = objectKey.Replace('/', '_').Replace('\\', '_');
        var localFilePath = Path.Combine(_localStorageFallbackPath, sanitizedKey);
        if (File.Exists(localFilePath))
        {
            try
            {
                File.Delete(localFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al eliminar archivo en fallback local: {Path}", localFilePath);
            }
        }

        return deletedFromMinio || !File.Exists(localFilePath);
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucketName, 
        string objectKey, 
        TimeSpan expiry, 
        CancellationToken ct = default)
    {
        var targetBucket = string.IsNullOrWhiteSpace(bucketName) ? _defaultBucket : bucketName;

        if (!_useLocalStorageFallback && _s3Client != null)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = targetBucket,
                    Key = objectKey,
                    Expires = DateTime.UtcNow.Add(expiry),
                    Verb = HttpVerb.GET
                };

                return await Task.FromResult(_s3Client.GetPreSignedURL(request));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo generar Presigned URL para MinIO ({Key}).", objectKey);
            }
        }

        return $"/api/documents/stream?key={Uri.EscapeDataString(objectKey)}";
    }
}
