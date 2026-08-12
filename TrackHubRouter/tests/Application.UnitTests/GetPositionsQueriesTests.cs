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
using Microsoft.Extensions.Logging;
using TrackHub.Router.Application.DevicePositions.Queries.Get;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;
using Common.Domain.Enums;
using Common.Mediator;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Application.DevicePositions.Events;

namespace TrackHub.Router.Application.UnitTests.DevicePositions.Queries.Get;

[TestFixture]
public class GetPositionsQueriesTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IPositionRegistry> _positionRegistryMock = null!;
    private Mock<IDeviceTransporterReader> _deviceReaderMock = null!;
    private Mock<IOperatorReader> _operatorReaderMock = null!;
    private Mock<ITransporterPositionReader> _transporterPositionReaderMock = null!;
    private Mock<IPositionSystemWriter> _positionSystemWriterMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _positionRegistryMock = new Mock<IPositionRegistry>();
        _deviceReaderMock = new Mock<IDeviceTransporterReader>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _transporterPositionReaderMock = new Mock<ITransporterPositionReader>();
        _positionSystemWriterMock = new Mock<IPositionSystemWriter>();

        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
    }

    // A device-catalog cache that always invokes the loader (behaves as if disabled), so tests
    // exercise the underlying device reader exactly as before the cache was introduced (A-12).
    private static IDeviceCatalogCache PassThroughCache()
    {
        var cache = new Mock<IDeviceCatalogCache>();
        cache
            .Setup(c => c.GetOrLoadAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Func<CancellationToken, Task<IEnumerable<DeviceTransporterVm>>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Guid, Guid, Func<CancellationToken, Task<IEnumerable<DeviceTransporterVm>>>, CancellationToken>(
                (_, _, loader, ct) => loader(ct));
        return cache.Object;
    }

    [Test]
    public async Task GetPositionsByOperator_WithCredential_PublishesPositions()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var readerMock = new Mock<IPositionReader>();

        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);

        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.GetDevicePositionAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow }]);

        _positionRegistryMock.Setup(x => x.GetReader(It.IsAny<ProtocolType>())).Returns(readerMock.Object);
        _deviceReaderMock.Setup(x => x.GetDeviceTransporterAsync(account.AccountId, operatorId, It.IsAny<CancellationToken>())).ReturnsAsync([new DeviceTransporterVm { TransporterId = Guid.NewGuid() }]);

        var handler = new GetPositionsByOperatorQueryHandler(
            publisherMock.Object,
            _configurationMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            PassThroughCache(),
            Mock.Of<ILogger<GetPositionsByOperatorQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByOperatorQuery(operatorVm, account), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        publisherMock.Verify(x => x.Publish(It.IsAny<PositionsRetrieved.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPositionsByOperator_ProviderThrows_PublishesNotificationWithProviderError()
    {
        // Regression for router-audit A-08: a provider fetch that throws must be surfaced on the
        // notification (ProviderErrorCode) rather than swallowed to an empty success.
        var publisherMock = new Mock<IPublisher>();
        var readerMock = new Mock<IPositionReader>();

        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);

        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.GetDevicePositionAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider down"));

        _positionRegistryMock.Setup(x => x.GetReader(It.IsAny<ProtocolType>())).Returns(readerMock.Object);
        _deviceReaderMock.Setup(x => x.GetDeviceTransporterAsync(account.AccountId, operatorId, It.IsAny<CancellationToken>())).ReturnsAsync([new DeviceTransporterVm { TransporterId = Guid.NewGuid() }]);

        var handler = new GetPositionsByOperatorQueryHandler(
            publisherMock.Object,
            _configurationMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            PassThroughCache(),
            Mock.Of<ILogger<GetPositionsByOperatorQueryHandler>>());

        // Act — must not throw; the error is carried on the notification.
        var result = await handler.Handle(new GetPositionsByOperatorQuery(operatorVm, account), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        publisherMock.Verify(x => x.Publish(
            It.Is<PositionsRetrieved.Notification>(n => n.ProviderErrorCode == "InvalidOperationException"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPositionsByOperator_WithNullCredential_DoesNotPublish()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();

        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), null);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);

        var handler = new GetPositionsByOperatorQueryHandler(
            publisherMock.Object,
            _configurationMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            PassThroughCache(),
            Mock.Of<ILogger<GetPositionsByOperatorQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByOperatorQuery(operatorVm, account), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        publisherMock.Verify(x => x.Publish(It.IsAny<PositionsRetrieved.Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetPositionByTransporter_ReturnsPosition_WhenCredentialPresent()
    {
        // Arrange
        var readerMock = new Mock<IPositionReader>();
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = operatorId };

        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.GetDevicePositionAsync(device, It.IsAny<CancellationToken>())).ReturnsAsync(new PositionVm { TransporterId = device.TransporterId, DeviceDateTime = DateTimeOffset.UtcNow });

        _positionRegistryMock.Setup(x => x.GetReader(It.IsAny<ProtocolType>())).Returns(readerMock.Object);
        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);
        _deviceReaderMock.Setup(x => x.GetDevicesTransporterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(device);

        var modeResolver = ModeResolverForDisabled(operatorVm.AccountId);
        var handler = new GetPositionByTransporterQueryHandler(
            _configurationMock.Object,
            modeResolver.Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionByTransporterQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionByTransporterQuery(device.TransporterId), CancellationToken.None);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(device.TransporterId));
        modeResolver.Verify(x => x.IsIntegrationEnabledAsync(operatorVm.AccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPositionByTransporter_WhenGpsIntegrationEnabled_ReturnsCachedPosition()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var cachedPosition = new PositionVm { TransporterId = transporterId, DeviceDateTime = DateTimeOffset.UtcNow };

        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);
        _transporterPositionReaderMock
            .Setup(x => x.GetTransporterPositionAsync(operatorVm.OperatorId, transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPosition);

        var handler = new GetPositionByTransporterQueryHandler(
            _configurationMock.Object,
            ModeResolverForEnabled(accountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionByTransporterQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionByTransporterQuery(transporterId), CancellationToken.None);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(transporterId));
        _positionRegistryMock.Verify(x => x.GetReader(It.IsAny<ProtocolType>()), Times.Never);
        _deviceReaderMock.Verify(x => x.GetDevicesTransporterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetPositionByTransporter_WhenLiveProviderFails_ReturnsCachedPosition()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = transporterId };
        var cachedPosition = new PositionVm { TransporterId = transporterId, DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(-5) };

        var readerMock = new Mock<IPositionReader>();
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicePositionAsync(device, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider unavailable"));

        _positionRegistryMock.Setup(x => x.GetReader(ProtocolType.CommandTrack)).Returns(readerMock.Object);
        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);
        _deviceReaderMock.Setup(x => x.GetDevicesTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _transporterPositionReaderMock
            .Setup(x => x.GetTransporterPositionAsync(operatorVm.OperatorId, transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPosition);

        var handler = new GetPositionByTransporterQueryHandler(
            _configurationMock.Object,
            ModeResolverForDisabled(accountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionByTransporterQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionByTransporterQuery(transporterId), CancellationToken.None);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(transporterId));
        readerMock.Verify(x => x.GetDevicePositionAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPositionByTransporter_WhenLiveProviderReturnsDefault_ReturnsCachedPosition()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = transporterId };
        var cachedPosition = new PositionVm { TransporterId = transporterId, DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(-5) };

        var readerMock = new Mock<IPositionReader>();
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicePositionAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(PositionVm));

        _positionRegistryMock.Setup(x => x.GetReader(ProtocolType.CommandTrack)).Returns(readerMock.Object);
        _operatorReaderMock.Setup(x => x.GetOperatorByTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);
        _deviceReaderMock.Setup(x => x.GetDevicesTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _transporterPositionReaderMock
            .Setup(x => x.GetTransporterPositionAsync(operatorVm.OperatorId, transporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPosition);

        var handler = new GetPositionByTransporterQueryHandler(
            _configurationMock.Object,
            ModeResolverForDisabled(accountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionByTransporterQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionByTransporterQuery(transporterId), CancellationToken.None);

        // Assert
        Assert.That(result.TransporterId, Is.EqualTo(transporterId));
    }

    [Test]
    public async Task GetPositionsByUser_ReturnsMostRecentPositionPerTransporter()
    {
        // Arrange
        var readerMock = new Mock<IPositionReader>();
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = operatorId };

        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.GetDevicePositionAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>())).ReturnsAsync([new PositionVm { TransporterId = device.TransporterId, DeviceDateTime = DateTimeOffset.UtcNow }]);

        _positionRegistryMock.Setup(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>())).Returns([readerMock.Object]);
        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([operatorVm]);
        _deviceReaderMock.Setup(x => x.GetVisibleDeviceTransportersByOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([device]);

        var handler = new GetPositionsByUserQueryHandler(
            _configurationMock.Object,
            ModeResolverForDisabled(operatorVm.AccountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionsByUserQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByUserQuery(), CancellationToken.None);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetPositionsByUser_WhenGpsIntegrationDisabled_ReadsProviderOnDemand()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = Guid.NewGuid() };
        var livePosition = new PositionVm { TransporterId = device.TransporterId, DeviceDateTime = DateTimeOffset.UtcNow };

        var readerMock = new Mock<IPositionReader>();
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicePositionAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([livePosition]);

        _positionRegistryMock.Setup(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>())).Returns([readerMock.Object]);
        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([operatorVm]);
        _deviceReaderMock.Setup(x => x.GetVisibleDeviceTransportersByOperatorAsync(operatorVm.OperatorId, It.IsAny<CancellationToken>())).ReturnsAsync([device]);

        var modeResolver = ModeResolverForDisabled(accountId);
        var handler = new GetPositionsByUserQueryHandler(
            _configurationMock.Object,
            modeResolver.Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionsByUserQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByUserQuery(), CancellationToken.None);

        // Assert
        Assert.That(result.Single().TransporterId, Is.EqualTo(livePosition.TransporterId));
        _positionRegistryMock.Verify(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>()), Times.Once);
        _deviceReaderMock.Verify(x => x.GetVisibleDeviceTransportersByOperatorAsync(operatorVm.OperatorId, It.IsAny<CancellationToken>()), Times.Once);
        _transporterPositionReaderMock.Verify(x => x.GetTransporterPositionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        modeResolver.Verify(x => x.IsIntegrationEnabledAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        // On-demand mode: the Router API persists the provider read with its service identity.
        _positionSystemWriterMock.Verify(x => x.AddOrUpdatePositionAsync(
            It.Is<IEnumerable<PositionVm>>(p => p.Single().TransporterId == livePosition.TransporterId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPositionsByUser_WhenGpsIntegrationEnabled_ReturnsCachedPositions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var cachedPosition = new PositionVm { TransporterId = Guid.NewGuid(), DeviceDateTime = DateTimeOffset.UtcNow };

        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([operatorVm]);
        // Stored-projection operators are read through ONE batched Telemetry call.
        _transporterPositionReaderMock
            .Setup(x => x.GetTransporterPositionsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Single() == operatorVm.OperatorId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([cachedPosition]);

        var handler = new GetPositionsByUserQueryHandler(
            _configurationMock.Object,
            ModeResolverForEnabled(accountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionsByUserQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByUserQuery(), CancellationToken.None);

        // Assert
        Assert.That(result.Single().TransporterId, Is.EqualTo(cachedPosition.TransporterId));
        _positionRegistryMock.Verify(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>()), Times.Never);
        _deviceReaderMock.Verify(x => x.GetVisibleDeviceTransportersByOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _transporterPositionReaderMock.Verify(x => x.GetTransporterPositionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        // Background-sync mode: the query only reads the cached projection; it never writes.
        _positionSystemWriterMock.Verify(x => x.AddOrUpdatePositionAsync(
            It.IsAny<IEnumerable<PositionVm>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetPositionsByUser_WhenLiveProviderReturnsNoPositions_ReturnsCachedPositions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var operatorVm = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        var device = new DeviceTransporterVm { TransporterId = Guid.NewGuid() };
        var cachedPosition = new PositionVm { TransporterId = device.TransporterId, DeviceDateTime = DateTimeOffset.UtcNow.AddMinutes(-10) };

        var readerMock = new Mock<IPositionReader>();
        readerMock.SetupGet(x => x.Protocol).Returns(ProtocolType.CommandTrack);
        readerMock.Setup(x => x.Init(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        readerMock.Setup(x => x.GetDevicePositionAsync(It.IsAny<IEnumerable<DeviceTransporterVm>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _positionRegistryMock.Setup(x => x.GetReaders(It.IsAny<IEnumerable<ProtocolType>>())).Returns([readerMock.Object]);
        _operatorReaderMock.Setup(x => x.GetOperatorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([operatorVm]);
        _deviceReaderMock.Setup(x => x.GetVisibleDeviceTransportersByOperatorAsync(operatorVm.OperatorId, It.IsAny<CancellationToken>())).ReturnsAsync([device]);
        _transporterPositionReaderMock
            .Setup(x => x.GetTransporterPositionAsync(operatorVm.OperatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([cachedPosition]);

        var handler = new GetPositionsByUserQueryHandler(
            _configurationMock.Object,
            ModeResolverForDisabled(accountId).Object,
            _operatorReaderMock.Object,
            _positionRegistryMock.Object,
            _deviceReaderMock.Object,
            _transporterPositionReaderMock.Object,
            _positionSystemWriterMock.Object,
            Mock.Of<ILogger<GetPositionsByUserQueryHandler>>());

        // Act
        var result = await handler.Handle(new GetPositionsByUserQuery(), CancellationToken.None);

        // Assert
        Assert.That(result.Single().TransporterId, Is.EqualTo(cachedPosition.TransporterId));
    }
}
