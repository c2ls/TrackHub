using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Staff.Commands;

// Overlapping open assignments for the same (staff, transporter) pair are rejected with 409 by the
// writer; cross-account transporters surface as 404 (spec 09 §7.4).
[Authorize(Resource = Resources.StaffOperations, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct AssignStaffToTransporterCommand(Guid StaffId, Guid TransporterId, DateTimeOffset StartsAt, string AssignmentType) : IRequest<StaffTransporterAssignmentVm>;
public class AssignStaffToTransporterCommandHandler(IStaffAssignmentWriter writer) : IRequestHandler<AssignStaffToTransporterCommand, StaffTransporterAssignmentVm>
{
    public async Task<StaffTransporterAssignmentVm> Handle(AssignStaffToTransporterCommand request, CancellationToken cancellationToken)
        => await writer.AssignStaffToTransporterAsync(request.StaffId, request.TransporterId, request.StartsAt, request.AssignmentType, cancellationToken);
}
public class AssignStaffToTransporterCommandValidator : AbstractValidator<AssignStaffToTransporterCommand>
{
    public AssignStaffToTransporterCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.TransporterId).NotEmpty();
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.AssignmentType).Must(StaffAssignmentTypes.IsValid).WithMessage("Invalid assignment type.");
    }
}

// Defaults to now; the writer rejects re-ending an already-closed assignment (immutability, AC3).
[Authorize(Resource = Resources.StaffOperations, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct EndStaffAssignmentCommand(Guid StaffTransporterAssignmentId, DateTimeOffset? EndsAt = null) : IRequest;
public class EndStaffAssignmentCommandHandler(IStaffAssignmentWriter writer) : IRequestHandler<EndStaffAssignmentCommand>
{
    public async Task Handle(EndStaffAssignmentCommand request, CancellationToken cancellationToken)
        => await writer.EndStaffAssignmentAsync(request.StaffTransporterAssignmentId, request.EndsAt, cancellationToken);
}
public class EndStaffAssignmentCommandValidator : AbstractValidator<EndStaffAssignmentCommand>
{
    public EndStaffAssignmentCommandValidator() => RuleFor(x => x.StaffTransporterAssignmentId).NotEmpty();
}
