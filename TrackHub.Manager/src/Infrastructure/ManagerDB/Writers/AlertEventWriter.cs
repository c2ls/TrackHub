using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AlertEventWriter(IApplicationDbContext context, ICurrentPrincipal principal, IAlertRecorder recorder) : AccountScopedDataAccess(context, principal), IAlertEventWriter
{
    public async Task<AlertRecordResult> RecordAlertEventAsync(AlertEventDto alertEvent, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(alertEvent.AccountId);
        await RequireResourceInAccountAsync(accountId, alertEvent.ResourceType, alertEvent.ResourceId, cancellationToken);
        return await recorder.RecordAsync(alertEvent, cancellationToken);
    }

    public async Task<int> ResolveAlertEventsAsync(Guid accountId, string resourceType, string resourceId, IReadOnlyCollection<string> eventTypes, CancellationToken cancellationToken)
    {
        RequireAccountWriteAccess(accountId);
        return await recorder.ResolveOpenAsync(accountId, resourceType, resourceId, eventTypes, cancellationToken);
    }

    public async Task AcknowledgeAlertEventAsync(Guid alertEventId, CancellationToken cancellationToken) => await UpdateStatusAsync(alertEventId, "Acknowledged", cancellationToken);
    public async Task ResolveAlertEventAsync(Guid alertEventId, CancellationToken cancellationToken) => await UpdateStatusAsync(alertEventId, "Resolved", cancellationToken);

    private async Task UpdateStatusAsync(Guid alertEventId, string status, CancellationToken cancellationToken)
    {
        var entity = await RequireScopedAsync(Context.AlertEvents.AsTracking(), x => x.AlertEventId == alertEventId, x => x.AccountId, alertEventId, forWrite: true, cancellationToken);
        if (entity.Status == status)
        {
            return;
        }

        // A resolved alert is closed; reopening it here would also collide with the open-dedup index.
        if (entity.Status == "Resolved")
        {
            throw new ConflictException("The alert is already resolved.");
        }

        entity.Status = status;
        await Context.SaveChangesAsync(cancellationToken);
    }

    // The source resource must belong to the event's account. Resource types without a
    // mapping in this context (e.g. Geofence, owned by the Geofencing service) pass through.
    //
    // DESIGN NOTE — "Trip" (spec 11 §12) deliberately falls through the `_ => true` default and MUST
    // NOT be "fixed" into a cross-service call. Manager does not store trips, so verifying one would
    // mean calling TripManagement from Manager, inverting the dependency direction for every alert
    // write. The check is not load-bearing here: the emitter is a trusted service identity
    // (`trip_client`) that has already resolved the trip and its account before emitting. The same
    // reasoning already covers "Geofence". Only resource types Manager actually owns get verified.
    private async Task RequireResourceInAccountAsync(Guid accountId, string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        var belongs = resourceType switch
        {
            "Transporter" => Guid.TryParse(resourceId, out var transporterId)
                && await Context.Transporters.AnyAsync(x => x.TransporterId == transporterId && x.AccountId == accountId, cancellationToken),
            "Operator" => Guid.TryParse(resourceId, out var operatorId)
                && await Context.Operators.AnyAsync(x => x.OperatorId == operatorId && x.AccountId == accountId, cancellationToken),
            "Document" => Guid.TryParse(resourceId, out var documentId)
                && await Context.Documents.AnyAsync(x => x.DocumentId == documentId && x.AccountId == accountId, cancellationToken),
            "Staff" => Guid.TryParse(resourceId, out var staffId)
                && await Context.StaffMembers.AnyAsync(x => x.StaffId == staffId && x.AccountId == accountId, cancellationToken),
            _ => true
        };

        if (!belongs)
        {
            throw new ForbiddenAccessException($"Alert source resource {resourceType} {resourceId} does not belong to account {accountId}.");
        }
    }
}
