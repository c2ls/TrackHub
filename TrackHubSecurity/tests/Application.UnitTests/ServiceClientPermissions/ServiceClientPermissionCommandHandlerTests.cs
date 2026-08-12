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
using TrackHub.Security.Application.ServiceClientPermissions.Commands;

namespace Application.UnitTests.ServiceClientPermissions;

[TestFixture]
public class ServiceClientPermissionCommandHandlerTests
{
    private Mock<IServiceClientPermissionWriter> _writer;
    private Mock<IPublisher> _publisher;
    private Mock<ICurrentPrincipal> _principal;

    [SetUp]
    public void SetUp()
    {
        _writer = new Mock<IServiceClientPermissionWriter>();
        _publisher = new Mock<IPublisher>();
        _principal = new Mock<ICurrentPrincipal>();
    }

    private static ServiceClientPermissionDto Dto()
        => new("router_client", null, "Positions", "Read", "service_scope", "trackhub_api", true, null, null);

    [Test]
    public async Task Create_DelegatesToWriter_AndPublishesAudit()
    {
        var vm = new ServiceClientPermissionVm(Guid.NewGuid(), "router_client", null, "Positions", "Read", "service_scope", "trackhub_api", true, null, null, DateTimeOffset.UtcNow);
        _writer.Setup(w => w.CreateServiceClientPermissionAsync(It.IsAny<ServiceClientPermissionDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var handler = new CreateServiceClientPermissionCommandHandler(_writer.Object, _publisher.Object, _principal.Object);

        var result = await handler.Handle(new CreateServiceClientPermissionCommand(Dto()), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
        _writer.Verify(w => w.CreateServiceClientPermissionAsync(It.IsAny<ServiceClientPermissionDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<TrackHub.Security.Application.Audit.Events.SecurityAuditForwarder.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Update_DelegatesToWriter_WithId()
    {
        var id = Guid.NewGuid();
        var handler = new UpdateServiceClientPermissionCommandHandler(_writer.Object, _publisher.Object, _principal.Object);

        await handler.Handle(new UpdateServiceClientPermissionCommand(id, Dto()), CancellationToken.None);

        _writer.Verify(w => w.UpdateServiceClientPermissionAsync(id, It.IsAny<ServiceClientPermissionDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsId_AndPublishesAudit()
    {
        var id = Guid.NewGuid();
        _writer.Setup(w => w.DeleteServiceClientPermissionAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(id);
        var handler = new DeleteServiceClientPermissionCommandHandler(_writer.Object, _publisher.Object, _principal.Object);

        var result = await handler.Handle(new DeleteServiceClientPermissionCommand(id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(id));
        _publisher.Verify(p => p.Publish(It.IsAny<TrackHub.Security.Application.Audit.Events.SecurityAuditForwarder.Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
