using TrackHub.Manager.Application.GpsIntegration.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<TransporterDeviceAssignmentsPageVm> GetTransporterDeviceAssignmentsByTransporter([Service] ISender sender, [AsParameters] GetTransporterDeviceAssignmentsByTransporterQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TransporterDeviceAssignmentsPageVm> GetTransporterDeviceAssignmentsByAccount([Service] ISender sender, [AsParameters] GetTransporterDeviceAssignmentsByAccountQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
