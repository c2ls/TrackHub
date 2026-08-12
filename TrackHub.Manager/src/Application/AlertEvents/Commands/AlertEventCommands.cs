using TrackHub.Manager.Application.AlertEvents.Events;

namespace TrackHub.Manager.Application.AlertEvents.Commands;

// The account is nested in AlertEventDto, so this surface was invisible to the tenant guard until
// TrackHubCommon 1.0.7 widened the resolver. It is a genuine cross-tenant surface: every producer
// is a global service identity emitting an alert for whichever account owns the resource.
[Authorize(Resource = Resources.Alerts, Action = Actions.Write)]
[AllowCrossAccount("Platform alert ingestion. Geofencing (geofence_client), TripManagement (trip_client) and Router/SyncWorker (router_client/syncworker_client) each emit alerts under their own global service identity for whichever account owns the geofence/trip/operator; none of those tokens carries an account claim, so there is nothing to compare the event's AccountId against.")]
public readonly record struct RecordAlertEventCommand(AlertEventDto AlertEvent) : IRequest<AlertEventVm>;
public class RecordAlertEventCommandHandler(IAlertEventWriter writer, IPublisher publisher) : IRequestHandler<RecordAlertEventCommand, AlertEventVm>
{
    public async Task<AlertEventVm> Handle(RecordAlertEventCommand request, CancellationToken cancellationToken)
    {
        var alertEvent = await writer.RecordAlertEventAsync(request.AlertEvent, cancellationToken);
        // Rule evaluation is non-blocking: the event handler swallows its own failures.
        await publisher.Publish(new AlertEventRecorded.Notification(alertEvent), cancellationToken);
        return alertEvent;
    }
}

[Authorize(Resource = Resources.Alerts, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct AcknowledgeAlertEventCommand(Guid AlertEventId) : IRequest;
public class AcknowledgeAlertEventCommandHandler(IAlertEventWriter writer) : IRequestHandler<AcknowledgeAlertEventCommand>
{
    public async Task Handle(AcknowledgeAlertEventCommand request, CancellationToken cancellationToken) => await writer.AcknowledgeAlertEventAsync(request.AlertEventId, cancellationToken);
}

[Authorize(Resource = Resources.Alerts, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct ResolveAlertEventCommand(Guid AlertEventId) : IRequest;
public class ResolveAlertEventCommandHandler(IAlertEventWriter writer) : IRequestHandler<ResolveAlertEventCommand>
{
    public async Task Handle(ResolveAlertEventCommand request, CancellationToken cancellationToken) => await writer.ResolveAlertEventAsync(request.AlertEventId, cancellationToken);
}
