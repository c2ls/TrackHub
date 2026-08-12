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

using Common.Domain.Constants;
using Common.Domain.Enums;

namespace TrackHub.Geofencing.Infrastructure.ManagerDB.Readers;

public sealed class GeofenceEventReader(IApplicationDbContext context) : IGeofenceEventReader
{
    public async Task<IReadOnlyCollection<GeofenceEventVm>> GetOpenEventsForTransporterAsync(
        Guid transporterId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var query = from evt in context.GeofenceEvents
                    where evt.TransporterId == transporterId && evt.AccountId == accountId && evt.DepartureTimestamp == null
                    select new GeofenceEventVm(
                        evt.GeofenceEventId,
                        evt.TransporterId,
                        evt.GeofenceId,
                        evt.EventDateTime,
                        evt.DepartureTimestamp,
                        evt.Latitude,
                        evt.Longitude);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<GeofenceEventsPageVm> GetGeofenceEventsAsync(
        Guid accountId,
        Guid userId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        Guid? transporterId,
        Guid? geofenceId,
        bool openOnly,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // No g.Active filter: deactivating a geofence must not erase its recorded visit
        // history from the event view.
        var query = from evt in context.GeofenceEvents
                    join g in context.Geofences on evt.GeofenceId equals g.GeofenceId
                    join t in context.Transporters on evt.TransporterId equals t.TransporterId
                    where g.AccountId == accountId
                    where t.UserId == userId
                    where evt.EventDateTime >= fromDate && evt.EventDateTime <= toDate
                    where transporterId == null || evt.TransporterId == transporterId
                    where geofenceId == null || evt.GeofenceId == geofenceId
                    where !openOnly || evt.DepartureTimestamp == null
                    select new
                    {
                        evt.GeofenceEventId,
                        evt.TransporterId,
                        evt.GeofenceId,
                        TransporterName = t.Name,
                        GeofenceName = g.Name,
                        evt.EventDateTime,
                        evt.DepartureTimestamp,
                        evt.Latitude,
                        evt.Longitude
                    };

        var totalCount = await query.CountAsync(cancellationToken);
        var rawEvents = await query
            .OrderByDescending(e => e.EventDateTime)
            .ThenByDescending(e => e.GeofenceEventId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new GeofenceEventsPageVm(
            [.. rawEvents.Select(e => new GeofenceEventReportVm(
                e.GeofenceEventId,
                e.TransporterId,
                e.GeofenceId,
                e.TransporterName,
                e.GeofenceName,
                e.EventDateTime,
                e.DepartureTimestamp,
                FormatTotalTime(e.EventDateTime, e.DepartureTimestamp),
                DwellSeconds(e.EventDateTime, e.DepartureTimestamp),
                e.Latitude,
                e.Longitude))],
            totalCount);
    }

    public async Task<IReadOnlyCollection<DwellAlertCandidateVm>> GetDwellAlertCandidatesAsync(
        CancellationToken cancellationToken)
    {
        // Defensive per-cycle cap: the open-visit set is naturally small, but a pathological
        // backlog must not materialize unbounded; oldest visits go first, the rest are
        // picked up on subsequent 60 s cycles.
        const int MaxCandidatesPerCycle = 1000;
        var now = DateTimeOffset.UtcNow;
        var query = from evt in context.GeofenceEvents
                    join g in context.Geofences on evt.GeofenceId equals g.GeofenceId
                    where evt.DepartureTimestamp == null && evt.DwellAlertedAt == null
                    where g.Active && g.DwellThresholdMinutes != null
                    where context.Accounts.Any(a => a.AccountId == evt.AccountId
                        && (a.Status == (short)AccountStatus.Active || a.Status == (short)AccountStatus.Trial))
                    where context.AccountFeatures.Any(f => f.AccountId == evt.AccountId
                        && f.FeatureKey == FeatureKeys.Geofencing
                        && f.Enabled
                        && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                        && (f.EffectiveTo == null || f.EffectiveTo >= now))
                    orderby evt.EventDateTime
                    select new DwellAlertCandidateVm(
                        evt.GeofenceEventId,
                        evt.AccountId,
                        evt.TransporterId,
                        evt.GeofenceId,
                        g.Name,
                        g.Type,
                        evt.EventDateTime,
                        evt.Latitude,
                        evt.Longitude,
                        g.DwellThresholdMinutes!.Value);

        return await query.Take(MaxCandidatesPerCycle).ToListAsync(cancellationToken);
    }

    private static string FormatTotalTime(DateTimeOffset dateTimeIn, DateTimeOffset? dateTimeOut)
    {
        if (dateTimeOut == null) return string.Empty;
        var duration = dateTimeOut.Value - dateTimeIn;
        return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    private static long? DwellSeconds(DateTimeOffset dateTimeIn, DateTimeOffset? dateTimeOut)
        => dateTimeOut == null ? null : (long)(dateTimeOut.Value - dateTimeIn).TotalSeconds;
}
