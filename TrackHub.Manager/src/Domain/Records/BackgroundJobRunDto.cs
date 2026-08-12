namespace TrackHub.Manager.Domain.Records;

public readonly record struct BackgroundJobRunDto(string JobKey, Guid? AccountId, string? ResourceKey, string IdempotencyKey, string Status, int Attempts, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, string? ErrorCode, string? ErrorMessage);
