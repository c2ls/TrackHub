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

using Geotab.Checkmate.ObjectModel;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Infrastructure.Geotab.Mappers;

internal static class PositionMapper
{
    /// <summary>
    /// Converts a Geotab SDK <see cref="DateTime"/> (UTC, but frequently carries Kind=Unspecified)
    /// into a UTC <see cref="DateTimeOffset"/> at the SDK boundary. Falls back to now when the SDK
    /// omits the timestamp.
    /// </summary>
    private static DateTimeOffset ToUtc(DateTime? value)
        => value.HasValue
            ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
            : DateTimeOffset.UtcNow;


    /// <summary>
    /// Maps a LogRecord object to a PositionVm object
    /// </summary>
    /// <param name="position"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a PositionVm object</returns>
    public static PositionVm MapToPositionVm(this LogRecord position, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position.Latitude ?? 0,
            position.Longitude ?? 0,
            null,
            ToUtc(position.DateTime),
            null,
            position.Speed ?? 0,
            0,
            null,
            null,
            null,
            null,
            null,
            null
        );

    /// <summary>
    /// Maps a DeviceStatusInfo object to a PositionVm object
    /// </summary>
    /// <param name="position"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a PositionVm object</returns>
    public static PositionVm MapToPositionVm(this DeviceStatusInfo position, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position.Latitude ?? 0,
            position.Longitude ?? 0,
            null,
            ToUtc(position.DateTime),
            null,
            position.Speed ?? 0,
            position.Bearing,
            null,
            null,
            null,
            null,
            null,
            null
        );

    /// <summary>
    /// Maps a collection of LogRecord objects to a collection of PositionVm objects
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="deviceDto"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<LogRecord> positions, DeviceTransporterVm deviceDto)
    {
        foreach (var position in positions)
        {
            yield return position.MapToPositionVm(deviceDto);
        }
    }

    /// <summary>
    /// Maps a collection of LogRecord objects to a collection of PositionVm objects using a dictionary of device transporters
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="devicesDictionary"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<LogRecord> positions, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var position in positions)
        {
            if (position.Device != null && position.Device.Name != null && devicesDictionary.TryGetValue(position.Device.Name, out var device))
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
    /// Maps a collection of DeviceStatusInfo objects to a collection of PositionVm objects using a dictionary of device transporters
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="devicesDictionary"></param>
    /// <returns>returns a collection of PositionVm objects</returns>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<DeviceStatusInfo> positions, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var position in positions)
        {
            if (position.Device != null && position.Device.Name != null && devicesDictionary.TryGetValue(position.Device.Name, out var device))
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
