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

using Microsoft.Extensions.Logging;
using TrackHub.Security.Application.Audit.Events;

namespace Application.UnitTests.Audit;

// Security audit forwarding to Manager is post-commit, best-effort. A transport
// failure (Manager down) or a missing security_client identity must be logged and swallowed so the
// originating security command still succeeds — audit transport failure != authorization failure.
[TestFixture]
public class SecurityAuditForwarderTests
{
    private static SecurityAuditForwarder.Notification MakeNotification()
        => new(new SecurityAuditEventDto(Guid.NewGuid(), "User", "actor", "CreateUser", "User", Guid.NewGuid().ToString(), null, "newvalues", "corr"));

    private static SecurityAuditForwarder.Notification.EventHandler MakeHandler(IServiceProvider provider)
        => new(provider, new Mock<ILogger<SecurityAuditForwarder.Notification.EventHandler>>().Object);

    [Test]
    public void Handle_WriterThrows_DoesNotBubble()
    {
        var writer = new Mock<IManagerAuditWriter>();
        writer.Setup(w => w.ForwardAuditEventAsync(It.IsAny<SecurityAuditEventDto>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("Manager down"));
        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(IManagerAuditWriter))).Returns(writer.Object);

        var handler = MakeHandler(provider.Object);

        Assert.DoesNotThrowAsync(async () => await handler.Handle(MakeNotification(), CancellationToken.None));
        writer.Verify(w => w.ForwardAuditEventAsync(It.IsAny<SecurityAuditEventDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_WriterResolutionFails_DoesNotBubble()
    {
        // security_client not registered in OpenIddict / DI resolution throws synchronously —
        // must still be swallowed (regression guard for the ctor-injection resilience bug).
        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(IManagerAuditWriter))).Returns((object?)null);

        var handler = MakeHandler(provider.Object);

        Assert.DoesNotThrowAsync(async () => await handler.Handle(MakeNotification(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_Success_ForwardsEventOnce()
    {
        var writer = new Mock<IManagerAuditWriter>();
        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(IManagerAuditWriter))).Returns(writer.Object);
        var notification = MakeNotification();

        var handler = MakeHandler(provider.Object);
        await handler.Handle(notification, CancellationToken.None);

        writer.Verify(w => w.ForwardAuditEventAsync(notification.AuditEvent, It.IsAny<CancellationToken>()), Times.Once);
    }
}
