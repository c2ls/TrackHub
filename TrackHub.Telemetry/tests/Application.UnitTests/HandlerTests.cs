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
using Common.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using TrackHub.Telemetry.Application.GpsIntegration.Commands;
using TrackHub.Telemetry.Application.GpsIntegration.Queries;
using TrackHub.Telemetry.Application.TransporterPosition.Commands.Create;
using TrackHub.Telemetry.Domain.Interfaces;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Application.UnitTests;

[TestFixture]
public class HandlerTests
{
    private static ICurrentPrincipal User(Guid userId)
    {
        var m = new Mock<ICurrentPrincipal>();
        m.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        m.SetupGet(p => p.UserId).Returns(userId);
        return m.Object;
    }

    private static ICurrentPrincipal ServiceClient()
    {
        var m = new Mock<ICurrentPrincipal>();
        m.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        m.SetupGet(p => p.UserId).Returns((Guid?)null);
        return m.Object;
    }

    private static GetPositionHistoryRangeQuery Query(Guid accountId, Guid transporterId)
        => new(accountId, transporterId, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow);

    [Test]
    public void HistoryRange_UserWithoutVisibility_IsForbidden_AndReaderNotCalled()
    {
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reader = new Mock<ITransporterPositionHistoryReader>();
        var visible = new Mock<IVisibleTransporterReader>();
        visible.Setup(v => v.GetVisibleTransporterIdsAsync(userId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid>()); // sees nothing
        var handler = new GetPositionHistoryRangeQueryHandler(reader.Object, visible.Object, User(userId));

        Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(Query(accountId, transporterId), CancellationToken.None));
        reader.Verify(r => r.GetRangeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HistoryRange_VisibleUser_ReadsRange()
    {
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var reader = new Mock<ITransporterPositionHistoryReader>();
        reader.Setup(r => r.GetRangeAsync(accountId, transporterId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var visible = new Mock<IVisibleTransporterReader>();
        visible.Setup(v => v.GetVisibleTransporterIdsAsync(userId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid> { transporterId });
        var handler = new GetPositionHistoryRangeQueryHandler(reader.Object, visible.Object, User(userId));

        await handler.Handle(Query(accountId, transporterId), CancellationToken.None);

        reader.Verify(r => r.GetRangeAsync(accountId, transporterId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HistoryRange_ServiceClient_BypassesVisibilityCheck()
    {
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var reader = new Mock<ITransporterPositionHistoryReader>();
        reader.Setup(r => r.GetRangeAsync(accountId, transporterId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var visible = new Mock<IVisibleTransporterReader>();
        var handler = new GetPositionHistoryRangeQueryHandler(reader.Object, visible.Object, ServiceClient());

        await handler.Handle(Query(accountId, transporterId), CancellationToken.None);

        visible.Verify(v => v.GetVisibleTransporterIdsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reader.Verify(r => r.GetRangeAsync(accountId, transporterId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task BulkPosition_KeepsOnlyFreshestFixPerTransporter()
    {
        var transporterId = Guid.NewGuid();
        var older = new TransporterPositionDto(transporterId, null, 1, 1, null, DateTimeOffset.UtcNow.AddMinutes(-10), 0, null, null, null, null, null, null, null);
        var newer = new TransporterPositionDto(transporterId, null, 2, 2, null, DateTimeOffset.UtcNow, 0, null, null, null, null, null, null, null);
        IEnumerable<TransporterPositionDto>? captured = null;
        var writer = new Mock<ITransporterPositionWriter>();
        writer.Setup(w => w.BulkTransporterPositionAsync(It.IsAny<IEnumerable<TransporterPositionDto>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<TransporterPositionDto>, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);
        var handler = new CreateTransporterCommandHandler(writer.Object);

        await handler.Handle(new BulkTransporterPositionCommand([older, newer]), CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        var list = captured!.ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list, Has.Count.EqualTo(1), "one fix per transporter");
            Assert.That(list[0].DeviceDateTime, Is.EqualTo(newer.DeviceDateTime), "freshest fix wins");
        }
    }
}
