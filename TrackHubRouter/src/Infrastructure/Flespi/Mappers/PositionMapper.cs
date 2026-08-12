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

namespace TrackHub.Router.Infrastructure.Flespi.Mappers;

internal static class PositionMapper
{

    /// <summary>
    /// Maps a Message to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this Message message, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            message.Position_latitude ?? 0,
            message.Position_longitude ?? 0,
            message.Position_altitude,
            message.Timestamp.FromUnixTimestamp(),
            null,
            message.Position_speed ?? 0,
            message.Position_direction,
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Satellites = message.Position_satellites
            }
        );

    /// <summary>
    /// Maps a collection of Message objects to PositionVm objects.
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Message> messages, DeviceTransporterVm deviceDto)
    {
        foreach (var message in messages)
        {
            if (message.Position_latitude.HasValue && message.Position_longitude.HasValue)
            {
                yield return message.MapToPositionVm(deviceDto);
            }
        }
    }
}
