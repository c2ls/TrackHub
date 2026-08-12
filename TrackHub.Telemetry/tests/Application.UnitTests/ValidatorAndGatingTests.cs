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

using Moq;
using TrackHub.Telemetry.Application.GpsIntegration.Commands;
using TrackHub.Telemetry.Application.GpsIntegration.Queries;
using TrackHub.Telemetry.Application.TransporterPosition.Commands.Create;
using TrackHub.Telemetry.Domain.Interfaces;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Application.UnitTests;

[TestFixture]
public class ValidatorAndGatingTests
{
    private static TransporterPositionDto Pos(double lat = 0, double lon = 0)
        => new(Guid.NewGuid(), null, lat, lon, null, DateTimeOffset.UtcNow, 0, null, null, null, null, null, null, null);

    [Test]
    public void HistoryRangeValidator_AcceptsAValidRange()
    {
        var v = new GetPositionHistoryRangeQueryValidator();
        var query = new GetPositionHistoryRangeQuery(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, 5000);
        Assert.That(v.Validate(query).IsValid, Is.True);
    }

    [Test]
    public void HistoryRangeValidator_RejectsInvertedRange_OversizedRange_AndBadMaxPoints()
    {
        var v = new GetPositionHistoryRangeQueryValidator();
        var now = DateTimeOffset.UtcNow;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v.Validate(new GetPositionHistoryRangeQuery(Guid.NewGuid(), Guid.NewGuid(), now, now.AddHours(-1), 10)).IsValid, Is.False, "From must be before To");
            Assert.That(v.Validate(new GetPositionHistoryRangeQuery(Guid.NewGuid(), Guid.NewGuid(), now.AddDays(-40), now, 10)).IsValid, Is.False, "range exceeds 31 days");
            Assert.That(v.Validate(new GetPositionHistoryRangeQuery(Guid.NewGuid(), Guid.NewGuid(), now.AddDays(-1), now, 0)).IsValid, Is.False, "MaxPoints must be >= 1");
            Assert.That(v.Validate(new GetPositionHistoryRangeQuery(Guid.NewGuid(), Guid.NewGuid(), now.AddDays(-1), now, 20000)).IsValid, Is.False, "MaxPoints capped at 10000");
            Assert.That(v.Validate(new GetPositionHistoryRangeQuery(Guid.Empty, Guid.NewGuid(), now.AddDays(-1), now, 10)).IsValid, Is.False, "AccountId required");
        }
    }

    [Test]
    public void BulkValidator_RejectsEmpty_OversizedBatch_AndOutOfRangeCoordinates()
    {
        var v = new BulkTransporterPositionCommandValidator();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(v.Validate(new BulkTransporterPositionCommand([])).IsValid, Is.False, "empty batch");
            Assert.That(v.Validate(new BulkTransporterPositionCommand([Pos(lat: 200)])).IsValid, Is.False, "latitude out of range");
            Assert.That(v.Validate(new BulkTransporterPositionCommand([Pos(lon: 500)])).IsValid, Is.False, "longitude out of range");
            Assert.That(v.Validate(new BulkTransporterPositionCommand([Pos(lat: 10, lon: 20)])).IsValid, Is.True, "valid position");
        }
    }

    [Test]
    public async Task AppendBatch_WhenHistoryDisabled_ReturnsZero_AndDoesNotWrite()
    {
        var accountId = Guid.NewGuid();
        var writer = new Mock<ITransporterPositionHistoryWriter>();
        var policy = new Mock<IPositionRetentionPolicyReader>();
        policy.Setup(p => p.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionRetentionPolicyVm(false, 0, "Default"));
        var handler = new AppendPositionHistoryBatchCommandHandler(writer.Object, policy.Object);

        var appended = await handler.Handle(new AppendPositionHistoryBatchCommand(accountId, [History(accountId)]), CancellationToken.None);

        Assert.That(appended, Is.EqualTo(0));
        writer.Verify(w => w.AppendRangeAsync(It.IsAny<IReadOnlyCollection<TransporterPositionHistoryDto>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AppendBatch_WhenHistoryEnabled_WritesOnlyRowsForTheRequestAccount()
    {
        var accountId = Guid.NewGuid();
        var otherAccount = Guid.NewGuid();
        IReadOnlyCollection<TransporterPositionHistoryDto>? captured = null;
        var writer = new Mock<ITransporterPositionHistoryWriter>();
        writer.Setup(w => w.AppendRangeAsync(It.IsAny<IReadOnlyCollection<TransporterPositionHistoryDto>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<TransporterPositionHistoryDto>, CancellationToken>((rows, _) => captured = rows)
            .ReturnsAsync(1);
        var policy = new Mock<IPositionRetentionPolicyReader>();
        policy.Setup(p => p.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionRetentionPolicyVm(true, 30, "Account"));
        var handler = new AppendPositionHistoryBatchCommandHandler(writer.Object, policy.Object);

        await handler.Handle(new AppendPositionHistoryBatchCommand(accountId, [History(accountId), History(otherAccount)]), CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(captured!, Has.Count.EqualTo(1), "cross-account rows are filtered out");
            Assert.That(captured!.Single().AccountId, Is.EqualTo(accountId));
        }
    }

    [Test]
    public async Task RecordHealthHandler_DelegatesToWriter()
    {
        var writer = new Mock<IOperatorHealthCheckWriter>();
        var dto = new OperatorHealthCheckDto(Guid.NewGuid(), Guid.NewGuid(), Domain.Enums.OperatorHealthCheckType.Ping, Domain.Enums.OperatorHealthStatus.Healthy, 10, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null, 0, null);
        writer.Setup(w => w.RecordAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperatorHealthCheckVm(Guid.NewGuid(), dto.AccountId, dto.OperatorId, dto.CheckType, dto.Status, dto.LatencyMs, dto.StartedAt, dto.CompletedAt, null, null, 0, null));
        var handler = new RecordOperatorHealthCommandHandler(writer.Object);

        await handler.Handle(new RecordOperatorHealthCommand(dto), CancellationToken.None);

        writer.Verify(w => w.RecordAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static TransporterPositionHistoryDto History(Guid accountId)
        => new(accountId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, 1, 2, null, 0, null, null, null, null, null, null, null, Guid.NewGuid().ToString());
}
