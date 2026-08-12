namespace TrackHub.Manager.Domain.Models;

public readonly record struct BackgroundJobRunVm(Guid BackgroundJobRunId, string JobKey, Guid? AccountId, string? ResourceKey, string IdempotencyKey, string Status, int Attempts, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, string? ErrorCode, string? ErrorMessage);
