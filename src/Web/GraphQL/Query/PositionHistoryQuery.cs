using TrackHub.Telemetry.Application.GpsIntegration.Queries;

namespace TrackHub.Telemetry.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<TransporterPositionHistoryVm>> GetPositionHistory([Service] ISender sender, [AsParameters] GetPositionHistoryQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<TransporterPositionHistoryVm>> GetPositionHistoryRange([Service] ISender sender, [AsParameters] GetPositionHistoryRangeQuery query)
        => await sender.Send(query);
}
