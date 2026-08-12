namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Storage abstraction for document bytes. S3-compatible object storage in
/// production (server-generated keys, short-lived presigned download URLs); local filesystem in
/// development. The database stores metadata only; this interface is the only path to the bytes.
/// </summary>
public interface IDocumentStorage
{
    /// <summary>Provider identifier persisted on the document (e.g. "LocalFileSystem", "S3").</summary>
    string Provider { get; }

    /// <summary>
    /// Streams <paramref name="content"/> to <paramref name="storageKey"/>, computing the SHA-256 and
    /// byte length server-side. Overwrites any existing object at the key.
    /// </summary>
    Task<StoredObject> SaveAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken);

    /// <summary>Opens the stored bytes for reading (local FS streaming path).</summary>
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a short-lived (≤5 min) presigned GET URL when the provider supports it (S3); returns
    /// null for providers that must stream (local FS), signalling the caller to stream instead.
    /// </summary>
    Task<Uri?> TryCreatePresignedDownloadUrlAsync(string storageKey, string fileName, TimeSpan ttl, CancellationToken cancellationToken);

    /// <summary>Deletes the stored bytes (retention cleanup). No-op if already absent.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);

    /// <summary>Whether bytes currently exist at the key.</summary>
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
}
