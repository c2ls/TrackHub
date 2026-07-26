using TrackHub.Manager.Application.GpsIntegration.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<OperatorSyncRunVm> SynchronizeOperatorDevices([Service] ISender sender, SynchronizeOperatorDevicesCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> SetSynchronizedDeviceIgnored([Service] ISender sender, SetSynchronizedDeviceIgnoredCommand command, CancellationToken cancellationToken)
    { await sender.Send(command, cancellationToken); return true; }
}
