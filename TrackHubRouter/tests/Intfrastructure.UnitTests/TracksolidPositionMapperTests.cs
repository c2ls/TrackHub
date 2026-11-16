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

using TrackHub.Router.Infrastructure.Tracksolid.Helpers;
using TrackHub.Router.Infrastructure.Tracksolid.Mappers;
using TrackHub.Router.Infrastructure.Tracksolid.Models;

namespace TrackHub.Router.Infrastructure.Tracksolid.Tests;

[TestFixture]
public class TracksolidPositionMapperTests
{
    [Test]
    public void Sign_ReproducesTheDocumentedExample()
    {
        var parameters = new Dictionary<string, string>
        {
            ["method"] = "jimi.oauth.token.get",
            ["timestamp"] = "2025-05-19 10:23:00",
            ["app_key"] = "8FB345B8693CCD00CE073CAB5F094009339A22A4105B6558",
            ["sign_method"] = "md5",
            ["v"] = "1.0",
            ["format"] = "json",
            ["user_id"] = "JMTEST123",
            ["user_pwd_md5"] = "21218cca77804d2ba1922c33e0151105",
            ["expires_in"] = "7200"
        };

        Assert.That(RequestSigner.Sign(parameters, "c0aa0226fddc4365a3c67fef45427f8a"), Is.EqualTo("4EE067D88EA65FF2AFD4890955E042CB"));
    }

    [Test]
    public void Sign_IgnoresSignAndEmptyValues()
    {
        var bare = new Dictionary<string, string> { ["foo"] = "1", ["bar"] = "2" };
        var noisy = new Dictionary<string, string> { ["foo"] = "1", ["bar"] = "2", ["sign"] = "X", ["empty"] = "" };

        Assert.That(RequestSigner.Sign(noisy, "s"), Is.EqualTo(RequestSigner.Sign(bare, "s")));
    }

    [Test]
    public void Md5Lower_IsLowerCaseHex()
        => Assert.That(RequestSigner.Md5Lower("123456"), Is.EqualTo("e10adc3949ba59abbe56e057f20f883e"));

    [Test]
    public void SplitRange_ExactlyOneWindow_IsOneCall()
    {
        var from = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

        var windows = PositionReader.SplitRange(from, from.AddDays(7).AddSeconds(-1)).ToList();

        Assert.That(windows, Is.EqualTo(new[] { (from, from.AddDays(7).AddSeconds(-1)) }));
    }

    [Test]
    public void SplitRange_OneSecondOverAWindow_AddsASecondCallWithoutOverlap()
    {
        var from = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

        var windows = PositionReader.SplitRange(from, from.AddDays(7)).ToList();

        Assert.That(windows, Is.EqualTo(new[]
        {
            (from, from.AddDays(7).AddSeconds(-1)),
            (from.AddDays(7), from.AddDays(7))
        }));
    }

    [Test]
    public void SplitRange_FromAfterTo_IsEmpty()
        => Assert.That(PositionReader.SplitRange(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(-1)), Is.Empty);

    [Test]
    public void ParseUtc_ReadsProviderTimeAsUtc()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(PositionMapper.ParseUtc("2026-03-18 10:20:37"), Is.EqualTo(new DateTimeOffset(2026, 3, 18, 10, 20, 37, TimeSpan.Zero)));
            Assert.That(PositionMapper.ParseUtc(null), Is.Null);
            Assert.That(PositionMapper.ParseUtc(""), Is.Null);
        }
    }

    [Test]
    public void FormatUtc_ConvertsOffsetsToUtc()
        => Assert.That(PositionMapper.FormatUtc(new DateTimeOffset(2026, 3, 18, 5, 20, 37, TimeSpan.FromHours(-5))), Is.EqualTo("2026-03-18 10:20:37"));

    [TestCase("1", true)]
    [TestCase("ON", true)]
    [TestCase("on", true)]
    [TestCase("0", false)]
    [TestCase("OFF", false)]
    [TestCase("", null)]
    [TestCase(null, null)]
    public void ParseIgnition_HandlesBothEncodings(string? value, bool? expected)
        => Assert.That(PositionMapper.ParseIgnition(value), Is.EqualTo(expected));

    [Test]
    public void MapToPositionVm_ConvertsMileageFromKilometresToMetres()
    {
        var device = new DeviceTransporterVm(Guid.NewGuid(), 0, "111", "Truck", "Truck", (short)TransporterType.Truck);
        var location = new LocationRecord("111", 1, 2, "2026-03-18 10:20:37", 30, 359.5, "1", 12, 86.5, 20);

        var position = location.MapToPositionVm(PositionMapper.ParseUtc(location.GpsTime)!.Value, device);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(position.Attributes!.Value.Mileage, Is.EqualTo(86500));
            Assert.That(position.Course, Is.EqualTo(359.5));
            Assert.That(position.DeviceName, Is.EqualTo("Truck"));
            Assert.That(position.ServerDateTime, Is.Null);
        }
    }

    [TestCase(0d, 0d)]
    [TestCase(null, null)]
    [TestCase(4.7, null)]
    public void MapToPositionVm_LocationWithoutAFix_IsSkipped(double? latitude, double? longitude)
    {
        var device = new DeviceTransporterVm(Guid.NewGuid(), 0, "111", "Truck", "Truck", (short)TransporterType.Truck);
        var lookup = new Dictionary<string, DeviceTransporterVm> { ["111"] = device };
        var location = new LocationRecord("111", latitude, longitude, "2026-03-18 10:20:37", 0, 0, "0", 0, 0, 0);
        var point = new TrackPoint(latitude, longitude, "2026-03-18 10:20:37", 0, 0, null, null);
        var time = PositionMapper.ParseUtc(location.GpsTime)!.Value;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(new[] { location }.MapToPositionVm(lookup), Is.Empty);
            Assert.That(new[] { point }.MapToPositionVm(time.AddHours(-1), time.AddHours(1), device), Is.Empty);
        }
    }
}
