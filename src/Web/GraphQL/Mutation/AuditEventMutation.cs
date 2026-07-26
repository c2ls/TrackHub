using TrackHub.Manager.Application.AuditEvents.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<AuditEventVm> CreateAuditEvent([Service] ISender sender, CreateAuditEventCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
}
