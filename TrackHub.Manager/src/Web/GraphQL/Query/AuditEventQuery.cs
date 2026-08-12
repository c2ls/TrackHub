using TrackHub.Manager.Application.AuditEvents.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<AuditEventVm>> GetAuditTrail([Service] ISender sender, [AsParameters] GetAuditTrailQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
