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

using Common.Domain.Extensions;

namespace TrackHub.Router.Infrastructure.Wialon.Mappers;

internal static class PositionMapper
{
    /// <summary>
    /// Maps a Unit with position data to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this Unit unit, DeviceTransporterVm deviceDto)
    {
        var position = unit.Pos;
        return new PositionVm(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position?.Y ?? 0,    // Y = latitude
            position?.X ?? 0,    // X = longitude
            position?.Z,         // Z = altitude
            position.HasValue ? position.Value.T.FromUnixTimestamp() : DateTimeOffset.MinValue,
            null,
            position?.S ?? 0,    // S = speed
            position?.C,         // C = course
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Satellites = position?.Sc
            }
        );
    }

    /// <summary>
    /// Maps a Message to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this Message message, DeviceTransporterVm deviceDto)
    {
        var position = message.Pos;
        return new PositionVm(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            position?.Y ?? 0,    // Y = latitude
            position?.X ?? 0,    // X = longitude
            position?.Z,         // Z = altitude
            message.T.FromUnixTimestamp(),
            null,
            position?.S ?? 0,    // S = speed
            position?.C,         // C = course
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Satellites = position?.Sc,
                Mileage = message.P?.Odo
            }
        );
    }

    /// <summary>
    /// Maps a collection of Unit objects to PositionVm objects using a dictionary of DeviceTransporterVm.
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Unit> units, IDictionary<int, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var unit in units)
        {
            if (unit.Pos.HasValue && devicesDictionary.TryGetValue((int)unit.Id, out var device))
            {
                yield return unit.MapToPositionVm(device);
            }
        }
    }

    /// <summary>
    /// Maps a collection of Message objects to PositionVm objects.
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Message> messages, DeviceTransporterVm deviceDto)
    {
        foreach (var message in messages)
        {
            if (message.Pos.HasValue)
            {
                yield return message.MapToPositionVm(deviceDto);
            }
        }
    }
}
