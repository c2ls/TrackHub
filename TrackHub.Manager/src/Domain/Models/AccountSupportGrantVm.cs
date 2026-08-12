namespace TrackHub.Manager.Domain.Models;

public readonly record struct AccountSupportGrantVm(Guid AccountSupportGrantId, Guid AccountId, Guid SupportUserId, string Reason, string TicketReference, string? ApprovedBy, DateTimeOffset? ApprovedAt, string AccessLevel, DateTimeOffset StartsAt, DateTimeOffset EndsAt, DateTimeOffset? RevokedAt, string? RevokedBy, DateTimeOffset LastModified);
