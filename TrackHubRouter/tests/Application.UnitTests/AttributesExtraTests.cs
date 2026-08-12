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

using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Application.UnitTests.Domain;

// Guards the open attribute bag plumbing (router-audit A-03).
[TestFixture]
public class AttributesExtraTests
{
    [Test]
    public void From_ThenToDictionary_RoundTripsSignals()
    {
        var extra = AttributesExtra.From(("fuelLevelPct", "80"), ("rpm", "1500"));

        Assert.That(extra, Is.Not.Null);

        var parsed = AttributesExtra.ToDictionary(extra);
        Assert.Multiple(() =>
        {
            Assert.That(parsed["fuelLevelPct"], Is.EqualTo("80"));
            Assert.That(parsed["rpm"], Is.EqualTo("1500"));
        });
    }

    [Test]
    public void From_DropsNullAndEmptyValues_AndReturnsNullWhenEmpty()
    {
        var partial = AttributesExtra.From(("rpm", "1500"), ("fuelLevelPct", null), ("battery", "   "));
        Assert.That(AttributesExtra.ToDictionary(partial), Has.Count.EqualTo(1));

        Assert.That(AttributesExtra.From(("a", null), ("b", "")), Is.Null,
            "an all-empty bag must not travel as \"{}\"");
    }

    [Test]
    public void ToDictionary_InvalidOrEmptyInput_ReturnsEmptyMap()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AttributesExtra.ToDictionary(null), Is.Empty);
            Assert.That(AttributesExtra.ToDictionary("not-json"), Is.Empty);
        });
    }
}
