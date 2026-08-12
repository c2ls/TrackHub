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

using Common.Domain.Enums;
using Common.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Router.Application.DevicePositions.Commands.Health;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Application.PingOperator.Queries;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Application.DevicePositions.Events;

namespace Application.UnitTests;

// An undeclared capability is a provider limitation, not a failure: the background loops
// (device sync, health probe, position sync) must skip such operators quietly — no FAILED
// runs, no OFFLINE marks, no recurring alerts — while the user-triggered surfaces (manual
// sync, ping) surface the client-facing PROVIDER_CAPABILITY_NOT_SUPPORTED error. Uses the
// reserved Mettax value, whose catalog entry declares no capabilities at all.
[TestFixture]
public class PartialProviderCapabilityGuardTests : TestsContext
{
    // The real catalog with no descriptors: every lookup resolves ProviderCapability.None.
    private static readonly IProviderCapabilityCatalog EmptyCatalog = new ProviderCapabilityCatalog([]);

    private static OperatorVm CapabilityLessOperator() => new(
        Guid.NewGuid(),
        (int)ProtocolType.Mettax,
        Guid.NewGuid(),
        new CredentialTokenVm(Guid.NewGuid(), "https://example.test/",
            "enc-user", "enc-pass", Convert.ToBase64String(new byte[16]), null, null, null, null, null, null));

    [Test]
    public async Task DeviceSync_Automatic_SkipsQuietlyWithoutRecordingAnything()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        var deviceRegistry = new Mock<IDeviceRegistry>();
        var syncRunWriter = new Mock<IOperatorSyncRunWriter>();
        var healthWriter = new Mock<IOperatorHealthCheckSystemWriter>();
        var alertWriter = new Mock<IAlertEventWriter>();
        var syncLock = new Mock<IOperatorSyncLock>();
        syncLock.Setup(x => x.AcquireAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDisposable>());
        var handler = new SyncOperatorDevicesCommandHandler(
            configuration.Object, deviceRegistry.Object, Mock.Of<IDeviceSyncWriter>(),
            syncRunWriter.Object, healthWriter.Object, alertWriter.Object, syncLock.Object,
            Mock.Of<IDeviceCatalogCache>(), EmptyCatalog,
            Mock.Of<ILogger<SyncOperatorDevicesCommandHandler>>());

        var result = await handler.Handle(
            new SyncOperatorDevicesCommand(CapabilityLessOperator(), "AUTOMATIC"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            deviceRegistry.Verify(r => r.GetReader(It.IsAny<ProtocolType>()), Times.Never);
            syncRunWriter.Verify(w => w.RecordAsync(It.IsAny<OperatorSyncRunDto>(), It.IsAny<CancellationToken>()), Times.Never);
            healthWriter.Verify(w => w.RecordAsync(It.IsAny<OperatorHealthCheckDto>(), It.IsAny<CancellationToken>()), Times.Never);
            alertWriter.Verify(w => w.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public void DeviceSync_Manual_ThrowsTheProviderLimitationError()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        var handler = new SyncOperatorDevicesCommandHandler(
            configuration.Object, Mock.Of<IDeviceRegistry>(), Mock.Of<IDeviceSyncWriter>(),
            Mock.Of<IOperatorSyncRunWriter>(), Mock.Of<IOperatorHealthCheckSystemWriter>(),
            Mock.Of<IAlertEventWriter>(), Mock.Of<IOperatorSyncLock>(),
            Mock.Of<IDeviceCatalogCache>(), EmptyCatalog,
            Mock.Of<ILogger<SyncOperatorDevicesCommandHandler>>());

        var ex = Assert.ThrowsAsync<ProviderCapabilityNotSupportedException>(
            async () => await handler.Handle(
                new SyncOperatorDevicesCommand(CapabilityLessOperator(), "MANUAL"), CancellationToken.None));
        Assert.Multiple(() =>
        {
            Assert.That(ex!.Protocol, Is.EqualTo(ProtocolType.Mettax));
            Assert.That(ex.Capability, Is.EqualTo(ProviderCapability.DeviceCatalog));
        });
    }

    [Test]
    public async Task HealthProbe_SkipsQuietlyWithoutRecordingAnything()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        var connectivityRegistry = new Mock<IConnectivityRegistry>();
        var healthWriter = new Mock<IOperatorHealthCheckWriter>();
        var alertWriter = new Mock<IAlertEventWriter>();
        var handler = new RecordOperatorHealthCommandHandler(
            configuration.Object, connectivityRegistry.Object, healthWriter.Object,
            alertWriter.Object, EmptyCatalog,
            Mock.Of<ILogger<RecordOperatorHealthCommandHandler>>());

        var result = await handler.Handle(
            new RecordOperatorHealthCommand(CapabilityLessOperator()), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            connectivityRegistry.Verify(r => r.GetTester(It.IsAny<ProtocolType>()), Times.Never);
            healthWriter.Verify(w => w.RecordAsync(It.IsAny<OperatorHealthCheckDto>(), It.IsAny<CancellationToken>()), Times.Never);
            alertWriter.Verify(w => w.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public async Task PositionSync_SkipsQuietlyWithoutPublishing()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        var publisher = new Mock<IPublisher>();
        var positionRegistry = new Mock<IPositionRegistry>();
        var handler = new GetPositionsByOperatorCommandHandler(
            publisher.Object, configuration.Object, positionRegistry.Object,
            Mock.Of<IDeviceTransporterReader>(), Mock.Of<IDeviceCatalogCache>(), EmptyCatalog,
            Mock.Of<ILogger<GetPositionsByOperatorCommandHandler>>());

        var result = await handler.Handle(
            new GetPositionsByOperatorCommand(CapabilityLessOperator(), new AccountSettingsVm(Guid.NewGuid(), 10, false, false)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            positionRegistry.Verify(r => r.GetReader(It.IsAny<ProtocolType>()), Times.Never);
            publisher.Verify(p => p.Publish(It.IsAny<PositionsRetrieved.Notification>(), It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Test]
    public void ManualPing_ThrowsTheProviderLimitationError()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
        var @operator = CapabilityLessOperator();
        var operatorReader = new Mock<IOperatorReader>();
        operatorReader.Setup(r => r.GetOperatorAsync(@operator.OperatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@operator);
        var operatorSystemReader = new Mock<IOperatorSystemReader>();
        operatorSystemReader.Setup(r => r.GetOperatorAsync(@operator.OperatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@operator);
        var connectivityRegistry = new Mock<IConnectivityRegistry>();
        var handler = new PingOperatorQueryHandler(
            configuration.Object, operatorReader.Object, operatorSystemReader.Object,
            connectivityRegistry.Object, Mock.Of<IOperatorHealthCheckSystemWriter>(), EmptyCatalog,
            Mock.Of<ILogger<PingOperatorQueryHandler>>());

        var ex = Assert.ThrowsAsync<ProviderCapabilityNotSupportedException>(
            async () => await handler.Handle(new PingOperatorQuery(@operator.OperatorId), CancellationToken.None));
        Assert.Multiple(() =>
        {
            Assert.That(ex!.Capability, Is.EqualTo(ProviderCapability.ConnectivityPing));
            connectivityRegistry.Verify(r => r.GetTester(It.IsAny<ProtocolType>()), Times.Never);
        });
    }
}
