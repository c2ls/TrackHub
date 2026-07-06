using TrackHub.Telemetry.Application.GpsIntegration.Commands;

namespace TrackHub.Telemetry.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<int> PurgeExpiredPositionHistory([Service] ISender sender, PurgeExpiredPositionHistoryCommand command)
        => await sender.Send(command);

    public async Task<bool> AppendPositionHistory([Service] ISender sender, AppendPositionHistoryCommand command)
        => await sender.Send(command);

    public async Task<int> AppendPositionHistoryBatch([Service] ISender sender, AppendPositionHistoryBatchCommand command)
        => await sender.Send(command);

    public async Task<bool> PersistResolvedAddress([Service] ISender sender, PersistResolvedAddressCommand command)
        => await sender.Send(command);
}
