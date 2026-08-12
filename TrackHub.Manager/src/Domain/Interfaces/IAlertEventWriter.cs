namespace TrackHub.Manager.Domain.Interfaces;

public interface IAlertEventWriter
{
    Task<AlertEventVm> RecordAlertEventAsync(AlertEventDto alertEvent, CancellationToken cancellationToken);
    Task AcknowledgeAlertEventAsync(Guid alertEventId, CancellationToken cancellationToken);
    Task ResolveAlertEventAsync(Guid alertEventId, CancellationToken cancellationToken);
}
