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
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Application.DevicePositions.Queries.Get;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;

namespace Application.UnitTests;

// The manual dispatch makes exactly ONE Manager read (the operator, which carries the account
// binding and credential). Manager already validated authorization/account status before
// dispatching, so no account/feature callbacks exist here.
[TestFixture]
public class TriggerOperatorSyncCommandHandlerTests : TestsContext
{
    private Mock<IOperatorReader> _operatorReaderMock = null!;
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _operatorReaderMock = new Mock<IOperatorReader>();
        _senderMock = new Mock<ISender>();
    }

    private TriggerOperatorSyncCommandHandler CreateHandler() => new(
        _operatorReaderMock.Object,
        _senderMock.Object,
        Mock.Of<ILogger<TriggerOperatorSyncCommandHandler>>());

    private void SetupOperator(OperatorVm op) =>
        _operatorReaderMock.Setup(x => x.GetOperatorAsync(op.OperatorId, It.IsAny<CancellationToken>())).ReturnsAsync(op);

    [Test]
    public void Handle_CrossAccountOperator_ThrowsOperatorNotFound()
    {
        var accountId = Guid.NewGuid();
        var otherAccount = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, otherAccount, TestCredentialTokenVm);
        SetupOperator(op);

        // A2: operator that does not belong to the account -> typed error, never a silent false.
        Assert.ThrowsAsync<OperatorNotFoundException>(() => CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId), CancellationToken.None));
        _senderMock.Verify(s => s.Send(It.IsAny<SyncOperatorDevicesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_OperatorDisabled_ThrowsOperatorDisabled()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm,
            Enabled: false);
        SetupOperator(op);

        // A2: disabled operator -> typed error, never a silent false.
        Assert.ThrowsAsync<OperatorDisabledException>(() => CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId), CancellationToken.None));
        _senderMock.Verify(s => s.Send(It.IsAny<SyncOperatorDevicesCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_HappyPath_MakesSingleManagerReadAndDispatches()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);
        _senderMock.Setup(s => s.Send(It.IsAny<SyncOperatorDevicesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId, "MANUAL", "corr-42"), CancellationToken.None);

        Assert.That(result, Is.True);
        _operatorReaderMock.Verify(x => x.GetOperatorAsync(op.OperatorId, It.IsAny<CancellationToken>()), Times.Once);
        _operatorReaderMock.VerifyNoOtherCalls();
        _senderMock.Verify(s => s.Send(
            It.Is<SyncOperatorDevicesCommand>(c =>
                c.Operator.OperatorId == op.OperatorId
                && c.Operator.AccountId == accountId
                && c.TriggerType == "MANUAL"
                && c.CorrelationId == "corr-42"
                && !c.ResetDeviceCatalog
                && c.AutoAssignNewDevices),
            It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPositionsByOperatorQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ResetDeviceCatalog_DispatchesDeviceSyncWithReset()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);
        _senderMock.Setup(s => s.Send(It.IsAny<SyncOperatorDevicesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId, "MANUAL", "corr-42", ResetDeviceCatalog: true),
            CancellationToken.None);

        Assert.That(result, Is.True);
        _senderMock.Verify(s => s.Send(
            It.Is<SyncOperatorDevicesCommand>(c =>
                c.Operator.OperatorId == op.OperatorId
                && c.ResetDeviceCatalog
                && c.CorrelationId == "corr-42"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DownstreamSyncReturnsFalse_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);
        _senderMock.Setup(s => s.Send(It.IsAny<SyncOperatorDevicesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId, "MANUAL", "corr-42"), CancellationToken.None);

        Assert.That(result, Is.False);
    }
}
