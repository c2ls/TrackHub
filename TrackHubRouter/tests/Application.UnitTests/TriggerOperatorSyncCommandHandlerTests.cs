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

using Common.Application.Exceptions;
using Common.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;

namespace Application.UnitTests;

// The manual dispatch makes exactly ONE Manager read (the operator, which carries the account
// binding and credential). Manager already validated authorization/account status before
// dispatching, so no account/feature callbacks exist here. The sync itself is QUEUED, not awaited:
// the caller reads what happened from operator_sync_runs.
[TestFixture]
public class TriggerOperatorSyncCommandHandlerTests : TestsContext
{
    private Mock<IOperatorSystemReader> _operatorSystemReaderMock = null!;
    private Mock<ISyncDispatchQueue> _queueMock = null!;

    [SetUp]
    public void SetUp()
    {
        _operatorSystemReaderMock = new Mock<IOperatorSystemReader>();
        _queueMock = new Mock<ISyncDispatchQueue>();
        _queueMock.Setup(q => q.TryEnqueue(It.IsAny<SyncDispatchRequest>())).Returns(true);
    }

    private TriggerOperatorSyncCommandHandler CreateHandler() => new(
        _operatorSystemReaderMock.Object,
        _queueMock.Object,
        Mock.Of<ILogger<TriggerOperatorSyncCommandHandler>>());

    private void SetupOperator(OperatorVm op)
    {
        _operatorSystemReaderMock.Setup(x => x.GetOperatorAsync(op.OperatorId, It.IsAny<CancellationToken>())).ReturnsAsync(op);
    }

    [Test]
    public async Task Handle_CrossAccountOperator_ThrowsOperatorNotFound()
    {
        var accountId = Guid.NewGuid();
        var otherAccount = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, otherAccount, TestCredentialTokenVm);
        SetupOperator(op);

        // A2: operator that does not belong to the account -> typed error, never a silent false.
        await Assert.ThrowsAsync<OperatorNotFoundException>(() => CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId), CancellationToken.None));
        _queueMock.Verify(q => q.TryEnqueue(It.IsAny<SyncDispatchRequest>()), Times.Never);
    }

    [Test]
    public async Task Handle_OperatorDisabled_ThrowsOperatorDisabled()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm,
            Enabled: false);
        SetupOperator(op);

        // A2: disabled operator -> typed error, never a silent false.
        await Assert.ThrowsAsync<OperatorDisabledException>(() => CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId), CancellationToken.None));
        _queueMock.Verify(q => q.TryEnqueue(It.IsAny<SyncDispatchRequest>()), Times.Never);
    }

    [Test]
    public async Task Handle_HappyPath_MakesSingleManagerReadAndQueues()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);

        var result = await CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId, "MANUAL", "corr-42"), CancellationToken.None);

        Assert.That(result, Is.True);
        _operatorSystemReaderMock.Verify(x => x.GetOperatorAsync(op.OperatorId, It.IsAny<CancellationToken>()), Times.Once);
        _operatorSystemReaderMock.VerifyNoOtherCalls();
        _queueMock.Verify(q => q.TryEnqueue(
            It.Is<SyncDispatchRequest>(r =>
                r.Operator.OperatorId == op.OperatorId
                && r.Operator.AccountId == accountId
                && r.TriggerType == "MANUAL"
                && r.CorrelationId == "corr-42"
                && !r.ResetDeviceCatalog
                && r.AutoAssignNewDevices)), Times.Once);
    }

    [Test]
    public async Task Handle_ResetDeviceCatalog_QueuesWithReset()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);

        var result = await CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId, "MANUAL", "corr-42", ResetDeviceCatalog: true),
            CancellationToken.None);

        Assert.That(result, Is.True);
        _queueMock.Verify(q => q.TryEnqueue(
            It.Is<SyncDispatchRequest>(r =>
                r.Operator.OperatorId == op.OperatorId
                && r.ResetDeviceCatalog
                && r.CorrelationId == "corr-42")), Times.Once);
    }

    [Test]
    public async Task Handle_QueueFull_ThrowsTooManyRequests()
    {
        var accountId = Guid.NewGuid();
        var op = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.CommandTrack, accountId, TestCredentialTokenVm);
        SetupOperator(op);
        _queueMock.Setup(q => q.TryEnqueue(It.IsAny<SyncDispatchRequest>())).Returns(false);

        // A refused trigger must say so — never answer "accepted" for work that was dropped.
        await Assert.ThrowsAsync<TooManyRequestsException>(() => CreateHandler().Handle(
            new TriggerOperatorSyncCommand(accountId, op.OperatorId), CancellationToken.None));
    }
}
