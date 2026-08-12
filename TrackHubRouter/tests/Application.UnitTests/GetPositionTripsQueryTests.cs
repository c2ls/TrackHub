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

using Moq;
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Application.Positions.Queries.GetTrips;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Application.Gating;
using TrackHub.Router.Domain.Models;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Application.UnitTests;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Application.UnitTests.Positions.Queries.GetTrips;

[TestFixture]
public class GetPositionTripsQueryTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IOperatorReader> _operatorReaderMock = null!;
    private Mock<IPositionRegistry> _positionRegistryMock = null!;
    private Mock<IDeviceTransporterReader> _deviceReaderMock = null!;
    private Mock<ITransporterTypeReader> _transporterTypeReaderMock = null!;
    private Mock<IAccountModeResolver> _modeResolverMock = null!;
    private Mock<IPositionHistoryReader> _positionHistoryReaderMock = null!;
    private Mock<IGroupVisibilityReader> _groupVisibilityReaderMock = null!;
    private Mock<ICurrentPrincipal> _principalMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _positionRegistryMock = new Mock<IPositionRegistry>();
        _deviceReaderMock = new Mock<IDeviceTransporterReader>();
        _transporterTypeReaderMock = new Mock<ITransporterTypeReader>();
        _modeResolverMock = new Mock<IAccountModeResolver>();
        _positionHistoryReaderMock = new Mock<IPositionHistoryReader>();
        _groupVisibilityReaderMock = new Mock<IGroupVisibilityReader>();
        _principalMock = new Mock<ICurrentPrincipal>();

        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
    }

    private GetPositionTripsQueryHandler CreateHandler()
        => new(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterTypeReaderMock.Object,
            _modeResolverMock.Object,
            _positionHistoryReaderMock.Object,
            _groupVisibilityReaderMock.Object,
            _principalMock.Object);

    [Test]
    public async Task Handle_WithPositions_ReturnsTrips()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;

        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = transporterId, TransporterTypeId = 1 };

        var positions = new[]
        {
            new PositionVm { TransporterId = transporterId, DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(-10), Latitude = 0, Longitude = 0, Speed = 10 },
            new PositionVm { TransporterId = transporterId, DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(-5), Latitude = 0.001, Longitude = 0.001, Speed = 12 }
        };

        var readerMock = new Mock<IPositionReader>();
        readerMock.SetupGet(r => r.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(r => r.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.Setup(r => r.GetPositionAsync(from, to, device, It.IsAny<CancellationToken>())).ReturnsAsync(positions);

        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(transporterId, It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);
        _deviceReaderMock.Setup(x => x.GetDevicesTransporterAsync(transporterId, It.IsAny<CancellationToken>())).ReturnsAsync(device);
        _positionRegistryMock.Setup(x => x.GetReader(It.IsAny<ProtocolType>())).Returns(readerMock.Object);
        _transporterTypeReaderMock.Setup(x => x.GetTransporterTypeAsync(device.TransporterTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(new TransporterTypeVm(false, 5, 10, 120));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetPositionTripsQuery(transporterId, from, to), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task Handle_WithoutCredential_ReturnsEmpty()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;

        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), null);
        var device = new DeviceTransporterVm { TransporterId = transporterId, TransporterTypeId = 1 };

        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(transporterId, It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);
        _deviceReaderMock.Setup(x => x.GetDevicesTransporterAsync(transporterId, It.IsAny<CancellationToken>())).ReturnsAsync(device);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetPositionTripsQuery(transporterId, from, to), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }
}
