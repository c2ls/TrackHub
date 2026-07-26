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

using TrackHub.Manager.Application.Lookups;

namespace Application.UnitTests.Lookups;

/// <summary>
/// The whole point of a lookup ceiling is that it REFUSES rather than trims. A ceiling that returned
/// the first N rows would reintroduce exactly the silent truncation the paged reads were introduced
/// to eliminate, only now in the picker feeds where the user cannot even see a page control to
/// suspect something is missing.
/// </summary>
[TestFixture]
public class LookupLimitsTests
{
    [Test]
    public void EnsureWithinCeiling_ReturnsTheSetUntouchedAtTheCeiling()
    {
        var rows = Enumerable.Range(0, LookupLimits.Ceiling).ToList();

        var result = LookupLimits.EnsureWithinCeiling(rows, "transporterLookupByAccount");

        Assert.That(result, Has.Count.EqualTo(LookupLimits.Ceiling));
    }

    [Test]
    public void EnsureWithinCeiling_ThrowsPastTheCeilingRatherThanTruncating()
    {
        // The reader fetches Ceiling + 1 precisely so this case is reachable.
        var rows = Enumerable.Range(0, LookupLimits.FetchSize).ToList();

        var ex = Assert.Throws<Common.Application.Exceptions.ValidationException>(
            () => LookupLimits.EnsureWithinCeiling(rows, "transporterLookupByAccount"));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Code, Is.EqualTo(LookupLimits.LimitExceededCode));
            Assert.That(ex.Errors, Contains.Key("transporterLookupByAccount"));
        });
    }

    [Test]
    public void FetchSize_IsOnePastTheCeilingSoAnOverflowIsDetectable()
        => Assert.That(LookupLimits.FetchSize, Is.EqualTo(LookupLimits.Ceiling + 1));

    [Test]
    public void UnpagedReadLimits_ThrowsPastTheCeilingRatherThanTruncating()
    {
        var rows = Enumerable.Range(0, UnpagedReadLimits.Ceiling + 1).ToList();

        var ex = Assert.Throws<Common.Application.Exceptions.ValidationException>(
            () => UnpagedReadLimits.EnsureWithinCeiling(rows, "assignedDeviceTransportersByOperator"));

        Assert.That(ex!.Code, Is.EqualTo(UnpagedReadLimits.LimitExceededCode));
    }

    [Test]
    public void UnpagedReadLimits_ReturnsTheWholeSetBelowTheCeiling()
    {
        var rows = Enumerable.Range(0, 3_000).ToList();

        var result = UnpagedReadLimits.EnsureWithinCeiling(rows, "assignedDeviceTransportersByOperator");

        Assert.That(result, Has.Count.EqualTo(3_000));
    }
}
