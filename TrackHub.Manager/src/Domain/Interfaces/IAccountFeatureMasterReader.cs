namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Cross-account (SuperAdministrator) read access to account features.
/// Unlike <see cref="IAccountFeatureReader"/>, this path is not scoped to the
/// caller's own account; authorization is enforced by the AccountFeaturesMaster resource.
/// </summary>
public interface IAccountFeatureMasterReader
{
    Task<IReadOnlyCollection<AccountFeatureVm>> GetAccountFeaturesAsync(Guid accountId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccountFeatureVm>> GetAllAccountFeaturesAsync(CancellationToken cancellationToken);
}
