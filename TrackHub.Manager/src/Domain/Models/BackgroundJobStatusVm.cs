namespace TrackHub.Manager.Domain.Models;

/// <summary>
/// The latest <c>BackgroundJobRun</c> for a distinct JobKey, platform-wide. Consumed by the
/// administrator tier of the platform status page. <see cref="RecordsEveryCycle"/> is the
/// verified recording semantic from <c>BackgroundJobKeys</c> — the portal may only assert
/// staleness for jobs where it is <c>true</c>.
/// </summary>
public readonly record struct BackgroundJobStatusVm(
    string JobKey,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int Attempts,
    string? ErrorCode,
    bool RecordsEveryCycle);
