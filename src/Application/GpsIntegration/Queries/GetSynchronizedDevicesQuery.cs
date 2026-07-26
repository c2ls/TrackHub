using Common.Application.Paging;

namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Read)]

public readonly record struct GetSynchronizedDevicesQuery(
    Guid AccountId,
    DetectedStatus? DetectedStatus = null,
    Guid? OperatorId = null,
    int? Skip = null,
    int? Take = null,
    string? Search = null,
    bool UnassignedOnly = false,
    bool RecentOnly = false) : IRequest<DevicesPageVm>;

public class GetSynchronizedDevicesQueryHandler(IDeviceReader reader)
    : IRequestHandler<GetSynchronizedDevicesQuery, DevicesPageVm>
{
    /// <summary>How far back "recently added" reaches, measured against FirstSeenAt.</summary>
    public static readonly TimeSpan RecentWindow = TimeSpan.FromHours(24);

    public Task<DevicesPageVm> Handle(GetSynchronizedDevicesQuery request, CancellationToken cancellationToken)
    {
        var filter = new SynchronizedDeviceFilter(
            request.DetectedStatus,
            request.OperatorId,
            request.UnassignedOnly,
            request.RecentOnly ? DateTimeOffset.UtcNow - RecentWindow : null);
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return reader.SearchDevicesAsync(request.AccountId, filter, skip, take, request.Search, cancellationToken);
    }
}
