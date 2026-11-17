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

namespace TrackHub.Router.Domain.Mappers;

public abstract class DeviceMapperBase<TDevice, TKey> where TKey : notnull
{
    protected virtual DeviceType DeviceType => DeviceType.Cellular;

    protected abstract TKey? Key(TDevice device);

    protected abstract string Serial(TDevice device);

    protected abstract string Name(TDevice device);

    protected virtual int Identifier(TDevice device, DeviceTransporterVm? assignment) => 0;

    protected virtual string? ProviderDisplayName(TDevice device) => null;

    protected virtual bool IsCatalogued(TDevice device) => true;

    public static int ToIdentifier(double? id) => id is >= 0 and <= int.MaxValue ? (int)id.Value : 0;

    public DeviceVm Map(TDevice device, DeviceTransporterVm? assignment = null)
        => new(
            assignment?.TransporterId,
            Identifier(device, assignment),
            Serial(device),
            Name(device),
            (short)DeviceType,
            (short)TransporterType.Truck,
            ProviderDisplayName(device));

    public IEnumerable<DeviceVm> Map(IEnumerable<TDevice> devices)
        => devices.Where(IsCatalogued).Select(device => Map(device));

    public IEnumerable<DeviceVm> Map(IEnumerable<TDevice> devices, IDictionary<TKey, DeviceTransporterVm> assignments)
    {
        foreach (var device in devices.Where(IsCatalogued))
        {
            if (Key(device) is { } key && assignments.TryGetValue(key, out var assignment))
            {
                yield return Map(device, assignment);
            }
        }
    }
}

public abstract class DeviceMapperBase<TDevice> : DeviceMapperBase<TDevice, string>
{
    protected override string? Key(TDevice device) => Serial(device);
}
