using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AuditEventWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAuditEventWriter
{
    public async Task<AuditEventVm> CreateAuditEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken)
    {
        // Audit events are append-only system records gated by [Authorize(Audit, Write)]. Platform-level
        // events (resource/action definitions, master flags) legitimately carry no account (Guid.Empty);
        // account scoping applies only when an account is present, so a null-account event still persists.
        var accountId = auditEvent.AccountId == Guid.Empty
            ? Guid.Empty
            : RequireAccountWriteAccess(auditEvent.AccountId);

        var entity = new AuditEvent(accountId, auditEvent.ActorType, auditEvent.ActorId, auditEvent.Action, auditEvent.ResourceType, auditEvent.ResourceId, auditEvent.Result, auditEvent.OldValuesJson, auditEvent.NewValuesJson, auditEvent.Reason, auditEvent.IpAddress, auditEvent.UserAgent, auditEvent.CorrelationId);
        await Context.AuditEvents.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return new AuditEventVm(entity.AuditEventId, entity.AccountId, entity.ActorType, entity.ActorId, entity.Action, entity.ResourceType, entity.ResourceId, entity.Result, entity.Reason, entity.IpAddress, entity.UserAgent, entity.CorrelationId, entity.OccurredAt);
    }
}
