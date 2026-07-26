using System.Text.RegularExpressions;

namespace TrackHub.Manager.Application.Drivers.Commands;

/// <summary>
/// Shared field rules for the driver write commands. Until spec 09 these commands shipped with NO
/// validators at all — a create with an empty name reached the writer (spec 09 §2.2.4). The rules are
/// deliberately permissive about formatting (international phone shapes vary) and strict only about
/// what the database and the portal form already require.
/// </summary>
internal static class DriverRules
{
    // Digits with the usual separators; length is bounded by the column, not by this pattern.
    private static readonly Regex PhonePattern = new(@"^[+]?[0-9()\-.\s]{5,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public static void ApplyTo<T>(AbstractValidator<T> validator, Func<T, DriverDto> driver)
    {
        validator.RuleFor(x => driver(x).AccountId).NotEmpty();
        validator.RuleFor(x => driver(x).Name).NotEmpty().MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => driver(x).Phone)
            .MaximumLength(ColumnMetadata.DefaultPhoneNumberLength)
            .Matches(PhonePattern).When(x => !string.IsNullOrWhiteSpace(driver(x).Phone))
            .WithMessage("Invalid phone number.");
        validator.RuleFor(x => driver(x).DocumentType).MaximumLength(ColumnMetadata.DefaultFieldLength);
        validator.RuleFor(x => driver(x).DocumentNumber).MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => driver(x).EmployeeCode).MaximumLength(ColumnMetadata.DefaultNameLength);
        validator.RuleFor(x => driver(x).LicenseNumber).MaximumLength(ColumnMetadata.DefaultNameLength);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Write)]
public readonly record struct CreateDriverCommand(DriverDto Driver) : IRequest<DriverVm>;
public class CreateDriverCommandHandler(IDriverWriter writer) : IRequestHandler<CreateDriverCommand, DriverVm>
{
    public async Task<DriverVm> Handle(CreateDriverCommand request, CancellationToken cancellationToken) => await writer.CreateDriverAsync(request.Driver, cancellationToken);
}
public class CreateDriverCommandValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverCommandValidator()
    {
        DriverRules.ApplyTo(this, x => x.Driver);
        // Create-only: onboarding a driver on an already-expired license is a data-entry error. Update
        // must stay permissive so an existing record can be corrected or left alone while it lapses.
        RuleFor(x => x.Driver.LicenseExpiresAt)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime))
            .When(x => x.Driver.LicenseExpiresAt.HasValue)
            .WithMessage("The license expiration date is already past.");
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Edit)]
public readonly record struct UpdateDriverCommand(Guid DriverId, DriverDto Driver) : IRequest;
public class UpdateDriverCommandHandler(IDriverWriter writer) : IRequestHandler<UpdateDriverCommand>
{
    public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken) => await writer.UpdateDriverAsync(request.DriverId, request.Driver, cancellationToken);
}
public class UpdateDriverCommandValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverCommandValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        DriverRules.ApplyTo(this, x => x.Driver);
    }
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeactivateDriverCommand(Guid DriverId) : IRequest;
public class DeactivateDriverCommandHandler(IDriverWriter writer) : IRequestHandler<DeactivateDriverCommand>
{
    public async Task Handle(DeactivateDriverCommand request, CancellationToken cancellationToken) => await writer.DeactivateDriverAsync(request.DriverId, cancellationToken);
}
public class DeactivateDriverCommandValidator : AbstractValidator<DeactivateDriverCommand>
{
    public DeactivateDriverCommandValidator() => RuleFor(x => x.DriverId).NotEmpty();
}
