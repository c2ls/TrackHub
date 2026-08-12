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
using Microsoft.Extensions.Logging;
using Moq;
using Common.Mediator;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Application.DevicePositions.Events;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.UnitTests.DevicePositions.Commands.Sync;

[TestFixture]
public class SyncPositionCommandHandlerTests : TestsContext
{
    private Mock<IAccountReader> _accountReaderMock = null!;
    private Mock<IOperatorReader> _operatorReaderMock = null!;
    private Mock<IExecutionIntervalManager> _intervalManagerMock = null!;
    private Mock<IPublisher> _publisherMock = null!;

    private UpdateTransporterCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _accountReaderMock = new Mock<IAccountReader>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _intervalManagerMock = new Mock<IExecutionIntervalManager>();
        _publisherMock = new Mock<IPublisher>();

        _handler = new UpdateTransporterCommandHandler(
            _accountReaderMock.Object,
            _operatorReaderMock.Object,
            _intervalManagerMock.Object,
            _publisherMock.Object,
            Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(),
            Mock.Of<ILogger<UpdateTransporterCommandHandler>>());
    }

    [Test]
    public async Task Handle_NoAccounts_ReturnsTrueAndDoesNotPublish()
    {
        // Arrange
        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AccountSettingsVm>());

        // Act
        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _intervalManagerMock.Verify(x => x.UpdateLastExecutionTime(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_AccountShouldExecute_PublishesForEachOperatorAndUpdatesInterval()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountSettingsVm(accountId, 10, false, false, GpsIntegrationEnabled: true);

        var operatorId1 = Guid.NewGuid();
        var operatorId2 = Guid.NewGuid();

        // The master projection carries the credential — the handler must not re-fetch operators.
        var operators = new[]
        {
            new OperatorVm(operatorId1, 1, accountId, TestCredentialTokenVm),
            new OperatorVm(operatorId2, 1, accountId, TestCredentialTokenVm)
        };

        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        _intervalManagerMock.Setup(x => x.ShouldExecuteTask(account)).Returns(true);

        _operatorReaderMock.Setup(x => x.GetOperatorsByAccountsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operators);

        // Act
        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        _publisherMock.Verify(x => x.Publish(It.IsAny<OperatorRetrieved.Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _intervalManagerMock.Verify(x => x.UpdateLastExecutionTime(accountId), Times.Once);
        _operatorReaderMock.Verify(x => x.GetOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_OneOperatorThrows_IsolatesFailureAndStillUpdatesInterval()
    {
        // Regression for router-audit A-05: a single failing operator (e.g. bad credentials, an
        // unregistered protocol) must not abort the account fan-out nor skip the interval update —
        // otherwise the account re-polls every loop tick instead of at its storing interval.
        var accountId = Guid.NewGuid();
        var account = new AccountSettingsVm(accountId, 10, false, false, GpsIntegrationEnabled: true);

        var badOperatorId = Guid.NewGuid();
        var goodOperatorId = Guid.NewGuid();
        var operators = new[]
        {
            new OperatorVm(badOperatorId, 1, accountId, TestCredentialTokenVm),
            new OperatorVm(goodOperatorId, 1, accountId, TestCredentialTokenVm)
        };

        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);
        _intervalManagerMock.Setup(x => x.ShouldExecuteTask(account)).Returns(true);
        _operatorReaderMock.Setup(x => x.GetOperatorsByAccountsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operators);

        _publisherMock
            .Setup(x => x.Publish(It.Is<OperatorRetrieved.Notification>(n => n.Operator.OperatorId == badOperatorId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider down"));

        // Act — must NOT throw despite the failing operator.
        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        _publisherMock.Verify(x => x.Publish(It.Is<OperatorRetrieved.Notification>(n => n.Operator.OperatorId == goodOperatorId), It.IsAny<CancellationToken>()), Times.Once);
        _intervalManagerMock.Verify(x => x.UpdateLastExecutionTime(accountId), Times.Once);
    }

    [Test]
    public async Task Handle_AccountShouldNotExecute_DoesNotPublishOrUpdate()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountSettingsVm(accountId, 10, false, false, GpsIntegrationEnabled: true);

        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        _intervalManagerMock.Setup(x => x.ShouldExecuteTask(account)).Returns(false);

        // Act
        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _intervalManagerMock.Verify(x => x.UpdateLastExecutionTime(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Handle_GpsIntegrationDisabled_SkipsAccount()
    {
        var accountId = Guid.NewGuid();
        var account = new AccountSettingsVm(accountId, 10, false, false, GpsIntegrationEnabled: false);

        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        Assert.That(result, Is.True);
        _intervalManagerMock.Verify(x => x.ShouldExecuteTask(It.IsAny<AccountSettingsVm>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_OperatorDisabled_DoesNotPublishForThatOperator()
    {
        var accountId = Guid.NewGuid();
        var account = new AccountSettingsVm(accountId, 10, false, false, GpsIntegrationEnabled: true);
        var enabledId = Guid.NewGuid();
        var disabledId = Guid.NewGuid();

        var operators = new[]
        {
            new OperatorVm(enabledId, 1, accountId, TestCredentialTokenVm, Enabled: true),
            new OperatorVm(disabledId, 1, accountId, TestCredentialTokenVm, Enabled: false)
        };

        _accountReaderMock.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);
        _intervalManagerMock.Setup(x => x.ShouldExecuteTask(account)).Returns(true);
        _operatorReaderMock.Setup(x => x.GetOperatorsByAccountsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operators);

        var result = await _handler.Handle(new SyncPositionCommand(), CancellationToken.None);

        Assert.That(result, Is.True);
        _publisherMock.Verify(x => x.Publish(It.IsAny<OperatorRetrieved.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _operatorReaderMock.Verify(x => x.GetOperatorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
