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

using System.Diagnostics;
using System.Net;
using Common.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.RedGps.Tests;

internal sealed class RedGpsFakeHttpClientService(Func<string, IReadOnlyDictionary<string, string>, string> respond) : IHttpClientService
{
    public const string BaseUrl = "https://api.service24gps.com/api/v1";

    public List<(string Action, Dictionary<string, string> Form)> Requests { get; } = [];

    public IEnumerable<string> Actions => Requests.Select(r => r.Action);

    public void Init(HttpClient httpClient, string clientName)
    {
    }

    public Task<T?> GetAsync<T>(string url, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public async Task<T?> PostAsync<T>(string url, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var body = await ((HttpContent)parameters!).ReadAsStringAsync(cancellationToken);
        var form = body.Split('&')
            .Select(pair => pair.Split('='))
            .ToDictionary(pair => WebUtility.UrlDecode(pair[0]), pair => WebUtility.UrlDecode(pair[1]));
        var action = url[(BaseUrl.Length + 1)..];
        Requests.Add((action, form));
        return respond(action, form).Deserialize<T>();
    }
}

internal static class RedGpsTestData
{
    public const string TokenJson = """{"status":200,"data":"tok-1"}""";

    public static CredentialTokenDto Credential(string? token = null, DateTimeOffset? expiration = null)
        => new(Guid.NewGuid(), RedGpsFakeHttpClientService.BaseUrl + "/", "fleet-user", "secret-pw", "api-key", null,
            token, expiration, null, null);

    public static ICredentialHttpClientFactory ClientFactory()
    {
        var factory = new Mock<ICredentialHttpClientFactory>();
        factory.Setup(f => f.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(() => new HttpClient());
        return factory.Object;
    }

    public static string Respond(string action, string json)
        => action == "gettoken" ? TokenJson : json;
}

[TestFixture]
public class RedGpsPositionReaderTests
{
    private Mock<ICredentialWriter> _credentialWriter = null!;
    private ProviderSessionStore _sessionStore = null!;

    [SetUp]
    public void SetUp()
    {
        RedGpsReaderBase.MinRequestInterval = TimeSpan.Zero;
        _credentialWriter = new Mock<ICredentialWriter>();
        _sessionStore = new ProviderSessionStore();
    }

    [TearDown]
    public void TearDown() => RedGpsReaderBase.MinRequestInterval = TimeSpan.FromSeconds(30);

    private PositionReader CreateReader(RedGpsFakeHttpClientService http)
        => new(RedGpsTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);

    private static DeviceTransporterVm Device(string serial)
        => new(Guid.NewGuid(), 0, serial, "Truck 1", "Truck", (short)TransporterType.Truck);

    [Test]
    public async Task Init_WithoutToken_RequestsTokenAndPersistsIt()
    {
        var http = new RedGpsFakeHttpClientService((_, _) => RedGpsTestData.TokenJson);
        var credential = RedGpsTestData.Credential();

        await CreateReader(http).Init(credential);

        var (action, form) = http.Requests.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action, Is.EqualTo("gettoken"));
            Assert.That(form["apikey"], Is.EqualTo("api-key"));
            Assert.That(form["token"], Is.Empty);
            Assert.That(form["username"], Is.EqualTo("fleet-user"));
            Assert.That(form["password"], Is.EqualTo("secret-pw"));
        }
        _credentialWriter.Verify(w => w.UpdateTokenAsync(credential.CredentialId,
            It.Is<UpdateTokenDto>(t => t.Token == "tok-1" && t.TokenExpiration > DateTimeOffset.UtcNow.AddMinutes(359)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Init_WithValidPersistedToken_MakesNoRequest()
    {
        var http = new RedGpsFakeHttpClientService((_, _) => throw new AssertionException("no call expected"));

        await CreateReader(http).Init(RedGpsTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(3)));

        Assert.That(http.Requests, Is.Empty);
    }

    [Test]
    public async Task Init_WithCachedSession_DoesNotRequestTokenTwice()
    {
        var http = new RedGpsFakeHttpClientService((_, _) => RedGpsTestData.TokenJson);
        var credential = RedGpsTestData.Credential();

        await CreateReader(http).Init(credential);
        await CreateReader(http).Init(credential);

        Assert.That(http.Requests, Has.Count.EqualTo(1));
    }

    [TestCase("""{"status":30500,"data":""}""")]
    [TestCase("""{"status":"200","data":""}""")]
    public async Task Init_WhenTokenIsRefused_Throws(string json)
    {
        var http = new RedGpsFakeHttpClientService((_, _) => json);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateReader(http).Init(RedGpsTestData.Credential()));
    }

    [Test]
    public async Task GetDevicePositionAsync_Many_UsesGetDataAndReturnsOnlyRequestedDevices()
    {
        const string data = """
            {"status":200,"data":[
              {"UnitId":"Jeep","GpsIdentif":"0580012342332337","ReportDate":"2021-04-26 16:42:43","Latitude":"+19.528530","Longitude":"-99.171165","Altitude":2240,"GpsSpeed":"55","Direction":"SO","Satellites":"22","Ignition":"1","Odometer":123456,"Domicilio":"Camino a San Juan 479"},
              {"UnitId":"Van","GpsIdentif":"0580012342332338","ReportDate":"2021-04-26 16:00:00","Latitude":"4.6","Longitude":"-74.08","Altitude":0,"GpsSpeed":"0","Direction":"W","Satellites":"-1","Ignition":"2","Odometer":"0"},
              {"UnitId":"Other","GpsIdentif":"999","ReportDate":"2021-04-26 16:00:00","Latitude":"1","Longitude":"1","GpsSpeed":"0","Direction":"N"}
            ]}
            """;
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, data));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());
        var first = Device("0580012342332337");
        var second = Device("0580012342332338");

        var result = (await reader.GetDevicePositionAsync([first, second], CancellationToken.None)).ToList();

        var request = http.Requests.Last();
        var a = result.Single(p => p.TransporterId == first.TransporterId);
        var b = result.Single(p => p.TransporterId == second.TransporterId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.Action, Is.EqualTo("getdata"));
            Assert.That(request.Form["UseUTCDate"], Is.EqualTo("1"));
            Assert.That(request.Form["apikey"], Is.EqualTo("api-key"));
            Assert.That(request.Form["token"], Is.EqualTo("tok-1"));
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(a.Latitude, Is.EqualTo(19.52853));
            Assert.That(a.Longitude, Is.EqualTo(-99.171165));
            Assert.That(a.Altitude, Is.EqualTo(2240));
            Assert.That(a.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2021, 4, 26, 16, 42, 43, TimeSpan.Zero)));
            Assert.That(a.Speed, Is.EqualTo(55));
            Assert.That(a.Course, Is.EqualTo(225));
            Assert.That(a.Address, Is.EqualTo("Camino a San Juan 479"));
            Assert.That(a.Attributes!.Value.Ignition, Is.True);
            Assert.That(a.Attributes!.Value.Satellites, Is.EqualTo(22));
            Assert.That(a.Attributes!.Value.Mileage, Is.EqualTo(123456));
            Assert.That(b.Course, Is.EqualTo(270));
            Assert.That(b.Attributes!.Value.Ignition, Is.Null);
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_Single_FiltersGetDataByIdentifier()
    {
        const string data = """{"status":200,"data":[{"GpsIdentif":"1","ReportDate":"2021-04-26 16:00:00","Latitude":"1","Longitude":"2"},{"GpsIdentif":"2","ReportDate":"2021-04-26 17:00:00","Latitude":"3","Longitude":"4","Direction":"NE"}]}""";
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, data));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        var result = await reader.GetDevicePositionAsync(Device("2"), CancellationToken.None);

        Assert.That((result.Latitude, result.Longitude, result.Course), Is.EqualTo((3d, 4d, (double?)45)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync(Device("3"), CancellationToken.None));
    }

    [Test]
    public async Task GetDevicePositionAsync_ErrorStatus_Throws()
    {
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":30700,"data":""}"""));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync([Device("1")], CancellationToken.None));
    }

    [Test]
    public async Task GetDevicePositionAsync_NoDataStatus_IsEmpty()
    {
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":40100,"data":""}"""));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        Assert.That(await reader.GetDevicePositionAsync([Device("1")], CancellationToken.None), Is.Empty);
    }

    [Test]
    public async Task InvalidToken_ReauthenticatesAndRetriesOnce()
    {
        var calls = 0;
        var http = new RedGpsFakeHttpClientService((action, _) => action switch
        {
            "getdata" when calls++ == 0 => """{"status":30400,"data":""}""",
            "getdata" => """{"status":200,"data":[{"GpsIdentif":"1","ReportDate":"2021-04-26 16:00:00","Latitude":"1","Longitude":"2"}]}""",
            _ => RedGpsTestData.TokenJson
        });
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential("stale", DateTimeOffset.UtcNow.AddHours(3)));

        var result = await reader.GetDevicePositionAsync([Device("1")], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(http.Actions, Is.EqualTo(new[] { "getdata", "gettoken", "getdata" }));
            Assert.That(http.Requests[0].Form["token"], Is.EqualTo("stale"));
            Assert.That(http.Requests[2].Form["token"], Is.EqualTo("tok-1"));
        }
    }

    [Test]
    public async Task Unauthorized_ReauthenticatesAndRetriesOnce()
    {
        var calls = 0;
        var http = new RedGpsFakeHttpClientService((action, _) => action switch
        {
            "getdata" when calls++ == 0 => throw new HttpRequestException("Invalid API Key or User Token", null, HttpStatusCode.Unauthorized),
            "getdata" => """{"status":200,"data":[]}""",
            _ => RedGpsTestData.TokenJson
        });
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential("stale", DateTimeOffset.UtcNow.AddHours(3)));

        await reader.GetDevicePositionAsync([Device("1")], CancellationToken.None);

        Assert.That(http.Actions, Is.EqualTo(new[] { "getdata", "gettoken", "getdata" }));
    }

    [Test]
    public async Task InvalidToken_AfterReauthentication_Throws()
    {
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":30400,"data":""}"""));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential("stale", DateTimeOffset.UtcNow.AddHours(3)));

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync([Device("1")], CancellationToken.None));
        Assert.That(http.Actions.Count(a => a == "gettoken"), Is.EqualTo(1));
    }

    [Test]
    public async Task GetPositionAsync_PadsAndChunksRequestsAndConvertsGmtToUtc()
    {
        var from = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2023, 3, 20, 0, 0, 0, TimeSpan.Zero);
        const string history = """
            {"status":200,"data":[
              {"equipo":"058","latitud":"+19.528530","longitud":"-99.171165","gmt":"-5","fecha":"2023-03-08","hora":"23:59:39","fecha_reporte":"20230308235939","velocidad":"42","orientacion":"90","satelites":"20","odometro":"57373562","ignicion":"1","domicilio":"Somewhere"},
              {"equipo":"058","latitud":"1","longitud":"1","gmt":"-5","fecha_reporte":"20230228185959","velocidad":"0","orientacion":"0","ignicion":"0"}
            ]}
            """;
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, history));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        var result = (await reader.GetPositionAsync(from, to, Device("058"), CancellationToken.None)).ToList();

        var windows = http.Requests.Where(r => r.Action == "history/get").Select(r => r.Form).ToList();
        var point = result.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(windows, Has.Count.EqualTo(2));
            Assert.That(windows[0]["equipo"], Is.EqualTo("058"));
            Assert.That(windows[0]["format"], Is.EqualTo("DateTime"));
            Assert.That(windows[0]["fechaIni"], Is.EqualTo("2023-02-28 10:00:00"));
            Assert.That(windows[0]["fechaFin"], Is.EqualTo("2023-03-15 09:59:59"));
            Assert.That(windows[1]["fechaIni"], Is.EqualTo("2023-03-14 06:00:00"));
            Assert.That(windows[1]["fechaFin"], Is.EqualTo("2023-03-20 14:00:00"));
            Assert.That(point.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2023, 3, 9, 4, 59, 39, TimeSpan.Zero)));
            Assert.That(point.Latitude, Is.EqualTo(19.52853));
            Assert.That(point.Speed, Is.EqualTo(42));
            Assert.That(point.Course, Is.EqualTo(90));
            Assert.That(point.Address, Is.EqualTo("Somewhere"));
            Assert.That(point.Attributes!.Value.Mileage, Is.EqualTo(57373562));
            Assert.That(point.Attributes!.Value.Satellites, Is.EqualTo(20));
            Assert.That(point.Attributes!.Value.Ignition, Is.True);
        }
    }

    [Test]
    public async Task GetPositionAsync_SingleObjectData_IsOnePoint()
    {
        const string history = """{"status":200,"data":{"latitud":"1","longitud":"2","gmt":"0","fecha_reporte":"20230308120000","velocidad":"0","orientacion":"0"}}""";
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, history));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        var result = await reader.GetPositionAsync(
            new DateTimeOffset(2023, 3, 8, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 3, 9, 0, 0, 0, TimeSpan.Zero),
            Device("1"), CancellationToken.None);

        Assert.That(result.Single().DeviceDateTime, Is.EqualTo(new DateTimeOffset(2023, 3, 8, 12, 0, 0, TimeSpan.Zero)));
    }

    [Test]
    public async Task GetPositionAsync_PointWithoutGmt_Throws()
    {
        const string history = """{"status":200,"data":[{"latitud":"1","longitud":"2","fecha_reporte":"20230308120000"}]}""";
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, history));
        var reader = CreateReader(http);
        await reader.Init(RedGpsTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetPositionAsync(
            new DateTimeOffset(2023, 3, 8, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 3, 9, 0, 0, 0, TimeSpan.Zero),
            Device("1"), CancellationToken.None));
    }

    [Test]
    public async Task Ping_CallsVehicleListEveryTime()
    {
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":200,"data":[]}"""));
        var tester = new ConnectivityTester(RedGpsTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);
        var credential = RedGpsTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(3));

        await tester.Ping(credential, CancellationToken.None);
        await tester.Ping(credential, CancellationToken.None);

        Assert.That(http.Actions, Is.EqualTo(new[] { "vehicleGetAll", "vehicleGetAll" }));
    }

    [Test]
    public async Task Pacer_SpacesRequestsOfOneCredential()
    {
        RedGpsReaderBase.MinRequestInterval = TimeSpan.FromMilliseconds(300);
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":200,"data":[]}"""));
        var tester = new ConnectivityTester(RedGpsTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);
        var credential = RedGpsTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(3));

        await tester.Ping(credential, CancellationToken.None);
        var stopwatch = Stopwatch.StartNew();
        await tester.Ping(credential, CancellationToken.None);

        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(250)));
    }

    [Test]
    public async Task Pacer_DoesNotDelayOtherCredentials()
    {
        RedGpsReaderBase.MinRequestInterval = TimeSpan.FromSeconds(30);
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":200,"data":[]}"""));
        var tester = new ConnectivityTester(RedGpsTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);

        var stopwatch = Stopwatch.StartNew();
        await tester.Ping(RedGpsTestData.Credential("a", DateTimeOffset.UtcNow.AddHours(3)), CancellationToken.None);
        await tester.Ping(RedGpsTestData.Credential("b", DateTimeOffset.UtcNow.AddHours(3)), CancellationToken.None);

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public async Task Pacer_WaitHonoursCancellation()
    {
        RedGpsReaderBase.MinRequestInterval = TimeSpan.FromSeconds(30);
        var http = new RedGpsFakeHttpClientService((action, _) => RedGpsTestData.Respond(action, """{"status":200,"data":[]}"""));
        var tester = new ConnectivityTester(RedGpsTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);
        var credential = RedGpsTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(3));
        await tester.Ping(credential, CancellationToken.None);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.CatchAsync<OperationCanceledException>(() => tester.Ping(credential, cancellation.Token));
        Assert.That(http.Requests, Has.Count.EqualTo(1));
    }
}
