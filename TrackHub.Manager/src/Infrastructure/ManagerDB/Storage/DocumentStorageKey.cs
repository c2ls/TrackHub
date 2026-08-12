namespace TrackHub.Manager.Infrastructure.ManagerDB.Storage;

// Server-generated storage key layout: {accountId}/{ownerType}/{documentId}/{version}.
// Never exposed in any read model.
public static class DocumentStorageKey
{
    public static string For(Guid accountId, string ownerEntityType, Guid documentId, int versionNumber)
        => $"{accountId:N}/{Sanitize(ownerEntityType)}/{documentId:N}/{versionNumber}";

    private static string Sanitize(string ownerEntityType)
        => string.IsNullOrWhiteSpace(ownerEntityType)
            ? "unknown"
            : new string([.. ownerEntityType.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_')]);
}
