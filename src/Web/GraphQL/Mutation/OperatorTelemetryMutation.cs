using TrackHub.Telemetry.Application.GpsIntegration.Commands;

namespace TrackHub.Telemetry.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<OperatorHealthCheckVm> RecordOperatorHealth([Service] ISender sender, RecordOperatorHealthCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<OperatorSyncRunVm> RecordOperatorSyncRun([Service] ISender sender, RecordOperatorSyncRunCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);
}
