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
using Moq;
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Application.Devices.Queries.Get;
using TrackHub.Router.Application.Devices.Queries.GetByOperator;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Models;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Application.UnitTests.Devices.Queries.Get;

[TestFixture]
public class GetDevicesQueriesTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IDeviceRegistry> _deviceRegistryMock = null!;
    private Mock<IDeviceTransporterReader> _deviceReaderMock = null!;
    private Mock<IOperatorReader> _operatorReaderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _deviceRegistryMock = new Mock<IDeviceRegistry>();
        _deviceReaderMock = new Mock<IDeviceTransporterReader>();
        _operatorReaderMock = new Mock<IOperatorReader>();

        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
    }

    [Test]
    public async Task GetDevicesByOperator_WithCredential_ReturnsDevices()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var devices = new[] { new DeviceVm { DeviceId = Guid.NewGuid(), Name = "Device 1" } };

        var readerMock = new Mock<IExternalDeviceReader>();
        readerMock.SetupGet(r => r.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicesAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>())).ReturnsAsync(devices);
        readerMock.Setup(x => x.GetDevicesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(devices);

        _deviceRegistryMock.Setup(x => x.GetReader(ProtocolType.CommandTrack)).Returns(readerMock.Object);
        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>())).ReturnsAsync(@operator);

        var handler = new GetDevicesByOperatorQueryHandler(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _deviceRegistryMock.Object);

        // Act
        var result = await handler.Handle(new GetDevicesByOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(devices.Length));
    }

    [Test]
    public async Task GetDevicesQuery_ReturnsDevices_FromReaders()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var deviceTransporter = new DeviceTransporterVm { TransporterId = Guid.NewGuid() };
        var devices = new[] { new DeviceVm { DeviceId = Guid.NewGuid(), Name = "Device 1" } };

        var readerMock = new Mock<IExternalDeviceReader>();
        readerMock.SetupGet(r => r.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicesAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>())).ReturnsAsync(devices);

        _deviceRegistryMock.Setup(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>())).Returns([readerMock.Object]);
        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([@operator]);
        _deviceReaderMock.Setup(x => x.GetVisibleDeviceTransportersByOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([deviceTransporter]);

        var handler = new GetDevicesQueryHandler(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _deviceRegistryMock.Object,
            _deviceReaderMock.Object);

        // Act
        var result = await handler.Handle(new GetDevicesQuery(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(devices.Length));
    }

    [Test]
    public async Task GetDevicesQuery_SkipsWhenNoCredential()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var @operator = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), null);

        var readerMock = new Mock<IExternalDeviceReader>();
        readerMock.SetupGet(r => r.Protocol).Returns(ProtocolType.CommandTrack);

        _deviceRegistryMock.Setup(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>())).Returns([readerMock.Object]);
        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([@operator]);

        var handler = new GetDevicesQueryHandler(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _deviceRegistryMock.Object,
            _deviceReaderMock.Object);

        // Act
        var result = await handler.Handle(new GetDevicesQuery(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }
}
