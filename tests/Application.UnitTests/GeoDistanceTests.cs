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

using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.UnitTests;

/// <summary>
/// The single function behind every trip's <c>ActualDistanceMeters</c>, and it was untested.
/// <para>
/// A wrong constant or a missing degrees-to-radians conversion here does not crash anything — it
/// silently rescales every distance report, every plan-vs-actual comparison and every downstream
/// fuel reconciliation in the platform by a constant factor that nobody would notice for months.
/// The cases below are anchored to independently-known ground truth (a degree of latitude, a
/// well-surveyed city pair) rather than to whatever the current implementation happens to return.
/// </para>
/// </summary>
[TestFixture]
public class GeoDistanceTests
{
    // One degree of latitude is ~111.19 km anywhere on a spherical earth. This is the case that
    // catches a missing ToRadians: feeding degrees straight into Math.Sin is off by ~57x.
    [Test]
    public void OneDegreeOfLatitude_IsAboutOneHundredAndElevenKilometres()
        => Assert.That(
            GeoDistance.HaversineMeters(4.0, -74.0, 5.0, -74.0),
            Is.EqualTo(111_195d).Within(200d));

    // A degree of LONGITUDE shrinks with the cosine of the latitude. Dropping the cos(lat) term
    // leaves east-west travel overstated everywhere except the equator.
    [Test]
    public void OneDegreeOfLongitudeAtFortyFiveDegreesNorth_IsAboutSeventyNineKilometres()
        => Assert.That(
            GeoDistance.HaversineMeters(45.0, -74.0, 45.0, -73.0),
            Is.EqualTo(78_626d).Within(300d));

    [Test]
    public void OneDegreeOfLongitudeAtTheEquator_IsAFullDegreeWide()
        => Assert.That(
            GeoDistance.HaversineMeters(0.0, 0.0, 0.0, 1.0),
            Is.EqualTo(111_195d).Within(200d));

    // Bogota to Medellin. A real-world anchor for the fleet this module was built for, derived
    // independently: 1.64213° of latitude (182.6 km) against 1.48184° of longitude at ~5.4°N
    // (164.1 km) is a 245.5 km hypotenuse.
    [Test]
    public void BogotaToMedellin_IsAboutTwoHundredAndFortyFiveKilometres()
        => Assert.That(
            GeoDistance.HaversineMeters(4.60971, -74.08175, 6.25184, -75.56359),
            Is.EqualTo(245_500d).Within(1_500d));

    [Test]
    public void TheSamePointTwice_IsZeroAndNotNaN()
    {
        // A stationary vehicle reports the same fix repeatedly. Math.Sqrt of a whisker-negative
        // rounding artefact would give NaN, and one NaN poisons the odometer for the whole trip.
        var distance = GeoDistance.HaversineMeters(4.60971, -74.08175, 4.60971, -74.08175);

        Assert.Multiple(() =>
        {
            Assert.That(double.IsNaN(distance), Is.False);
            Assert.That(distance, Is.EqualTo(0d).Within(1e-6));
        });
    }

    [Test]
    public void TheDistanceIsSymmetric()
        => Assert.That(
            GeoDistance.HaversineMeters(4.60971, -74.08175, 6.25184, -75.56359),
            Is.EqualTo(GeoDistance.HaversineMeters(6.25184, -75.56359, 4.60971, -74.08175)).Within(1e-6));

    // Antipodal points are where the haversine's inner term reaches 1, the edge of asin's domain.
    // These assert the ANSWER (half the circumference, never NaN) for the extreme input; note that
    // none of them actually reaches the `Math.Min(1d, …)` clamp — every pair tried lands on 1.0 or
    // just under it, so that clamp remains unreached defensive code rather than tested code.
    [TestCase(0d, 0d, 0d, 180d)]
    [TestCase(45d, 0d, -45d, 180d)]
    [TestCase(30d, -74d, -30d, 106d)]
    public void AntipodalPoints_ClampCleanlyToHalfTheCircumference(double fromLat, double fromLon, double toLat, double toLon)
    {
        var distance = GeoDistance.HaversineMeters(fromLat, fromLon, toLat, toLon);

        Assert.Multiple(() =>
        {
            Assert.That(double.IsNaN(distance), Is.False, "the asin domain clamp is missing");
            Assert.That(distance, Is.EqualTo(20_015_000d).Within(50_000d));
        });
    }

    [Test]
    public void ShortUrbanHops_AreMeasuredInMetresNotKilometres()
    {
        // The realistic per-fix leg: a fix every 30 s at city speed is tens of metres. Nothing
        // downstream would flag it if these came back 1000x too large.
        var distance = GeoDistance.HaversineMeters(4.60971, -74.08175, 4.61071, -74.08175);

        Assert.That(distance, Is.EqualTo(111d).Within(2d));
    }
}
