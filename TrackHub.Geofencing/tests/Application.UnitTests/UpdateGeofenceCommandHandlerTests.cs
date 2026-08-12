using TrackHub.Geofencing.Application.Geofences.Commands.Update;
using TrackHub.Geofencing.Domain.Interfaces;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Application.UnitTests.Geofences.Commands.Update;

[TestFixture]
public class UpdateGeofenceCommandHandlerTests
{
    private Mock<IGeofenceWriter> _writerMock = null!;
    private Mock<IGeofenceReader> _readerMock = null!;
    private Mock<IUserReader> _userReaderMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IAccountFeatureReader> _featureReaderMock = null!;
    private Guid _userId;
    private Guid _accountId;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IGeofenceWriter>();
        _readerMock = new Mock<IGeofenceReader>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
        _featureReaderMock = new Mock<IAccountFeatureReader>();
        _userId = Guid.NewGuid();
        _accountId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(_userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(_userId, "testuser", _accountId));
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Test]
    public async Task Handle_ShouldCallUpdateOnWriter()
    {
        // Arrange
        var dto = new GeofenceDto(Guid.NewGuid(), null, "name", null, 1, 1, true, null, null, false, false, null);
        _readerMock.Setup(r => r.GetGeofenceAsync(dto.GeofenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeofenceVm(dto.GeofenceId, _accountId, default!, "existing", null, 1, 1, true, null, null, false, false, null));
        _writerMock.Setup(w => w.UpdateGeofenceAsync(dto, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateGeofenceCommandHandler(_writerMock.Object, _readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);
        var command = new UpdateGeofenceCommand(dto);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(_accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.UpdateGeofenceAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_ShouldThrowForbiddenAccessException_WhenAccountDoesNotMatch()
    {
        var dto = new GeofenceDto(Guid.NewGuid(), null, "name", null, 1, 1, true, null, null, false, false, null);
        _readerMock.Setup(r => r.GetGeofenceAsync(dto.GeofenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeofenceVm(dto.GeofenceId, Guid.NewGuid(), default!, "existing", null, 1, 1, true, null, null, false, false, null));

        var handler = new UpdateGeofenceCommandHandler(_writerMock.Object, _readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(new UpdateGeofenceCommand(dto), CancellationToken.None));
        _writerMock.Verify(w => w.UpdateGeofenceAsync(It.IsAny<GeofenceDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

