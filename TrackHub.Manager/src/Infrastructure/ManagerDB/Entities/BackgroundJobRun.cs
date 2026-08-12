using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class BackgroundJobRun(string jobKey, Guid? accountId, string? resourceKey, string idempotencyKey, string status, int attempts, DateTimeOffset startedAt) : BaseEntity
{
    public Guid BackgroundJobRunId { get; private set; } = Guid.NewGuid();
    public string JobKey { get; set; } = jobKey;
    public Guid? AccountId { get; set; } = accountId;
    public string? ResourceKey { get; set; } = resourceKey;
    public string IdempotencyKey { get; set; } = idempotencyKey;
    public string Status { get; set; } = status;
    public int Attempts { get; set; } = attempts;
    public DateTimeOffset StartedAt { get; set; } = startedAt;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
