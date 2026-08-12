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
using TrackHub.Router.Application.DevicePositions.Events;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Domain.Models;
using Common.Mediator;

namespace TrackHub.Router.Application.UnitTests.DevicePositions.Events;

[TestFixture]
public class OperatorRetrievedTests : TestsContext
{
    [Test]
    public async Task EventHandler_SendsGetPositionsByOperatorCommand()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var handler = new OperatorRetrieved.Notification.EventHandler(senderMock.Object);

        var operatorVm = new OperatorVm(Guid.NewGuid(), 1, Guid.NewGuid(), null);
        var account = new AccountSettingsVm(Guid.NewGuid(), 10, false, false);
        var notification = new OperatorRetrieved.Notification(operatorVm, account);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        senderMock.Verify(x => x.Send(It.Is<GetPositionsByOperatorCommand>(q => q.Operator == operatorVm && q.Settings == account), It.IsAny<CancellationToken>()), Times.Once);
    }
}
