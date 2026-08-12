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

using TrackHub.Router.Domain.Exceptions;

namespace TrackHub.Router.Application.PingOperator;

// Scoped registry resolving the keyed connectivity tester from the caller's own request scope
// (router-audit A-07).
public class ConnectivityRegistry(IServiceProvider serviceProvider) : IConnectivityRegistry
{
    public IConnectivityTester GetTester(ProtocolType type)
        => serviceProvider.GetKeyedService<IConnectivityTester>(type)
            ?? throw new ProtocolNotSupportedException(type);
}
