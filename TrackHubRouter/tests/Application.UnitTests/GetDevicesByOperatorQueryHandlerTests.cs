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

using Application.UnitTests;
using Common.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Moq;
using TrackHub.Router.Application.Devices.Queries.GetByOperator;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Application.UnitTests.Devices.Queries.GetByOperator;

[TestFixture]
public class GetDevicesByOperatorQueryHandlerTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<IOperatorReader> _operatorReaderMock;
    private Mock<IDeviceRegistry> _deviceRegistryMock;

    private GetDevicesByOperatorQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _deviceRegistryMock = new Mock<IDeviceRegistry>();

        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        _handler = new GetDevicesByOperatorQueryHandler(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _deviceRegistryMock.Object);
    }

    [Test]
    public async Task Handle_WithValidOperatorId_ReturnsDevices()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm
        {
            OperatorId = operatorId,
            ProtocolTypeId = (int)ProtocolType.CommandTrack,
            Credential = TestCredentialTokenVm,
            Enabled = true
        };
        var devices = new List<DeviceVm>
        {
            new() { DeviceId = Guid.NewGuid(), Name = "Device 1" },
            new() { DeviceId = Guid.NewGuid(), Name = "Device 2" }
        };

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@operator);

        _deviceRegistryMock.Setup(x => x.GetReader(ProtocolType.CommandTrack))
            .Returns(Mock.Of<IExternalDeviceReader>());

        var readerMock = new Mock<IExternalDeviceReader>();
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(devices);

        _deviceRegistryMock.Setup(x => x.GetReader(ProtocolType.CommandTrack))
            .Returns(readerMock.Object);

        // Act
        var result = await _handler.Handle(new GetDevicesByOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(devices.Count));
    }

    [Test]
    public void Handle_WithInvalidOperator_ThrowsException()
    {
        // Arrange
        var operatorId = Guid.NewGuid();

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .Throws(new ArgumentException());

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _handler.Handle(new GetDevicesByOperatorQuery(operatorId), CancellationToken.None));
    }

    [Test]
    public async Task Handle_WithNullCredential_ReturnsEmptyDevices()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm
        {
            OperatorId = operatorId,
            ProtocolTypeId = (int)ProtocolType.CommandTrack,
            Credential = null,
            Enabled = true
        };

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@operator);

        // Act
        var result = await _handler.Handle(new GetDevicesByOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Handle_WithDisabledOperator_ReturnsEmptyDevices()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm
        {
            OperatorId = operatorId,
            ProtocolTypeId = (int)ProtocolType.CommandTrack,
            Credential = TestCredentialTokenVm,
            Enabled = false
        };

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@operator);

        // Act
        var result = await _handler.Handle(new GetDevicesByOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
        _deviceRegistryMock.Verify(x => x.GetReader(It.IsAny<ProtocolType>()), Times.Never);
    }
}
