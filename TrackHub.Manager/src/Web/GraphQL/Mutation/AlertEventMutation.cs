using TrackHub.Manager.Application.AlertEvents.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<AlertEventVm> RecordAlertEvent([Service] ISender sender, RecordAlertEventCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> AcknowledgeAlertEvent([Service] ISender sender, AcknowledgeAlertEventCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> ResolveAlertEvent([Service] ISender sender, ResolveAlertEventCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
