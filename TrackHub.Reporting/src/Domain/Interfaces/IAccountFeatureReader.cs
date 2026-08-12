namespace TrackHub.Reporting.Domain.Interfaces;

public interface IAccountFeatureReader
{
    Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);
}

