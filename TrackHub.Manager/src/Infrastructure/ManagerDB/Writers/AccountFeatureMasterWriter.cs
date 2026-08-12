using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

/// <summary>
/// Cross-account write of account features for the SuperAdministrator. Feature enablement,
/// tier and configuration are billing-owned, so this path targets any account directly
/// (no <c>RequireAccountAccess</c> scoping); the AccountFeaturesMaster resource gates access.
/// </summary>
public sealed class AccountFeatureMasterWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountFeatureMasterWriter
{
    public async Task<AccountFeatureVm> SetAccountFeatureAsync(AccountFeatureDto feature, CancellationToken cancellationToken)
    {
        var entity = await Context.AccountFeatures.FirstOrDefaultAsync(x => x.AccountId == feature.AccountId && x.FeatureKey == feature.FeatureKey, cancellationToken);
        string? oldValues = null;
        if (entity is null)
        {
            entity = new AccountFeature(feature.AccountId, feature.FeatureKey, feature.Enabled, feature.Tier, feature.Source, feature.EffectiveFrom, feature.EffectiveTo, feature.ConfigurationJson);
            await Context.AccountFeatures.AddAsync(entity, cancellationToken);
        }
        else
        {
            oldValues = AuditValues(entity);
            Context.AccountFeatures.Attach(entity);
            entity.Enabled = feature.Enabled;
            entity.Tier = feature.Tier;
            entity.Source = feature.Source;
            entity.EffectiveFrom = feature.EffectiveFrom;
            entity.EffectiveTo = feature.EffectiveTo;
            entity.ConfigurationJson = feature.ConfigurationJson;
        }

        AddAuditEvent(feature.AccountId, "SetAccountFeatureMaster", "AccountFeature", entity.AccountFeatureId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return new AccountFeatureVm(entity.AccountFeatureId, entity.AccountId, entity.FeatureKey, entity.Enabled, entity.Tier, entity.Source, entity.EffectiveFrom, entity.EffectiveTo, entity.ConfigurationJson, entity.LastModified);
    }

    private static string AuditValues(AccountFeature feature)
        => $$"""{"featureKey":"{{feature.FeatureKey}}","enabled":{{feature.Enabled.ToString().ToLowerInvariant()}},"tier":"{{feature.Tier}}","source":"{{feature.Source}}","effectiveFrom":{{Quote(feature.EffectiveFrom)}},"effectiveTo":{{Quote(feature.EffectiveTo)}},"configurationJson":{{Quote(feature.ConfigurationJson)}}}""";
}
