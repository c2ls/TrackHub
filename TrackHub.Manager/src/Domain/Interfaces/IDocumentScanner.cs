namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Virus/malware scanning abstraction. Uploaded files are Quarantined until an
/// async scan returns Clean. The dev/no-op scanner marks Clean immediately; production selects a real
/// AV provider per deployment.
/// </summary>
public interface IDocumentScanner
{
    /// <summary>Provider identifier (e.g. "NoOp", "ClamAV").</summary>
    string Provider { get; }

    /// <summary>
    /// Scans the bytes at <paramref name="storageKey"/> and returns a
    /// <see cref="Constants.DocumentScanStatuses"/> value (Clean/Infected/Failed).
    /// </summary>
    Task<string> ScanAsync(string storageKey, CancellationToken cancellationToken);
}
