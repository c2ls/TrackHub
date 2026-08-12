using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AccountFeatureReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAccountFeatureReader
{
    public async Task<IReadOnlyCollection<AccountFeatureVm>> GetAccountFeaturesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.AccountFeatures
            .Where(x => x.AccountId == scopedAccountId)
            .OrderBy(x => x.FeatureKey)
            .Select(x => new AccountFeatureVm(x.AccountFeatureId, x.AccountId, x.FeatureKey, x.Enabled, x.Tier, x.Source, x.EffectiveFrom, x.EffectiveTo, x.ConfigurationJson, x.LastModified))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ValidateFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.AccountFeatures.AnyAsync(x =>
            x.AccountId == scopedAccountId
            && x.FeatureKey == featureKey
            && x.Enabled
            && (!x.EffectiveFrom.HasValue || x.EffectiveFrom <= now)
            && (!x.EffectiveTo.HasValue || x.EffectiveTo >= now), cancellationToken);
    }
}
