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
    public void RegisterProtocol_Protrack_ResolvesReadersFromTheProtackFolderAssembly()
    {
        // Protrack's repo FOLDER is "Protack" but its assembly/namespace are
        // "TrackHub.Router.Infrastructure.Protrack" (matching the enum). Wired into
        // Infrastructure/Common 2026-07-18 — this guards the assembly reference so configuring
        // "Protrack" keeps resolving instead of failing at startup.
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
    public void RegisterProtocol_UnknownConfiguredProtocol_ThrowsToFailFast()
    {
        // Regression for router-audit A-06: a configured protocol with no provider assembly must
        // fail fast at startup, not be silently skipped (which later surfaced as a masked
        // "Unexpected Execution Error" on the first sync).
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.RegisterProtocol("NoSuchProviderXyz"));
    }
}
