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
using TrackHub.Router.Application.PingOperator;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces;

namespace TrackHub.Router.Application.UnitTests.PingOperator.Registry;

[TestFixture]
public class ConnectivityRegistryTests
{
    private static IConnectivityTester TesterFor(ProtocolType protocol)
    {
        var tester = new Mock<IConnectivityTester>();
        tester.SetupGet(t => t.Protocol).Returns(protocol);
        return tester.Object;
    }

    private static ConnectivityRegistry BuildRegistry(params ProtocolType[] protocols)
    {
        var services = new ServiceCollection();
        foreach (var protocol in protocols)
        {
            var tester = TesterFor(protocol);
            services.AddKeyedScoped<IConnectivityTester>(protocol, (_, _) => tester);
        }
        return new ConnectivityRegistry(services.BuildServiceProvider());
    }

    [Test]
    public void GetTester_ReturnsTesterForRequestedProtocol()
    {
        var registry = BuildRegistry(ProtocolType.CommandTrack, ProtocolType.Samsara);

        var result = registry.GetTester(ProtocolType.Samsara);

        Assert.That(result.Protocol, Is.EqualTo(ProtocolType.Samsara));
    }

    [Test]
    public void GetTester_NoMatch_ThrowsProtocolNotSupportedException()
    {
        var registry = BuildRegistry(ProtocolType.CommandTrack);

        Assert.Throws<ProtocolNotSupportedException>(() => registry.GetTester(ProtocolType.Samsara));
    }
}
