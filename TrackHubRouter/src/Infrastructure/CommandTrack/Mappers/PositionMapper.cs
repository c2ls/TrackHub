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

using TrackHub.Router.Infrastructure.CommandTrack.Extensions;

namespace TrackHub.Router.Infrastructure.CommandTrack.Mappers;

internal static class PositionMapper
{

    /// <summary>
    /// Maps a Position object to a PositionVm object
    /// </summary>
    /// <param name="position"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a PositionVm object</returns>
    public static PositionVm MapToPositionVm(this Position position, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position.Latitude,
            position.Longitude,
            position.Altitude,
            position.DeviceDateTime,
            position.ServerDateTime,
            position.Speed,
            position.Course,
            position.EventId,
            position.Address.GetAddress(position.DistanceToAddress),
            position.City,
            position.State,
            position.Country,
            new AttributesVm
            {
                Ignition = position.Ignition,
                Satellites = position.Satellites.HasValue ? (int?)position.Satellites.Value : null,
                Mileage = position.Mileage,
                Temperature = position.Temperature
            }
        );

    /// <summary>
    /// Maps a DevicePosition object to a PositionVm object
    /// </summary>
    /// <param name="position"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a PositionVm object</returns>
    public static PositionVm MapToPositionVm(this DevicePosition position, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position.Latitude,
            position.Longitude,
            position.Altitude,
            position.DeviceDateTime,
            null,
            position.Speed,
            position.Course,
            null,
            position.Address.GetAddress(position.DistanceToAddress),
            position.City,
            position.State,
            position.Country,
            new AttributesVm
            {
                Ignition = position.Ignition,
                Satellites = position.Satellites.HasValue ? (int?)position.Satellites.Value : null,
                Mileage = position.Mileage,
                Temperature = position.Temperature,
                Hourmeter = position.Hourmeter
            }
        );

    /// <summary>
    /// Maps a collection of Position objects to a collection of PositionVm objects
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Position> positions, DeviceTransporterVm deviceDto)
    {
        foreach (var position in positions)
        {
            yield return position.MapToPositionVm(deviceDto);
        }
    }

    /// <summary>
    /// Maps a collection of Position objects to a collection of PositionVm objects using a dictionary of device transporters
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="devicesDictionary"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Position> positions, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var position in positions)
        {
            if (devicesDictionary.TryGetValue(position.Plate, out var device))
            {
                yield return position.MapToPositionVm(device);
            }
            else
            {
                continue;
            }
        }
    }

    /// <summary>
    /// Maps a collection of DevicePosition objects to a collection of PositionVm objects using a dictionary of device transporters
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="devicesDictionary"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<DevicePosition> positions, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var position in positions)
        {
            if (devicesDictionary.TryGetValue(position.Plate, out var device))
            {
                yield return position.MapToPositionVm(device);
            }
            else
            {
                continue;
            }
        }
    }
}
