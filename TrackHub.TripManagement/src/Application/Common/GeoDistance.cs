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

namespace TrackHub.TripManagement.Application.Common;

/// <summary>Great-circle distance, used to accumulate a trip's actual travelled distance.</summary>
public static class GeoDistance
{
    private const double EarthRadiusMeters = 6371008.8d;

    public static double HaversineMeters(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
    {
        var dLat = ToRadians(toLatitude - fromLatitude);
        var dLon = ToRadians(toLongitude - fromLongitude);
        var lat1 = ToRadians(fromLatitude);
        var lat2 = ToRadians(toLatitude);

        var a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2))
              + (Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2));
        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1d, Math.Sqrt(a)));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
}
