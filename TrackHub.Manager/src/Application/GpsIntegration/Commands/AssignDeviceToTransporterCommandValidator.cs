namespace TrackHub.Manager.Application.GpsIntegration.Commands;

public class AssignDeviceToTransporterCommandValidator : AbstractValidator<AssignDeviceToTransporterCommand>
{
    public AssignDeviceToTransporterCommandValidator()
    {
        RuleFor(x => x.Assignment.AccountId).NotEmpty();
        RuleFor(x => x.Assignment.TransporterId).NotEmpty();
        RuleFor(x => x.Assignment.DeviceId).NotEmpty();
        RuleFor(x => x.Assignment.Priority).GreaterThanOrEqualTo(0);
    }
}
