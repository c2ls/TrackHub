namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Cross-account (SuperAdministrator) write access to account features.
/// Feature enablement/tier/configuration is billing-owned and therefore managed
/// centrally; authorization is enforced by the AccountFeaturesMaster resource.
/// </summary>
public interface IAccountFeatureMasterWriter
{
    Task<AccountFeatureVm> SetAccountFeatureAsync(AccountFeatureDto feature, CancellationToken cancellationToken);
}
