using TrackHub.Manager.Application.Lookups;

namespace TrackHub.Manager.Application.GpsIntegration.Queries;

// Deliberately NOT paged. The SyncWorker's 10-second position loop reads this through
// IDeviceCatalogCache; a window would permanently stop position sync for every device past it, and
// the cache would make that state sticky long after the window moved. Bounded instead, so an
// implausible catalog raises rather than silently shrinking.
[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Read, PrincipalTypes = "User,ServiceClient")]
[AllowCrossAccount("Read side of the same SyncWorker device-sync loop: one global service identity iterates every account's operators to resolve their assigned device/transporter pairs before fetching positions.")]
public readonly record struct GetAssignedDeviceTransportersByOperatorQuery(Guid AccountId, Guid OperatorId)
    : IRequest<IReadOnlyCollection<DeviceTransporterVm>>;

public class GetAssignedDeviceTransportersByOperatorQueryHandler(IDeviceTransporterReader reader)
    : IRequestHandler<GetAssignedDeviceTransportersByOperatorQuery, IReadOnlyCollection<DeviceTransporterVm>>
{
    public async Task<IReadOnlyCollection<DeviceTransporterVm>> Handle(GetAssignedDeviceTransportersByOperatorQuery request, CancellationToken cancellationToken)
        => UnpagedReadLimits.EnsureWithinCeiling(
            await reader.GetAssignedDeviceTransportersByOperatorAsync(request.AccountId, request.OperatorId, cancellationToken),
            "assignedDeviceTransportersByOperator");
}
