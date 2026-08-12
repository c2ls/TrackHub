namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class OperatorSyncRun(
    Guid accountId,
    Guid operatorId,
    int triggerType,
    int result,
    DateTimeOffset startedAt)
{
    public Guid OperatorSyncRunId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid OperatorId { get; set; } = operatorId;
    public int TriggerType { get; set; } = triggerType;
    public int Result { get; set; } = result;
    public DateTimeOffset StartedAt { get; set; } = startedAt;
    public DateTimeOffset? CompletedAt { get; set; }
    public int DevicesSeen { get; set; }
    public int DevicesAdded { get; set; }
    public int DevicesUpdated { get; set; }
    public int DevicesRemoved { get; set; }
    public int DevicesIgnored { get; set; }
    public int PositionsRead { get; set; }
    public int PositionsAccepted { get; set; }
    public int PositionsRejected { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CorrelationId { get; set; }
}
