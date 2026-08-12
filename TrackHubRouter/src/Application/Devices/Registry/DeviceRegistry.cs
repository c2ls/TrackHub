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

namespace TrackHub.Router.Application.Devices.Registry;

// Scoped registry resolving the keyed device reader from the caller's own request scope — one
// reader per lookup, no resolve-all-and-filter, no disposed-scope hand-off (router-audit A-07).
public class DeviceRegistry(IServiceProvider serviceProvider) : IDeviceRegistry
{
    public IEnumerable<IExternalDeviceReader> GetReaders(IEnumerable<ProtocolType> types)
        => types
            .Select(type => serviceProvider.GetKeyedService<IExternalDeviceReader>(type))
            .Where(reader => reader is not null)!;

    public IExternalDeviceReader GetReader(ProtocolType type)
        => serviceProvider.GetKeyedService<IExternalDeviceReader>(type)
            ?? throw new ProtocolNotSupportedException(type);
}
