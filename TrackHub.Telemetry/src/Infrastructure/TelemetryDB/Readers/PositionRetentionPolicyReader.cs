using System.Text.Json;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

/// <summary>
/// Read-only view of the position retention policy. Whether history is retained and for how
/// long is derived from the SuperAdministrator-owned <c>gps.positionHistory</c> feature
/// (enable flag + <c>retentionDays</c> configuration). Account admins/managers only visualize it.
/// </summary>
public sealed class PositionRetentionPolicyReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IPositionRetentionPolicyReader
{
    public async Task<PositionRetentionPolicyVm> GetAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        var feature = await Context.AccountFeatures
            .Where(f => f.AccountId == scoped && f.FeatureKey == FeatureKeys.GpsPositionHistory)
            .OrderByDescending(f => f.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (feature is null || !feature.Enabled)
        {
            return new PositionRetentionPolicyVm(false, 0, "Default");
        }

        if (!string.IsNullOrWhiteSpace(feature.ConfigurationJson))
        {
            try
            {
                var doc = JsonDocument.Parse(feature.ConfigurationJson!);
                var retention = doc.RootElement.TryGetProperty("retentionDays", out var rd) ? rd.GetInt32() : 30;
                return new PositionRetentionPolicyVm(true, retention, feature.Source);
            }
            catch
            {
                // fall through to default
            }
        }
        return new PositionRetentionPolicyVm(true, 30, feature.Source);
    }
}
