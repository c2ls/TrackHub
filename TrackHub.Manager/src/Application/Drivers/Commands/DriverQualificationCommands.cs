using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Drivers.Commands;

[Authorize(Resource = Resources.Drivers, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Workforce)]
public readonly record struct CreateDriverQualificationCommand(DriverQualificationDto Qualification) : IRequest<DriverQualificationVm>;
public class CreateDriverQualificationCommandHandler(IDriverQualificationWriter writer) : IRequestHandler<CreateDriverQualificationCommand, DriverQualificationVm>
{
    public async Task<DriverQualificationVm> Handle(CreateDriverQualificationCommand request, CancellationToken cancellationToken) => await writer.CreateDriverQualificationAsync(request.Qualification, cancellationToken);
}
public class CreateDriverQualificationCommandValidator : AbstractValidator<CreateDriverQualificationCommand>
{
    public CreateDriverQualificationCommandValidator()
    {
        RuleFor(x => x.Qualification.AccountId).NotEmpty();
        RuleFor(x => x.Qualification.DriverId).NotEmpty();
        RuleFor(x => x.Qualification.QualificationType).Must(DriverQualificationTypes.IsValid).WithMessage("Invalid qualification type.");
        RuleFor(x => x.Qualification.Status).Must(DriverQualificationStatuses.IsValid).WithMessage("Invalid qualification status.");
        RuleFor(x => x.Qualification.Category).MaximumLength(ColumnMetadata.DefaultFieldLength);
        RuleFor(x => x.Qualification.Number).MaximumLength(ColumnMetadata.DefaultNameLength);
        RuleFor(x => x.Qualification.IssuingAuthority).MaximumLength(ColumnMetadata.DefaultNameLength);
        RuleFor(x => x.Qualification.Notes).MaximumLength(ColumnMetadata.DefaultDescriptionLength);
        RuleFor(x => x.Qualification.ExpiresAt)
            .GreaterThanOrEqualTo(x => x.Qualification.IssuedAt!.Value)
            .When(x => x.Qualification.IssuedAt.HasValue && x.Qualification.ExpiresAt.HasValue)
            .WithMessage("A qualification cannot expire before it was issued.");
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Workforce)]
public readonly record struct UpdateDriverQualificationCommand(Guid DriverQualificationId, DriverQualificationDto Qualification) : IRequest;
public class UpdateDriverQualificationCommandHandler(IDriverQualificationWriter writer) : IRequestHandler<UpdateDriverQualificationCommand>
{
    public async Task Handle(UpdateDriverQualificationCommand request, CancellationToken cancellationToken) => await writer.UpdateDriverQualificationAsync(request.DriverQualificationId, request.Qualification, cancellationToken);
}
public class UpdateDriverQualificationCommandValidator : AbstractValidator<UpdateDriverQualificationCommand>
{
    public UpdateDriverQualificationCommandValidator()
    {
        RuleFor(x => x.DriverQualificationId).NotEmpty();
        RuleFor(x => x.Qualification.AccountId).NotEmpty();
        RuleFor(x => x.Qualification.DriverId).NotEmpty();
        RuleFor(x => x.Qualification.QualificationType).Must(DriverQualificationTypes.IsValid).WithMessage("Invalid qualification type.");
        RuleFor(x => x.Qualification.Status).Must(DriverQualificationStatuses.IsValid).WithMessage("Invalid qualification status.");
        RuleFor(x => x.Qualification.Category).MaximumLength(ColumnMetadata.DefaultFieldLength);
        RuleFor(x => x.Qualification.Number).MaximumLength(ColumnMetadata.DefaultNameLength);
        RuleFor(x => x.Qualification.IssuingAuthority).MaximumLength(ColumnMetadata.DefaultNameLength);
        RuleFor(x => x.Qualification.Notes).MaximumLength(ColumnMetadata.DefaultDescriptionLength);
        RuleFor(x => x.Qualification.ExpiresAt)
            .GreaterThanOrEqualTo(x => x.Qualification.IssuedAt!.Value)
            .When(x => x.Qualification.IssuedAt.HasValue && x.Qualification.ExpiresAt.HasValue)
            .WithMessage("A qualification cannot expire before it was issued.");
    }
}

// Hard delete — the before-image survives in the audit trail (spec 09 §7.1).
[Authorize(Resource = Resources.Drivers, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteDriverQualificationCommand(Guid DriverQualificationId) : IRequest;
public class DeleteDriverQualificationCommandHandler(IDriverQualificationWriter writer) : IRequestHandler<DeleteDriverQualificationCommand>
{
    public async Task Handle(DeleteDriverQualificationCommand request, CancellationToken cancellationToken) => await writer.DeleteDriverQualificationAsync(request.DriverQualificationId, cancellationToken);
}
public class DeleteDriverQualificationCommandValidator : AbstractValidator<DeleteDriverQualificationCommand>
{
    public DeleteDriverQualificationCommandValidator() => RuleFor(x => x.DriverQualificationId).NotEmpty();
}
