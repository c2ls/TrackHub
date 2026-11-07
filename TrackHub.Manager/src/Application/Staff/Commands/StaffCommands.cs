using Common.Application.Interfaces;
using Common.Domain.Time;
using System.Text.RegularExpressions;

namespace TrackHub.Manager.Application.Staff.Commands;

/// <summary>
/// Shared field rules for the staff write commands. Until spec 09 these commands shipped with NO
/// validators at all — a create with an empty name reached the writer (spec 09 §2.2.4). The rules are
/// deliberately permissive about formatting (international phone shapes vary) and strict only about
/// what the database and the portal form already require.
/// </summary>
internal static class StaffMemberRules
{
    // Digits with the usual separators; length is bounded by the column, not by this pattern.
    private static readonly Regex PhonePattern = new(@"^[+]?[0-9()\-.\s]{5,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public static void ApplyTo<T>(AbstractValidator<T> validator, Func<T, StaffMemberDto> staff)
    {
        validator.RuleFor(x => staff(x).AccountId).NotEmpty();
        validator.RuleFor(x => staff(x).Name).NotEmpty().MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => staff(x).Phone)
            .MaximumLength(ColumnMetadata.DefaultPhoneNumberLength)
            .Matches(PhonePattern).When(x => !string.IsNullOrWhiteSpace(staff(x).Phone))
            .WithMessage("Invalid phone number.");
        validator.RuleFor(x => staff(x).DocumentType).MaximumLength(ColumnMetadata.DefaultFieldLength);
        validator.RuleFor(x => staff(x).DocumentNumber).MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => staff(x).EmployeeCode).MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => staff(x).LicenseNumber).MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => staff(x).StaffType).Must(StaffTypes.IsValid).WithMessage("Unknown staff type.");
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Write)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct CreateStaffMemberCommand(StaffMemberDto Staff) : IRequest<StaffMemberVm>;
public class CreateStaffMemberCommandHandler(IStaffMemberWriter writer) : IRequestHandler<CreateStaffMemberCommand, StaffMemberVm>
{
    public async Task<StaffMemberVm> Handle(CreateStaffMemberCommand request, CancellationToken cancellationToken) => await writer.CreateStaffMemberAsync(request.Staff, cancellationToken);
}
public class CreateStaffMemberCommandValidator : AbstractValidator<CreateStaffMemberCommand>
{
    public CreateStaffMemberCommandValidator(IAccountTimeZoneResolver? zones = null)
    {
        StaffMemberRules.ApplyTo(this, x => x.Staff);
        // Create-only: onboarding a staff on an already-expired license is a data-entry error. Update
        // must stay permissive so an existing record can be corrected or left alone while it lapses.
        RuleFor(x => x.Staff.LicenseExpiresAt)
            .MustAsync(async (command, expiresAt, cancellationToken) =>
            {
                // "Already past" is judged on the staff account's calendar, not the server's UTC day.
                var calendar = zones is not null
                    ? await zones.ResolveAsync(command.Staff.AccountId, cancellationToken)
                    : AccountTimeZone.Utc;
                return expiresAt!.Value >= calendar.Today();
            })
            .When(x => x.Staff.LicenseExpiresAt.HasValue)
            .WithMessage("The license expiration date is already past.");
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Edit)]
// Enforcement: the writer or reader checks every referenced id against the request's account.
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateStaffMemberCommand(Guid StaffId, StaffMemberDto Staff) : IRequest;
public class UpdateStaffMemberCommandHandler(IStaffMemberReader reader, IStaffCredentialRevoker revoker, IStaffMemberWriter writer) : IRequestHandler<UpdateStaffMemberCommand>
{
    // Credentials are revoked BEFORE the local write: a Security outage then fails the command and
    // the staff stays active, never active-here-but-still-signing-in.
    public async Task Handle(UpdateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        var current = await reader.GetStaffMemberAsync(request.StaffId, cancellationToken);
        if (current.Active && !request.Staff.Active)
        {
            await revoker.RevokeStaffCredentialsAsync(current.StaffId, current.AccountId, cancellationToken);
        }

        await writer.UpdateStaffMemberAsync(request.StaffId, request.Staff, cancellationToken);
    }
}
public class UpdateStaffMemberCommandValidator : AbstractValidator<UpdateStaffMemberCommand>
{
    public UpdateStaffMemberCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        StaffMemberRules.ApplyTo(this, x => x.Staff);
    }
}

[Authorize(Resource = Resources.Staff, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeactivateStaffMemberCommand(Guid StaffId) : IRequest;
public class DeactivateStaffMemberCommandHandler(IStaffMemberReader reader, IStaffCredentialRevoker revoker, IStaffMemberWriter writer) : IRequestHandler<DeactivateStaffMemberCommand>
{
    public async Task Handle(DeactivateStaffMemberCommand request, CancellationToken cancellationToken)
    {
        var current = await reader.GetStaffMemberAsync(request.StaffId, cancellationToken);
        await revoker.RevokeStaffCredentialsAsync(current.StaffId, current.AccountId, cancellationToken);
        await writer.DeactivateStaffMemberAsync(request.StaffId, cancellationToken);
    }
}
public class DeactivateStaffMemberCommandValidator : AbstractValidator<DeactivateStaffMemberCommand>
{
    public DeactivateStaffMemberCommandValidator() => RuleFor(x => x.StaffId).NotEmpty();
}
