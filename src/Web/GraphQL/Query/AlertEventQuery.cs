using TrackHub.Manager.Application.AlertEvents.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<AlertEventVm>> GetAlertEvents([Service] ISender sender, [AsParameters] GetAlertEventsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
