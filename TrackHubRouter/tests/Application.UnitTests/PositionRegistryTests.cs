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

using Microsoft.Extensions.DependencyInjection;
using Moq;
using Common.Domain.Enums;
using TrackHub.Router.Application.DevicePositions.Registry;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Application.UnitTests.DevicePositions.Registry;

[TestFixture]
public class PositionRegistryTests
{
    private static IPositionReader ReaderFor(ProtocolType protocol)
    {
        var reader = new Mock<IPositionReader>();
        reader.SetupGet(r => r.Protocol).Returns(protocol);
        return reader.Object;
    }

    // Registers the given readers keyed by ProtocolType (mirroring RegisterProtocol's keyed
    // registration) and returns a registry backed by the built provider.
    private static PositionRegistry BuildRegistry(params ProtocolType[] protocols)
    {
        var services = new ServiceCollection();
        foreach (var protocol in protocols)
        {
            var reader = ReaderFor(protocol);
            services.AddKeyedScoped<IPositionReader>(protocol, (_, _) => reader);
        }
        return new PositionRegistry(services.BuildServiceProvider());
    }

    [Test]
    public void GetReaders_ReturnsOnlyMatchingProtocols()
    {
        var registry = BuildRegistry(ProtocolType.CommandTrack, ProtocolType.Samsara);

        var results = registry.GetReaders([ProtocolType.CommandTrack]).ToList();

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Protocol, Is.EqualTo(ProtocolType.CommandTrack));
    }

    [Test]
    public void GetReader_ReturnsReaderForRequestedProtocol()
    {
        var registry = BuildRegistry(ProtocolType.GpsGate, ProtocolType.Samsara);

        var result = registry.GetReader(ProtocolType.Samsara);

        Assert.That(result.Protocol, Is.EqualTo(ProtocolType.Samsara));
    }

    [Test]
    public void GetReaders_NoMatch_ReturnsEmpty()
    {
        var registry = BuildRegistry(ProtocolType.GpsGate);

        var results = registry.GetReaders([ProtocolType.CommandTrack]).ToList();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetReader_NoMatch_ThrowsProtocolNotSupportedException()
    {
        var registry = BuildRegistry(ProtocolType.GpsGate);

        Assert.Throws<ProtocolNotSupportedException>(() => registry.GetReader(ProtocolType.CommandTrack));
    }
}
