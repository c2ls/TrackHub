namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountFeatureWriter
{
    Task<AccountFeatureVm> SetAccountFeatureAsync(AccountFeatureDto feature, CancellationToken cancellationToken);
    Task DisableAccountFeatureAsync(Guid accountFeatureId, CancellationToken cancellationToken);
    Task UpdateAccountFeatureConfigurationAsync(Guid accountFeatureId, string? configurationJson, CancellationToken cancellationToken);
    Task<int> SeedPublicLinksFeatureAsync(CancellationToken cancellationToken);
}
