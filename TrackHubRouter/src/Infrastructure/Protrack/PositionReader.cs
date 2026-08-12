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
using TrackHub.Router.Infrastructure.Protrack.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.Protrack;

/// <summary>
/// Reader for retrieving position information from Protrack API.
/// Uses /api/track for current positions and /api/playback for historical data.
/// Playback is paged (max 1000 records per request).
/// </summary>
public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
    : ProtrackReaderBase(httpClientFactory, httpClientService, credentialWriter, sessionStore), IPositionReader
{
    // Maximum number of records returned per playback request
    private const int PlaybackPageSize = 1000;

    /// <summary>
    /// Retrieves the last position of a single device asynchronously.
    /// Uses /api/track with the device's IMEI.
    /// </summary>
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/api/track?access_token={AccessToken}&imeis={deviceDto.Serial}";
        var result = await HttpClientService.GetAsync<TrackResponse>(url, cancellationToken: cancellationToken);

        var track = result?.Record?.FirstOrDefault(t => t.Imei == deviceDto.Serial);
        return track is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Serial}")
            : track.Value.MapToPositionVm(deviceDto);
    }

    /// <summary>
    /// Retrieves the last positions of multiple devices asynchronously.
    /// Uses /api/track with comma-separated IMEIs (max 100).
    /// </summary>
    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var devicesList = devices.ToList();
        if (devicesList.Count == 0)
        {
            return [];
        }

        var imeis = string.Join(",", devicesList.Select(d => d.Serial));
        var url = $"{BaseUrl}/api/track?access_token={AccessToken}&imeis={imeis}";
        var result = await HttpClientService.GetAsync<TrackResponse>(url, cancellationToken: cancellationToken);

        if (result?.Record is null || !result.Record.Any())
        {
            return [];
        }

        var devicesDictionary = devicesList.ToDictionary(device => device.Serial, device => device);
        return result.Record.MapToPositionVm(devicesDictionary).Distinct();
    }

    /// <summary>
    /// Retrieves the positions of a device within a specified time range asynchronously.
    /// Uses /api/playback endpoint with pagination (max 1000 records per page).
    /// When 1000 records are returned, continues requesting with begintime = last record's gpstime.
    /// </summary>
    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var allPositions = new List<PositionVm>();
        var beginTime = from.ToUnixTimeSeconds();
        var endTime = to.ToUnixTimeSeconds();

        while (true)
        {
            var url = $"{BaseUrl}/api/playback?access_token={AccessToken}&imei={deviceDto.Serial}&begintime={beginTime}&endtime={endTime}";
            var result = await HttpClientService.GetAsync<PlaybackResponse>(url, cancellationToken: cancellationToken);

            if (result?.Record is null || string.IsNullOrEmpty(result.Record))
            {
                break;
            }

            var positions = ParsePlaybackRecord(result.Record, deviceDto);
            allPositions.AddRange(positions);

            // If we got fewer than the page size, we have all the data
            if (positions.Count < PlaybackPageSize)
            {
                break;
            }

            // For the next page, use the last record's gpstime as the new begintime
            var lastPosition = positions[^1];
            beginTime = lastPosition.DeviceDateTime.ToUnixTimeSeconds();
        }

        return allPositions;
    }

    /// <summary>
    /// Parses the playback record string into a list of PositionVm objects.
    /// Format: "longitude,latitude,gpstime,speed,course;longitude,latitude,gpstime,speed,course;..."
    /// </summary>
    internal static List<PositionVm> ParsePlaybackRecord(string record, DeviceTransporterVm deviceDto)
    {
        var positions = new List<PositionVm>();
        var entries = record.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split(',');
            if (parts.Length < 5)
            {
                continue;
            }

            if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
                long.TryParse(parts[2], out var gpstime) &&
                int.TryParse(parts[3], out var speed) &&
                int.TryParse(parts[4], out var course))
            {
                positions.Add(PositionMapper.MapPlaybackToPositionVm(
                    longitude, latitude, gpstime, speed, course, deviceDto));
            }
        }

        return positions;
    }
}
