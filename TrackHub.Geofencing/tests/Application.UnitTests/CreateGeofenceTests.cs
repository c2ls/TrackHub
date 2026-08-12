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

using Common.Application.Interfaces;
using TrackHub.Geofencing.Application.Geofences.Commands.Create;
using TrackHub.Geofencing.Domain.Interfaces;
using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Application.UnitTests.Credentials.Command.Create;

[TestFixture]
public class CreateGeofenceCommandHandlerTests
{
    private readonly Mock<IGeofenceWriter> _geofenceWriterMock = new();
    private readonly Mock<IUserReader> _userReaderMock = new();
    private readonly Mock<IUser> _userMock = new();
    private readonly Mock<IAccountFeatureReader> _featureReaderMock = new();

    [SetUp]
    public void SetUp()
    {
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
    }

    [Test]
    public async Task Handle_ShouldCreateGeofence()
    {
        // Arrange
        var geofenceDto = new GeofenceDto();
        var command = new CreateGeofenceCommand(geofenceDto);
        var userVm = new UserVm { AccountId = Guid.NewGuid() };
        var geofenceVm = new GeofenceVm();
        var handler = new CreateGeofenceCommandHandler(_geofenceWriterMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object);

        _userReaderMock.Setup(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(userVm);
        _featureReaderMock.Setup(r => r.EnsureFeatureEnabledAsync(userVm.AccountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _geofenceWriterMock.Setup(w => w.CreateGeofenceAsync(It.IsAny<GeofenceDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(geofenceVm);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(geofenceVm));
        _userReaderMock.Verify(r => r.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _featureReaderMock.Verify(r => r.EnsureFeatureEnabledAsync(userVm.AccountId, FeatureKeys.Geofencing, It.IsAny<CancellationToken>()), Times.Once);
        _geofenceWriterMock.Verify(w => w.CreateGeofenceAsync(It.IsAny<GeofenceDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Constructor_ShouldThrowUnauthorizedAccessException_WhenUserIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns(() => null);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => new CreateGeofenceCommandHandler(_geofenceWriterMock.Object, _userReaderMock.Object, _userMock.Object, _featureReaderMock.Object));
    }
}

