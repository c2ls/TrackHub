namespace TrackHub.Router.Infrastructure.ManagerApi;

// asService — see the note on OperatorSyncRunWriter. RecordAlertEventCommand needs Alerts/Write,
// which the Manager role does not hold (Read + Edit only) and the User role does not hold at all,
// so a sync-failure alert raised during a user-triggered sync was itself refused.
public class AlertEventWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IAlertEventWriter
{
    internal const string RecordAlertEventMutation = @"
                mutation($command: RecordAlertEventCommandInput!) {
                    recordAlertEvent(command: $command) { alertEventId }
                }";

    public async Task RecordAsync(AlertEventDto dto, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RecordAlertEventMutation,
            Variables = new
            {
                command = new
                {
                    alertEvent = new
                    {
                        accountId = dto.AccountId,
                        eventType = dto.EventType,
                        severity = dto.Severity,
                        sourceModule = dto.SourceModule,
                        resourceType = dto.ResourceType,
                        resourceId = dto.ResourceId,
                        status = dto.Status,
                        payloadJson = dto.PayloadJson,
                        deduplicationKey = dto.DeduplicationKey
                    }
                }
            }
        };
        await MutationAsync<object>(request, cancellationToken);
    }
}
