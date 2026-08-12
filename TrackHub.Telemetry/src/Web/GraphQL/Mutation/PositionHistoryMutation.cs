using TrackHub.Telemetry.Application.GpsIntegration.Commands;

namespace TrackHub.Telemetry.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<bool> AppendPositionHistory([Service] ISender sender, AppendPositionHistoryCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<int> AppendPositionHistoryBatch([Service] ISender sender, AppendPositionHistoryBatchCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> PersistResolvedAddress([Service] ISender sender, PersistResolvedAddressCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);
}
