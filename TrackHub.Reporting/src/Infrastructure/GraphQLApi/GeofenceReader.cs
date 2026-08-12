using TrackHub.Reporting.Domain.Interfaces.Geofence;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Infrastructure.GraphQLApi;

public class GeofenceReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Geofence)), IGeofenceReader
{
    // Single source of truth for the queries this reader sends; the
    // ServiceContracts tests validate these exact strings against the Geofence schema.
    internal const string TransportersInGeofenceQuery = @"
                query {
                    transportersInGeofence {
                      transporterName
                      transporterId
                      geofenceName
                      geofenceId
                    }
              }";

    internal const string GeofenceEventsQuery = @"
                query($from: DateTime!, $to: DateTime!, $transporterId: UUID, $geofenceId: UUID, $skip: Int, $take: Int) {
                    geofenceEvents(query: {from: $from, to: $to, transporterId: $transporterId, geofenceId: $geofenceId, skip: $skip, take: $take}) {
                        items {
                            transporterName
                            geofenceName
                            datetimeIn
                            datetimeOut
                            totalTime
                            dwellSeconds
                            latitude
                            longitude
                        }
                        totalCount
                    }
                }";

    // Producer-side page clamp is 500; loop until the page count is reached. The row ceiling
    // is a defensive source-fetch cap — the governed export limit is enforced downstream
    // (AppSettings:Reporting), same rationale as DocumentReportReader.MaxRows.
    private const int PageSize = 500;
    private const int MaxRows = 100_000;

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TransporterInGeofenceVm>> GetTransportersInGeofenceAsync(CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = TransportersInGeofenceQuery
        };
        return await QueryAsync<IEnumerable<TransporterInGeofenceVm>>(request, cancellationToken);

    }

    /// <summary>
    /// Retrieves geofence events asynchronously filtered by date range and optional
    /// transporter/geofence, draining the producer's server-side pages.
    /// </summary>
    public async Task<IEnumerable<GeofenceEventReportVm>> GetGeofenceEventsAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var rows = new List<GeofenceEventReportVm>();

        while (rows.Count < MaxRows)
        {
            var request = new GraphQLRequest
            {
                Query = GeofenceEventsQuery,
                Variables = new
                {
                    from = filters.GetDate(FilterNames.From),
                    to = filters.GetDate(FilterNames.To),
                    transporterId = filters.GetGuid(FilterNames.Transporter),
                    geofenceId = filters.GetGuid(FilterNames.Geofence),
                    skip = rows.Count,
                    take = PageSize
                }
            };
            var page = await QueryAsync<GeofenceEventsPageVm>(request, cancellationToken);
            var items = page.Items as ICollection<GeofenceEventReportVm> ?? [.. page.Items ?? []];
            if (items.Count == 0)
                break;

            rows.AddRange(items);
            if (rows.Count >= page.TotalCount || items.Count < PageSize)
                break;
        }

        return rows;
    }
}

