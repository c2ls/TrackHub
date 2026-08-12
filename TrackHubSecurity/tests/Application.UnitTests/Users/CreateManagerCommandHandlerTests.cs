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
using TrackHub.Security.Application.Audit.Events;
using TrackHub.Security.Application.Users.Commands.CreateManager;

namespace Application.UnitTests.Users;

// Creating a Manager user (Administrative/Write) is a user-creation path and must
// produce exactly one security audit event, on parity with CreateUserCommand.
[TestFixture]
public class CreateManagerCommandHandlerTests
{
    [Test]
    public async Task Handle_CreatesUser_AndPublishesSecurityAudit()
    {
        var accountId = Guid.NewGuid();
        var vm = default(UserVm) with { UserId = Guid.NewGuid(), Username = "mgr" };
        var writer = new Mock<IUserWriter>();
        writer.Setup(w => w.CreateUserAsync(It.IsAny<CreateUserDto>(), accountId, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var publisher = new Mock<IPublisher>();
        var principal = new Mock<ICurrentPrincipal>();

        var handler = new CreateManagerCommandHandler(writer.Object, publisher.Object, principal.Object);

        var result = await handler.Handle(new CreateManagerCommand(new CreateUserDto { Password = "password" }, accountId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
        publisher.Verify(p => p.Publish(It.IsAny<SecurityAuditForwarder.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
