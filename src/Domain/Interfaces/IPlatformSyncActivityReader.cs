using TrackHub.Telemetry.Domain.Models;

namespace TrackHub.Telemetry.Domain.Interfaces;

/// <summary>
/// Deliberately platform-scoped (unscoped) read — it does NOT go through
/// <c>AccountScopedDataAccess</c>. Access is gated upstream by
/// <c>[Authorize(Administrative, Read)]</c>, which only the Administrator role holds.
/// </summary>
public interface IPlatformSyncActivityReader
{
    Task<PlatformSyncActivityVm> GetAsync(DateTimeOffset since, CancellationToken cancellationToken);
}
