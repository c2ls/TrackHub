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

using System.Globalization;
using System.Net;
using Common.Domain.Extensions;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Common;
using TrackHub.Router.Infrastructure.Tracksolid.Helpers;

namespace TrackHub.Router.Infrastructure.Tracksolid.Tests;

internal sealed class TracksolidFakeHttpClientService(Func<IReadOnlyDictionary<string, string>, string> respond) : IHttpClientService
{
    public List<(string Url, Dictionary<string, string> Form)> Requests { get; } = [];

    public IEnumerable<string> Methods => Requests.Select(r => r.Form["method"]);

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
        Requests.Add((url, form));
        return respond(form).Deserialize<T>();
    }
}

internal static class TracksolidTestData
{
    public const string AppSecret = "app-secret";

    public const string TokenJson =
        """{"code":0,"message":"success","result":{"accessToken":"tok-1","expiresIn":7200,"refreshToken":"ref-1","time":"2026-11-15 10:00:00"}}""";

    public static CredentialTokenDto Credential(string? token = null, DateTimeOffset? expiration = null, string? refreshToken = null)
        => new(Guid.NewGuid(), "https://us-open.tracksolidpro.com/", "fleet-account", "secret-pw", "app-key", AppSecret,
            token, expiration, refreshToken, null);

    public static ICredentialHttpClientFactory ClientFactory()
    {
        var factory = new Mock<ICredentialHttpClientFactory>();
        factory.Setup(f => f.CreateClientAsync(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()))
            .Returns(() => new HttpClient());
        return factory.Object;
    }

    public static string Respond(IReadOnlyDictionary<string, string> form, string methodJson)
        => form["method"] == "jimi.oauth.token.get" ? TokenJson : methodJson;
}

[TestFixture]
public class TracksolidPositionReaderTests
{
    private Mock<ICredentialWriter> _credentialWriter = null!;
    private ProviderSessionStore _sessionStore = null!;

    [SetUp]
    public void SetUp()
    {
        _credentialWriter = new Mock<ICredentialWriter>();
        _sessionStore = new ProviderSessionStore();
    }

    private PositionReader CreateReader(TracksolidFakeHttpClientService http)
        => new(TracksolidTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);

    private static DeviceTransporterVm Device(string imei, string name = "Truck 1")
        => new(Guid.NewGuid(), 0, imei, name, "Truck", (short)TransporterType.Truck);

    [Test]
    public async Task Init_WithoutToken_RequestsSignedTokenAndPersistsIt()
    {
        var http = new TracksolidFakeHttpClientService(_ => TracksolidTestData.TokenJson);
        var credential = TracksolidTestData.Credential();

        await CreateReader(http).Init(credential);

        var (url, form) = http.Requests.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(url, Is.EqualTo("https://us-open.tracksolidpro.com/route/rest"));
            Assert.That(form["method"], Is.EqualTo("jimi.oauth.token.get"));
            Assert.That(form["user_id"], Is.EqualTo("fleet-account"));
            Assert.That(form["user_pwd_md5"], Is.EqualTo(RequestSigner.Md5Lower("secret-pw")));
            Assert.That(form["expires_in"], Is.EqualTo("7200"));
            Assert.That(form["app_key"], Is.EqualTo("app-key"));
            Assert.That(form["v"], Is.EqualTo("1.0"));
            Assert.That(form["sign"], Is.EqualTo(RequestSigner.Sign(form, TracksolidTestData.AppSecret)));
            Assert.That(DateTime.TryParseExact(form["timestamp"], "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out _), Is.True);
        }
        _credentialWriter.Verify(w => w.UpdateTokenAsync(credential.CredentialId,
            It.Is<UpdateTokenDto>(t => t.Token == "tok-1" && t.RefreshToken == "ref-1" && t.TokenExpiration > DateTimeOffset.UtcNow.AddMinutes(119)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Init_WithValidPersistedToken_MakesNoProviderCall()
    {
        var http = new TracksolidFakeHttpClientService(_ => throw new AssertionException("no call expected"));

        await CreateReader(http).Init(TracksolidTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(1), "ref"));

        Assert.That(http.Requests, Is.Empty);
    }

    [Test]
    public async Task Init_WithCachedSession_DoesNotRequestTokenTwice()
    {
        var http = new TracksolidFakeHttpClientService(_ => TracksolidTestData.TokenJson);
        var credential = TracksolidTestData.Credential();

        await CreateReader(http).Init(credential);
        await CreateReader(http).Init(credential);

        Assert.That(http.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Init_WithExpiredTokenAndRefreshToken_UsesRefresh()
    {
        var http = new TracksolidFakeHttpClientService(_ => TracksolidTestData.TokenJson);

        await CreateReader(http).Init(TracksolidTestData.Credential("old", DateTimeOffset.UtcNow.AddMinutes(-5), "ref-0"));

        var form = http.Requests.Single().Form;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(form["method"], Is.EqualTo("jimi.oauth.token.refresh"));
            Assert.That(form["access_token"], Is.EqualTo("old"));
            Assert.That(form["refresh_token"], Is.EqualTo("ref-0"));
        }
    }

    [Test]
    public async Task Init_WhenRefreshIsRefused_FallsBackToNewToken()
    {
        var http = new TracksolidFakeHttpClientService(form => form["method"] == "jimi.oauth.token.refresh"
            ? """{"code":1004,"message":"Illegal request, token is invalid"}"""
            : TracksolidTestData.TokenJson);

        await CreateReader(http).Init(TracksolidTestData.Credential("old", DateTimeOffset.UtcNow.AddMinutes(-5), "ref-0"));

        Assert.That(http.Methods, Is.EqualTo(new[] { "jimi.oauth.token.refresh", "jimi.oauth.token.get" }));
    }

    [Test]
    public async Task Init_WhenTokenRequestFails_Throws()
    {
        var http = new TracksolidFakeHttpClientService(_ => """{"code":1002,"message":"illegal user"}""");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateReader(http).Init(TracksolidTestData.Credential()));
    }

    [Test]
    public async Task GetDevicePositionAsync_Many_UsesAccountListAndReturnsOnlyRequestedDevices()
    {
        const string locations = """
            {"code":0,"message":"success","result":[
              {"imei":"111","deviceName":"A","lat":4.711,"lng":-74.072,"gpsTime":"2026-11-15 08:30:15","speed":"42","direction":"-1","accStatus":"1","gpsNum":"9","currentMileage":"12.5","temperature":"","hbTime":"2026-11-15 08:31:00"},
              {"imei":"222","deviceName":"B","lat":"6.25","lng":"-75.56","gpsTime":"2026-11-15 08:00:00","speed":"0","direction":"181.5","accStatus":"0","gpsNum":"","currentMileage":null,"temperature":"21.5"},
              {"imei":"333","deviceName":"C","lat":1,"lng":1,"gpsTime":"2026-11-15 08:00:00","speed":"0"},
              {"imei":"444","deviceName":"D","lat":0,"lng":0,"gpsTime":null,"speed":"0"}
            ],"data":null}
            """;
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, locations));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());
        var first = Device("111");
        var second = Device("222");

        var result = (await reader.GetDevicePositionAsync([first, second, Device("444")], CancellationToken.None)).ToList();

        var request = http.Requests.Last().Form;
        var a = result.Single(p => p.TransporterId == first.TransporterId);
        var b = result.Single(p => p.TransporterId == second.TransporterId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request["method"], Is.EqualTo("jimi.user.device.location.list"));
            Assert.That(request["target"], Is.EqualTo("fleet-account"));
            Assert.That(request["access_token"], Is.EqualTo("tok-1"));
            Assert.That(request.ContainsKey("map_type"), Is.False);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(a.Latitude, Is.EqualTo(4.711));
            Assert.That(a.Longitude, Is.EqualTo(-74.072));
            Assert.That(a.Altitude, Is.Null);
            Assert.That(a.DeviceDateTime, Is.EqualTo(new DateTimeOffset(2026, 11, 15, 8, 30, 15, TimeSpan.Zero)));
            Assert.That(a.Speed, Is.EqualTo(42));
            Assert.That(a.Course, Is.Null);
            Assert.That(a.Attributes!.Value.Ignition, Is.True);
            Assert.That(a.Attributes!.Value.Satellites, Is.EqualTo(9));
            Assert.That(a.Attributes!.Value.Mileage, Is.EqualTo(12500));
            Assert.That(a.Attributes!.Value.Temperature, Is.Null);
            Assert.That(b.Latitude, Is.EqualTo(6.25));
            Assert.That(b.Course, Is.EqualTo(181.5));
            Assert.That(b.Attributes!.Value.Ignition, Is.False);
            Assert.That(b.Attributes!.Value.Satellites, Is.Null);
            Assert.That(b.Attributes!.Value.Mileage, Is.Null);
            Assert.That(b.Attributes!.Value.Temperature, Is.EqualTo(21.5));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_Single_UsesLocationGet()
    {
        const string locations = """{"code":0,"result":[{"imei":"111","lat":4.7,"lng":-74.1,"gpsTime":"2026-11-15 08:30:15","speed":"10","direction":"90"}]}""";
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, locations));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());

        var result = await reader.GetDevicePositionAsync(Device("111"), CancellationToken.None);

        var request = http.Requests.Last().Form;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request["method"], Is.EqualTo("jimi.device.location.get"));
            Assert.That(request["imeis"], Is.EqualTo("111"));
            Assert.That(result.Course, Is.EqualTo(90));
            Assert.That(result.Speed, Is.EqualTo(10));
        }
    }

    [Test]
    public async Task GetDevicePositionAsync_Single_NotReturned_Throws()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":0,"result":[]}"""));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync(Device("111"), CancellationToken.None));
    }

    [TestCase("""{"code":1001,"message":"Parameter error"}""")]
    [TestCase("""{"code":"1006","message":"Illegal access, request frequency is too high","result":null,"data":null}""")]
    public async Task GetDevicePositionAsync_ErrorPayload_Throws(string error)
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, error));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync([Device("111")], CancellationToken.None));
    }

    [Test]
    public async Task InvalidToken_ReauthenticatesAndRetriesOnce()
    {
        var calls = 0;
        var http = new TracksolidFakeHttpClientService(form => form["method"] switch
        {
            "jimi.user.device.location.list" when calls++ == 0 => """{"code":1004,"message":"token exception"}""",
            "jimi.user.device.location.list" => """{"code":0,"result":[{"imei":"111","lat":1,"lng":2,"gpsTime":"2026-11-15 08:00:00","speed":"0"}]}""",
            _ => TracksolidTestData.TokenJson
        });
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential("stale", DateTimeOffset.UtcNow.AddHours(1), null));

        var result = await reader.GetDevicePositionAsync([Device("111")], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(http.Methods, Is.EqualTo(new[] { "jimi.user.device.location.list", "jimi.oauth.token.get", "jimi.user.device.location.list" }));
            Assert.That(http.Requests[0].Form["access_token"], Is.EqualTo("stale"));
            Assert.That(http.Requests[2].Form["access_token"], Is.EqualTo("tok-1"));
        }
    }

    [Test]
    public async Task InvalidToken_AfterReauthentication_Throws()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":1004,"message":"token exception"}"""));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential("stale", DateTimeOffset.UtcNow.AddHours(1), null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetDevicePositionAsync([Device("111")], CancellationToken.None));
        Assert.That(http.Methods.Count(m => m == "jimi.oauth.token.get"), Is.EqualTo(1));
    }

    [Test]
    public async Task GetPositionAsync_SplitsIntoSevenDayWindowsAndKeepsPointsInRange()
    {
        var from = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.FromHours(-5));
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, form.GetValueOrDefault("begin_time") == "2026-10-01 00:00:00"
            ? """
              {"code":0,"result":[
                {"lat":4.7,"lng":-74.1,"gpsTime":"2026-09-30 23:59:59","direction":0,"gpsSpeed":0.0,"ignition":"OFF"},
                {"lat":4.7,"lng":-74.1,"gpsTime":"2026-10-01 00:00:00","direction":45,"gpsSpeed":36.5,"ignition":"ON","accStatus":"ON"},
                {"lat":4.8,"lng":-74.2,"gpsTime":"2026-10-02 10:00:00","direction":"270","gpsSpeed":"60","accStatus":"OFF"}
              ],"data":{"mileage":0}}
              """
            : """{"code":0,"result":[{"lat":5,"lng":-75,"gpsTime":"2026-10-16 17:00:01","direction":0,"gpsSpeed":0}],"data":{"mileage":0}}"""));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());

        var result = (await reader.GetPositionAsync(from, to, Device("111"), CancellationToken.None)).ToList();

        var windows = http.Requests.Where(r => r.Form["method"] == "jimi.device.track.list").Select(r => r.Form).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(windows.Select(w => (w["begin_time"], w["end_time"])), Is.EqualTo(new[]
            {
                ("2026-10-01 00:00:00", "2026-10-07 23:59:59"),
                ("2026-10-08 00:00:00", "2026-10-14 23:59:59"),
                ("2026-10-15 00:00:00", "2026-10-16 17:00:00")
            }));
            Assert.That(windows.All(w => w["imei"] == "111" && !w.ContainsKey("map_type")), Is.True);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].DeviceDateTime, Is.EqualTo(from));
            Assert.That(result[0].Speed, Is.EqualTo(36.5));
            Assert.That(result[0].Course, Is.EqualTo(45));
            Assert.That(result[0].Attributes!.Value.Ignition, Is.True);
            Assert.That(result[1].Course, Is.EqualTo(270));
            Assert.That(result[1].Speed, Is.EqualTo(60));
            Assert.That(result[1].Attributes!.Value.Ignition, Is.False);
        }
    }

    [Test]
    public async Task GetPositionAsync_RangeEndingInTheFuture_CapsEndTimeBeforeNow()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":0,"result":[]}"""));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());
        var now = DateTimeOffset.UtcNow;

        await reader.GetPositionAsync(now.AddHours(-1), now.AddDays(1), Device("111"), CancellationToken.None);

        var request = http.Requests.Single(r => r.Form["method"] == "jimi.device.track.list");
        Assert.That(
            DateTimeOffset.ParseExact(request.Form["end_time"], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            Is.LessThan(now));
    }

    [Test]
    public async Task GetPositionAsync_ErrorPayload_Throws()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":1002,"message":"IMEI does not exist{111}"}"""));
        var reader = CreateReader(http);
        await reader.Init(TracksolidTestData.Credential());

        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetPositionAsync(
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, Device("111"), CancellationToken.None));
    }

    [Test]
    public async Task Ping_CallsDeviceListEveryTime()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":0,"result":[]}"""));
        var tester = new ConnectivityTester(TracksolidTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);
        var credential = TracksolidTestData.Credential("persisted", DateTimeOffset.UtcNow.AddHours(1), null);

        await tester.Ping(credential, CancellationToken.None);
        await tester.Ping(credential, CancellationToken.None);

        Assert.That(http.Methods, Is.EqualTo(new[] { "jimi.user.device.list", "jimi.user.device.list" }));
    }

    [Test]
    public async Task Ping_ErrorPayload_Throws()
    {
        var http = new TracksolidFakeHttpClientService(form => TracksolidTestData.Respond(form, """{"code":1005,"message":"IP access exceeds limit"}"""));
        var tester = new ConnectivityTester(TracksolidTestData.ClientFactory(), http, _credentialWriter.Object, _sessionStore);

        await Assert.ThrowsAsync<InvalidOperationException>(() => tester.Ping(TracksolidTestData.Credential(), CancellationToken.None));
    }
}
