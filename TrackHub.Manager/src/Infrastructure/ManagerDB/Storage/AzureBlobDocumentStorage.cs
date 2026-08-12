using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Storage;

/// <summary>
/// Azure Blob Storage provider. Downloads use a short-lived read-only SAS URL (the
/// download endpoint 302-redirects the browser straight to the blob). SAS generation requires the client
/// to hold a shared key (connection string); when it doesn't (e.g. managed identity), this returns null and
/// the endpoint streams the bytes instead. Configured from the <c>DocumentStorage:AzureBlob</c> section.
/// </summary>
public sealed class AzureBlobDocumentStorage(BlobContainerClient container, TimeSpan maxSasTtl, ILogger<AzureBlobDocumentStorage> logger) : IDocumentStorage
{
    public const string ProviderName = "AzureBlob";

    public string Provider => ProviderName;

    public async Task<StoredObject> SaveAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        // Buffer once to get a known length + SHA-256 (document sizes are bounded, so buffering is fine).
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var length = buffer.Length;
        buffer.Position = 0;
        var hash = await SHA256.HashDataAsync(buffer, cancellationToken);
        buffer.Position = 0;

        await container.GetBlobClient(storageKey).UploadAsync(buffer, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
        }, cancellationToken);

        logger.LogDebug("Stored document object {Key} ({Size} bytes) in Azure container {Container}.", storageKey, length, container.Name);
        return new StoredObject(length, Convert.ToHexString(hash));
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
        => await container.GetBlobClient(storageKey).OpenReadAsync(cancellationToken: cancellationToken);

    public Task<Uri?> TryCreatePresignedDownloadUrlAsync(string storageKey, string fileName, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var blob = container.GetBlobClient(storageKey);

        // Shared-key SAS only. With a token credential (managed identity) CanGenerateSasUri is false → stream.
        if (!blob.CanGenerateSasUri)
        {
            return Task.FromResult<Uri?>(null);
        }

        var expiry = ttl > maxSasTtl ? maxSasTtl : ttl;
        var sas = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry))
        {
            BlobContainerName = container.Name,
            BlobName = storageKey,
            ContentDisposition = $"attachment; filename=\"{SanitizeFileName(fileName)}\"",
        };

        return Task.FromResult<Uri?>(blob.GenerateSasUri(sas));
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
        => await container.GetBlobClient(storageKey).DeleteIfExistsAsync(cancellationToken: cancellationToken); // idempotent

    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
        => await container.GetBlobClient(storageKey).ExistsAsync(cancellationToken);

    private static string SanitizeFileName(string fileName)
        => fileName.Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
