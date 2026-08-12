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
using Microsoft.Extensions.DependencyInjection;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Interfaces.Operator;
using TrackHub.Router.Infrastructure.Common.Helpers;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class ProtocolRegistrationExtensionsTests
{
    [Test]
    public void RegisterProtocol_GeoTab_ResolvesTheGeotabNamespaceReaders()
    {
        // Regression for router-audit A-01: the config/enum spelling "GeoTab" differs from the
        // provider assembly namespace "Geotab". Case-sensitive type resolution silently registered
        // nothing (Geotab was dead); the fix resolves it case-insensitively.
        var services = new ServiceCollection();

        services.RegisterProtocol(ProtocolType.GeoTab.ToString());

        // Readers are registered KEYED by ProtocolType (router-audit A-07); the keyed
        // implementation type must resolve to the actual Geotab-namespace reader.
        var positionReader = services.FirstOrDefault(d => d.ServiceType == typeof(IPositionReader) && d.IsKeyedService);
        var deviceReader = services.FirstOrDefault(d => d.ServiceType == typeof(IExternalDeviceReader) && d.IsKeyedService);
        var connectivityTester = services.FirstOrDefault(d => d.ServiceType == typeof(IConnectivityTester) && d.IsKeyedService);

        Assert.Multiple(() =>
        {
            Assert.That(positionReader?.ServiceKey, Is.EqualTo(ProtocolType.GeoTab));
            Assert.That(positionReader?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Geotab"));
            Assert.That(deviceReader?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Geotab"));
            Assert.That(connectivityTester?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Geotab"));
        });
    }

    [Test]
    public void RegisterProtocol_Protrack_ResolvesReadersFromTheProtrackAssembly()
    {
        // Wired into Infrastructure/Common 2026-07-18 — this guards the assembly reference so
        // configuring "Protrack" keeps resolving instead of failing at startup.
        var services = new ServiceCollection();

        services.RegisterProtocol(ProtocolType.Protrack.ToString());

        var positionReader = services.FirstOrDefault(d => d.ServiceType == typeof(IPositionReader) && d.IsKeyedService);
        var deviceReader = services.FirstOrDefault(d => d.ServiceType == typeof(IExternalDeviceReader) && d.IsKeyedService);
        var connectivityTester = services.FirstOrDefault(d => d.ServiceType == typeof(IConnectivityTester) && d.IsKeyedService);

        Assert.Multiple(() =>
        {
            Assert.That(positionReader?.ServiceKey, Is.EqualTo(ProtocolType.Protrack));
            Assert.That(positionReader?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Protrack"));
            Assert.That(deviceReader?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Protrack"));
            Assert.That(connectivityTester?.KeyedImplementationType?.Namespace, Is.EqualTo("TrackHub.Router.Infrastructure.Protrack"));
        });
    }

    [Test]
    public void RegisterProtocol_GpsGate_RegistersWithUndeclaredPositionHistory()
    {
        // GpsGate ships a PositionReader whose history method is a capability-guarded stub, so its
        // descriptor declares no PositionHistory. The startup cross-check must accept that shape:
        // history is the one capability a PositionReader may stub out.
        var services = new ServiceCollection();

        var descriptor = services.RegisterProtocol(ProtocolType.GpsGate.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(
                services.Any(d => d.ServiceType == typeof(IPositionReader) && Equals(d.ServiceKey, ProtocolType.GpsGate)),
                Is.True);
            Assert.That(descriptor.Protocol, Is.EqualTo(ProtocolType.GpsGate));
            Assert.That(descriptor.Capabilities.HasFlag(ProviderCapability.PositionHistory), Is.False);
            Assert.That(descriptor.Capabilities.HasFlag(ProviderCapability.RealTimePositions), Is.True);
        });
    }

    [Test]
    public void EveryProtocolTypeValue_IsEitherRegistrableOrReserved()
    {
        // Replaces the old central-catalog completeness guard: every value the enum can name must
        // either resolve a self-describing provider assembly (descriptor discovered and validated
        // by RegisterProtocol) or appear in the reserved placeholder list — a new enum value with
        // neither would silently report ProviderCapability.None to clients.
        var reserved = ProtocolRegistrationExtensions.ReservedDescriptors
            .Select(d => d.Protocol)
            .ToHashSet();

        foreach (var protocol in Enum.GetValues<ProtocolType>().Where(p => !reserved.Contains(p)))
        {
            var services = new ServiceCollection();

            var descriptor = services.RegisterProtocol(protocol.ToString());

            Assert.Multiple(() =>
            {
                Assert.That(descriptor.Protocol, Is.EqualTo(protocol));
                Assert.That(descriptor.DisplayName, Is.Not.Empty);
            });
        }
    }

    [Test]
    public void RegisterProtocol_UnknownConfiguredProtocol_ThrowsToFailFast()
    {
        // Regression for router-audit A-06: a configured protocol with no provider assembly must
        // fail fast at startup, not be silently skipped (which later surfaced as a masked
        // "Unexpected Execution Error" on the first sync).
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.RegisterProtocol("NoSuchProviderXyz"));
    }
}
