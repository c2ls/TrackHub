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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

/// <summary>
/// Projects the anonymous tracking snapshot.
/// <para>
/// This reader IS the disclosure boundary (acceptance 23): the projection is driven by the share
/// field flags server-side and never filtered by the client. Toll and cost figures, driver contact
/// data, internal notes, raw position history and document bytes have no path into
/// <see cref="PublicTripVm"/> at all - they are not read here, not merely omitted downstream.
/// </para>
/// </summary>
public sealed class TripShareReader(IApplicationDbContext context) : ITripShareReader
{
    public async Task<PublicTripVm?> GetPublicSnapshotAsync(Guid publicLinkGrantId, Guid accountId, CancellationToken cancellationToken)
    {
        var share = await context.TripShares
            .FirstOrDefaultAsync(
                s => s.PublicLinkGrantId == publicLinkGrantId && s.AccountId == accountId && s.RevokedAt == null,
                cancellationToken);

        if (share is null)
        {
            return null;
        }

        var trip = await context.Trips
            .FirstOrDefaultAsync(t => t.TripId == share.TripId && t.AccountId == accountId, cancellationToken);

        if (trip is null)
        {
            return null;
        }

        var stops = share.IncludeStopDetail
            ? await context.TripStops
                .Where(s => s.TripId == trip.TripId)
                .OrderBy(s => s.Sequence)
                .ToListAsync(cancellationToken)
            : [];

        var stopIds = stops.ConvertAll(s => s.TripStopId);
        var podStopIds = share.IncludePodSummary && stopIds.Count > 0
            ? await context.ProofsOfDelivery
                .Where(p => stopIds.Contains(p.TripStopId))
                .Select(p => p.TripStopId)
                .Distinct()
                .ToListAsync(cancellationToken)
            : [];

        // Gated by IncludeRoute, not attached unconditionally: §7.8 exposes route geometry "per
        // field flags", and a planned route is a disclosure — it draws the customer's competitors'
        // drop sequence across the map.
        var plannedRoute = share.IncludeRoute
            ? await PlannedRouteAsync(trip.TripId, accountId, cancellationToken)
            : null;

        // Live position only while the trip is actually running AND the flag is set.
        var showLive = share.IncludeLivePosition
            && string.Equals(trip.Status, TripStatuses.InProgress, StringComparison.Ordinal);

        // Both resolved from the Manager-owned tables mapped read-only into this context
        // (SVD-05 / the vw_visible_transporter rationale) — no per-request cross-service call, and
        // neither projection can reach contact data: Driver maps Name only, Transporter maps Name
        // and type id only. Phone and document number are not in this DbContext at all.
        var vehicleLabel = share.IncludeVehicle
            ? await context.Transporters
                .Where(t => t.TransporterId == trip.TransporterId && t.AccountId == accountId)
                .Select(t => t.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var driverGivenName = share.IncludeDriverName && trip.DriverId is { } driverId
            ? GivenName(await context.Drivers
                .Where(d => d.DriverId == driverId && d.AccountId == accountId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync(cancellationToken))
            : null;

        return new PublicTripVm(
            trip.TripId,
            trip.Code,
            trip.Status,
            trip.PlannedStartAt,
            trip.PlannedEndAt,
            trip.ActualStartAt,
            trip.ActualEndAt,
            // CustomerName is absent from PublicTripVm entirely — see the type's doc comment. It
            // used to be a parameter hardcoded to null here, which held the line but left a
            // null-shaped slot on the disclosure boundary that the next edit could fill in.
            [.. stops.Select(s => new PublicTripStopVm(
                s.Sequence,
                s.Name,
                // s.City, NEVER s.Address. §7.8 allows "city"; TripStop.Address is the ≤500-char
                // reverse-geocoded street label ("Cra 7 #71-52, Bogota"), so passing it here
                // disclosed every OTHER stop's exact delivery address to any link holder. The two
                // are separate columns precisely because they carry different disclosure levels.
                s.City,
                s.Status,
                s.PlannedArrivalFrom,
                s.PlannedArrivalTo,
                s.ActualArrivalAt,
                s.EtaAt,
                podStopIds.Contains(s.TripStopId)))],
            vehicleLabel,
            driverGivenName,
            showLive ? trip.LastPoint?.Y : null,
            showLive ? trip.LastPoint?.X : null,
            showLive ? trip.LastPositionAt : null,
            plannedRoute);
    }

    /// <summary>
    /// The driver's GIVEN NAME only — the first whitespace-delimited token of the stored display
    /// name (§7.8 says "driver <b>given name only</b>"). Returning the full name would hand a link
    /// holder a first-and-last-name identification of a specific employee, which is one public
    /// records search away from an address; the given name is enough for "your driver is Carlos".
    /// Truncation happens HERE, at the disclosure boundary, so no caller can forget it.
    /// </summary>
    private static string? GivenName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var span = fullName.AsSpan().Trim();
        var end = span.IndexOfAny(' ', '\t');
        return end < 0 ? span.ToString() : span[..end].ToString();
    }

    public async Task<Guid?> FindTripIdByShareAsync(Guid tripShareId, Guid accountId, CancellationToken cancellationToken)
    {
        // Deliberately NOT filtered on RevokedAt: re-revoking an already-revoked share must stay
        // idempotent (the writer keeps the first revocation instant), and a share that vanished
        // from this lookup once revoked would turn the second call into a spurious 404.
        var tripId = await context.TripShares
            .Where(s => s.TripShareId == tripShareId && s.AccountId == accountId)
            .Select(s => (Guid?)s.TripId)
            .FirstOrDefaultAsync(cancellationToken);

        return tripId;
    }

    private async Task<GeometryLineVm?> PlannedRouteAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
    {
        var plan = await context.RoutePlans
            .Where(p => p.TripId == tripId && p.AccountId == accountId && p.Status == RoutePlanStatuses.Ready)
            .OrderByDescending(p => p.ComputedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return TripMapper.ToLine(plan?.Geom);
    }
}
