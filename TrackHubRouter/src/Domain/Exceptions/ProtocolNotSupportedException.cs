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

namespace TrackHub.Router.Domain.Exceptions;

// No provider reader/tester is registered for the requested protocol. Thrown instead of a bare
// LINQ .First() InvalidOperationException so the failure names the offending protocol in logs and
// in the recorded sync run (an operator row may carry a ProtocolTypeId whose provider assembly is
// not built or not listed in AppSettings:Protocols).
public sealed class ProtocolNotSupportedException(Common.Domain.Enums.ProtocolType protocol)
    : Exception($"No provider is registered for protocol '{protocol}'. Ensure the provider assembly "
        + "is built and the protocol is listed in AppSettings:Protocols.")
{
    public Common.Domain.Enums.ProtocolType Protocol { get; } = protocol;
}
