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

using Common.Domain.Constants;
using Common.Mediator;
using HotChocolate.Execution;
using Moq;
using TrackHub.Manager.Application.AuditEvents.Commands;
using TrackHub.Reporting.Domain.Records;
using TrackHub.Reporting.Infrastructure.GraphQLApi;
using TrackHub.ServiceContracts.Harness;
using TrackHub.ServiceContracts.Tests.Harness;
using TrackHub.Telemetry.Application.GpsIntegration.Queries;
using TrackHubRouter.Application.Positions.Queries.GetRange;
using ManagerAuditVm = TrackHub.Manager.Domain.Models.AuditEventVm;
using RouterModels = TrackHubRouter.Domain.Models;
using TelemetryModels = TrackHub.Telemetry.Domain.Models;

namespace TrackHub.ServiceContracts.Tests.RoundTripTests;

// For Reporting's complex/critical flows: the health-summary
// aggregation (Telemetry), the date-range position replay (Router), and the per-export
// audit write (Manager). Simple CRUD reads are covered by Layer A only.
[TestFixture]
public class ReportingToTelemetryRoundTripTests
{
    private Mock<ISender> _sender = null!;
    private InProcessGraphQLClientFactory _factory = null!;

    [OneTimeSetUp]
    public async Task BuildTelemetryExecutor()
    {
        _sender = new Mock<ISender>();
        var executor = await ProducerSchema.BuildTelemetryExecutorAsync(_sender.Object);
        _factory = new InProcessGraphQLClientFactory(
            new Dictionary<string, IRequestExecutor> { [Clients.Telemetry] = executor });
    }

    [SetUp]
    public void ResetSender() => _sender.Reset();

    [Test]
    public async Task GetOperatorHealthSummary_RoundTripsAggregationIntoReportingVm()
    {
        _sender
            .Setup(s => s.Send(It.IsAny<GetOperatorHealthSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TelemetryModels.OperatorHealthSummaryVm(
                OperatorId: FakeData.OperatorId,
                Since: FakeData.Timestamp.AddHours(-24),
                TotalChecks: 48,
                HealthyChecks: 40,
                DegradedChecks: 6,
                OfflineChecks: 2,
                FailureCount: 8,
                UptimePercent: 83.33,
                AverageLatencyMs: 145.5,
                LastCheckAt: FakeData.Timestamp,
                LastFailureAt: FakeData.Timestamp.AddHours(-3),
                LastFailureCode: "TIMEOUT"));

        var reader = new GpsTelemetryReader(_factory);
        var summary = await reader.GetOperatorHealthSummaryAsync(FakeData.OperatorId, 24, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(summary.OperatorId, Is.EqualTo(FakeData.OperatorId));
            Assert.That(summary.Since, Is.EqualTo(FakeData.Timestamp.AddHours(-24)));
            Assert.That(summary.TotalChecks, Is.EqualTo(48));
            Assert.That(summary.HealthyChecks, Is.EqualTo(40));
            Assert.That(summary.DegradedChecks, Is.EqualTo(6));
            Assert.That(summary.OfflineChecks, Is.EqualTo(2));
            Assert.That(summary.FailureCount, Is.EqualTo(8));
            Assert.That(summary.UptimePercent, Is.EqualTo(83.33));
            Assert.That(summary.AverageLatencyMs, Is.EqualTo(145.5));
            Assert.That(summary.LastCheckAt, Is.EqualTo(FakeData.Timestamp));
            Assert.That(summary.LastFailureCode, Is.EqualTo("TIMEOUT"));
        }

        _sender.Verify(s => s.Send(
            It.Is<GetOperatorHealthSummaryQuery>(q => q.OperatorId == FakeData.OperatorId && q.LookbackHours == 24),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

[TestFixture]
public class ReportingToRouterRoundTripTests
{
    private Mock<ISender> _sender = null!;
    private InProcessGraphQLClientFactory _factory = null!;

    [OneTimeSetUp]
    public async Task BuildRouterExecutor()
    {
        _sender = new Mock<ISender>();
        var executor = await ProducerSchema.BuildRouterExecutorAsync(_sender.Object);
        _factory = new InProcessGraphQLClientFactory(
            new Dictionary<string, IRequestExecutor> { [Clients.Router] = executor });
    }

    [SetUp]
    public void ResetSender() => _sender.Reset();

    [Test]
    public async Task GetPositionsRecord_RoundTripsDateRangeReplayIncludingHourmeterBinding()
    {
        var from = FakeData.Timestamp.AddHours(-6);
        var to = FakeData.Timestamp;
        _sender
            .Setup(s => s.Send(It.IsAny<GetPositionsRecordQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new RouterModels.PositionVm(
                    TransporterId: FakeData.TransporterId,
                    DeviceName: "Device-01",
                    TransporterType: "CAR",
                    Latitude: 4.6534,
                    Longitude: -74.0837,
                    Altitude: 2601.5,
                    DeviceDateTime: FakeData.Timestamp,
                    ServerDateTime: FakeData.Timestamp.AddSeconds(2),
                    Speed: 42.5,
                    Course: 187.3,
                    EventId: 7,
                    Address: "Cll 100 # 8-20",
                    City: "Bogota",
                    State: "Bogota D.C.",
                    Country: "CO",
                    Attributes: new RouterModels.AttributesVm(
                        Ignition: true,
                        Satellites: 12,
                        Mileage: 12345.6,
                        Hourmeter: 220.5,
                        Temperature: 21.5)),
            ]);

        var reader = new RouterReader(_factory);
        var filters = default(FilterDto) with
        {
            StringFilter1 = FakeData.TransporterId.ToString(),
            DateTimeFilter1 = from,
            DateTimeFilter2 = to,
        };
        var positions = (await reader.GetPositionsRecordAsync(filters, CancellationToken.None)).ToList();

        Assert.That(positions, Has.Count.EqualTo(1));
        var position = positions[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.TransporterId, Is.EqualTo(FakeData.TransporterId));
            Assert.That(position.DeviceName, Is.EqualTo("Device-01"));
            Assert.That(position.TransporterType, Is.EqualTo("CAR"));
            Assert.That(position.DeviceDateTime, Is.EqualTo(FakeData.Timestamp));
            Assert.That(position.ServerDateTime, Is.EqualTo(FakeData.Timestamp.AddSeconds(2)));
            Assert.That(position.Speed, Is.EqualTo(42.5));
            Assert.That(position.Attributes, Is.Not.Null);
            Assert.That(position.Attributes!.Value.Satellites, Is.EqualTo(12));
            Assert.That(position.Attributes!.Value.Hourmeter, Is.EqualTo(220.5),
                "the wire field 'hourmeter' must bind to the Reporting hourmeter column");
            Assert.That(position.Attributes!.Value.Temperature, Is.EqualTo(21.5));
        }

        // The consumer sends the transporter id as a STRING variable typed UUID! — this pins
        // that coercion, plus the date-range variables, into the Router's request type.
        _sender.Verify(s => s.Send(
            It.Is<GetPositionsRecordQuery>(q =>
                q.TransporterId == FakeData.TransporterId && q.From == from && q.To == to),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

[TestFixture]
public class ReportingToManagerRoundTripTests
{
    private Mock<ISender> _sender = null!;
    private InProcessGraphQLClientFactory _factory = null!;

    [OneTimeSetUp]
    public async Task BuildManagerExecutor()
    {
        _sender = new Mock<ISender>();
        var executor = await ProducerSchema.BuildManagerExecutorAsync(_sender.Object);
        _factory = new InProcessGraphQLClientFactory(
            new Dictionary<string, IRequestExecutor> { [Clients.Manager] = executor });
    }

    [SetUp]
    public void ResetSender() => _sender.Reset();

    [Test]
    public async Task RecordReportExport_DeliversAuditEventForEveryExport()
    {
        CreateAuditEventCommand? received = null;
        _sender
            .Setup(s => s.Send(It.IsAny<CreateAuditEventCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ManagerAuditVm>, CancellationToken>((cmd, _) => received = (CreateAuditEventCommand)cmd)
            .ReturnsAsync(default(ManagerAuditVm) with { AuditEventId = FakeData.SyncRunId });

        var writer = new ReportAuditWriter(_factory);
        await writer.RecordReportExportAsync(
            FakeData.AccountId,
            actorType: "User",
            actorId: FakeData.OperatorId.ToString(),
            reportCode: "gps.syncStatistics",
            filtersJson: "{\"operatorId\":null}",
            rowCount: 42,
            format: "xlsx",
            correlationId: "corr-report-1",
            CancellationToken.None);

        Assert.That(received, Is.Not.Null, "the real CreateAuditEventCommand must reach the Manager handler");
        var audit = received!.Value.AuditEvent;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(audit.AccountId, Is.EqualTo(FakeData.AccountId));
            Assert.That(audit.ActorType, Is.EqualTo("User"));
            Assert.That(audit.ActorId, Is.EqualTo(FakeData.OperatorId.ToString()));
            Assert.That(audit.Action, Is.EqualTo("ReportExported"));
            Assert.That(audit.ResourceType, Is.EqualTo("Report"));
            Assert.That(audit.ResourceId, Is.EqualTo("gps.syncStatistics"));
            Assert.That(audit.Result, Is.EqualTo("Success"));
            Assert.That(audit.NewValuesJson, Does.Contain("\"rowCount\":42"));
            Assert.That(audit.NewValuesJson, Does.Contain("gps.syncStatistics"));
            Assert.That(audit.CorrelationId, Is.EqualTo("corr-report-1"));
        }
    }
}
