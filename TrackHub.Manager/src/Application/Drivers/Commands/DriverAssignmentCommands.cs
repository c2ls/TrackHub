using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Drivers.Commands;

// Overlapping open assignments for the same (driver, transporter) pair are rejected with 409 by the
// writer; cross-account transporters surface as 404 (spec 09 §7.4).
[Authorize(Resource = Resources.Drivers, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct AssignDriverToTransporterCommand(Guid DriverId, Guid TransporterId, DateTimeOffset StartsAt, string AssignmentType) : IRequest<DriverTransporterAssignmentVm>;
public class AssignDriverToTransporterCommandHandler(IDriverAssignmentWriter writer) : IRequestHandler<AssignDriverToTransporterCommand, DriverTransporterAssignmentVm>
{
    public async Task<DriverTransporterAssignmentVm> Handle(AssignDriverToTransporterCommand request, CancellationToken cancellationToken)
        => await writer.AssignDriverToTransporterAsync(request.DriverId, request.TransporterId, request.StartsAt, request.AssignmentType, cancellationToken);
}
public class AssignDriverToTransporterCommandValidator : AbstractValidator<AssignDriverToTransporterCommand>
{
    public AssignDriverToTransporterCommandValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.TransporterId).NotEmpty();
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.AssignmentType).Must(DriverAssignmentTypes.IsValid).WithMessage("Invalid assignment type.");
    }
}

// Defaults to now; the writer rejects re-ending an already-closed assignment (immutability, AC3).
[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct EndDriverAssignmentCommand(Guid DriverTransporterAssignmentId, DateTimeOffset? EndsAt = null) : IRequest;
public class EndDriverAssignmentCommandHandler(IDriverAssignmentWriter writer) : IRequestHandler<EndDriverAssignmentCommand>
{
    public async Task Handle(EndDriverAssignmentCommand request, CancellationToken cancellationToken)
        => await writer.EndDriverAssignmentAsync(request.DriverTransporterAssignmentId, request.EndsAt, cancellationToken);
}
public class EndDriverAssignmentCommandValidator : AbstractValidator<EndDriverAssignmentCommand>
{
    public EndDriverAssignmentCommandValidator() => RuleFor(x => x.DriverTransporterAssignmentId).NotEmpty();
}
