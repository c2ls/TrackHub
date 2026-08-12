using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AccountSupportGrant(Guid accountId, Guid supportUserId, string reason, string ticketReference, string accessLevel, DateTimeOffset startsAt, DateTimeOffset endsAt) : BaseAuditableEntity
{
    public Guid AccountSupportGrantId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid SupportUserId { get; set; } = supportUserId;
    public string Reason { get; set; } = reason;
    public string TicketReference { get; set; } = ticketReference;
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string AccessLevel { get; set; } = accessLevel;
    public DateTimeOffset StartsAt { get; set; } = startsAt;
    public DateTimeOffset EndsAt { get; set; } = endsAt;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
}
