using Common.Application.Paging;
using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Staff.Queries;

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffMemberQuery(Guid StaffId) : IRequest<StaffMemberVm>;
public class GetStaffMemberQueryHandler(IStaffMemberReader reader) : IRequestHandler<GetStaffMemberQuery, StaffMemberVm>
{
    public async Task<StaffMemberVm> Handle(GetStaffMemberQuery request, CancellationToken cancellationToken) => await reader.GetStaffMemberAsync(request.StaffId, cancellationToken);
}

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
public readonly record struct GetStaffMembersByAccountQuery(Guid AccountId, string? Search = null, string? StaffType = null, int Skip = 0, int Take = 50) : IRequest<StaffMembersPageVm>;
public class GetStaffMembersByAccountQueryHandler(IStaffMemberReader reader) : IRequestHandler<GetStaffMembersByAccountQuery, StaffMembersPageVm>
{
    public async Task<StaffMembersPageVm> Handle(GetStaffMembersByAccountQuery request, CancellationToken cancellationToken) => await reader.GetStaffMembersByAccountAsync(request.AccountId, request.Search, request.StaffType, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
// Enforcement: StaffMemberReader.GetStaffMembersByGroupAsync checks the group's account and the caller's membership.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffMembersByGroupQuery(long GroupId, string? Search = null, int Skip = 0, int Take = 50) : IRequest<StaffMembersPageVm>;
public class GetStaffMembersByGroupQueryHandler(IStaffMemberReader reader) : IRequestHandler<GetStaffMembersByGroupQuery, StaffMembersPageVm>
{
    public async Task<StaffMembersPageVm> Handle(GetStaffMembersByGroupQuery request, CancellationToken cancellationToken) => await reader.GetStaffMembersByGroupAsync(request.GroupId, request.Search, request.Skip, request.Take, cancellationToken);
}
public class GetStaffMembersByGroupQueryValidator : AbstractValidator<GetStaffMembersByGroupQuery>
{
    public GetStaffMembersByGroupQueryValidator() => RuleFor(x => x.GroupId).NotEmpty();
}

[Authorize(Resource = Resources.StaffOperations, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffAssignmentsQuery(Guid StaffId) : IRequest<IReadOnlyCollection<StaffAssignmentVm>>;
public class GetStaffAssignmentsQueryHandler(IStaffMemberReader reader) : IRequestHandler<GetStaffAssignmentsQuery, IReadOnlyCollection<StaffAssignmentVm>>
{
    public async Task<IReadOnlyCollection<StaffAssignmentVm>> Handle(GetStaffAssignmentsQuery request, CancellationToken cancellationToken) => await reader.GetStaffAssignmentsAsync(request.StaffId, cancellationToken);
}

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
[AllowCrossAccount("Assignment probe issued by TripManagement's global trip_client service identity (no account claim) when validating a trip's staff, mirroring the validateGroupVisibility/validateFeatureEnabled probes. Returns a bare boolean.")]
public readonly record struct ValidateStaffAssignmentQuery(Guid StaffId, string ResourceType, string ResourceId) : IRequest<bool>;
public class ValidateStaffAssignmentQueryHandler(IStaffMemberReader reader) : IRequestHandler<ValidateStaffAssignmentQuery, bool>
{
    public async Task<bool> Handle(ValidateStaffAssignmentQuery request, CancellationToken cancellationToken) => await reader.ValidateStaffAssignmentAsync(request.StaffId, request.ResourceType, request.ResourceId, cancellationToken);
}

[Authorize(Resource = Resources.Staff, Action = Actions.Read, PrincipalTypes = "ServiceClient")]
[AllowCrossAccount("Security's global security_client identity carries no account claim; the probe answers whether the named staff belongs to the named account and returns nothing else.")]
public readonly record struct ValidateStaffAccountQuery(Guid StaffId, Guid AccountId) : IRequest<bool>;
public class ValidateStaffAccountQueryHandler(IStaffMemberReader reader) : IRequestHandler<ValidateStaffAccountQuery, bool>
{
    public async Task<bool> Handle(ValidateStaffAccountQuery request, CancellationToken cancellationToken)
        => await reader.StaffBelongsToAccountAsync(request.StaffId, request.AccountId, cancellationToken);
}

// The staff-operations picker: ids and names only, for callers who run staff operations without
// reading the staff catalogue.
[Authorize(Resource = Resources.StaffOperations, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffOptionsQuery(string? Search = null, int? Skip = null, int? Take = null) : IRequest<StaffOptionsPageVm>;
public class GetStaffOptionsQueryHandler(IStaffMemberReader reader, IUserReader userReader, IUser user) : IRequestHandler<GetStaffOptionsQuery, StaffOptionsPageVm>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<StaffOptionsPageVm> Handle(GetStaffOptionsQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var (skip, take) = PageRequest.Clamp(request.Skip, request.Take);
        var page = await reader.GetStaffMembersByAccountAsync(caller.AccountId, request.Search, null, skip, take, cancellationToken);
        return new StaffOptionsPageVm([.. page.Items.Select(d => new StaffLookupVm(d.StaffId, d.Name, d.Active))], page.TotalCount);
    }
}

// The trip board's staff picker: a dispatcher holds Trips/Read, not Staff/Read, and needs names
// and ids, never the phone, document or licence the full staff read carries.
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffLookupQuery(string? Search = null, IReadOnlyCollection<Guid>? StaffIds = null) : IRequest<IReadOnlyCollection<StaffLookupVm>>;
public class GetStaffLookupQueryHandler(IStaffMemberReader reader, IUserReader userReader, IUser user) : IRequestHandler<GetStaffLookupQuery, IReadOnlyCollection<StaffLookupVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<StaffLookupVm>> Handle(GetStaffLookupQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var rows = await reader.GetStaffLookupAsync(caller.AccountId, request.Search, request.StaffIds, LookupLimits.FetchSize, cancellationToken);
        return LookupLimits.EnsureWithinCeiling(rows, "staffLookup");
    }
}
