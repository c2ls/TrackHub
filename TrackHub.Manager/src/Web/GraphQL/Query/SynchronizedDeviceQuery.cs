using TrackHub.Manager.Application.GpsIntegration.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<DevicesPageVm> GetSynchronizedDevices([Service] ISender sender, [AsParameters] GetSynchronizedDevicesQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<DevicesPageVm> GetUnassignedSynchronizedDevices([Service] ISender sender, [AsParameters] GetUnassignedSynchronizedDevicesQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<DeviceTransporterVm>> GetAssignedDeviceTransportersByOperator(
        [Service] ISender sender,
        [AsParameters] GetAssignedDeviceTransportersByOperatorQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
