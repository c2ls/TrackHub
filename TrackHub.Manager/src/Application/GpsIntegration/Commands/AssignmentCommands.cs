namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Edit)]

public readonly record struct AssignDeviceToTransporterCommand(TransporterDeviceAssignmentDto Assignment) : IRequest<TransporterDeviceAssignmentVm>;
public class AssignDeviceToTransporterCommandHandler(ITransporterDeviceAssignmentWriter writer)
    : IRequestHandler<AssignDeviceToTransporterCommand, TransporterDeviceAssignmentVm>
{
    public Task<TransporterDeviceAssignmentVm> Handle(AssignDeviceToTransporterCommand request, CancellationToken cancellationToken)
        => writer.AssignAsync(request.Assignment, cancellationToken);
}
