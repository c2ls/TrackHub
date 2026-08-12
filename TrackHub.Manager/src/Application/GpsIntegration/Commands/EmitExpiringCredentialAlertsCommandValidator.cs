namespace TrackHub.Manager.Application.GpsIntegration.Commands;

public class EmitExpiringCredentialAlertsCommandValidator : AbstractValidator<EmitExpiringCredentialAlertsCommand>
{
    public EmitExpiringCredentialAlertsCommandValidator()
    {
        RuleFor(x => x.WithinDays).InclusiveBetween(1, 90);
    }
}
