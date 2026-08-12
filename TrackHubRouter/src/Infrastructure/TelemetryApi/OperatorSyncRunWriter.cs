namespace TrackHub.Router.Infrastructure.TelemetryApi;

public class OperatorSyncRunWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Telemetry)), IOperatorSyncRunWriter
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
