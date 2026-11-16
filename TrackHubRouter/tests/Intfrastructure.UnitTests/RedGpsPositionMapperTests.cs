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

using TrackHub.Router.Infrastructure.RedGps.Mappers;
using TrackHub.Router.Infrastructure.RedGps.Models;

namespace TrackHub.Router.Infrastructure.RedGps.Tests;

[TestFixture]
public class RedGpsPositionMapperTests
{
    [TestCase("N", 0)]
    [TestCase("NE", 45)]
    [TestCase("E", 90)]
    [TestCase("SE", 135)]
    [TestCase("S", 180)]
    [TestCase("SO", 225)]
    [TestCase("SW", 225)]
    [TestCase("O", 270)]
    [TestCase("W", 270)]
    [TestCase("NO", 315)]
    [TestCase("nw", 315)]
    [TestCase(" N ", 0)]
    [TestCase("112.5", 112.5)]
    public void ParseDirection_ConvertsCompassToDegrees(string value, double expected)
        => Assert.That(PositionMapper.ParseDirection(value), Is.EqualTo(expected));

    [TestCase("")]
    [TestCase("X")]
    [TestCase(null)]
    public void ParseDirection_Unknown_IsNull(string? value)
        => Assert.That(PositionMapper.ParseDirection(value), Is.Null);

    [TestCase(0d, false)]
    [TestCase(1d, true)]
    [TestCase(2d, null)]
    [TestCase(null, null)]
    public void ParseIgnition_TreatsTwoAsUnknown(double? value, bool? expected)
        => Assert.That(PositionMapper.ParseIgnition(value), Is.EqualTo(expected));

    [TestCase("-5", "2023-03-09T04:59:39Z")]
    [TestCase("0", "2023-03-08T23:59:39Z")]
    [TestCase("5.5", "2023-03-08T18:29:39Z")]
    [TestCase("+3", "2023-03-08T20:59:39Z")]
    public void ParseHistoryTime_AppliesTheReportedGmt(string gmt, string expectedUtc)
    {
        var point = new HistoryPoint(0, 0, gmt, "20230308235939", 0, 0, null, null, null, null);

        Assert.That(PositionMapper.ParseHistoryTime(point), Is.EqualTo(DateTimeOffset.Parse(expectedUtc)));
    }

    [Test]
    public void ParseUtcReportDate_ReadsUtc()
        => Assert.That(PositionMapper.ParseUtcReportDate("2021-04-26 16:42:43"), Is.EqualTo(new DateTimeOffset(2021, 4, 26, 16, 42, 43, TimeSpan.Zero)));

    [Test]
    public void RequestWindow_PadsBothSidesInUtc()
    {
        var (fechaIni, fechaFin) = PositionMapper.RequestWindow(
            new DateTimeOffset(2023, 3, 8, 0, 0, 0, TimeSpan.FromHours(-5)),
            new DateTimeOffset(2023, 3, 8, 23, 59, 59, TimeSpan.FromHours(-5)));

        Assert.That((fechaIni, fechaFin), Is.EqualTo(("2023-03-07 15:00:00", "2023-03-09 18:59:59")));
    }

    [Test]
    public void SplitRange_PaddedWindowStaysWithinFifteenDays()
    {
        var from = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);

        var windows = PositionReader.SplitRange(from, from.AddDays(40)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(windows.All(w => w.End - w.Start + 2 * PositionMapper.RequestZonePadding < TimeSpan.FromDays(15)), Is.True);
            Assert.That(windows[0].Start, Is.EqualTo(from));
            Assert.That(windows[^1].End, Is.EqualTo(from.AddDays(40)));
            Assert.That(windows.Zip(windows.Skip(1)).All(p => p.Second.Start == p.First.End.AddSeconds(1)), Is.True);
        }
    }

    [Test]
    public void SplitRange_ShortRange_IsOneRequest()
    {
        var from = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.That(PositionReader.SplitRange(from, from.AddHours(1)).Count(), Is.EqualTo(1));
    }

    [TestCase(0d, 0d)]
    [TestCase(null, null)]
    [TestCase(4.7, null)]
    public void MapToPositionVm_UnitWithoutAFix_IsSkipped(double? latitude, double? longitude)
    {
        var device = new DeviceTransporterVm(Guid.NewGuid(), 0, "IMEI1", "Truck", "Truck", 1);
        var lookup = new Dictionary<string, DeviceTransporterVm> { ["IMEI1"] = device };
        var unit = new UnitData("IMEI1", "2026-03-18 10:20:37", latitude, longitude, null, 0, "N", 0, 0, 0, null);
        var point = new HistoryPoint(latitude, longitude, "0", "20260318102037", 0, 0, 0, 0, 0, null);
        var time = new DateTimeOffset(2026, 3, 18, 10, 20, 37, TimeSpan.Zero);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(new[] { unit }.MapToPositionVm(lookup), Is.Empty);
            Assert.That(new[] { point }.MapToPositionVm(time.AddHours(-1), time.AddHours(1), device), Is.Empty);
        }
    }
}
