using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Storage;

/// <summary>
/// Development virus scanner: marks every file Clean immediately. Production
/// selects a real AV provider per deployment; until then quarantine-until-clean
/// is exercised end-to-end with this no-op returning Clean.
/// </summary>
public sealed class NoOpDocumentScanner : IDocumentScanner
{
    public const string ProviderName = "NoOp";

    public string Provider => ProviderName;

    public Task<string> ScanAsync(string storageKey, CancellationToken cancellationToken)
        => Task.FromResult(DocumentScanStatuses.Clean);
}
