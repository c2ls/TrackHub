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

using Common.Application.Interfaces;
using TrackHub.Security.Application.Users.Commands.Unlock;

namespace Application.UnitTests.Users;

[TestFixture]
public class UnlockUserTests
{
    [Test]
    public async Task Handle_ResetsLockout_AndPublishesAudit()
    {
        var writer = new Mock<IUserWriter>();
        var reader = new Mock<IUserReader>();
        var publisher = new Mock<IPublisher>();
        var principal = new Mock<ICurrentPrincipal>();
        var userId = Guid.NewGuid();

        var handler = new UnlockUserCommandHandler(writer.Object, reader.Object, publisher.Object, principal.Object);

        await handler.Handle(new UnlockUserCommand(userId), CancellationToken.None);

        writer.Verify(w => w.UnlockUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.Publish(It.IsAny<TrackHub.Security.Application.Audit.Events.SecurityAuditForwarder.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
