using Common.Application.Paging;

namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Read)]

// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetTransporterDeviceAssignmentsByTransporterQuery(
    Guid TransporterId,
    bool ActiveOnly = false,
    int? Skip = null,
    int? Take = null)
    : IRequest<TransporterDeviceAssignmentsPageVm>;

public class GetTransporterDeviceAssignmentsByTransporterQueryHandler(ITransporterDeviceAssignmentReader reader)
    : IRequestHandler<GetTransporterDeviceAssignmentsByTransporterQuery, TransporterDeviceAssignmentsPageVm>
{
    public Task<TransporterDeviceAssignmentsPageVm> Handle(GetTransporterDeviceAssignmentsByTransporterQuery request, CancellationToken cancellationToken)
    {
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        return reader.GetByTransporterAsync(request.TransporterId, request.ActiveOnly, skip, take, cancellationToken);
    }
}
