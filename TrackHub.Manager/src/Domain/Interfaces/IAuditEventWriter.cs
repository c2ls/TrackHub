namespace TrackHub.Manager.Domain.Interfaces;

public interface IAuditEventWriter
{
    Task<AuditEventVm> CreateAuditEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken);
}
