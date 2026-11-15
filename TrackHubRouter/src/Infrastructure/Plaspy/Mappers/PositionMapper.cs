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

namespace TrackHub.Router.Infrastructure.Plaspy.Mappers;

internal static class PositionMapper
{
    // Plaspy documents no units: speed is taken as km/h and milleage as km, emitted in meters like the other providers.
    private const double MetersPerMileageUnit = 1000;

    public static DateTimeOffset ParseUtc(string value)
        => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static string FormatUtc(DateTimeOffset instant)
        => instant.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    public static PositionVm MapToPositionVm(this Location location, DeviceTransporterVm deviceDto)
        => new(
            deviceDto.TransporterId,
            deviceDto.Name,
            deviceDto.TransporterType,
            location.Latitude,
            location.Longitude,
            null,
            ParseUtc(location.DateTime),
            null,
            location.Speed ?? 0,
            location.Course,
            null,
            null,
            null,
            null,
            null,
            new AttributesVm
            {
                Mileage = location.Milleage * MetersPerMileageUnit,
                Temperature = location.Temperature
            });
}
