using Microsoft.Extensions.Logging;
using Common.Domain.Enums;
using TrackHub.Manager.Application.GpsIntegration.Commands;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class SynchronizeOperatorDevicesCommandHandlerTests
{
    private Mock<IDeviceWriter> _deviceWriter = null!;
    private Mock<IDeviceReader> _deviceReader = null!;
    private Mock<ITransporterWriter> _transporterWriter = null!;
    private Mock<ITransporterDeviceAssignmentWriter> _assignmentWriter = null!;
    private Mock<IGroupReader> _groupReader = null!;
    private Mock<IGroupWriter> _groupWriter = null!;
    private Mock<ITransporterGroupWriter> _transporterGroupWriter = null!;
    private Mock<IOperatorWriter> _operatorWriter = null!;
    private Mock<IAlertEventWriter> _alertWriter = null!;

    private const long DefaultGroupId = 42L;

    [SetUp]
    public void SetUp()
    {
        _deviceWriter = new Mock<IDeviceWriter>();
        _deviceReader = new Mock<IDeviceReader>();
        _transporterWriter = new Mock<ITransporterWriter>();
        _assignmentWriter = new Mock<ITransporterDeviceAssignmentWriter>();
        _groupReader = new Mock<IGroupReader>();
        _groupWriter = new Mock<IGroupWriter>();
        _transporterGroupWriter = new Mock<ITransporterGroupWriter>();
        _operatorWriter = new Mock<IOperatorWriter>();
        _alertWriter = new Mock<IAlertEventWriter>();
        _deviceReader.Setup(x => x.GetDevicesByOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _deviceReader.Setup(x => x.FindDuplicateSerialsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        // No existing groups -> the default "General" group is created on first use.
        _groupReader.Setup(x => x.GetGroupsByAccountAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupsPageVm([], 0));
        _groupWriter.Setup(x => x.CreateGroupAsync(It.IsAny<GroupDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupDto dto, Guid accountId, CancellationToken _) => new GroupVm(DefaultGroupId, dto.Name, dto.Description, dto.Active, accountId));
        _transporterGroupWriter.Setup(x => x.CreateTransporterGroupAsync(It.IsAny<TransporterGroupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransporterGroupDto dto, CancellationToken _) => new TransporterGroupVm(dto.TransporterId, dto.GroupId));
    }

    private SynchronizeOperatorDevicesCommandHandler CreateHandler() => new(
        _deviceWriter.Object,
        _deviceReader.Object,
        _transporterWriter.Object,
        _assignmentWriter.Object,
        _groupReader.Object,
        _groupWriter.Object,
        _transporterGroupWriter.Object,
        _operatorWriter.Object,
        _alertWriter.Object,
        Mock.Of<ILogger<SynchronizeOperatorDevicesCommandHandler>>());

    [Test]
    public async Task Handle_NewDevices_AutoCreatesTransportersAssignsToDefaultGroupAndReturnsManualCounts()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var device = new DeviceDto(
            accountId,
            operatorId,
            "SER-1",
            "Device 1",
            101,
            "Truck 101",
            (short)DeviceType.OBDScanner,
            null,
            "hash",
            "ACTIVE");
        _deviceWriter.Setup(x => x.UpsertSynchronizedDeviceAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeviceVm(
                deviceId,
                accountId,
                operatorId,
                device.Serial,
                device.Name,
                device.Identifier,
                device.ProviderDisplayName,
                DeviceType.OBDScanner,
                device.DeviceTypeId,
                device.Description,
                device.ProviderMetadataHash,
                device.ProviderStatus,
                DetectedStatus.New,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null,
                null));
        _transporterWriter.Setup(x => x.CreateTransporterAsync(
                It.Is<TransporterDto>(dto =>
                    dto.AccountId == accountId
                    && dto.Name == "Truck 101"
                    && dto.TransporterTypeId == (short)TransporterType.FleetVehicle),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransporterVm(transporterId, "Truck 101", TransporterType.FleetVehicle, (short)TransporterType.FleetVehicle));
        _assignmentWriter.Setup(x => x.AssignAsync(It.IsAny<TransporterDeviceAssignmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransporterDeviceAssignmentDto dto, CancellationToken _) => new TransporterDeviceAssignmentVm(
                Guid.NewGuid(),
                dto.AccountId,
                dto.TransporterId,
                dto.DeviceId,
                DateTimeOffset.UtcNow,
                null,
                dto.Priority,
                dto.IsPrimary,
                AssignmentStatus.Active,
                dto.AssignmentReason,
                "ServiceClient",
                "syncworker_client"));

        var result = await CreateHandler().Handle(
            new SynchronizeOperatorDevicesCommand(accountId, operatorId, [device], "corr-1", "MANUAL"),
            CancellationToken.None);

        // Manager no longer records the run (A6); it returns the counts in the VM for Router to write.
        Assert.Multiple(() =>
        {
            Assert.That(result.TriggerType, Is.EqualTo(SyncTriggerType.Manual));
            Assert.That(result.Result, Is.EqualTo(OperatorSyncResult.Succeeded));
            Assert.That(result.DevicesSeen, Is.EqualTo(1));
            Assert.That(result.DevicesAdded, Is.EqualTo(1));
            Assert.That(result.CorrelationId, Is.EqualTo("corr-1"));
        });
        _assignmentWriter.Verify(x => x.AssignAsync(
            It.Is<TransporterDeviceAssignmentDto>(dto =>
                dto.AccountId == accountId
                && dto.TransporterId == transporterId
                && dto.DeviceId == deviceId
                && dto.IsPrimary
                && dto.AssignmentReason == "Initial provider sync"),
            It.IsAny<CancellationToken>()), Times.Once);
        // The auto-provisioned transporter is placed into the account's default group (A1).
        _groupWriter.Verify(x => x.CreateGroupAsync(
            It.Is<GroupDto>(dto => dto.Name == "General"),
            accountId,
            It.IsAny<CancellationToken>()), Times.Once);
        _transporterGroupWriter.Verify(x => x.CreateTransporterGroupAsync(
            It.Is<TransporterGroupDto>(dto => dto.TransporterId == transporterId && dto.GroupId == DefaultGroupId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AutoAssignDisabled_DoesNotCreateTransporterOrAssignment()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var device = new DeviceDto(accountId, operatorId, "SER-1", "Device 1", 101, null, (short)DeviceType.OBDScanner, null, null, "ACTIVE");
        _deviceWriter.Setup(x => x.UpsertSynchronizedDeviceAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeviceVm(
                Guid.NewGuid(),
                accountId,
                operatorId,
                device.Serial,
                device.Name,
                device.Identifier,
                device.ProviderDisplayName,
                DeviceType.OBDScanner,
                device.DeviceTypeId,
                device.Description,
                device.ProviderMetadataHash,
                device.ProviderStatus,
                DetectedStatus.New,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null,
                null));

        await CreateHandler().Handle(
            new SynchronizeOperatorDevicesCommand(accountId, operatorId, [device], "corr-2", AutoAssignNewDevices: false),
            CancellationToken.None);

        _transporterWriter.Verify(x => x.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _assignmentWriter.Verify(x => x.AssignAsync(It.IsAny<TransporterDeviceAssignmentDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The default-group resolution used to SCAN the account's groups for one named "General". Now
    // that groupsByAccount is paged, a scan would only ever see page 1 — so an account whose default
    // group sat past the window would fail to find it and create a SECOND "General" on every sync.
    // The resolution must therefore go to the database by name, not filter a page in memory.
    [Test]
    public async Task Handle_ResolvesDefaultGroupByNameRatherThanScanningAPageOfGroups()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        const long existingGroupId = 777L;

        var device = new DeviceDto(
            accountId, operatorId, "SER-9", "Device 9", 109, "Truck 109",
            (short)DeviceType.OBDScanner, null, "hash", "ACTIVE");

        _deviceWriter.Setup(x => x.UpsertSynchronizedDeviceAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeviceVm(
                deviceId, accountId, operatorId, device.Serial, device.Name, device.Identifier,
                device.ProviderDisplayName, DeviceType.OBDScanner, device.DeviceTypeId, device.Description,
                device.ProviderMetadataHash, device.ProviderStatus, DetectedStatus.New,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null));

        _transporterWriter.Setup(x => x.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransporterVm(transporterId, "Truck 109", TransporterType.FleetVehicle, (short)TransporterType.FleetVehicle));

        _assignmentWriter.Setup(x => x.AssignAsync(It.IsAny<TransporterDeviceAssignmentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransporterDeviceAssignmentDto dto, CancellationToken _) => new TransporterDeviceAssignmentVm(
                Guid.NewGuid(), dto.AccountId, dto.TransporterId, dto.DeviceId, DateTimeOffset.UtcNow, null,
                dto.Priority, dto.IsPrimary, AssignmentStatus.Active, dto.AssignmentReason, "ServiceClient", "syncworker_client"));

        // The store answers the NAME-FILTERED read only. A handler that instead asked for an
        // unfiltered first page would get nothing back and wrongly create a duplicate group.
        _groupReader
            .Setup(x => x.GetGroupsByAccountAsync(
                accountId,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.Is<string?>(s => s == Common.Domain.Constants.GroupMetadata.DefaultGroupName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupsPageVm(
                [new GroupVm(existingGroupId, Common.Domain.Constants.GroupMetadata.DefaultGroupName, "d", true, accountId)], 1));

        await CreateHandler().Handle(
            new SynchronizeOperatorDevicesCommand(accountId, operatorId, [device], "corr-3", "MANUAL"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            _groupWriter.Verify(
                x => x.CreateGroupAsync(It.IsAny<GroupDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "the existing default group was not found, so the sync created a duplicate");
            _transporterGroupWriter.Verify(
                x => x.CreateTransporterGroupAsync(
                    It.Is<TransporterGroupDto>(dto => dto.GroupId == existingGroupId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }
}
