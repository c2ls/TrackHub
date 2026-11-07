// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Domain.Enums;
using Common.Domain.Time;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Application.Accounts.Events;
using TrackHub.Manager.Application.BackgroundJobs;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using Common.Domain.Constants;

namespace Application.UnitTests.BackgroundJobs;

// Trial-end resolution and the qualification bands had no unit-test surface while they lived in the
// Web layer against a raw DbContext.
[TestFixture]
public class TrialAndWorkforceJobTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 9, 14);

    [Test]
    public void TrialEnd_TakesTheEarliestAcrossFeatures()
    {
        var resolved = TrialExpirationJob.ResolveTrialEnd(
        [
            new TrialFeatureVm("trial", Now.AddDays(30)),
            new TrialFeatureVm("trial", Now.AddDays(5)),
        ]);

        Assert.That(resolved, Is.EqualTo(Now.AddDays(5)));
    }

    [Test]
    public void TrialEnd_NonTrialTier_IsIgnored()
        => Assert.That(TrialExpirationJob.ResolveTrialEnd([new TrialFeatureVm("standard", Now.AddDays(5))]), Is.Null);

    [Test]
    public async Task Trial_NotYetExpired_IsLeftAlone()
    {
        var store = new Mock<ITrialExpirationStore>();
        store.Setup(s => s.GetTrialAccountsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([AccountId]);
        store.Setup(s => s.GetAccountFeaturesAsync(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TrialFeatureVm("trial", Now.AddDays(3))]);

        await new TrialExpirationJob(store.Object, Mock.Of<IPublisher>(), Mock.Of<ILogger<TrialExpirationJob>>())
            .RunOnceAsync(Now, CancellationToken.None);

        store.Verify(s => s.SuspendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Trial_Expired_SuspendsAndPublishesTheStatusChange()
    {
        var trialEnd = Now.AddDays(-1);
        var store = new Mock<ITrialExpirationStore>();
        store.Setup(s => s.GetTrialAccountsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([AccountId]);
        store.Setup(s => s.GetAccountFeaturesAsync(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TrialFeatureVm("trial", trialEnd)]);
        store.Setup(s => s.SuspendAsync(AccountId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var publisher = new Mock<IPublisher>();

        await new TrialExpirationJob(store.Object, publisher.Object, Mock.Of<ILogger<TrialExpirationJob>>())
            .RunOnceAsync(Now, CancellationToken.None);

        store.Verify(s => s.SuspendAsync(AccountId, TrialExpirationJob.SuspensionReason,
            $"trial-expire:{AccountId:N}:{trialEnd:yyyyMMdd}", Now, It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.Publish(
            It.Is<AccountStatusChanged.Notification>(n => n.AccountId == AccountId && n.NewStatus == AccountStatus.Suspended),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Trial_StatusChangedSinceSelection_PublishesNothing()
    {
        var store = new Mock<ITrialExpirationStore>();
        store.Setup(s => s.GetTrialAccountsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([AccountId]);
        store.Setup(s => s.GetAccountFeaturesAsync(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TrialFeatureVm("trial", Now.AddDays(-1))]);
        store.Setup(s => s.SuspendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var publisher = new Mock<IPublisher>();

        await new TrialExpirationJob(store.Object, publisher.Object, Mock.Of<ILogger<TrialExpirationJob>>())
            .RunOnceAsync(Now, CancellationToken.None);

        publisher.Verify(p => p.Publish(It.IsAny<AccountStatusChanged.Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Qualification_RaisesOnlyTheNearestCrossedBand()
        => Assert.That(WorkforceExpirationJob.ThresholdFor(Today.AddDays(5), Today), Is.EqualTo(7));

    [Test]
    public void Qualification_AlreadyDue_IsTheZeroBand()
        => Assert.That(WorkforceExpirationJob.ThresholdFor(Today.AddDays(-2), Today), Is.EqualTo(0));

    [Test]
    public void Qualification_OutsideEveryBand_RaisesNothing()
        => Assert.That(WorkforceExpirationJob.ThresholdFor(Today.AddDays(60), Today), Is.Null);

    [Test]
    public async Task Qualification_BandsAreDecidedAgainstTheAccountCalendar()
    {
        // Midnight UTC on the 15th is still the 14th in Bogota, so the qualification expiring on the
        // 21st is 7 days out there and 6 in UTC — the account's calendar is the one that counts.
        var now = new DateTimeOffset(2026, 9, 15, 0, 30, 0, TimeSpan.Zero);
        var qualification = new ExpiringQualificationVm(Guid.NewGuid(), AccountId, Guid.NewGuid(), "License", new DateOnly(2026, 9, 21));

        var features = new Mock<IAccountFeatureGate>();
        features.Setup(f => f.EnabledAccountsAsync(FeatureKeys.Workforce, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([AccountId]);
        var store = new Mock<IWorkforceExpirationStore>();
        store.Setup(s => s.GetExpiringAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([qualification]);
        store.Setup(s => s.JobRunSucceededAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var zones = new Mock<IAccountTimeZoneResolver>();
        var recorder = new Mock<IAlertRecorder>();
        zones.Setup(z => z.ResolveAsync(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountTimeZone.For("America/Bogota"));

        await new WorkforceExpirationJob(features.Object, store.Object, zones.Object,
                recorder.Object, Mock.Of<ILogger<WorkforceExpirationJob>>())
            .RunOnceAsync(now, CancellationToken.None);

        store.Verify(s => s.RecordJobRunAsync(AccountId, It.IsAny<string>(),
            $"{qualification.StaffQualificationId:N}:20260921:7", now, It.IsAny<CancellationToken>()), Times.Once);
        recorder.Verify(r => r.RecordAsync(It.Is<AlertEventDto>(a => a.DeduplicationKey == $"staff-qual:{qualification.StaffQualificationId:N}:20260921:7"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Qualification_ThresholdAlreadyNotified_RaisesNothing()
    {
        var qualification = new ExpiringQualificationVm(Guid.NewGuid(), AccountId, Guid.NewGuid(), "License", Today.AddDays(5));
        var features = new Mock<IAccountFeatureGate>();
        features.Setup(f => f.EnabledAccountsAsync(FeatureKeys.Workforce, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([AccountId]);
        var store = new Mock<IWorkforceExpirationStore>();
        store.Setup(s => s.GetExpiringAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([qualification]);
        store.Setup(s => s.JobRunSucceededAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var zones = new Mock<IAccountTimeZoneResolver>();
        zones.Setup(z => z.ResolveAsync(AccountId, It.IsAny<CancellationToken>())).ReturnsAsync(AccountTimeZone.Utc);
        var recorder = new Mock<IAlertRecorder>();

        await new WorkforceExpirationJob(features.Object, store.Object, zones.Object, recorder.Object,
                Mock.Of<ILogger<WorkforceExpirationJob>>())
            .RunOnceAsync(Now, CancellationToken.None);

        store.Verify(s => s.RecordJobRunAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
        recorder.Verify(r => r.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
