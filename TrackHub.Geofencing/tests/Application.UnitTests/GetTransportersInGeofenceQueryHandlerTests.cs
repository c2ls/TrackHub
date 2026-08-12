using TrackHub.Geofencing.Application.TransportersInGeofence.Queries.Get;
using TrackHub.Geofencing.Domain.Interfaces;
using Common.Application.Interfaces;

namespace TrackHub.Geofencing.Application.UnitTests.TransportersInGeofence.Queries.Get;

[TestFixture]
public class GetTransportersInGeofenceQueryHandlerTests
{
    private Mock<ITransportersInGeofence> _readerMock = null!;
    private Mock<IUserReader> _userReaderMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IAccountFeatureReader> _featureReaderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _readerMock = new Mock<ITransportersInGeofence>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
        _featureReaderMock = new Mock<IAccountFeatureReader>();
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
    }

    [Test]
    public async Task Handle_ShouldReturnTransportersForUserAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var items = new[] { new TransporterInGeofenceVm(userId, "name", Guid.NewGuid(), "gname") };
        _userReaderMock.Setup(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UserVm { UserId = userId, Username = "u", AccountId = accountId });
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var geofenceId = Guid.NewGuid();
        short type = 2;
        _readerMock.Setup(r => r.GetTransportersInGeofencesAsync(accountId, It.IsAny<Guid>(), geofenceId, type, It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var handler = new GetTransportersInGeofenceQueryHandler(_readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);
        var query = new GetTransportersInGeofenceQuery(geofenceId, type);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(items));
        _userReaderMock.Verify(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
        _readerMock.Verify(r => r.GetTransportersInGeofencesAsync(accountId, It.IsAny<Guid>(), geofenceId, type, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Constructor_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns(() => null);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => new GetTransportersInGeofenceQueryHandler(_readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object));
    }
}

