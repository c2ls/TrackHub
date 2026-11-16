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

using System.Globalization;
using TrackHub.Router.Domain.Mappers;

namespace TrackHub.Router.Infrastructure.Pegasus.Mappers;

internal sealed class DeviceMapper : DeviceMapperBase<Vehicle>
{
    public static DeviceMapper Instance { get; } = new();

    public static string ToSerial(long imei) => imei.ToString(CultureInfo.InvariantCulture);

    protected override bool IsCatalogued(Vehicle vehicle) => vehicle.HasDevice;

    protected override string Serial(Vehicle vehicle) => ToSerial(vehicle.Device!.Value);

    protected override string Name(Vehicle vehicle) => vehicle.Name ?? Serial(vehicle);

    protected override int Identifier(Vehicle vehicle, DeviceTransporterVm? assignment) => ToIdentifier(vehicle.Id);
}
