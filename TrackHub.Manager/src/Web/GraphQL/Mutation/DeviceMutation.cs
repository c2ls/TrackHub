using TrackHub.Manager.Application.Device.Commands.Delete;
using TrackHub.Manager.Application.Device.Commands.Wipe;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<Guid> WipeDevices([Service] ISender sender, Guid operatorId, CancellationToken cancellationToken)
    {
        await sender.Send(new WipeDevicesCommand(operatorId), cancellationToken);
        return operatorId;
    }

    public async Task<Guid> DeleteDevice([Service] ISender sender, Guid deviceId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDeviceCommand(deviceId), cancellationToken);
        return deviceId;
    }
}
