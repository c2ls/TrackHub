using TrackHub.Geofencing.Application.Geofences.Queries.GetByAccount;
using TrackHub.Geofencing.Domain.Interfaces;
using Common.Application.Interfaces;

namespace TrackHub.Geofencing.Application.UnitTests.Geofences.Queries.GetByAccount;

[TestFixture]
public class GetGeofencesByAccountQueryHandlerTests
{
    private Mock<IGeofenceReader> _readerMock = null!;
    private Mock<IUserReader> _userReaderMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IAccountFeatureReader> _featureReaderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _readerMock = new Mock<IGeofenceReader>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
        _featureReaderMock = new Mock<IAccountFeatureReader>();
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
    }

    [Test]
    public async Task Handle_ShouldReturnGeofencePageForUserAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var geofences = new[] { new GeofenceVm(Guid.NewGuid(), accountId, new MultiPolygonVm([], 4326), "name", null, 1, 1, true, null, null, false, false, null) };
        var page = new GeofencesPageVm(geofences, 1);
        _userReaderMock.Setup(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UserVm { AccountId = accountId });
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _readerMock.Setup(r => r.GetGeofencesPageAsync(accountId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<short?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var handler = new GetGeofencesByAccountQueryHandler(_readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);
        var query = new GetGeofencesByAccountQuery(true, null, null, null, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items, Is.EqualTo(geofences));
        _userReaderMock.Verify(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
        // Defaults clamp to skip 0, take 50.
        _readerMock.Verify(r => r.GetGeofencesPageAsync(accountId, 0, 50, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldClampSkipAndTake_AndForwardFilters()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var page = new GeofencesPageVm([], 0);
        _userReaderMock.Setup(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UserVm { AccountId = accountId });
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(accountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _readerMock.Setup(r => r.GetGeofencesPageAsync(accountId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<short?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var handler = new GetGeofencesByAccountQueryHandler(_readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);
        // Skip below 0 clamps to 0; Take above 500 clamps to 500.
        var query = new GetGeofencesByAccountQuery(true, -5, 1000, (short)2, true, "warehouse");

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _readerMock.Verify(r => r.GetGeofencesPageAsync(accountId, 0, 500, (short)2, true, "warehouse", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Constructor_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns(() => null);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => new GetGeofencesByAccountQueryHandler(_readerMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object));
    }
}

