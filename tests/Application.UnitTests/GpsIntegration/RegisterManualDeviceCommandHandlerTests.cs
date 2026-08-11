using Common.Domain.Constants;
using Common.Domain.Enums;
using TrackHub.Manager.Application.GpsIntegration.Commands;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class RegisterManualDeviceCommandHandlerTests
{
    private Mock<IDeviceWriter> _deviceWriter = null!;
    private Mock<ITransporterReader> _transporterReader = null!;
    private Mock<ITransporterWriter> _transporterWriter = null!;
    private Mock<IGroupReader> _groupReader = null!;
    private Mock<IGroupWriter> _groupWriter = null!;
    private Mock<ITransporterGroupWriter> _transporterGroupWriter = null!;
    private Mock<ITransporterDeviceAssignmentWriter> _assignmentWriter = null!;

    private const long DefaultGroupId = 42L;

    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly Guid _deviceId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _deviceWriter = new Mock<IDeviceWriter>();
        _transporterReader = new Mock<ITransporterReader>();
        // Default: nothing to adopt -> the handler provisions a new transporter.
        _transporterReader.Setup(x => x.FindAdoptableTransporterAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        _transporterWriter = new Mock<ITransporterWriter>();
        _groupReader = new Mock<IGroupReader>();
        _groupWriter = new Mock<IGroupWriter>();
        _transporterGroupWriter = new Mock<ITransporterGroupWriter>();
        _assignmentWriter = new Mock<ITransporterDeviceAssignmentWriter>();
        _groupReader.Setup(x => x.GetGroupsByAccountAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupsPageVm([], 0));
        _groupWriter.Setup(x => x.CreateGroupAsync(It.IsAny<GroupDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupDto dto, Guid accountId, CancellationToken _) => new GroupVm(DefaultGroupId, dto.Name, dto.Description, dto.Active, accountId));
        _transporterGroupWriter.Setup(x => x.CreateTransporterGroupAsync(It.IsAny<TransporterGroupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransporterGroupDto dto, CancellationToken _) => new TransporterGroupVm(dto.TransporterId, dto.GroupId));
    }

    private RegisterManualDeviceCommandHandler CreateHandler() => new(
        _deviceWriter.Object,
        _transporterReader.Object,
        _transporterWriter.Object,
        _groupReader.Object,
        _groupWriter.Object,
        _transporterGroupWriter.Object,
        _assignmentWriter.Object);

    private DeviceDto CreateDto(string name = "ABC123", string serial = "SER-1") => new(
        _accountId,
        _operatorId,
        serial,
        name,
        0,
        null,
        (short)DeviceType.Cellular,
        null,
        null,
        null);

    private DeviceVm CreateVm(DeviceDto dto, int identifier = 7) => new(
        _deviceId,
        dto.AccountId,
        dto.OperatorId,
        dto.Serial,
        dto.Name,
        identifier,
        dto.ProviderDisplayName,
        (DeviceType)dto.DeviceTypeId,
        dto.DeviceTypeId,
        dto.Description,
        dto.ProviderMetadataHash,
        dto.ProviderStatus,
        DetectedStatus.New,
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow,
        null,
        null);

    [Test]
    public async Task Handle_AutoAssign_CreatesTransporterNamedAfterDeviceAndAssigns()
    {
        // Arrange
        var dto = CreateDto();
        var vm = CreateVm(dto);
        var transporterId = Guid.NewGuid();
        _deviceWriter.Setup(x => x.CreateManualDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        _transporterWriter.Setup(x => x.CreateTransporterAsync(
                It.Is<TransporterDto>(t => t.AccountId == _accountId
                    && t.Name == "ABC123"
                    && t.TransporterTypeId == (short)TransporterType.Asset),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransporterVm(transporterId, "ABC123", TransporterType.Asset, (short)TransporterType.Asset));

        // Act
        var result = await CreateHandler().Handle(new RegisterManualDeviceCommand(dto), CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(vm));
        _transporterGroupWriter.Verify(x => x.CreateTransporterGroupAsync(
            It.Is<TransporterGroupDto>(g => g.TransporterId == transporterId && g.GroupId == DefaultGroupId),
            It.IsAny<CancellationToken>()), Times.Once);
        _assignmentWriter.Verify(x => x.AssignAsync(
            It.Is<TransporterDeviceAssignmentDto>(a => a.AccountId == _accountId
                && a.TransporterId == transporterId
                && a.DeviceId == _deviceId
                && a.IsPrimary
                && a.AssignmentReason == "Manual registration"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AutoAssign_AdoptsExistingTransporterWithoutCreatingOne()
    {
        // Arrange
        var dto = CreateDto();
        var vm = CreateVm(dto);
        var adoptedId = Guid.NewGuid();
        _deviceWriter.Setup(x => x.CreateManualDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        _transporterReader.Setup(x => x.FindAdoptableTransporterAsync(_accountId, "ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(adoptedId);

        // Act
        await CreateHandler().Handle(new RegisterManualDeviceCommand(dto), CancellationToken.None);

        // Assert
        _transporterWriter.Verify(x => x.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _transporterGroupWriter.Verify(x => x.CreateTransporterGroupAsync(It.IsAny<TransporterGroupDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _assignmentWriter.Verify(x => x.AssignAsync(
            It.Is<TransporterDeviceAssignmentDto>(a => a.TransporterId == adoptedId
                && a.AssignmentReason == "Manual registration (adopted existing transporter)"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AutoAssignDisabled_OnlyCreatesTheDevice()
    {
        // Arrange
        var dto = CreateDto();
        var vm = CreateVm(dto);
        _deviceWriter.Setup(x => x.CreateManualDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);

        // Act
        var result = await CreateHandler().Handle(new RegisterManualDeviceCommand(dto, AutoAssign: false), CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(vm));
        _transporterReader.Verify(x => x.FindAdoptableTransporterAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _assignmentWriter.Verify(x => x.AssignAsync(It.IsAny<TransporterDeviceAssignmentDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_BlankDeviceName_NamesTransporterAfterSerial()
    {
        // Arrange: the writer echoes whatever name it stored; the transporter falls back to serial.
        var dto = CreateDto(name: "  ", serial: "IMEI-9");
        var vm = CreateVm(dto);
        _deviceWriter.Setup(x => x.CreateManualDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        _transporterWriter.Setup(x => x.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransporterVm(Guid.NewGuid(), "IMEI-9", TransporterType.Person, (short)TransporterType.Person));

        // Act
        await CreateHandler().Handle(new RegisterManualDeviceCommand(dto), CancellationToken.None);

        // Assert
        _transporterWriter.Verify(x => x.CreateTransporterAsync(
            It.Is<TransporterDto>(t => t.Name == "IMEI-9"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ExistingDefaultGroup_DoesNotCreateAnother()
    {
        // Arrange
        var dto = CreateDto();
        var vm = CreateVm(dto);
        _deviceWriter.Setup(x => x.CreateManualDeviceAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vm);
        _groupReader.Setup(x => x.GetGroupsByAccountAsync(
                _accountId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupsPageVm([new GroupVm(DefaultGroupId, GroupMetadata.DefaultGroupName, GroupMetadata.DefaultGroupDescription, true, _accountId)], 1));
        _transporterWriter.Setup(x => x.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransporterVm(Guid.NewGuid(), "ABC123", TransporterType.Person, (short)TransporterType.Person));

        // Act
        await CreateHandler().Handle(new RegisterManualDeviceCommand(dto), CancellationToken.None);

        // Assert
        _groupWriter.Verify(x => x.CreateGroupAsync(It.IsAny<GroupDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
