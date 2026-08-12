using GraphQL.Client.Abstractions;

namespace TrackHub.Router.Infrastructure.TelemetryApi;

public class OperatorHealthCheckWriter : GraphQLService, IOperatorHealthCheckWriter
{
    public OperatorHealthCheckWriter(IGraphQLClientFactory graphQLClient) : base(graphQLClient.CreateClient(Clients.Telemetry)) { }

    protected OperatorHealthCheckWriter(IGraphQLClient graphQLClient) : base(graphQLClient) { }

    internal const string RecordOperatorHealthMutation = @"
                mutation($command: RecordOperatorHealthCommandInput!) {
                    recordOperatorHealth(command: $command) { operatorHealthCheckId }
                }";

    public async Task RecordAsync(OperatorHealthCheckDto dto, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RecordOperatorHealthMutation,
            Variables = new
            {
                command = new
                {
                    check = new
                    {
                        accountId = dto.AccountId,
                        operatorId = dto.OperatorId,
                        checkType = dto.CheckType,
                        status = dto.Status,
                        latencyMs = dto.LatencyMs,
                        startedAt = dto.StartedAt,
                        completedAt = dto.CompletedAt,
                        errorCode = dto.ErrorCode,
                        errorMessage = dto.ErrorMessage,
                        retryCount = dto.RetryCount,
                        correlationId = dto.CorrelationId
                    }
                }
            }
        };
        await MutationAsync<object>(request, cancellationToken);
    }
}

// Records with the Router's own service identity (never the user token) so manual,
// user-triggered health checks can persist their result.
public sealed class OperatorHealthCheckSystemWriter(IGraphQLClientFactory graphQLClient)
    : OperatorHealthCheckWriter(graphQLClient.CreateClient(Clients.Telemetry, asService: true)), IOperatorHealthCheckSystemWriter;
