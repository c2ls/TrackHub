using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Staff.Commands;

[Authorize(Resource = Resources.Staff, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct CreateStaffQualificationCommand(StaffQualificationDto Qualification) : IRequest<StaffQualificationVm>;
public class CreateStaffQualificationCommandHandler(IStaffQualificationWriter writer) : IRequestHandler<CreateStaffQualificationCommand, StaffQualificationVm>
{
    public async Task<StaffQualificationVm> Handle(CreateStaffQualificationCommand request, CancellationToken cancellationToken) => await writer.CreateStaffQualificationAsync(request.Qualification, cancellationToken);
}
public class CreateStaffQualificationCommandValidator : AbstractValidator<CreateStaffQualificationCommand>
{
    public CreateStaffQualificationCommandValidator()
    {
        RuleFor(x => x.Qualification.AccountId).NotEmpty();
        RuleFor(x => x.Qualification.StaffId).NotEmpty();
        RuleFor(x => x.Qualification.QualificationType).Must(StaffQualificationTypes.IsValid).WithMessage("Invalid qualification type.");
        RuleFor(x => x.Qualification.Status).Must(StaffQualificationStatuses.IsValid).WithMessage("Invalid qualification status.");
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

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateStaffQualificationCommand(Guid StaffQualificationId, StaffQualificationDto Qualification) : IRequest;
public class UpdateStaffQualificationCommandHandler(IStaffQualificationWriter writer) : IRequestHandler<UpdateStaffQualificationCommand>
{
    public async Task Handle(UpdateStaffQualificationCommand request, CancellationToken cancellationToken) => await writer.UpdateStaffQualificationAsync(request.StaffQualificationId, request.Qualification, cancellationToken);
}
public class UpdateStaffQualificationCommandValidator : AbstractValidator<UpdateStaffQualificationCommand>
{
    public UpdateStaffQualificationCommandValidator()
    {
        RuleFor(x => x.StaffQualificationId).NotEmpty();
        RuleFor(x => x.Qualification.AccountId).NotEmpty();
        RuleFor(x => x.Qualification.StaffId).NotEmpty();
        RuleFor(x => x.Qualification.QualificationType).Must(StaffQualificationTypes.IsValid).WithMessage("Invalid qualification type.");
        RuleFor(x => x.Qualification.Status).Must(StaffQualificationStatuses.IsValid).WithMessage("Invalid qualification status.");
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
[Authorize(Resource = Resources.Staff, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.Workforce)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteStaffQualificationCommand(Guid StaffQualificationId) : IRequest;
public class DeleteStaffQualificationCommandHandler(IStaffQualificationWriter writer) : IRequestHandler<DeleteStaffQualificationCommand>
{
    public async Task Handle(DeleteStaffQualificationCommand request, CancellationToken cancellationToken) => await writer.DeleteStaffQualificationAsync(request.StaffQualificationId, cancellationToken);
}
public class DeleteStaffQualificationCommandValidator : AbstractValidator<DeleteStaffQualificationCommand>
{
    public DeleteStaffQualificationCommandValidator() => RuleFor(x => x.StaffQualificationId).NotEmpty();
}
