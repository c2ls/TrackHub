namespace TrackHub.Manager.Domain.Records;

public readonly record struct AccountSupportGrantDto(Guid AccountId, Guid SupportUserId, string Reason, string TicketReference, string AccessLevel, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
