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

namespace TrackHub.Router.Infrastructure.Protrack.Mappers;

internal static class PositionMapper
{
    /// <summary>
    /// Converts a Unix timestamp (seconds) to DateTimeOffset.
    /// </summary>
    private static DateTimeOffset FromUnixTime(long unixTime)
        => unixTime > 0
            ? DateTimeOffset.FromUnixTimeSeconds(unixTime)
            : DateTimeOffset.MinValue;

    /// <summary>
    /// Maps a TrackRecord (current position) to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this TrackRecord track, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            track.Latitude,
            track.Longitude,
            null,
            FromUnixTime(track.Gpstime),
            FromUnixTime(track.Servertime),
            track.Speed,
            track.Course,
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Ignition = track.Accstatus == 1 ? true : track.Accstatus == 0 ? false : null
            }
        );

    /// <summary>
    /// Maps a parsed playback point to a PositionVm object.
    /// Playback data format: longitude,latitude,gpstime,speed,course
    /// </summary>
    public static PositionVm MapPlaybackToPositionVm(
        double longitude,
        double latitude,
        long gpstime,
        int speed,
        int course,
        DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            latitude,
            longitude,
            null,
            FromUnixTime(gpstime),
            null,
            speed,
            course,
            null,
            null,
            null,
            null,
            null,
            null
        );

    /// <summary>
    /// Maps a collection of TrackRecord objects to PositionVm objects using a dictionary of DeviceTransporterVm keyed by Serial (IMEI).
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<TrackRecord> tracks, IDictionary<string, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var track in tracks)
        {
            if (devicesDictionary.TryGetValue(track.Imei, out var device))
            {
                yield return track.MapToPositionVm(device);
            }
        }
    }
}
