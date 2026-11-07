using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Staff.Queries;

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffQualificationsQuery(Guid AccountId, Guid? StaffId = null, int? ExpiringWithinDays = null, int Skip = 0, int Take = 50) : IRequest<StaffQualificationsPageVm>;
public class GetStaffQualificationsQueryHandler(IStaffQualificationReader reader) : IRequestHandler<GetStaffQualificationsQuery, StaffQualificationsPageVm>
{
    public async Task<StaffQualificationsPageVm> Handle(GetStaffQualificationsQuery request, CancellationToken cancellationToken)
        => await reader.GetStaffQualificationsAsync(request.AccountId, request.StaffId, request.ExpiringWithinDays, request.Skip, request.Take, cancellationToken);
}
public class GetStaffQualificationsQueryValidator : AbstractValidator<GetStaffQualificationsQuery>
{
    public GetStaffQualificationsQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.ExpiringWithinDays).GreaterThanOrEqualTo(0).When(x => x.ExpiringWithinDays.HasValue);
    }
}

[Authorize(Resource = Resources.StaffOperations, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffAssignmentHistoryQuery(Guid AccountId, Guid? StaffId = null, Guid? TransporterId = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<StaffAssignmentHistoryPageVm>;
public class GetStaffAssignmentHistoryQueryHandler(IStaffAssignmentReader reader) : IRequestHandler<GetStaffAssignmentHistoryQuery, StaffAssignmentHistoryPageVm>
{
    public async Task<StaffAssignmentHistoryPageVm> Handle(GetStaffAssignmentHistoryQuery request, CancellationToken cancellationToken)
        => await reader.GetStaffAssignmentHistoryAsync(request.AccountId, request.StaffId, request.TransporterId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}
public class GetStaffAssignmentHistoryQueryValidator : AbstractValidator<GetStaffAssignmentHistoryQuery>
{
    public GetStaffAssignmentHistoryQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("The history window must not end before it starts.");
    }
}

/// <summary>
/// Staff self-service (spec 09 §7.3). Core surface — deliberately NOT gated by the <c>workforce</c>
/// feature, so a staff app keeps working on accounts that never bought the workforce module (the
/// qualification/assignment collections simply come back empty). The handler pins the staff id from
/// the authenticated principal, so a staff token can never read another staff's data (AC2).
/// </summary>
[Authorize(Resource = Resources.Staff, Action = Actions.Read, PrincipalTypes = "Staff")]
public readonly record struct GetMyStaffProfileQuery : IRequest<MyStaffProfileVm>;
public class GetMyStaffProfileQueryHandler(IStaffMemberReader reader, ICurrentPrincipal principal) : IRequestHandler<GetMyStaffProfileQuery, MyStaffProfileVm>
{
    public async Task<MyStaffProfileVm> Handle(GetMyStaffProfileQuery request, CancellationToken cancellationToken)
    {
        var staffId = principal.StaffId
            ?? throw new UnauthorizedAccessException("The current principal does not carry a staff identity.");
        return await reader.GetMyStaffProfileAsync(staffId, cancellationToken);
    }
}
