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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Router.Application.DevicePositions.Commands.Health;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Records;

namespace Application.UnitTests;

[TestFixture]
public class RecordOperatorHealthCommandHandlerTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IConnectivityRegistry> _connectivityRegistryMock = null!;
    private Mock<IOperatorHealthCheckWriter> _healthWriterMock = null!;
    private Mock<IAlertEventWriter> _alertWriterMock = null!;
    private Mock<IConnectivityTester> _testerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");

        _connectivityRegistryMock = new Mock<IConnectivityRegistry>();
        _healthWriterMock = new Mock<IOperatorHealthCheckWriter>();
        _alertWriterMock = new Mock<IAlertEventWriter>();
        _testerMock = new Mock<IConnectivityTester>();
        _testerMock.SetupGet(t => t.Protocol).Returns(ProtocolType.CommandTrack);
        _connectivityRegistryMock.Setup(r => r.GetTester(It.IsAny<ProtocolType>())).Returns(_testerMock.Object);
    }

    private RecordOperatorHealthCommandHandler CreateHandler() => new(
        _configurationMock.Object,
        _connectivityRegistryMock.Object,
        _healthWriterMock.Object,
        _alertWriterMock.Object,
        Mock.Of<ILogger<RecordOperatorHealthCommandHandler>>());

    private static OperatorVm OperatorWith(CredentialTokenVm? credential, string? previousStatus = null) =>
        new(Guid.NewGuid(), (int)ProtocolType.CommandTrack, Guid.NewGuid(), credential,
            Enabled: true, SyncIntervalMinutes: 60, LastDeviceSyncAt: null, LastPositionSyncAt: null,
            HealthStatus: previousStatus);

    private static AccountSettingsVm EnabledAccount(Guid accountId) =>
        new(accountId, 0, false, false, GpsIntegrationEnabled: true);

    [Test]
    public async Task Handle_NoCredential_ReturnsFalseAndRecordsNothing()
    {
        var op = OperatorWith(null);
        var account = EnabledAccount(op.AccountId);

        var result = await CreateHandler().Handle(
            new RecordOperatorHealthCommand(op), CancellationToken.None);

        Assert.That(result, Is.False);
        _healthWriterMock.Verify(w => w.RecordAsync(It.IsAny<OperatorHealthCheckDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _alertWriterMock.Verify(w => w.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_PingSucceeds_RecordsHealthyAndNoAlert_WhenPreviousIsHealthy()
    {
        var op = OperatorWith(TestCredentialTokenVm, previousStatus: "HEALTHY");
        var account = EnabledAccount(op.AccountId);
        _testerMock.Setup(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new RecordOperatorHealthCommand(op), CancellationToken.None);

        Assert.That(result, Is.True);
        _healthWriterMock.Verify(w => w.RecordAsync(
            It.Is<OperatorHealthCheckDto>(c => c.Status == "HEALTHY" && c.ErrorCode == null),
            It.IsAny<CancellationToken>()), Times.Once);
        _alertWriterMock.Verify(w => w.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_PingFails_RecordsUnhealthyAndRaisesOfflineAlert()
    {
        var op = OperatorWith(TestCredentialTokenVm, previousStatus: "HEALTHY");
        var account = EnabledAccount(op.AccountId);
        _testerMock.Setup(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("dead"));

        var result = await CreateHandler().Handle(
            new RecordOperatorHealthCommand(op), CancellationToken.None);

        Assert.That(result, Is.False);
        _healthWriterMock.Verify(w => w.RecordAsync(
            It.Is<OperatorHealthCheckDto>(c => c.Status == "OFFLINE"
                                                 && c.ErrorCode == "HttpRequestException"
                                                 && c.ErrorMessage == "dead"),
            It.IsAny<CancellationToken>()), Times.Once);
        _alertWriterMock.Verify(w => w.RecordAsync(
            It.Is<AlertEventDto>(a => a.EventType == "GpsOperatorOffline"
                                       && a.Severity == "Critical"
                                       && a.DeduplicationKey == $"operator-offline:{op.OperatorId}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_RecoveryTransition_RaisesRecoveredAlert()
    {
        var op = OperatorWith(TestCredentialTokenVm, previousStatus: "OFFLINE");
        var account = EnabledAccount(op.AccountId);
        _testerMock.Setup(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new RecordOperatorHealthCommand(op), CancellationToken.None);

        Assert.That(result, Is.True);
        _healthWriterMock.Verify(w => w.RecordAsync(
            It.Is<OperatorHealthCheckDto>(c => c.Status == "HEALTHY"), It.IsAny<CancellationToken>()), Times.Once);
        _alertWriterMock.Verify(w => w.RecordAsync(
            It.Is<AlertEventDto>(a => a.EventType == "GpsOperatorRecovered" && a.Severity == "Info"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_NoPreviousStatus_RecoveryAlertNotEmitted_OnFirstHealthy()
    {
        // First-ever health check (HealthStatus null) should not emit a recovered alert.
        var op = OperatorWith(TestCredentialTokenVm, previousStatus: null);
        var account = EnabledAccount(op.AccountId);
        _testerMock.Setup(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(
            new RecordOperatorHealthCommand(op), CancellationToken.None);

        _alertWriterMock.Verify(w => w.RecordAsync(It.IsAny<AlertEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
