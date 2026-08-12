using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Drivers.Queries;

[Authorize(Resource = Resources.Drivers, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Workforce)]
public readonly record struct GetDriverQualificationsQuery(Guid AccountId, Guid? DriverId = null, int? ExpiringWithinDays = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DriverQualificationVm>>;
public class GetDriverQualificationsQueryHandler(IDriverQualificationReader reader) : IRequestHandler<GetDriverQualificationsQuery, IReadOnlyCollection<DriverQualificationVm>>
{
    public async Task<IReadOnlyCollection<DriverQualificationVm>> Handle(GetDriverQualificationsQuery request, CancellationToken cancellationToken)
        => await reader.GetDriverQualificationsAsync(request.AccountId, request.DriverId, request.ExpiringWithinDays, request.Skip, request.Take, cancellationToken);
}
public class GetDriverQualificationsQueryValidator : AbstractValidator<GetDriverQualificationsQuery>
{
    public GetDriverQualificationsQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.ExpiringWithinDays).GreaterThanOrEqualTo(0).When(x => x.ExpiringWithinDays.HasValue);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Workforce)]
public readonly record struct GetDriverAssignmentHistoryQuery(Guid AccountId, Guid? DriverId = null, Guid? TransporterId = null, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DriverTransporterAssignmentVm>>;
public class GetDriverAssignmentHistoryQueryHandler(IDriverAssignmentReader reader) : IRequestHandler<GetDriverAssignmentHistoryQuery, IReadOnlyCollection<DriverTransporterAssignmentVm>>
{
    public async Task<IReadOnlyCollection<DriverTransporterAssignmentVm>> Handle(GetDriverAssignmentHistoryQuery request, CancellationToken cancellationToken)
        => await reader.GetDriverAssignmentHistoryAsync(request.AccountId, request.DriverId, request.TransporterId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}
public class GetDriverAssignmentHistoryQueryValidator : AbstractValidator<GetDriverAssignmentHistoryQuery>
{
    public GetDriverAssignmentHistoryQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("The history window must not end before it starts.");
    }
}

/// <summary>
/// Driver self-service (spec 09 §7.3). Core surface — deliberately NOT gated by the <c>workforce</c>
/// feature, so a driver app keeps working on accounts that never bought the workforce module (the
/// qualification/assignment collections simply come back empty). The handler pins the driver id from
/// the authenticated principal, so a driver token can never read another driver's data (AC2).
/// </summary>
[Authorize(Resource = Resources.Drivers, Action = Actions.Read, PrincipalTypes = "Driver")]
public readonly record struct GetMyDriverProfileQuery : IRequest<MyDriverProfileVm>;
public class GetMyDriverProfileQueryHandler(IDriverReader reader, ICurrentPrincipal principal) : IRequestHandler<GetMyDriverProfileQuery, MyDriverProfileVm>
{
    public async Task<MyDriverProfileVm> Handle(GetMyDriverProfileQuery request, CancellationToken cancellationToken)
    {
        var driverId = principal.DriverId
            ?? throw new UnauthorizedAccessException("The current principal does not carry a driver identity.");
        return await reader.GetMyDriverProfileAsync(driverId, cancellationToken);
    }
}
