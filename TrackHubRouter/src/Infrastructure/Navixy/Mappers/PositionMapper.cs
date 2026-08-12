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

namespace TrackHub.Router.Infrastructure.Navixy.Mappers;

internal static class PositionMapper
{
    // Navixy date format: yyyy-MM-dd HH:mm:ss
    private const string NavixyDateFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Parses a Navixy date string to DateTimeOffset.
    /// </summary>
    private static DateTimeOffset ParseNavixyDate(string? dateStr)
        // Navixy timestamps are naive (no zone); assume UTC and normalize to UTC.
        => !string.IsNullOrEmpty(dateStr) && DateTimeOffset.TryParseExact(dateStr, NavixyDateFormat, null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var result)
            ? result
            : DateTimeOffset.MinValue;

    /// <summary>
    /// Maps a Tracker with last_update to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this Tracker tracker, DeviceTransporterVm deviceDto)
    {
        var lastUpdate = tracker.Last_update;
        return new PositionVm(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            lastUpdate?.Lat ?? 0,
            lastUpdate?.Lng ?? 0,
            null,  // last_update doesn't include altitude
            ParseNavixyDate(lastUpdate?.Time),
            null,
            lastUpdate?.Speed ?? 0,
            lastUpdate?.Heading,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }

    /// <summary>
    /// Maps a TrackPoint to a PositionVm object.
    /// </summary>
    public static PositionVm MapToPositionVm(this TrackPoint point, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            point.Lat,
            point.Lng,
            point.Alt,
            ParseNavixyDate(point.Get_time),
            null,
            point.Speed,
            point.Heading,
            null,
            point.Address,
            null,
            null,
            null,
            null
        );

    /// <summary>
    /// Maps a collection of Tracker objects to PositionVm objects using a dictionary of DeviceTransporterVm.
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<Tracker> trackers, IDictionary<int, DeviceTransporterVm> devicesDictionary)
    {
        foreach (var tracker in trackers)
        {
            if (tracker.Last_update.HasValue && devicesDictionary.TryGetValue((int)tracker.Tracker_id, out var device))
            {
                yield return tracker.MapToPositionVm(device);
            }
        }
    }

    /// <summary>
    /// Maps a collection of TrackPoint objects to PositionVm objects.
    /// </summary>
    public static IEnumerable<PositionVm> MapToPositionVm(this IEnumerable<TrackPoint> points, DeviceTransporterVm deviceDto)
    {
        foreach (var point in points)
        {
            yield return point.MapToPositionVm(deviceDto);
        }
    }
}
