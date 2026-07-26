using TrackHub.Manager.Application.Drivers.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<DriverVm> GetDriver([Service] ISender sender, [AsParameters] GetDriverQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DriverVm>> GetDriversByAccount([Service] ISender sender, [AsParameters] GetDriversByAccountQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DriverAssignmentVm>> GetDriverAssignments([Service] ISender sender, [AsParameters] GetDriverAssignmentsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<bool> ValidateDriverAssignment([Service] ISender sender, [AsParameters] ValidateDriverAssignmentQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DriverQualificationVm>> GetDriverQualifications([Service] ISender sender, [AsParameters] GetDriverQualificationsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DriverTransporterAssignmentVm>> GetDriverAssignmentHistory([Service] ISender sender, [AsParameters] GetDriverAssignmentHistoryQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<MyDriverProfileVm> GetMyDriverProfile([Service] ISender sender, CancellationToken cancellationToken) => await sender.Send(new GetMyDriverProfileQuery(), cancellationToken);
}
