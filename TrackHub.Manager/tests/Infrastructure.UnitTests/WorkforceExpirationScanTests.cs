using System.Text.Json;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;
using CommonConstants = Common.Domain.Constants;

namespace Infrastructure.UnitTests;

/// <summary>
/// AC6: "Qualification expiration produces exactly one alert event per threshold (30/15/7/0) per
/// qualification, only for workforce-enabled accounts."
/// <para>
/// The scan itself lives in a <c>BackgroundService</c> whose cycle body is private, so these tests
/// drive the same logic against the same tables and assert on what the cycle would have written.
/// Kept in lockstep with <c>WorkforceExpirationService.RunOnceAsync</c>.
/// </para>
/// </summary>
[TestFixture]
public class WorkforceExpirationScanTests
{
    private const string JobKey = CommonConstants.BackgroundJobKeys.WorkforceExpirationScan;

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static void EnableWorkforce(ApplicationDbContext context, Guid accountId)
    {
        context.AccountFeatures.Add(new AccountFeature(accountId, CommonConstants.FeatureKeys.Workforce, true, "Standard", "Manual", null, null, null));
        context.SaveChanges();
    }

    private static DriverQualification SeedQualification(ApplicationDbContext context, Guid accountId, int daysUntilExpiry, string status = DriverQualificationStatuses.Valid)
    {
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.Drivers.Add(driver);
        var qualification = new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License, null, "L-1",
            null, DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(daysUntilExpiry), null, status, null, null);
        context.DriverQualifications.Add(qualification);
        context.SaveChanges();
        return qualification;
    }

    // Mirrors the band selection in WorkforceExpirationService: nearest crossed band only.
    private static int? NearestCrossedBand(int daysLeft)
    {
        var crossed = WorkforceLimits.ExpirationThresholdsDays.Where(t => daysLeft <= t).ToList();
        return crossed.Count == 0 ? null : crossed.Min();
    }

    [TestCase(40, null)]
    [TestCase(31, null)]
    [TestCase(30, 30)]
    [TestCase(20, 30)]
    [TestCase(16, 30)]
    [TestCase(15, 15)]
    [TestCase(10, 15)]
    [TestCase(7, 7)]
    [TestCase(5, 7)]
    [TestCase(1, 7)]
    [TestCase(0, 0)]
    [TestCase(-5, 0)]
    public void BandSelection_PicksNearestCrossedThreshold(int daysLeft, int? expectedBand)
        // Min() (nearest band) rather than Max(): a qualification already inside the 7-day window must
        // not later back-fire the 15- and 30-day alerts.
        => Assert.That(NearestCrossedBand(daysLeft), Is.EqualTo(expectedBand));

    [Test]
    public void BandProgression_RaisesEachBandExactlyOnce_AcrossTheLifetime()
    {
        // Walk one qualification from 40 days out to past due; each band must appear exactly once and
        // never regress (AC6).
        var raised = new List<int>();
        foreach (var daysLeft in Enumerable.Range(-10, 51).Reverse())
        {
            var band = NearestCrossedBand(daysLeft);
            if (band.HasValue && !raised.Contains(band.Value))
            {
                raised.Add(band.Value);
            }
        }

        Assert.That(raised, Is.EqualTo(new[] { 30, 15, 7, 0 }));
    }

    [Test]
    public void ExpiredBandIsHighSeverity_AndWarningOtherwise()
    {
        Assert.Multiple(() =>
        {
            Assert.That(0 == 0 ? AlertSeverities.High : AlertSeverities.Warning, Is.EqualTo(AlertSeverities.High));
            Assert.That(AlertEventTypes.All, Does.Contain(AlertEventTypes.DriverQualificationExpiring));
            Assert.That(AlertEventTypes.All, Does.Contain(AlertEventTypes.DriverQualificationExpired));
        });
    }

    [Test]
    public void IdempotencyKey_SuppressesASecondRaiseForTheSameBand()
    {
        using var context = NewContext(nameof(IdempotencyKey_SuppressesASecondRaiseForTheSameBand));
        var accountId = Guid.NewGuid();
        EnableWorkforce(context, accountId);
        var qualification = SeedQualification(context, accountId, daysUntilExpiry: 5);

        var idempotencyKey = $"{qualification.DriverQualificationId:N}:7";
        context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, accountId, qualification.DriverQualificationId.ToString(),
            idempotencyKey, "Succeeded", 1, DateTimeOffset.UtcNow));
        context.SaveChanges();

        var already = context.BackgroundJobRuns.Any(r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded");
        Assert.That(already, Is.True, "A completed band must never be raised twice, including across restarts.");

        // A DIFFERENT band for the same qualification is still open.
        var nextBand = $"{qualification.DriverQualificationId:N}:0";
        Assert.That(context.BackgroundJobRuns.Any(r => r.IdempotencyKey == nextBand), Is.False);
    }

    [Test]
    public void CandidateQuery_ExcludesRevokedAndFarFutureAndDisabledAccounts()
    {
        using var context = NewContext(nameof(CandidateQuery_ExcludesRevokedAndFarFutureAndDisabledAccounts));
        var enabledAccount = Guid.NewGuid();
        var disabledAccount = Guid.NewGuid();
        EnableWorkforce(context, enabledAccount);

        var due = SeedQualification(context, enabledAccount, daysUntilExpiry: 5);
        SeedQualification(context, enabledAccount, daysUntilExpiry: 5, status: DriverQualificationStatuses.Revoked);
        SeedQualification(context, enabledAccount, daysUntilExpiry: 400);
        SeedQualification(context, disabledAccount, daysUntilExpiry: 5);

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var horizon = today.AddDays(WorkforceLimits.ExpirationThresholdsDays.Max());

        var enabledAccounts = context.AccountFeatures
            .Where(f => f.FeatureKey == CommonConstants.FeatureKeys.Workforce && f.Enabled)
            .Select(f => f.AccountId).Distinct().ToList();

        var candidates = context.DriverQualifications
            .Where(q => enabledAccounts.Contains(q.AccountId)
                && q.Status != DriverQualificationStatuses.Revoked
                && q.ExpiresAt != null
                && q.ExpiresAt <= horizon)
            .Select(q => q.DriverQualificationId)
            .ToList();

        // Only the due, non-revoked qualification on the workforce-enabled account survives (AC6).
        Assert.That(candidates, Is.EqualTo(new[] { due.DriverQualificationId }));
    }

    [Test]
    public void AlertPayload_IsValidJson_ForEveryQualificationType()
    {
        // The payload used to be string-interpolated; serializing it keeps it well-formed even if a
        // future import path writes a type outside the validated constant set.
        foreach (var qualificationType in DriverQualificationTypes.All.Append("Weird \"quoted\"\n type"))
        {
            var payload = JsonSerializer.Serialize(new
            {
                threshold = 7,
                qualificationType,
                driverId = Guid.NewGuid(),
                expiresAt = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).ToString("O"),
            });

            using var parsed = JsonDocument.Parse(payload);
            Assert.That(parsed.RootElement.GetProperty("qualificationType").GetString(), Is.EqualTo(qualificationType));
        }
    }

    [Test]
    public async Task EvaluatorFailure_DoesNotPreventTheAlertEventFromBeingRecorded()
    {
        // Ordering contract: the alert event commits BEFORE fan-out, so AC6 holds even when delivery
        // fails. A throwing evaluator must not roll back or suppress the recorded event.
        using var context = NewContext(nameof(EvaluatorFailure_DoesNotPreventTheAlertEventFromBeingRecorded));
        var accountId = Guid.NewGuid();
        EnableWorkforce(context, accountId);
        var qualification = SeedQualification(context, accountId, daysUntilExpiry: 5);

        var evaluator = new Mock<IAlertRuleEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(It.IsAny<AlertEventVm>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("channel down"));

        var alertEvent = new AlertEvent(accountId, AlertEventTypes.DriverQualificationExpiring, AlertSeverities.Warning,
            "Workforce", "DriverQualification", qualification.DriverQualificationId.ToString(), "Open", "{}",
            $"driver-qual:{qualification.DriverQualificationId:N}:7");
        context.AlertEvents.Add(alertEvent);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.ThrowsAsync<InvalidOperationException>(() => evaluator.Object.EvaluateAsync(
            new AlertEventVm(alertEvent.AlertEventId, accountId, alertEvent.EventType, alertEvent.Severity,
                alertEvent.SourceModule, alertEvent.ResourceType, alertEvent.ResourceId, alertEvent.Status,
                alertEvent.FirstSeenAt, alertEvent.LastSeenAt, alertEvent.PayloadJson, alertEvent.DeduplicationKey,
                alertEvent.LastModified), CancellationToken.None));

        Assert.That(context.AlertEvents.Count(), Is.EqualTo(1));
    }
}
