using Common.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

// Latest-run-per-JobKey semantics for the administrator tier of the status page. The reader is
// deliberately unscoped, so these tests assert that rows from every account (and the null-AccountId
// platform jobs) participate.
[TestFixture]
public class BackgroundJobStatusReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static BackgroundJobRun Run(string jobKey, DateTimeOffset startedAt, string status = "Succeeded", Guid? accountId = null, string? errorCode = null)
        => new(jobKey, accountId, null, $"{jobKey}:{startedAt:O}:{Guid.NewGuid()}", status, 1, startedAt)
        {
            CompletedAt = startedAt.AddSeconds(1),
            ErrorCode = errorCode
        };

    [Test]
    public async Task ReturnsLatestRunPerKey_AcrossAccountsAndNullAccountIds()
    {
        var now = DateTimeOffset.UtcNow;
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        await using var context = NewContext(nameof(ReturnsLatestRunPerKey_AcrossAccountsAndNullAccountIds));
        await context.BackgroundJobRuns.AddRangeAsync(
            Run(BackgroundJobKeys.DocumentScan, now.AddMinutes(-30), accountId: accountA),
            Run(BackgroundJobKeys.DocumentScan, now.AddMinutes(-5), "Failed", accountB, "SCAN_FAILED"),
            Run(BackgroundJobKeys.AlertEvaluation, now.AddHours(-2)),
            Run(BackgroundJobKeys.AlertEvaluation, now.AddMinutes(-10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var status = await new BackgroundJobStatusReader(context).GetBackgroundJobStatusAsync(CancellationToken.None);

        var scan = status.Single(x => x.JobKey == BackgroundJobKeys.DocumentScan);
        var alert = status.Single(x => x.JobKey == BackgroundJobKeys.AlertEvaluation);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(status, Has.Count.EqualTo(2), "one row per distinct JobKey");
            Assert.That(scan.StartedAt, Is.EqualTo(now.AddMinutes(-5)), "the newest run wins regardless of account");
            Assert.That(scan.Status, Is.EqualTo("Failed"));
            Assert.That(scan.ErrorCode, Is.EqualTo("SCAN_FAILED"));
            Assert.That(alert.StartedAt, Is.EqualTo(now.AddMinutes(-10)), "null-AccountId platform jobs are included");
        }
    }

    [Test]
    public async Task FlagsOnlyAuditedPerCycleJobs()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(FlagsOnlyAuditedPerCycleJobs));
        await context.BackgroundJobRuns.AddRangeAsync(
            Run(BackgroundJobKeys.AlertEvaluation, now),
            Run(BackgroundJobKeys.NotificationDigest, now),
            Run(BackgroundJobKeys.GeofenceDwellEvaluation, now));
        await context.SaveChangesAsync(CancellationToken.None);

        var status = await new BackgroundJobStatusReader(context).GetBackgroundJobStatusAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            // Per the spec 28 §4.2 audit, alert-evaluation is the ONLY key with a guaranteed
            // recording floor — every other job records only when it actually did work, so a stale
            // timestamp there is normal and must never be rendered as a failure.
            Assert.That(status.Single(x => x.JobKey == BackgroundJobKeys.AlertEvaluation).RecordsEveryCycle, Is.True);
            Assert.That(status.Single(x => x.JobKey == BackgroundJobKeys.NotificationDigest).RecordsEveryCycle, Is.False);
            Assert.That(status.Single(x => x.JobKey == BackgroundJobKeys.GeofenceDwellEvaluation).RecordsEveryCycle, Is.False);
        }
    }

    [Test]
    public async Task IsEmptyWhenNoJobsHaveEverRun()
    {
        await using var context = NewContext(nameof(IsEmptyWhenNoJobsHaveEverRun));
        Assert.That(await new BackgroundJobStatusReader(context).GetBackgroundJobStatusAsync(CancellationToken.None), Is.Empty);
    }

    [Test]
    public async Task OrdersByJobKeyForStableRendering()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(OrdersByJobKeyForStableRendering));
        await context.BackgroundJobRuns.AddRangeAsync(
            Run(BackgroundJobKeys.TrialExpiration, now),
            Run(BackgroundJobKeys.AlertEvaluation, now),
            Run(BackgroundJobKeys.DocumentScan, now));
        await context.SaveChangesAsync(CancellationToken.None);

        var status = await new BackgroundJobStatusReader(context).GetBackgroundJobStatusAsync(CancellationToken.None);

        Assert.That(status.Select(x => x.JobKey), Is.Ordered.Using<string>(StringComparer.OrdinalIgnoreCase));
    }
}
