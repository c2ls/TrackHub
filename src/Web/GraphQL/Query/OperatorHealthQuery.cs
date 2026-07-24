using TrackHub.Telemetry.Application.GpsIntegration.Queries;

namespace TrackHub.Telemetry.Web.GraphQL.Query;

public partial class Query
{
    public async Task<OperatorHealthVm> GetOperatorHealth([Service] ISender sender, [AsParameters] GetOperatorHealthQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<OperatorHealthCheckVm>> GetOperatorHealthHistory([Service] ISender sender, [AsParameters] GetOperatorHealthHistoryQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<OperatorHealthSummaryVm> GetOperatorHealthSummary([Service] ISender sender, [AsParameters] GetOperatorHealthSummaryQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
