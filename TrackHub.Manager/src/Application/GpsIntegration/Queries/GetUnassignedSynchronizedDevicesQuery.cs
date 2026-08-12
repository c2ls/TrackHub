using Common.Application.Paging;

namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Read)]

public readonly record struct GetUnassignedSynchronizedDevicesQuery(
    Guid AccountId,
    int? Skip = null,
    int? Take = null,
    string? Search = null) : IRequest<DevicesPageVm>;

public class GetUnassignedSynchronizedDevicesQueryHandler(IDeviceReader reader)
    : IRequestHandler<GetUnassignedSynchronizedDevicesQuery, DevicesPageVm>
{
    public Task<DevicesPageVm> Handle(GetUnassignedSynchronizedDevicesQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return reader.GetUnassignedDevicesAsync(request.AccountId, skip, take, request.Search, cancellationToken);
    }
}
