using TrackHub.Manager.Application.Staff.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<StaffMemberVm> GetStaffMember([Service] ISender sender, [AsParameters] GetStaffMemberQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<StaffMembersPageVm> GetStaffMembersByAccount([Service] ISender sender, [AsParameters] GetStaffMembersByAccountQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<StaffMembersPageVm> GetStaffMembersByGroup([Service] ISender sender, [AsParameters] GetStaffMembersByGroupQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<StaffAssignmentVm>> GetStaffAssignments([Service] ISender sender, [AsParameters] GetStaffAssignmentsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<bool> ValidateStaffAssignment([Service] ISender sender, [AsParameters] ValidateStaffAssignmentQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<bool> ValidateStaffAccount([Service] ISender sender, [AsParameters] ValidateStaffAccountQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<StaffQualificationsPageVm> GetStaffQualifications([Service] ISender sender, [AsParameters] GetStaffQualificationsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<StaffAssignmentHistoryPageVm> GetStaffAssignmentHistory([Service] ISender sender, [AsParameters] GetStaffAssignmentHistoryQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<MyStaffProfileVm> GetMyStaffProfile([Service] ISender sender, CancellationToken cancellationToken) => await sender.Send(new GetMyStaffProfileQuery(), cancellationToken);
    public async Task<StaffOptionsPageVm> GetStaffOptions([Service] ISender sender, [AsParameters] GetStaffOptionsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<StaffLookupVm>> GetStaffLookup([Service] ISender sender, [AsParameters] GetStaffLookupQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
