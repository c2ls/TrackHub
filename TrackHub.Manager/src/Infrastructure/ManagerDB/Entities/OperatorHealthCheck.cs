namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class OperatorHealthCheck(
    Guid accountId,
    Guid operatorId,
    int checkType,
    int status,
    int? latencyMs,
    DateTimeOffset startedAt,
    DateTimeOffset? completedAt,
    string? errorCode,
    string? errorMessage,
    int retryCount,
    string? correlationId)
{
    public Guid OperatorHealthCheckId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid OperatorId { get; set; } = operatorId;
    public int CheckType { get; set; } = checkType;
    public int Status { get; set; } = status;
    public int? LatencyMs { get; set; } = latencyMs;
    public DateTimeOffset StartedAt { get; set; } = startedAt;
    public DateTimeOffset? CompletedAt { get; set; } = completedAt;
    public string? ErrorCode { get; set; } = errorCode;
    public string? ErrorMessage { get; set; } = errorMessage;
    public int RetryCount { get; set; } = retryCount;
    public string? CorrelationId { get; set; } = correlationId;
}
