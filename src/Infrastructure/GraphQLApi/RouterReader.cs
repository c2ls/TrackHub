using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class RouterReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Router)), IRouterReader
{
    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Router schema.
    internal const string DevicePositionsByUserQuery = @"
            query {
                devicePositionsByUser {
                    altitude
                    address
                    deviceName
                    transporterType
                    state
                    speed
                    serverDateTime
                    longitude
                    latitude
                    eventId
                    transporterId
                    deviceDateTime
                    course
                    country
                    city
                }
            }";

    internal const string PositionsByTransporterQuery = @"
                query($transporterId: UUID!, $to: DateTime!, $from: DateTime!) {
                    positionsByTransporter(query: { transporterId: $transporterId, to: $to, from: $from }) {
                        altitude
                        address
                        deviceName
                        transporterType
                        state
                        speed
                        serverDateTime
                        longitude
                        latitude
                        eventId
                        transporterId
                        deviceDateTime
                        course
                        country
                        city
                    }
                }";

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<PositionVm>> GetDevicePositionsAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = DevicePositionsByUserQuery
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
        
    }

    public async Task<IEnumerable<PositionVm>> GetPositionsRecordAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = PositionsByTransporterQuery,
            Variables = new
            {
                transporterId = filters.GetGuid(FilterNames.Transporter),
                from = filters.GetDate(FilterNames.From),
                to = filters.GetDate(FilterNames.To)
            }
        };
        return await QueryAsync<IEnumerable<PositionVm>>(request, cancellationToken);
    }
}

