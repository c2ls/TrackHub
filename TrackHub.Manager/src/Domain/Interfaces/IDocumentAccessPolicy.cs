namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Maps a document's <c>OwnerEntityType</c> to the correct visibility primitive:
/// Transporter → group visibility, Driver → assignment/default-transporter visibility, and
/// deny-by-default for owner types without a registered resolver. Also enforces the classification gate.
/// </summary>
public interface IDocumentAccessPolicy
{
    /// <summary>
    /// Whether the current principal may access a document owned by
    /// (<paramref name="ownerEntityType"/>, <paramref name="ownerEntityId"/>) in
    /// <paramref name="accountId"/>. Drivers are assignment-scoped; users get group visibility; owner
    /// types without a resolver return false.
    /// </summary>
    Task<bool> CanAccessOwnerAsync(Guid accountId, string ownerEntityType, string ownerEntityId, bool forWrite, CancellationToken cancellationToken);

    /// <summary>
    /// Whether the current principal is cleared for <paramref name="classification"/>. Public/Internal
    /// are always allowed; Confidential/Legal require an elevated (privileged) principal.
    /// </summary>
    bool IsClearedForClassification(string classification);

    /// <summary>The set of transporter ids visible to the current principal (for library search scoping).</summary>
    Task<IReadOnlySet<Guid>> GetVisibleTransporterIdsAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Whether the current principal reads account-wide (Administrator/Manager/global service client).</summary>
    bool IsPrivilegedPrincipal { get; }
}
