namespace TrackHub.Telemetry.Application.GpsIntegration.Queries;

// Platform-wide SyncWorker liveness for the status page.
//
// Guarded by OperatorSyncRuns/Read rather than Administrative/Read (Sergio, 2026-07-19): both
// SuperAdministrators and Managers see the "GPS synchronisation" tile, because it pairs with the
// gpsIntegration dashboard Managers already own. The response is deliberately platform-wide and
// carries ONLY timestamps and counts — never an account id, name, or operator — so a Manager
// learns that the worker is alive, not who it synced for. The jobs table and announcement
// management stay Administrative/Read (Administrator-only).
// No [Caching]: freshness is the point.
[Authorize(Resource = Resources.OperatorSyncRuns, Action = Actions.Read)]
[PlatformScoped("SVD-10 platform status: SyncWorker liveness timestamps and counts only — no account, operator, or per-tenant data in the response.")]
public readonly record struct GetPlatformSyncActivityQuery(int LookbackMinutes = 60) : IRequest<PlatformSyncActivityVm>;

public class GetPlatformSyncActivityQueryHandler(IPlatformSyncActivityReader reader)
    : IRequestHandler<GetPlatformSyncActivityQuery, PlatformSyncActivityVm>
{
    public Task<PlatformSyncActivityVm> Handle(GetPlatformSyncActivityQuery request, CancellationToken cancellationToken)
    {
        var minutes = request.LookbackMinutes <= 0 ? 60 : Math.Min(request.LookbackMinutes, 60 * 24);
        return reader.GetAsync(DateTimeOffset.UtcNow.AddMinutes(-minutes), cancellationToken);
    }
}
