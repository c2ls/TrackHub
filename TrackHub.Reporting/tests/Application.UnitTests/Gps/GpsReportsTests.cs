using Moq;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Application.Report.Factory.Gps;
using TrackHub.Reporting.Application.UnitTests;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models.Manager;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.UnitTests.Gps;

[TestFixture]
public class GpsReportsTests
{
    private Mock<IUser> _user = null!;
    private Mock<IAccountFeatureReader> _features = null!;
    private Mock<IGpsManagerReader> _manager = null!;
    private Mock<IGpsTelemetryReader> _telemetry = null!;
    private Mock<IReportNameReader> _names = null!;
    private ReportingLimitsOptions _limits = null!;
    private Guid _accountId;
    private FilterDto _filters;

    [SetUp]
    public void SetUp()
    {
        _names = new Mock<IReportNameReader>();
        _names.Setup(n => n.GetTransporterNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetOperatorNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetDeviceSerialsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _names.Setup(n => n.GetStaffNamesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string>());
        _user = new Mock<IUser>();
        _features = new Mock<IAccountFeatureReader>();
        _manager = new Mock<IGpsManagerReader>();
        _telemetry = new Mock<IGpsTelemetryReader>();
        _limits = new ReportingLimitsOptions();
        _accountId = Guid.NewGuid();
        _user.Setup(u => u.AccountId).Returns(_accountId);
        _features.Setup(f => f.EnsureFeatureEnabledAsync(_accountId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _filters = new FilterDto { Language = "en", Name = "Test" };
    }

    private static IReadOnlyCollection<T> List<T>(params T[] items) => items;

    [Test]
    public void Codes_ShouldMatchSpecConstants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new GpsProviderHealthSummaryReport(_manager.Object, _telemetry.Object).ReportCode, Is.EqualTo("gps.provider-health-summary"));
            Assert.That(new GpsProviderSyncHistoryReport(_user.Object, _features.Object, _telemetry.Object, _names.Object, _limits).ReportCode, Is.EqualTo("gps.provider-sync-history"));
            Assert.That(new GpsSyncStatisticsReport(_user.Object, _features.Object, _manager.Object, _telemetry.Object, _limits).ReportCode, Is.EqualTo("gps.sync-statistics"));
            Assert.That(new GpsSynchronizedDeviceInventoryReport(_user.Object, _features.Object, _manager.Object).ReportCode, Is.EqualTo("gps.synchronized-device-inventory"));
            Assert.That(new GpsRecentlyAddedDevicesReport(_user.Object, _features.Object, _manager.Object).ReportCode, Is.EqualTo("gps.recently-added-devices"));
            Assert.That(new GpsUnassignedDevicesReport(_user.Object, _features.Object, _manager.Object).ReportCode, Is.EqualTo("gps.unassigned-devices"));
            Assert.That(new GpsIgnoredDevicesReport(_user.Object, _features.Object, _manager.Object).ReportCode, Is.EqualTo("gps.ignored-devices"));
            Assert.That(new GpsAssignmentHistoryReport(_user.Object, _features.Object, _manager.Object, _names.Object).ReportCode, Is.EqualTo("gps.assignment-history"));
            Assert.That(new GpsLatestPositionFreshnessReport(_user.Object, _features.Object, _manager.Object, _telemetry.Object).ReportCode, Is.EqualTo("gps.latest-position-freshness"));
            Assert.That(new GpsPositionHistoryReport(_user.Object, _features.Object, _telemetry.Object, _names.Object, _limits).ReportCode, Is.EqualTo("gps.position-history"));
        }
    }

    [Test]
    public async Task ProviderHealthSummary_ShouldAggregatePerOperator()
    {
        var opId = Guid.NewGuid();
        _manager.Setup(m => m.GetOperatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(new ManagerOperatorVm(opId, "Op1", true)));
        _telemetry.Setup(m => m.GetOperatorHealthSummaryAsync(opId, 24, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerOperatorHealthSummaryVm(opId, DateTimeOffset.UtcNow.AddDays(-1), 10, 9, 0, 0, 1, 0.9, 25.5, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "TIMEOUT"));

        var report = new GpsProviderHealthSummaryReport(_manager.Object, _telemetry.Object);
        var result = await report.GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
        Assert.That(result.Cell(0, "OperatorName"), Is.EqualTo("Op1"));
        _telemetry.Verify(m => m.GetOperatorHealthSummaryAsync(opId, 24, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PositionHistory_ShouldRequireGpsPositionHistoryFeature()
    {
        _features.Setup(f => f.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.GpsPositionHistory, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FeatureDisabledException(FeatureKeys.GpsPositionHistory));
        var report = new GpsPositionHistoryReport(_user.Object, _features.Object, _telemetry.Object, _names.Object, _limits);
        await Assert.ThrowsAsync<FeatureDisabledException>(() => report.GetDatasetAsync(_filters, CancellationToken.None));
        _telemetry.Verify(m => m.GetPositionHistoryAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UnassignedDevices_ShouldRequireGpsIntegrationFeature()
    {
        _features.Setup(f => f.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.GpsIntegration, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FeatureDisabledException(FeatureKeys.GpsIntegration));
        var report = new GpsUnassignedDevicesReport(_user.Object, _features.Object, _manager.Object);
        await Assert.ThrowsAsync<FeatureDisabledException>(() => report.GetDatasetAsync(_filters, CancellationToken.None));
        _manager.Verify(m => m.GetUnassignedDevicesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Report_ShouldThrowUnauthorized_WhenAccountClaimMissing()
    {
        _user.Setup(u => u.AccountId).Returns((Guid?)null);
        var report = new GpsUnassignedDevicesReport(_user.Object, _features.Object, _manager.Object);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => report.GetDatasetAsync(_filters, CancellationToken.None));
        _features.Verify(f => f.EnsureFeatureEnabledAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RecentlyAddedDevices_ShouldFilterByFirstSeenCutoff()
    {
        var opId = Guid.NewGuid();
        var oldDevice = new ManagerDeviceVm(Guid.NewGuid(), _accountId, opId, "serial-old", "Old", 1, null, null, "AVAILABLE", DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow, null, null);
        var newDevice = new ManagerDeviceVm(Guid.NewGuid(), _accountId, opId, "serial-new", "New", 2, null, null, "AVAILABLE", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, null, null);
        _manager.Setup(m => m.GetSynchronizedDevicesAsync(_accountId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(oldDevice, newDevice));
        _manager.Setup(m => m.GetOperatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(new ManagerOperatorVm(opId, "Op1", true)));

        var filters = _filters with { Values = FilterValues.Of((FilterNames.WithinDays, "7")) };
        var report = new GpsRecentlyAddedDevicesReport(_user.Object, _features.Object, _manager.Object);
        var result = await report.GetDatasetAsync(filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
    }

    [Test]
    public async Task SyncHistory_ShouldComputeFailedDevicesResidual()
    {
        var opId = Guid.NewGuid();
        var run = new ManagerOperatorSyncRunVm(Guid.NewGuid(), _accountId, opId, "SCHEDULED", "PARTIAL",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(2),
            DevicesSeen: 10, DevicesAdded: 2, DevicesUpdated: 3, DevicesRemoved: 0, DevicesIgnored: 1,
            PositionsRead: 100, PositionsAccepted: 95, PositionsRejected: 5, ErrorCode: null, ErrorMessage: null, CorrelationId: null);
        _telemetry.Setup(m => m.GetOperatorSyncRunFeedAsync(_accountId, null, It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerOperatorSyncRunPageVm(List(run), false, null));
        _names.Setup(n => n.GetOperatorNamesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [opId] = "Op1" });

        var report = new GpsProviderSyncHistoryReport(_user.Object, _features.Object, _telemetry.Object, _names.Object, _limits);
        var result = await report.GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
        Assert.That(result.Cell(0, "Operator"), Is.EqualTo("Op1"));
    }

    [Test]
    public async Task AssignmentHistory_ShowsUnitNamesAndDeviceSerials()
    {
        var unit = Guid.NewGuid();
        var device = Guid.NewGuid();
        var unknownDevice = Guid.NewGuid();
        _manager.Setup(m => m.GetAssignmentsByAccountAsync(_accountId, false, It.IsAny<CancellationToken>())).ReturnsAsync(List(
            new ManagerTransporterDeviceAssignmentVm(Guid.NewGuid(), _accountId, unit, device, DateTimeOffset.UtcNow, null, 1, true, "Active", null),
            new ManagerTransporterDeviceAssignmentVm(Guid.NewGuid(), _accountId, unit, unknownDevice, DateTimeOffset.UtcNow.AddDays(-1), null, 2, false, "Active", null)));
        _names.Setup(n => n.GetTransporterNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [unit] = "AAA000" });
        _names.Setup(n => n.GetDeviceSerialsAsync(_accountId, It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [device] = "SN-1" });

        var result = await new GpsAssignmentHistoryReport(_user.Object, _features.Object, _manager.Object, _names.Object).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Cell(0, "TransporterName"), Is.EqualTo("AAA000"));
        Assert.That(result.Cell(0, "Serial"), Is.EqualTo("SN-1"));
        Assert.That(result.Cell(1, "Serial"), Is.EqualTo(unknownDevice.ToString()));
    }

    [Test]
    public async Task PositionHistory_ShowsUnitNamesAndDeviceSerials_WithoutTheAccountColumn()
    {
        var unit = Guid.NewGuid();
        var device = Guid.NewGuid();
        _telemetry.Setup(t => t.GetPositionHistoryAsync(_accountId, null, null, It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerTransporterPositionHistoryPageVm(
                List(new ManagerTransporterPositionHistoryVm(Guid.NewGuid(), _accountId, Guid.NewGuid(), device, unit, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 4.6, -74.1)),
                false, null));
        _names.Setup(n => n.GetTransporterNamesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [unit] = "AAA000" });
        _names.Setup(n => n.GetDeviceSerialsAsync(_accountId, It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<Guid, string> { [device] = "SN-1" });

        var result = await new GpsPositionHistoryReport(_user.Object, _features.Object, _telemetry.Object, _names.Object, _limits).GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.Columns.Select(c => c.PropertyName), Is.EqualTo(new[] { "TransporterName", "DeviceDateTime", "Latitude", "Longitude", "Serial" }));
        Assert.That(result.Cell(0, "TransporterName"), Is.EqualTo("AAA000"));
        Assert.That(result.Cell(0, "Serial"), Is.EqualTo("SN-1"));
    }

    [Test]
    public async Task IgnoredDevices_ShouldOnlyReturnIgnoredStatus()
    {
        var opId = Guid.NewGuid();
        var ignored = new ManagerDeviceVm(Guid.NewGuid(), _accountId, opId, "i1", "Ign", 1, null, null, "IGNORED", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow);
        _manager.Setup(m => m.GetSynchronizedDevicesAsync(_accountId, "IGNORED", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(ignored));
        _manager.Setup(m => m.GetOperatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(List(new ManagerOperatorVm(opId, "Op1", true)));

        var report = new GpsIgnoredDevicesReport(_user.Object, _features.Object, _manager.Object);
        var result = await report.GetDatasetAsync(_filters, CancellationToken.None);

        Assert.That(result.RowCount, Is.EqualTo(1));
        _manager.Verify(m => m.GetSynchronizedDevicesAsync(_accountId, "IGNORED", null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
