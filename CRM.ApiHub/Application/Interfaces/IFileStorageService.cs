using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.ApiHub.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Sube un archivo al almacenamiento de objetos (MinIO / S3).
    /// </summary>
    Task<string> UploadFileAsync(
        string bucketName, 
        string objectKey, 
        Stream content, 
        string contentType, 
        CancellationToken ct = default);

    /// <summary>
    /// Descarga un archivo en streaming desde el almacenamiento de objetos.
    /// </summary>
    Task<Stream> DownloadFileAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken ct = default);

    /// <summary>
    /// Genera una URL prefirmada temporal para descarga segura directa.
    /// </summary>
    Task<string> GetPresignedUrlAsync(
        string bucketName, 
        string objectKey, 
        TimeSpan expiry, 
        CancellationToken ct = default);

    /// <summary>
    /// Elimina un archivo del almacenamiento de objetos.
    /// </summary>
    Task<bool> DeleteFileAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken ct = default);

    /// <summary>
    /// Asegura que el bucket exista, creándolo si es necesario.
    /// </summary>
    Task EnsureBucketExistsAsync(
        string bucketName, 
        CancellationToken ct = default);
}
