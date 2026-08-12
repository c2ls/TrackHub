namespace TrackHub.Manager.Domain.Interfaces;

public interface IAuditEventReader
{
    Task<IReadOnlyCollection<AuditEventVm>> GetAuditTrailAsync(Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
}
