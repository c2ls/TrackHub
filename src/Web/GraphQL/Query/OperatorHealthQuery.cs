using TrackHub.Telemetry.Application.GpsIntegration.Queries;

namespace TrackHub.Telemetry.Web.GraphQL.Query;

public partial class Query
{
    public async Task<OperatorHealthVm> GetOperatorHealth([Service] ISender sender, [AsParameters] GetOperatorHealthQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<OperatorHealthCheckVm>> GetOperatorHealthHistory([Service] ISender sender, [AsParameters] GetOperatorHealthHistoryQuery query)
        => await sender.Send(query);

    public async Task<OperatorHealthSummaryVm> GetOperatorHealthSummary([Service] ISender sender, [AsParameters] GetOperatorHealthSummaryQuery query)
        => await sender.Send(query);
}
