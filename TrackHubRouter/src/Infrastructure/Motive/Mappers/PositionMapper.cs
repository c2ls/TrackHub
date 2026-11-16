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

namespace TrackHub.Router.Infrastructure.Motive.Mappers;

internal static class PositionMapper
{
    // Requests never send X-Metric-Units: the docs state mph and miles for the default but never name the
    // metric units, so the imperial values are converted here.
    internal const double KilometresPerMile = 1.609344;
    internal const double MetresPerMile = 1609.344;

    public static PositionVm MapToPositionVm(this VehicleLocation location, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            location.Lat,
            location.Lon,
            null,
            location.LocatedAt,
            null,
            (location.Speed ?? 0) * KilometresPerMile,
            location.Bearing,
            null,
            location.Description,
            null,
            null,
            null,
            new AttributesVm(
                null,
                null,
                location.Odometer * MetresPerMile,
                location.EngineHours,
                null));

    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Vehicle> vehicles, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.CurrentLocation is { } location && devicesDictionary.TryGetValue(vehicle.Serial(), out var device))
            {
                yield return location.MapToPositionVm(device);
            }
        }
    }
}
