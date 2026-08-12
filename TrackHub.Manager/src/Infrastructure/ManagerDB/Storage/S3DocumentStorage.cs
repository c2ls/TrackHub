using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Storage;

/// <summary>
/// S3-compatible object storage — AWS S3, MinIO, or any S3 API. Downloads use a
/// short-lived presigned GET URL (the download endpoint 302-redirects the browser straight to S3, so the
/// bytes never pass through the app). Works with any <see cref="IAmazonS3"/> (IAM role, keys, or S3-compatible
/// service URL); the client is configured in <c>DependencyInjection</c> from the <c>DocumentStorage:S3</c> section.
/// </summary>
public sealed class S3DocumentStorage(IAmazonS3 s3, string bucketName, TimeSpan maxPresignedTtl, ILogger<S3DocumentStorage> logger) : IDocumentStorage
{
    public const string ProviderName = "S3";

    public string Provider => ProviderName;

    public async Task<StoredObject> SaveAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        // Buffer once to get a seekable body with a known length + SHA-256 (the S3 SDK wants Content-Length;
        // document sizes are bounded by the platform ceilings, so buffering is acceptable here).
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var length = buffer.Length;
        buffer.Position = 0;
        var hash = await SHA256.HashDataAsync(buffer, cancellationToken);
        buffer.Position = 0;

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = storageKey,
            InputStream = buffer,
            ContentType = contentType,
            AutoCloseStream = false,
        }, cancellationToken);

        logger.LogDebug("Stored document object {Key} ({Size} bytes) in S3 bucket {Bucket}.", storageKey, length, bucketName);
        return new StoredObject(length, Convert.ToHexString(hash));
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        var response = await s3.GetObjectAsync(bucketName, storageKey, cancellationToken);
        return response.ResponseStream;
    }

    public Task<Uri?> TryCreatePresignedDownloadUrlAsync(string storageKey, string fileName, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var expiry = ttl > maxPresignedTtl ? maxPresignedTtl : ttl;
        var url = s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Expires = DateTimeOffset.UtcNow.Add(expiry).UtcDateTime,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = $"attachment; filename=\"{SanitizeFileName(fileName)}\"",
            },
        });

        return Task.FromResult<Uri?>(new Uri(url));
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
        => await s3.DeleteObjectAsync(bucketName, storageKey, cancellationToken); // idempotent: succeeds if absent

    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        try
        {
            await s3.GetObjectMetadataAsync(bucketName, storageKey, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static string SanitizeFileName(string fileName)
        => fileName.Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}
