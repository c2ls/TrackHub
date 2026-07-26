using Common.Application.Paging;

namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Read)]

public readonly record struct GetTransporterDeviceAssignmentsByAccountQuery(
    Guid AccountId,
    bool ActiveOnly = false,
    int? Skip = null,
    int? Take = null)
    : IRequest<TransporterDeviceAssignmentsPageVm>;

public class GetTransporterDeviceAssignmentsByAccountQueryHandler(ITransporterDeviceAssignmentReader reader)
    : IRequestHandler<GetTransporterDeviceAssignmentsByAccountQuery, TransporterDeviceAssignmentsPageVm>
{
    public Task<TransporterDeviceAssignmentsPageVm> Handle(GetTransporterDeviceAssignmentsByAccountQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return reader.GetByAccountAsync(request.AccountId, request.ActiveOnly, skip, take, cancellationToken);
    }
}
