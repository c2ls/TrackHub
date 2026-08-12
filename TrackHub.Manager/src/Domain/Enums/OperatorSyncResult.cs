namespace TrackHub.Manager.Domain.Enums;

public enum OperatorSyncResult
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    PartiallySucceeded = 3,
    Throttled = 4
}
