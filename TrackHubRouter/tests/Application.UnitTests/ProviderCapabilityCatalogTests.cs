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

using Common.Domain.Enums;
using TrackHub.Router.Application.Providers.Queries;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Exceptions;
using Application.UnitTests;

namespace TrackHub.Router.Application.UnitTests.Providers;

[TestFixture]
public class ProviderCapabilityCatalogTests
{
    [Test]
    public void UndeclaredProtocol_ReportsNoCapabilities()
    {
        // A protocol with no registered descriptor must read as "provider supports nothing",
        // so every capability check on it fails as a provider limitation, never a masked error.
        var catalog = TestProviderDescriptors.DefaultCatalog;

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Get(ProtocolType.Mettax), Is.EqualTo(ProviderCapability.None));
            Assert.That(catalog.Supports(ProtocolType.Mettax, ProviderCapability.RealTimePositions), Is.False);
        });
    }

    [Test]
    public void GpsGate_DeclaresNoPositionHistory_ButRealTimePositions()
    {
        var catalog = TestProviderDescriptors.DefaultCatalog;

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Supports(ProtocolType.GpsGate, ProviderCapability.PositionHistory), Is.False);
            Assert.That(catalog.Supports(ProtocolType.GpsGate, ProviderCapability.RealTimePositions), Is.True);
        });
    }

    [Test]
    public void EnsureSupports_MissingCapability_ThrowsWithProtocolAndCapability()
    {
        var catalog = TestProviderDescriptors.DefaultCatalog;

        var ex = Assert.Throws<ProviderCapabilityNotSupportedException>(
            () => catalog.EnsureSupports(ProtocolType.GpsGate, ProviderCapability.PositionHistory));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Protocol, Is.EqualTo(ProtocolType.GpsGate));
            Assert.That(ex.Capability, Is.EqualTo(ProviderCapability.PositionHistory));
            Assert.That(ex.Message, Does.Contain("GpsGate").And.Contain("provider"));
        });
    }

    [Test]
    public async Task GetProviderCapabilitiesQuery_ReturnsTheDeclaredMatrix()
    {
        var handler = new GetProviderCapabilitiesQueryHandler(TestProviderDescriptors.DefaultCatalog);

        var result = (await handler.Handle(new GetProviderCapabilitiesQuery(), CancellationToken.None)).ToList();

        var gpsGate = result.Single(r => r.ProtocolTypeId == (int)ProtocolType.GpsGate);
        var traccar = result.Single(r => r.ProtocolTypeId == (int)ProtocolType.Traccar);
        Assert.Multiple(() =>
        {
            // Exactly the registered declarations — the matrix doubles as the deployment's
            // client-facing provider list.
            Assert.That(result, Has.Count.EqualTo(TestProviderDescriptors.DefaultCatalog.All.Count));
            Assert.That(gpsGate.Protocol, Is.EqualTo(nameof(ProtocolType.GpsGate)));
            Assert.That(gpsGate.DisplayName, Is.EqualTo("GpsGate"));
            Assert.That(gpsGate.PositionHistory, Is.False);
            Assert.That(gpsGate.RealTimePositions, Is.True);
            Assert.That(traccar.PositionHistory, Is.True);
        });
    }
}
