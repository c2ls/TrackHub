using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class AccountFeatureReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IAccountFeatureReader
{
    internal const string ValidateFeatureEnabledQuery = @"
                query($accountId: UUID!, $featureKey: String!) {
                    validateFeatureEnabled(query: { accountId: $accountId, featureKey: $featureKey })
                }";

    public async Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateFeatureEnabledQuery,
            Variables = new
            {
                accountId,
                featureKey
            }
        };

        var enabled = await QueryAsync<bool>(request, cancellationToken);
        if (!enabled)
        {
            throw new FeatureDisabledException(featureKey);
        }
    }
}

public class ReportAuditWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IReportAuditWriter
{
    internal const string CreateAuditEventMutation = @"
                mutation(
                    $accountId: UUID!,
                    $actorType: String!,
                    $actorId: String!,
                    $reportCode: String!,
                    $newValuesJson: String!,
                    $correlationId: String
                ) {
                    createAuditEvent(command: { auditEvent: {
                        accountId: $accountId,
                        actorType: $actorType,
                        actorId: $actorId,
                        action: ""ReportExported"",
                        resourceType: ""Report"",
                        resourceId: $reportCode,
                        result: ""Succeeded"",
                        oldValuesJson: null,
                        newValuesJson: $newValuesJson,
                        reason: null,
                        ipAddress: null,
                        userAgent: null,
                        correlationId: $correlationId
                    }}) {
                        auditEventId
                    }
                }";

    public async Task RecordReportExportAsync(
        Guid accountId,
        string actorType,
        string actorId,
        string reportCode,
        string filtersJson,
        int rowCount,
        string format,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = CreateAuditEventMutation,
            Variables = new
            {
                accountId,
                actorType,
                actorId,
                reportCode,
                newValuesJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    reportCode,
                    filters = filtersJson,
                    rowCount,
                    format
                }),
                correlationId
            }
        };

        await MutationAsync<object>(request, cancellationToken);
    }
}
