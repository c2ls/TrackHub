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
using Common.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TrackHub.Router.Application.DevicePositions.Commands.Health;
using TrackHub.Router.Application.DevicePositions.Commands.Sync;
using TrackHub.Router.Application.DevicePositions.Events;
using TrackHub.Router.Application.DevicePositions.Registry;
using TrackHub.Router.Application.Devices.Registry;
using TrackHub.Router.Application.PingOperator;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Infrastructure.Common;
using TrackHub.Router.Infrastructure.Common.Helpers;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class ProviderSyncTests : TestsContext
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:EncryptionKey"] = "4F2C2E66-107F-452A-ACDE-402DFD47B84C" })
        .Build();

    private RecordingHttpHandler _handler = null!;
    private ServiceProvider _provider = null!;
    private ProviderCapabilityCatalog _catalog = null!;
    private TimeSpan _gpsTrackitInterval;
    private TimeSpan _redGpsInterval;

    [SetUp]
    public void SetUp()
    {
        _gpsTrackitInterval = GpsTrackit.GpsTrackitReaderBase.MinRequestInterval;
        _redGpsInterval = RedGps.RedGpsReaderBase.MinRequestInterval;
        GpsTrackit.GpsTrackitReaderBase.MinRequestInterval = TimeSpan.Zero;
        RedGps.RedGpsReaderBase.MinRequestInterval = TimeSpan.Zero;
    }

    [TearDown]
    public void TearDown()
    {
        GpsTrackit.GpsTrackitReaderBase.MinRequestInterval = _gpsTrackitInterval;
        RedGps.RedGpsReaderBase.MinRequestInterval = _redGpsInterval;
        _provider?.Dispose();
        _handler?.Dispose();
    }

    private void Start(ProtocolType protocol)
    {
        _handler = new RecordingHttpHandler();
        _handler.Route((request, body) => EmptyPayload(protocol, request.RequestUri!.AbsolutePath, Uri.UnescapeDataString(request.RequestUri.Query) + body));
        var factory = new Mock<ICredentialHttpClientFactory>();
        factory.Setup(x => x.CreateClientAsync(It.IsAny<TrackHub.Router.Domain.Records.CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(() => _handler.CreateClient("https://provider.example/"));
        var services = new ServiceCollection();
        var descriptor = services.RegisterProtocol(protocol.ToString());
        services.AddTransient<IHttpClientService, HttpClientService>();
        services.AddSingleton<IProviderSessionStore, ProviderSessionStore>();
        services.AddSingleton(factory.Object);
        services.AddSingleton(Mock.Of<ICredentialWriter>());
        _provider = services.BuildServiceProvider();
        _catalog = new ProviderCapabilityCatalog([descriptor]);
    }

    private static string? EmptyPayload(ProtocolType protocol, string path, string call) => protocol switch
    {
        ProtocolType.Cartrack when path.EndsWith("/vehicles") || path.EndsWith("/vehicles/status") => """{"data":[]}""",
        ProtocolType.CommandTrack when path.EndsWith("/Auth/authenticate") => """{"token":"token","expires":"2099-01-01T00:00:00Z"}""",
        ProtocolType.CommandTrack when path.EndsWith("/api/AllDevices") || path.EndsWith("/api/Devices") => "[]",
        ProtocolType.Flespi when path.EndsWith("/devices/all") || path.EndsWith("/messages") => """{"result":[]}""",
        ProtocolType.GpsGate when path.EndsWith("/devices") || path.EndsWith("/applications") => "[]",
        ProtocolType.GpsGate when path.EndsWith("/devices/0") => """{"id":0,"name":"unit-1","imei":"unit-1"}""",
        ProtocolType.GpsTrackit when path.EndsWith("unit/search") => """{"items":[],"total":0}""",
        ProtocolType.Mapon when path.EndsWith("unit/list.json") => """{"data":{"units":[]}}""",
        ProtocolType.Mapon when path.EndsWith("company/get.json") => """{"data":{"companies":[]}}""",
        ProtocolType.Mettax when path.EndsWith("createToken") => """{"code":0,"data":"token"}""",
        ProtocolType.Mettax when path.EndsWith("device/page") => """{"code":0,"data":{"records":[],"total":0}}""",
        ProtocolType.Mettax when path.EndsWith("device/list") || path.EndsWith("shadow/deviceIds") => """{"code":0,"data":[]}""",
        ProtocolType.Motive when path.EndsWith("/v1/vehicles") || path.EndsWith("/v1/vehicle_locations") => """{"vehicles":[]}""",
        ProtocolType.Navixy when path.EndsWith("/user/auth") => """{"success":true,"hash":"hash"}""",
        ProtocolType.Navixy when path.EndsWith("/user/settings/get") => """{"success":true,"settings":{"time_zone":"UTC"}}""",
        ProtocolType.Navixy when path.EndsWith("/tracker/list") => """{"success":true,"list":[]}""",
        ProtocolType.Pegasus when path.EndsWith("/login") => """{"auth":"token"}""",
        ProtocolType.Pegasus when path.EndsWith("/vehicles") => """{"data":[],"page":1,"pages":1}""",
        ProtocolType.Pegasus when path.EndsWith("/user") => "{}",
        ProtocolType.Plaspy when path.EndsWith("/api/Auth/Token") => """{"success":true,"token":"token"}""",
        ProtocolType.Plaspy when path.EndsWith("/api/devices") => """{"success":true,"devices":[]}""",
        ProtocolType.Plaspy when path.EndsWith("/lastLocation") => """{"success":true}""",
        ProtocolType.Protrack when path.EndsWith("/api/authorization") => """{"code":0,"record":{"access_token":"token","expires_in":7200}}""",
        ProtocolType.Protrack when path.EndsWith("/api/device/list") || path.EndsWith("/api/track") => """{"code":0,"record":[]}""",
        ProtocolType.RedGps when path.EndsWith("/gettoken") => """{"status":200,"data":"token"}""",
        ProtocolType.RedGps when path.EndsWith("/vehicleGetAll") => """{"status":200,"data":[]}""",
        ProtocolType.RedGps when path.EndsWith("/getdata") => """{"status":40100}""",
        ProtocolType.Samsara when path.EndsWith("/fleet/vehicles/stats") => """{"data":[],"pagination":{"endCursor":"","hasNextPage":false}}""",
        ProtocolType.Traccar when path.EndsWith("/api/devices") => "[]",
        ProtocolType.Traccar when path.EndsWith("/api/server") => "{}",
        ProtocolType.Tracksolid when call.Contains("method=jimi.oauth.token.get") => """{"code":0,"result":{"accessToken":"token","expiresIn":7200}}""",
        ProtocolType.Tracksolid when path.EndsWith("/route/rest") => """{"code":0,"result":[]}""",
        ProtocolType.TrustTrack when path.EndsWith("/objects") => "[]",
        ProtocolType.TrustTrack when path.EndsWith("/objects-last-coordinate") => """{"results":[]}""",
        ProtocolType.Webfleet when call.Contains("action=showObjectReportExtern") || call.Contains("action=showSettings") => "[]",
        ProtocolType.Wialon when call.Contains("svc=token/login") => """{"eid":"session","user":"fleet","uid":1}""",
        ProtocolType.Wialon when call.Contains("svc=core/search_items") => """{"items":[],"totalItemsCount":0}""",
        _ => null
    };


    private OperatorVm Operator(ProtocolType protocol) => new(Guid.NewGuid(), (int)protocol, Guid.NewGuid(),
        TestCredentialTokenVm with { CredentialId = Guid.NewGuid(), Key2 = TestCredentialTokenVm.Key });

    // Geotab is excluded: its vendor SDK builds its own HttpClient, which the fake transport cannot intercept.
    private static readonly ProtocolType[] Providers =
    [
        ProtocolType.Cartrack, ProtocolType.CommandTrack, ProtocolType.Flespi, ProtocolType.GpsGate, ProtocolType.GpsTrackit,
        ProtocolType.Mapon, ProtocolType.Mettax, ProtocolType.Motive, ProtocolType.Navixy, ProtocolType.Pegasus,
        ProtocolType.Plaspy, ProtocolType.Protrack, ProtocolType.RedGps, ProtocolType.Samsara, ProtocolType.Traccar,
        ProtocolType.Tracksolid, ProtocolType.TrustTrack, ProtocolType.Webfleet, ProtocolType.Wialon,
    ];

    private static IEnumerable<ProtocolType> ProvidersWithDeviceCatalog => Providers.Where(SupportsDeviceCatalog);

    private static IEnumerable<ProtocolType> ProvidersWithoutDeviceCatalog => Providers.Where(protocol => !SupportsDeviceCatalog(protocol));

    private static bool SupportsDeviceCatalog(ProtocolType protocol)
        => new ServiceCollection().RegisterProtocol(protocol.ToString()).Capabilities.HasFlag(ProviderCapability.DeviceCatalog);

    private SyncOperatorDevicesCommandHandler DeviceSyncHandler(IDeviceSyncWriter deviceSyncWriter, IOperatorSyncRunWriter runWriter)
    {
        var syncLock = new Mock<IOperatorSyncLock>();
        syncLock.Setup(x => x.AcquireAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<IDisposable>());
        return new SyncOperatorDevicesCommandHandler(Configuration, new DeviceRegistry(_provider), deviceSyncWriter, runWriter,
            Mock.Of<IOperatorHealthCheckSystemWriter>(), Mock.Of<IAlertEventWriter>(), syncLock.Object, _catalog,
            NullLogger<SyncOperatorDevicesCommandHandler>.Instance);
    }

    [TestCaseSource(nameof(ProvidersWithDeviceCatalog))]
    public async Task DeviceSync_EmptyCatalog_RecordsASucceededRunWithNoDevices(ProtocolType protocol)
    {
        Start(protocol);
        var deviceSyncWriter = new Mock<IDeviceSyncWriter>();
        var runWriter = new Mock<IOperatorSyncRunWriter>();
        OperatorSyncRunDto? run = null;
        runWriter.Setup(x => x.RecordAsync(It.IsAny<OperatorSyncRunDto>(), It.IsAny<CancellationToken>()))
            .Callback<OperatorSyncRunDto, CancellationToken>((dto, _) => run = dto);

        await DeviceSyncHandler(deviceSyncWriter.Object, runWriter.Object).Handle(new SyncOperatorDevicesCommand(Operator(protocol), "AUTOMATIC"), CancellationToken.None);

        Assert.That(run?.Result, Is.EqualTo("SUCCEEDED"), run?.ErrorMessage);
        deviceSyncWriter.Verify(x => x.SynchronizeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.Is<IEnumerable<SynchronizedDeviceDto>>(devices => !devices.Any()), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCaseSource(nameof(ProvidersWithoutDeviceCatalog))]
    public async Task DeviceSync_ProviderWithoutDeviceCatalog_SkipsWithoutCallingTheProviderOrRecordingARun(ProtocolType protocol)
    {
        Start(protocol);
        var deviceSyncWriter = new Mock<IDeviceSyncWriter>();
        var runWriter = new Mock<IOperatorSyncRunWriter>();

        var synced = await DeviceSyncHandler(deviceSyncWriter.Object, runWriter.Object).Handle(new SyncOperatorDevicesCommand(Operator(protocol), "AUTOMATIC"), CancellationToken.None);

        Assert.That(synced, Is.EqualTo(OperatorRunOutcome.Skipped));
        Assert.That(_handler.Requests, Is.Empty);
        runWriter.Verify(x => x.RecordAsync(It.IsAny<OperatorSyncRunDto>(), It.IsAny<CancellationToken>()), Times.Never);
        deviceSyncWriter.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(Providers))]
    public async Task PositionSync_NoPositions_PublishesAnEmptyReadWithoutAProviderError(ProtocolType protocol)
    {
        Start(protocol);
        var catalog = new Mock<IDeviceCatalogCache>();
        catalog.Setup(x => x.GetOrLoadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset?>(),
                It.IsAny<Func<CancellationToken, Task<IEnumerable<DeviceTransporterVm>>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DeviceTransporterVm(Guid.NewGuid(), 0, "unit-1", "ABC123", "Truck", (short)TransporterType.Truck)]);
        var publisher = new Mock<IPublisher>();
        PositionsRetrieved.Notification? published = null;
        publisher.Setup(x => x.Publish(It.IsAny<PositionsRetrieved.Notification>(), It.IsAny<CancellationToken>()))
            .Callback<PositionsRetrieved.Notification, CancellationToken>((notification, _) => published = notification);
        var handler = new GetPositionsByOperatorCommandHandler(publisher.Object, Configuration, new PositionRegistry(_provider),
            Mock.Of<IDeviceTransporterReader>(), catalog.Object, _catalog, NullLogger<GetPositionsByOperatorCommandHandler>.Instance);
        var @operator = Operator(protocol);

        await handler.Handle(new GetPositionsByOperatorCommand(@operator, new AccountSettingsVm(@operator.AccountId, 60, false, false, true)), CancellationToken.None);

        Assert.That(published, Is.Not.Null);
        Assert.That(published!.Value.ProviderErrorCode, Is.Null, published.Value.ProviderErrorMessage);
        Assert.That(published.Value.Positions, Is.Empty);
        Assert.That(_handler.Requests, Is.Not.Empty);
    }

    [TestCaseSource(nameof(Providers))]
    public async Task HealthCheck_ProviderAnswers_RecordsTheOperatorHealthy(ProtocolType protocol)
    {
        Start(protocol);
        var healthWriter = new Mock<IOperatorHealthCheckWriter>();
        OperatorHealthCheckDto? check = null;
        healthWriter.Setup(x => x.RecordAsync(It.IsAny<OperatorHealthCheckDto>(), It.IsAny<CancellationToken>()))
            .Callback<OperatorHealthCheckDto, CancellationToken>((dto, _) => check = dto);
        var handler = new RecordOperatorHealthCommandHandler(Configuration, new ConnectivityRegistry(_provider), healthWriter.Object,
            Mock.Of<IAlertEventWriter>(), _catalog, NullLogger<RecordOperatorHealthCommandHandler>.Instance);

        await handler.Handle(new RecordOperatorHealthCommand(Operator(protocol)), CancellationToken.None);

        Assert.That(check?.Status, Is.EqualTo("HEALTHY"), check?.ErrorMessage);
    }
}
