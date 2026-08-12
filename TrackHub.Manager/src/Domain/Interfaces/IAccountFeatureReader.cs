namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountFeatureReader
{
    Task<IReadOnlyCollection<AccountFeatureVm>> GetAccountFeaturesAsync(Guid accountId, CancellationToken cancellationToken);
    Task<bool> ValidateFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);
}
