using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AccountFeatureWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAccountFeatureWriter
{
    /// <summary>
    /// One-time platform migration (authorized via AccountFeaturesMaster at the command layer):
    /// every account that holds a live public-link grant gets an enabled <c>public-links</c> feature
    /// row so existing links keep working after feature-gating. Idempotent; returns the rows touched.
    /// </summary>
    public async Task<int> SeedPublicLinksFeatureAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var accountIds = await Context.PublicLinkGrants
            .Where(x => !x.RevokedAt.HasValue && x.ExpiresAt > now)
            .Select(x => x.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var seeded = 0;
        foreach (var accountId in accountIds)
        {
            var existing = await Context.AccountFeatures
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.FeatureKey == FeatureKeys.PublicLinks, cancellationToken);

            if (existing != null)
            {
                if (existing.Enabled)
                {
                    continue;
                }

                Context.AccountFeatures.Attach(existing);
                var oldValues = AuditValues(existing);
                existing.Enabled = true;
                AddAuditEvent(accountId, "SeedPublicLinksFeature", "AccountFeature", existing.AccountFeatureId.ToString(), oldValues, AuditValues(existing));
            }
            else
            {
                var entity = new AccountFeature(accountId, FeatureKeys.PublicLinks, true, "standard", "migration", null, null, null);
                await Context.AccountFeatures.AddAsync(entity, cancellationToken);
                AddAuditEvent(accountId, "SeedPublicLinksFeature", "AccountFeature", entity.AccountFeatureId.ToString(), null, AuditValues(entity));
            }

            seeded++;
        }

        if (seeded > 0)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        return seeded;
    }

    public async Task<AccountFeatureVm> SetAccountFeatureAsync(AccountFeatureDto feature, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(feature.AccountId);
        var entity = await Context.AccountFeatures.FirstOrDefaultAsync(x => x.AccountId == accountId && x.FeatureKey == feature.FeatureKey, cancellationToken);
        string? oldValues = null;
        if (entity == null)
        {
            entity = new AccountFeature(accountId, feature.FeatureKey, feature.Enabled, feature.Tier, feature.Source, feature.EffectiveFrom, feature.EffectiveTo, feature.ConfigurationJson);
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

        AddAuditEvent(accountId, "SetAccountFeature", "AccountFeature", entity.AccountFeatureId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task DisableAccountFeatureAsync(Guid accountFeatureId, CancellationToken cancellationToken)
    {
        var entity = await Context.AccountFeatures.FirstAsync(x => x.AccountFeatureId == accountFeatureId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        Context.AccountFeatures.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.Enabled = false;
        AddAuditEvent(entity.AccountId, "DisableAccountFeature", "AccountFeature", entity.AccountFeatureId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAccountFeatureConfigurationAsync(Guid accountFeatureId, string? configurationJson, CancellationToken cancellationToken)
    {
        var entity = await Context.AccountFeatures.FirstAsync(x => x.AccountFeatureId == accountFeatureId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        Context.AccountFeatures.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.ConfigurationJson = configurationJson;
        AddAuditEvent(entity.AccountId, "UpdateAccountFeatureConfiguration", "AccountFeature", entity.AccountFeatureId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static AccountFeatureVm ToVm(AccountFeature x) 
        => new(x.AccountFeatureId, x.AccountId, x.FeatureKey, x.Enabled, x.Tier, x.Source, x.EffectiveFrom, x.EffectiveTo, x.ConfigurationJson, x.LastModified);

    private static string AuditValues(AccountFeature feature) 
        => $$"""{"featureKey":"{{feature.FeatureKey}}","enabled":{{feature.Enabled.ToString().ToLowerInvariant()}},"tier":"{{feature.Tier}}","source":"{{feature.Source}}","effectiveFrom":{{Quote(feature.EffectiveFrom)}},"effectiveTo":{{Quote(feature.EffectiveTo)}},"configurationJson":{{Quote(feature.ConfigurationJson)}}}""";

}
