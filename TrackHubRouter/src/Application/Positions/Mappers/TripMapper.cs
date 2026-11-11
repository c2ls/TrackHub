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

using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Positions.Mappers;

public static class TripMapper
{
    private const double DwellRadiusKm = 0.1;

    /// <summary>
    /// Splits a position history into a continuous sequence of moving, stop and no-data segments.
    /// </summary>
    /// <param name="accBased">Ignition decides movement; falls back to speed when the device reports none.</param>
    /// <param name="stoppedGap">Minutes standing still, or without leaving the spot, before a halt counts as a stop.</param>
    /// <param name="maxDistance">Kilometers between consecutive points beyond which the leg is a reporting gap.</param>
    /// <param name="maxTimeGap">Silence while moving beyond which the leg is a reporting gap.</param>
    public static IEnumerable<TripVm> GroupPositionsIntoTrips(
        this IEnumerable<PositionVm> positions,
        bool accBased,
        double stoppedGap,
        double maxDistance,
        TimeSpan maxTimeGap)
    {
        var points = positions.CalculatePoints(accBased, stoppedGap);
        if (points.Count == 0)
        {
            return [];
        }

        var trips = new List<TripVm>();
        var current = CreateTrip(TypeOf(points[0]), points[0]);

        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1];
            var point = points[i];
            var distance = CalculateDistance(previous, point);
            var silence = point.DeviceDateTime - previous.DeviceDateTime;

            // A parked device may legitimately stay silent for hours, so silence only
            // breaks the track while moving; a position jump always does.
            if (distance > maxDistance || (previous.Movement && silence > maxTimeGap))
            {
                trips.Add(FinalizeTrip(current));
                trips.Add(FinalizeTrip(CreateTrip(TripTypeEnum.NO_DATA, previous, point)));
                current = CreateTrip(TypeOf(point), point);
                continue;
            }

            // Consecutive segments share their boundary point, so the track has no holes:
            // a trip ends on the first stopped point and a stop ends on the first moving one.
            current.Points.Add(point);
            if (current.Type == (short)TripTypeEnum.MOVING)
            {
                current.TotalDistance += distance;
            }
            if (previous.Movement != point.Movement)
            {
                trips.Add(FinalizeTrip(current));
                current = CreateTrip(TypeOf(point), point);
            }
        }

        // A trailing single point already closes the previous segment, unless it stands alone.
        if (current.Points.Count > 1 || trips.Count == 0 || trips[^1].Type == (short)TripTypeEnum.NO_DATA)
        {
            trips.Add(FinalizeTrip(current));
        }

        return trips;
    }

    private static TripTypeEnum TypeOf(TripPointVm point)
        => point.Movement ? TripTypeEnum.MOVING : TripTypeEnum.STOP;

    private static TripVm CreateTrip(TripTypeEnum type, params TripPointVm[] points)
        => new()
        {
            TripId = Guid.NewGuid(),
            Points = [.. points],
            Type = (short)type
        };

    private static TripVm FinalizeTrip(TripVm trip)
    {
        trip.From = trip.Points[0].DeviceDateTime;
        trip.To = trip.Points[^1].DeviceDateTime;
        trip.Duration = trip.To - trip.From;
        if (trip.Type == (short)TripTypeEnum.MOVING)
        {
            trip.AverageSpeed = trip.Points.Average(p => p.Speed);
        }
        else if (trip.Points.Count > 2)
        {
            trip.Points = [trip.Points[0], trip.Points[^1]];
        }
        return trip;
    }

    /// <summary>
    /// Haversine distance between two points, in kilometers.
    /// </summary>
    private static double CalculateDistance(TripPointVm p1, TripPointVm p2)
    {
        const double r = 6371e3;
        var lat1 = p1.Latitude * Math.PI / 180;
        var lat2 = p2.Latitude * Math.PI / 180;
        var deltaLat = (p2.Latitude - p1.Latitude) * Math.PI / 180;
        var deltaLon = (p2.Longitude - p1.Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return r * c / 1000;
    }

    private static bool IsDuplicate(TripPointVm p1, TripPointVm p2)
        => p1.Latitude == p2.Latitude &&
           p1.Longitude == p2.Longitude &&
           p1.DeviceDateTime == p2.DeviceDateTime;

    private static List<TripPointVm> CalculatePoints(this IEnumerable<PositionVm> positions, bool accBased, double stoppedGap)
    {
        var ordered = positions.OrderBy(p => p.DeviceDateTime).ToList();
        var useIgnition = accBased && ordered.Any(p => p.Attributes?.Ignition is not null);
        var points = new List<TripPointVm>(ordered.Count);

        foreach (var position in ordered)
        {
            var moving = useIgnition
                ? position.Attributes?.Ignition ?? position.Speed > 0
                : position.Speed > 0;
            var point = position.CastPoint(moving);
            if (points.Count == 0 || !IsDuplicate(points[^1], point))
            {
                points.Add(point);
            }
        }

        if (!useIgnition)
        {
            AbsorbBriefHalts(points, stoppedGap);
        }
        MarkDwells(points, stoppedGap);
        return points;
    }

    // Ignition, or a noisy speed reading, can claim movement while the unit never leaves the spot
    // (an excavator at work, GPS drift on an idle tracker): that is a stop, not a trip.
    private static void MarkDwells(List<TripPointVm> points, double stoppedGap)
    {
        for (var start = 0; start < points.Count; start++)
        {
            if (!points[start].Movement)
            {
                continue;
            }

            var end = start;
            while (end + 1 < points.Count && CalculateDistance(points[start], points[end + 1]) <= DwellRadiusKm)
            {
                end++;
            }

            if ((points[end].DeviceDateTime - points[start].DeviceDateTime).TotalMinutes < stoppedGap)
            {
                continue;
            }

            // The window also catches the last meters of the approach and the first of the
            // departure; those belong to the trips on either side.
            var spot = points[start] with
            {
                Latitude = points.Skip(start).Take(end - start + 1).Average(p => p.Latitude),
                Longitude = points.Skip(start).Take(end - start + 1).Average(p => p.Longitude)
            };
            while (start < end && CalculateDistance(points[start], spot) > DwellRadiusKm / 2)
            {
                start++;
            }
            while (end > start && CalculateDistance(points[end], spot) > DwellRadiusKm / 2)
            {
                end--;
            }
            for (var i = start; i <= end; i++)
            {
                points[i] = points[i] with { Movement = false };
            }
            start = end;
        }
    }

    // A halt shorter than stoppedGap (a traffic light) stays part of the trip. A longer one is a
    // stop from its first standing point, measured up to the point where movement resumes. A halt
    // cut by the edge of the range is kept: its real length is unknown.
    private static void AbsorbBriefHalts(List<TripPointVm> points, double stoppedGap)
    {
        for (var start = 0; start < points.Count; start++)
        {
            if (points[start].Movement)
            {
                continue;
            }

            var end = start;
            while (end + 1 < points.Count && !points[end + 1].Movement)
            {
                end++;
            }

            var bounded = start > 0 && end + 1 < points.Count;
            if (bounded && (points[end + 1].DeviceDateTime - points[start].DeviceDateTime).TotalMinutes < stoppedGap)
            {
                for (var i = start; i <= end; i++)
                {
                    points[i] = points[i] with { Movement = true };
                }
            }
            start = end;
        }
    }

    public static TripPointVm CastPoint(this PositionVm position, bool movement)
        => new (
            position.Latitude,
            position.Longitude,
            position.DeviceDateTime,
            position.Speed,
            position.Course,
            position.EventId,
            movement);

}
