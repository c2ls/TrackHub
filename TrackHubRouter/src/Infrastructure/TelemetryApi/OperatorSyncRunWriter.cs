namespace TrackHub.Router.Infrastructure.TelemetryApi;

// asService: RecordOperatorSyncRunCommand is gated on OperatorSyncRuns/Write, a SERVICE grant that
// no portal role holds (Manager and User get Read only). The propagating client forwards the
// triggering user's token, so a MANUAL sync recorded its run as that user and was refused — the run
// vanished and the caller reported failure even though the sync itself had succeeded. Background
// syncs were unaffected because the SyncWorker host registers these clients with
// headerPropagation: false, which is precisely why this only ever surfaced on manual sync.
public class OperatorSyncRunWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry, asService: true)), IOperatorSyncRunWriter
{
    internal const string RecordOperatorSyncRunMutation = @"
                mutation($command: RecordOperatorSyncRunCommandInput!) {
                    recordOperatorSyncRun(command: $command) { operatorSyncRunId }
                }";

    public async Task RecordAsync(OperatorSyncRunDto dto, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RecordOperatorSyncRunMutation,
            Variables = new
            {
                command = new
                {
                    run = new
                    {
                        accountId = dto.AccountId,
                        operatorId = dto.OperatorId,
                        triggerType = dto.TriggerType,
                        result = dto.Result,
                        startedAt = dto.StartedAt,
                        completedAt = dto.CompletedAt,
                        devicesSeen = dto.DevicesSeen,
                        devicesAdded = dto.DevicesAdded,
                        devicesUpdated = dto.DevicesUpdated,
                        devicesRemoved = dto.DevicesRemoved,
                        devicesIgnored = dto.DevicesIgnored,
                        positionsRead = dto.PositionsRead,
                        positionsAccepted = dto.PositionsAccepted,
                        positionsRejected = dto.PositionsRejected,
                        errorCode = dto.ErrorCode,
                        errorMessage = dto.ErrorMessage,
                        correlationId = dto.CorrelationId
                    }
                }
            }
        };
        await MutationAsync<object>(request, cancellationToken);
    }
}
