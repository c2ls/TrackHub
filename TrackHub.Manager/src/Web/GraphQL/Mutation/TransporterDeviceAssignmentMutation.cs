using TrackHub.Manager.Application.GpsIntegration.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<TransporterDeviceAssignmentVm> AssignDeviceToTransporter([Service] ISender sender, AssignDeviceToTransporterCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> EndDeviceTransporterAssignment([Service] ISender sender, EndDeviceTransporterAssignmentCommand command, CancellationToken cancellationToken)
    { await sender.Send(command, cancellationToken); return true; }
}
