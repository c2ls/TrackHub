using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Storage;

/// <summary>
/// Development storage provider: persists document bytes on the local filesystem
/// under a configured root. Streams downloads (no presigned URL). Storage keys are server-generated
/// (<c>{accountId}/{ownerType}/{documentId}/{version}</c>) and map to nested directories.
/// </summary>
public sealed class LocalFileSystemDocumentStorage(string rootPath, ILogger<LocalFileSystemDocumentStorage> logger) : IDocumentStorage
{
    public const string ProviderName = "LocalFileSystem";

    public string Provider => ProviderName;

    public async Task<StoredObject> SaveAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var path = ResolvePath(storageKey);

        long size;
        byte[] hash;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            using var sha = SHA256.Create();
            await using var crypto = new CryptoStream(file, sha, CryptoStreamMode.Write);
            await content.CopyToAsync(crypto, cancellationToken);
            await crypto.FlushFinalBlockAsync(cancellationToken);
            size = file.Length;
            hash = sha.Hash!;
        }
        catch (UnauthorizedAccessException ex)
        {
            // A filesystem access-denied must NOT surface as an HTTP 401 (auth). Re-throw as IOException so
            // it maps to a 500 with a clear cause: the configured storage root is not writable.
            logger.LogError(ex, "Document storage root is not writable ({Root}). Set DocumentStorage:LocalRootPath to a writable path.", rootPath);
            throw new IOException($"Document storage root '{rootPath}' is not writable. Configure DocumentStorage:LocalRootPath.", ex);
        }

        logger.LogDebug("Stored document object {Key} ({Size} bytes) on local filesystem.", storageKey, size);
        return new StoredObject(size, Convert.ToHexString(hash));
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        var path = ResolvePath(storageKey);
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    // Local filesystem cannot presign; signal the caller to stream instead.
    public Task<Uri?> TryCreatePresignedDownloadUrlAsync(string storageKey, string fileName, TimeSpan ttl, CancellationToken cancellationToken)
        => Task.FromResult<Uri?>(null);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var path = ResolvePath(storageKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
        => Task.FromResult(File.Exists(ResolvePath(storageKey)));

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains(".."))
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        var relative = storageKey.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullRoot = Path.GetFullPath(rootPath);
        var full = Path.GetFullPath(Path.Combine(fullRoot, relative));

        // Defence-in-depth: never escape the configured root.
        if (!full.StartsWith(fullRoot, StringComparison.Ordinal))
        {
            throw new ArgumentException("Storage key resolves outside the storage root.", nameof(storageKey));
        }

        return full;
    }
}
