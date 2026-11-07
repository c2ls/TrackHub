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

using Common.Infrastructure;
using Common.Application.Paging;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

/// <summary>
/// Read side of the trip aggregate.
/// <para>
/// Visibility rule (acceptance 4): a non-null <c>userId</c> filters every query through
/// <c>trip.vw_visible_transporter</c> with an EXISTS predicate; a null <c>userId</c> means the
/// principal sees the whole account (Administrator/Manager roles and account-scoped service
/// clients). The view is the single source of group visibility - no handler re-implements it, and
/// no reader here joins the group graph by hand.
/// </para>
/// </summary>
public sealed class TripReader(IApplicationDbContext context, IAccountFeatureReader accountFeatureReader) : ITripReader
{
    /// <summary>
    /// The phase every list and detail read is computed against. It needs one number from the
    /// account's configuration (how long past a planned start counts as overdue) and the stops'
    /// current state — both fetched once per read, alongside the stop counts the page already
    /// gathered (spec 11a §10).
    /// </summary>
    private async Task<(int OverdueGraceMinutes, Dictionary<Guid, List<PhaseStopVm>> StopsByTrip)> PhaseContextAsync(
        Guid accountId, IReadOnlyCollection<Guid> tripIds, CancellationToken cancellationToken)
    {
        var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);

        if (tripIds.Count == 0)
        {
            return (config.OverdueGraceMinutes, []);
        }

        var ids = tripIds.ToList();

        // One grouped query for the whole page, not one per row. Ordering on the entity column
        // inside the query and before the projection - see TripMapper for why.
        var stops = await context.TripStops
            .Where(s => ids.Contains(s.TripId) && s.AccountId == accountId)
            .OrderBy(s => s.TripId)
            .ThenBy(s => s.Sequence)
            .Select(s => new { s.TripId, s.Sequence, s.Name, s.Activity, s.Status, s.EtaAt, s.DelayAlertedAt })
            .ToListAsync(cancellationToken);

        var byTrip = stops
            .GroupBy(s => s.TripId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => new PhaseStopVm(s.Sequence, s.Name, s.Activity, s.Status, s.EtaAt, s.DelayAlertedAt)).ToList());

        return (config.OverdueGraceMinutes, byTrip);
    }

    public async Task<TripsPageVm> GetTripsPageAsync(
        Guid accountId,
        Guid? userId,
        IReadOnlyCollection<string>? statuses,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? transporterId,
        Guid? staffId,
        string? customer,
        string? search,
        string? exception,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = Visible(accountId, userId);

        if (statuses is { Count: > 0 })
        {
            var statusList = statuses.ToList();
            query = query.Where(t => statusList.Contains(t.Status));
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.PlannedStartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.PlannedStartAt <= to.Value);
        }

        if (transporterId.HasValue)
        {
            query = query.Where(t => t.TransporterId == transporterId.Value);
        }

        if (staffId.HasValue)
        {
            query = query.Where(t => t.StaffId == staffId.Value);
        }

        if (!string.IsNullOrWhiteSpace(customer))
        {
            query = query.Where(t => t.CustomerName != null && EF.Functions.ILike(t.CustomerName, SearchPattern.Contains(customer), SearchPattern.Escape));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                EF.Functions.ILike(t.Code, SearchPattern.Contains(search), SearchPattern.Escape)
                || (t.ExternalReference != null && EF.Functions.ILike(t.ExternalReference, SearchPattern.Contains(search), SearchPattern.Escape))
                || EF.Functions.ILike(t.OriginName, SearchPattern.Contains(search), SearchPattern.Escape));
        }

        if (exception is not null)
        {
            var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);
            query = WithException(query, exception, DateTimeOffset.UtcNow.AddMinutes(-config.OverdueGraceMinutes));
        }

        return await PageAsync(query, skip, take, cancellationToken);
    }

    // Mirrors TripPhaseResolver in SQL, so the filter pages over every matching trip instead of a window.
    private IQueryable<Trip> WithException(IQueryable<Trip> query, string exception, DateTimeOffset overdueBefore)
    {
        var stops = context.TripStops;
        var running = query.Where(t => t.Status == TripStatuses.InProgress && !(t.OriginArrivedAt != null && t.OriginDepartedAt == null));

        return exception switch
        {
            TripExceptions.Overdue => query.Where(t => t.Status == TripStatuses.Created && t.PlannedStartAt < overdueBefore),
            TripExceptions.OffCorridor => query.Where(t => t.DeviationOpenedAt != null),
            TripExceptions.StalledFinalStop => running.Where(t =>
                stops.Any(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Arrived)
                && !stops.Any(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Pending)),
            TripExceptions.Delayed => running.Where(t =>
                stops.Any(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Arrived)
                    ? stops.Where(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Arrived)
                        .OrderBy(s => s.Sequence).Select(s => s.DelayAlertedAt).FirstOrDefault() != null
                    : stops.Where(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Pending)
                        .OrderBy(s => s.Sequence).Select(s => s.DelayAlertedAt).FirstOrDefault() != null),
            _ => throw new ArgumentOutOfRangeException(nameof(exception), exception, "Unknown trip exception."),
        };
    }

    public async Task<TripDetailVm> GetTripDetailAsync(Guid tripId, Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        var trip = await Visible(accountId, userId).FirstOrDefaultAsync(t => t.TripId == tripId, cancellationToken)
            ?? throw new NotFoundException($"{tripId}", nameof(Trip));

        var stops = await context.TripStops
            .Where(s => s.TripId == tripId && s.AccountId == accountId)
            .OrderBy(s => s.Sequence)
            .ToListAsync(cancellationToken);

        var stopIds = stops.ConvertAll(s => s.TripStopId);

        var deliveries = await context.Deliveries
            .Where(d => stopIds.Contains(d.TripStopId))
            .OrderBy(d => d.SequenceIndex)
            .ToListAsync(cancellationToken);

        var assignment = await context.TripAssignments
            .Where(a => a.TripId == tripId && a.Status == TripAssignmentStatuses.Active)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var plan = await LatestPlanAsync(tripId, accountId, cancellationToken);

        var pods = await context.ProofsOfDelivery
            .Where(p => stopIds.Contains(p.TripStopId))
            .OrderBy(p => p.CapturedAt)
            .ToListAsync(cancellationToken);

        var documents = await context.TripDocuments
            .Where(d => d.TripId == tripId)
            .ToListAsync(cancellationToken);

        var shares = await context.TripShares
            .Where(s => s.TripId == tripId && s.AccountId == accountId)
            .OrderByDescending(s => s.Created)
            .ToListAsync(cancellationToken);

        var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);
        var phaseStops = stops.ConvertAll(s => new PhaseStopVm(s.Sequence, s.Name, s.Activity, s.Status, s.EtaAt, s.DelayAlertedAt));

        return new TripDetailVm(
            TripMapper.ToVm(trip, stops.Count, phaseStops, config.OverdueGraceMinutes),
            [.. stops.Select(s => TripMapper.ToVm(
                s,
                [.. deliveries.Where(d => d.TripStopId == s.TripStopId).Select(TripMapper.ToVm)]))],
            assignment is null ? null : TripMapper.ToVm(assignment),
            plan is null ? null : TripMapper.ToVm(plan),
            [.. pods.Select(p => TripMapper.ToVm(
                p,
                [.. documents.Where(d => d.ProofOfDeliveryId == p.ProofOfDeliveryId).Select(TripMapper.ToVm)]))],
            [.. shares.Select(s => TripMapper.ToVm(s))]);
    }

    public async Task<IReadOnlyCollection<TripVm>> GetActiveTripsAsync(Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        var trips = await Visible(accountId, userId)
            .Where(t => t.Status == TripStatuses.InProgress || t.Status == TripStatuses.Paused)
            .OrderBy(t => t.PlannedStartAt)
            .ThenBy(t => t.Code)
            .ToListAsync(cancellationToken);

        return await WithStopCountsAsync(trips, cancellationToken);
    }

    public async Task<TripTimelineFeedPageVm> GetTimelineAsync(Guid tripId, Guid accountId, Guid? userId, string? cursor, int take, CancellationToken cancellationToken)
    {
        var visible = await Visible(accountId, userId).AnyAsync(t => t.TripId == tripId, cancellationToken);
        if (!visible)
        {
            throw new NotFoundException($"{tripId}", nameof(Trip));
        }

        var query = context.TripEvents.Where(e => e.TripId == tripId && e.AccountId == accountId);

        // Strict tuple comparison against the sort key, which ends on the unique id: the seek lands
        // exactly after the last row of the previous page even when several share an instant.
        if (FeedCursor.TryDecode(cursor, out var at, out var id))
        {
            query = query.Where(e => e.OccurredAt < at || (e.OccurredAt == at && e.TripEventId.CompareTo(id) < 0));
        }

        // Ordering on entity columns, before any projection - see TripMapper for why. One row past
        // the page is all a reader needs to know whether to offer "more".
        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.TripEventId)
            .Take(take + 1)
            .ToListAsync(cancellationToken);

        var hasMore = events.Count > take;
        var page = hasMore ? events.GetRange(0, take) : events;

        return new TripTimelineFeedPageVm(
            [.. page.Select(TripMapper.ToVm)],
            hasMore,
            page.Count > 0 ? FeedCursor.Encode(page[^1].OccurredAt, page[^1].TripEventId) : null);
    }

    public async Task<TripTimelinePageVm> GetTimelineByOffsetAsync(Guid tripId, Guid accountId, Guid? userId, int skip, int take, CancellationToken cancellationToken)
    {
        var visible = await Visible(accountId, userId).AnyAsync(t => t.TripId == tripId, cancellationToken);
        if (!visible)
        {
            throw new NotFoundException($"{tripId}", nameof(Trip));
        }

        var query = context.TripEvents.Where(e => e.TripId == tripId && e.AccountId == accountId);
        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.TripEventId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new TripTimelinePageVm([.. events.Select(TripMapper.ToVm)], totalCount);
    }

    public async Task<TripVm> GetTripAsync(Guid tripId, Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        // Visible(), not a bare account predicate: this is the lookup every write path uses, so a
        // group-scoped dispatcher must not be able to act on another group's trip by id. NotFound
        // rather than Forbidden - non-disclosure (spec 11 section 7.10).
        var trip = await Visible(accountId, userId)
            .FirstOrDefaultAsync(t => t.TripId == tripId, cancellationToken)
            ?? throw new NotFoundException($"{tripId}", nameof(Trip));

        var (overdueGraceMinutes, stopsByTrip) = await PhaseContextAsync(accountId, [tripId], cancellationToken);
        var stops = stopsByTrip.TryGetValue(tripId, out var found) ? found : [];

        return TripMapper.ToVm(trip, stops.Count, stops, overdueGraceMinutes);
    }

    public Task<TripVm?> FindByCodeAsync(Guid accountId, string code, CancellationToken cancellationToken)
        => FindOneAsync(accountId, t => t.Code == code, cancellationToken);

    public Task<TripVm?> FindByExternalReferenceAsync(Guid accountId, string externalReference, CancellationToken cancellationToken)
        => FindOneAsync(accountId, t => t.ExternalReference == externalReference, cancellationToken);

    private async Task<TripVm?> FindOneAsync(
        Guid accountId, System.Linq.Expressions.Expression<Func<Trip, bool>> predicate, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .Where(t => t.AccountId == accountId)
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return null;
        }

        var (overdueGraceMinutes, stopsByTrip) = await PhaseContextAsync(accountId, [trip.TripId], cancellationToken);
        var stops = stopsByTrip.TryGetValue(trip.TripId, out var found) ? found : [];

        return TripMapper.ToVm(trip, stops.Count, stops, overdueGraceMinutes);
    }

    public async Task<Guid?> FindVisibleTripIdByStopAsync(Guid tripStopId, Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = from stop in context.TripStops
                    where stop.TripStopId == tripStopId && stop.AccountId == accountId
                    join trip in Visible(accountId, userId) on stop.TripId equals trip.TripId
                    select trip.TripId;

        var tripId = await query.FirstOrDefaultAsync(cancellationToken);
        return tripId == Guid.Empty ? null : tripId;
    }

    public async Task<Guid?> FindVisibleTripIdByDeliveryAsync(Guid deliveryId, Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = from delivery in context.Deliveries
                    where delivery.DeliveryId == deliveryId && delivery.AccountId == accountId
                    join stop in context.TripStops on delivery.TripStopId equals stop.TripStopId
                    join trip in Visible(accountId, userId) on stop.TripId equals trip.TripId
                    select trip.TripId;

        var tripId = await query.FirstOrDefaultAsync(cancellationToken);
        return tripId == Guid.Empty ? null : tripId;
    }

    public async Task<bool> TransporterExistsInAccountAsync(Guid transporterId, Guid accountId, CancellationToken cancellationToken)
        => await context.Transporters.AnyAsync(
            t => t.TransporterId == transporterId && t.AccountId == accountId,
            cancellationToken);

    public async Task<bool> GeofenceExistsInAccountAsync(Guid geofenceId, Guid accountId, CancellationToken cancellationToken)
        => await context.Geofences.AnyAsync(
            g => g.GeofenceId == geofenceId && g.AccountId == accountId,
            cancellationToken);

    public async Task<IReadOnlyCollection<NamedEntityVm>> GetTransporterNamesAsync(Guid accountId, Guid? userId, CancellationToken cancellationToken)
    {
        var query = context.Transporters.Where(t => t.AccountId == accountId);

        // The same EXISTS predicate the trip queries use, against the same view: bulk planning is
        // subject to group visibility exactly as the create dialog is (acceptance 4).
        if (userId is { } actingUserId)
        {
            query = query.Where(t => context.VisibleTransporters.Any(v =>
                v.AccountId == accountId
                && v.UserId == actingUserId
                && v.TransporterId == t.TransporterId));
        }

        var rows = await query
            .OrderBy(t => t.Name)
            .Select(t => new { t.TransporterId, t.Name })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new NamedEntityVm(r.TransporterId, r.Name))];
    }

    public async Task<IReadOnlyCollection<NamedEntityVm>> GetStaffNamesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var rows = await context.StaffMembers
            .Where(d => d.AccountId == accountId)
            .OrderBy(d => d.Name)
            .Select(d => new { d.StaffId, d.Name })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new NamedEntityVm(r.StaffId, r.Name))];
    }

    public async Task<TripsPageVm> GetReportDataAsync(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = Visible(accountId, userId)
            .Where(t => t.PlannedStartAt >= from && t.PlannedStartAt <= to);

        if (transporterId.HasValue)
        {
            query = query.Where(t => t.TransporterId == transporterId.Value);
        }

        if (staffId.HasValue)
        {
            query = query.Where(t => t.StaffId == staffId.Value);
        }

        return await PageAsync(query, skip, take, cancellationToken);
    }

    public async Task<bool> IsTransporterVisibleAsync(Guid accountId, Guid userId, Guid transporterId, CancellationToken cancellationToken)
        => await context.VisibleTransporters.AnyAsync(
            v => v.AccountId == accountId && v.UserId == userId && v.TransporterId == transporterId,
            cancellationToken);

    public async Task<TripReportPageVm> GetTripReportRowsAsync(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = ReportScope(accountId, userId, from, to, transporterId, staffId);
        var totalCount = await query.CountAsync(cancellationToken);

        // Ordering on entity columns, inside the query expression and BEFORE the projection -
        // Npgsql cannot translate an ordering over a constructor-built record struct member, and
        // EF InMemory would not catch it (rules.md, Forbidden patterns).
        var rows = await query
            .OrderBy(t => t.PlannedStartAt)
            .ThenBy(t => t.Code)
            .Skip(skip)
            .Take(take)
            .Select(t => new
            {
                Trip = t,
                TransporterName = context.Transporters
                    .Where(x => x.TransporterId == t.TransporterId).Select(x => x.Name).FirstOrDefault(),
                StaffName = context.StaffMembers
                    .Where(x => x.StaffId == t.StaffId).Select(x => x.Name).FirstOrDefault(),
                StopCount = context.TripStops.Count(s => s.TripId == t.TripId),
                Plan = context.RoutePlans
                    .Where(p => p.TripId == t.TripId && p.Status == RoutePlanStatuses.Ready)
                    .OrderByDescending(p => p.ComputedAt)
                    .Select(p => new { p.PlannedDistanceMeters, p.EstimatedTollAmount, p.TollCurrency, p.TollStatus })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var durations = await MeasuredDurationsAsync(
            accountId, rows.ConvertAll(r => r.Trip.TripId), cancellationToken);

        return new TripReportPageVm(
            [.. rows.Select(r => new TripReportRowVm(
                r.Trip.TripId,
                r.Trip.AccountId,
                r.Trip.Code,
                r.Trip.Status,
                r.Trip.TransporterId,
                r.Trip.StaffId,
                r.Trip.RoutePlanId,
                r.Trip.ServiceOrderId,
                r.Trip.ExternalReference,
                r.Trip.CustomerName,
                r.Trip.OriginName,
                r.Trip.OriginPoint.Y,
                r.Trip.OriginPoint.X,
                r.Trip.PlannedStartAt,
                r.Trip.PlannedEndAt,
                r.Trip.ActualStartAt,
                r.Trip.ActualEndAt,
                r.Trip.OriginArrivedAt,
                r.Trip.OriginDepartedAt,
                Minutes(r.Trip.OriginArrivedAt, r.Trip.OriginDepartedAt),
                TransitMinutes(r.Trip, durations.GetValueOrDefault(r.Trip.TripId)),
                Minutes(r.Trip.ActualStartAt, r.Trip.ActualEndAt),
                r.Trip.Notes,
                r.Trip.LastPositionAt,
                r.Trip.LastPoint == null ? null : r.Trip.LastPoint.Y,
                r.Trip.LastPoint == null ? null : r.Trip.LastPoint.X,
                r.Trip.ActualDistanceMeters,
                r.Trip.TollVehicleClass,
                r.Trip.DeviationOpenedAt,
                r.Trip.CancellationReason,
                r.StopCount,
                r.Trip.LastModified,
                r.TransporterName,
                r.StaffName,
                r.Plan == null ? null : r.Plan.PlannedDistanceMeters,
                r.Plan == null ? null : r.Plan.EstimatedTollAmount,
                r.Plan == null ? null : r.Plan.TollCurrency,
                r.Plan == null ? null : r.Plan.TollStatus))],
            totalCount,
            skip + rows.Count,
            skip + rows.Count < totalCount);
    }

    public async Task<TripStopReportPageVm> GetTripStopReportRowsAsync(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var trips = ReportScope(accountId, userId, from, to, transporterId, staffId);

        var query = from stop in context.TripStops
                    join trip in trips on stop.TripId equals trip.TripId
                    select new { Stop = stop, Trip = trip };

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(x => x.Trip.PlannedStartAt)
            .ThenBy(x => x.Trip.Code)
            .ThenBy(x => x.Stop.Sequence)
            .Skip(skip)
            .Take(take)
            .Select(x => new
            {
                x.Stop,
                x.Trip.TripId,
                x.Trip.Code,
                TripCustomerName = x.Trip.CustomerName,
                TransporterName = context.Transporters
                    .Where(t => t.TransporterId == x.Trip.TransporterId).Select(t => t.Name).FirstOrDefault(),
                StaffName = context.StaffMembers
                    .Where(d => d.StaffId == x.Trip.StaffId).Select(d => d.Name).FirstOrDefault(),
                StopCustomerName = context.Deliveries
                    .Where(d => d.TripStopId == x.Stop.TripStopId)
                    .OrderBy(d => d.SequenceIndex)
                    .Select(d => d.ClientName)
                    .FirstOrDefault(),
                DeliveryCount = context.Deliveries.Count(d => d.TripStopId == x.Stop.TripStopId),
                DeliveredCount = context.Deliveries.Count(d => d.TripStopId == x.Stop.TripStopId && d.Status == DeliveryStatuses.Delivered),
                FailedDeliveryCount = context.Deliveries.Count(d => d.TripStopId == x.Stop.TripStopId && d.Status == DeliveryStatuses.Rejected),
                PartialDeliveryCount = context.Deliveries.Count(d => d.TripStopId == x.Stop.TripStopId && d.Status == DeliveryStatuses.PartiallyDelivered),
            })
            .ToListAsync(cancellationToken);

        return new TripStopReportPageVm(
            [.. rows.Select(r => new TripStopReportRowVm(
                r.Stop.TripStopId,
                r.TripId,
                r.Code,
                r.TransporterName,
                r.StaffName,
                // The stop's delivery client, falling back to the trip's customer.
                r.StopCustomerName ?? r.TripCustomerName,
                r.Stop.Sequence,
                r.Stop.Name,
                r.Stop.Activity,
                r.Stop.Status,
                r.Stop.PlannedArrivalFrom,
                r.Stop.PlannedArrivalTo,
                r.Stop.ActualArrivalAt,
                r.Stop.ActualDepartureAt,
                r.DeliveryCount,
                r.DeliveredCount,
                r.FailedDeliveryCount,
                r.PartialDeliveryCount))],
            totalCount,
            skip + rows.Count,
            skip + rows.Count < totalCount);
    }

    public async Task<TripTollReportPageVm> GetTripTollReportRowsAsync(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var trips = ReportScope(accountId, userId, from, to, transporterId, staffId);

        // The per-station breakdown is stored as the plan's TollStationsJson, so paging happens on
        // PLANS and each plan expands to its station rows after materialization. Paging on the
        // expanded rows would need the json exploded in SQL, which Postgres cannot index or
        // de-duplicate cheaply (rules.md: never Distinct over a json-bearing projection).
        var query = from plan in context.RoutePlans
                    join trip in trips on plan.TripId equals trip.TripId
                    where plan.Status == RoutePlanStatuses.Ready
                        && plan.TollStationsJson != null
                        && trip.RoutePlanId == plan.RoutePlanId
                    select new { Plan = plan, Trip = trip };

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(x => x.Trip.PlannedStartAt)
            .ThenBy(x => x.Trip.Code)
            .Skip(skip)
            .Take(take)
            .Select(x => new
            {
                x.Plan.RoutePlanId,
                x.Plan.TollVehicleClass,
                x.Plan.TollStationsJson,
                x.Trip.TripId,
                x.Trip.Code,
                x.Trip.PlannedStartAt,
            })
            .ToListAsync(cancellationToken);

        return new TripTollReportPageVm(
            [.. rows.SelectMany(r => TripMapper.DeserializeStations(r.TollStationsJson)
                .Select(station => new TripTollReportRowVm(
                    r.TripId,
                    r.Code,
                    r.RoutePlanId,
                    r.PlannedStartAt,
                    r.TollVehicleClass,
                    station.TollStationId,
                    station.Name,
                    station.Code,
                    station.RoadName,
                    station.Direction,
                    // Null amount + HasTariff false is the PartialNoTariff signal. Never zero:
                    // a catalog gap must stay visible instead of netting silently into the total.
                    station.Amount,
                    station.Currency,
                    station.HasTariff)))],
            totalCount,
            // Both in PLANS — the unit this feed pages in. The returned row count is the expanded
            // station count and is deliberately NOT used here: advancing by it skipped roughly three
            // plans for every one consumed, and comparing it against a plan totalCount ended the
            // drain after the first page, dropping trips from a financial report with no error.
            skip + rows.Count,
            skip + rows.Count < totalCount);
    }

    public async Task<TripPodReportPageVm> GetTripPodReportRowsAsync(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var trips = ReportScope(accountId, userId, from, to, transporterId, staffId);

        var query = from pod in context.ProofsOfDelivery
                    join stop in context.TripStops on pod.TripStopId equals stop.TripStopId
                    join trip in trips on stop.TripId equals trip.TripId
                    select new { Pod = pod, Stop = stop, Trip = trip };

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(x => x.Pod.CapturedAt)
            .ThenBy(x => x.Pod.ProofOfDeliveryId)
            .Skip(skip)
            .Take(take)
            .Select(x => new
            {
                x.Pod,
                x.Stop.Sequence,
                StopName = x.Stop.Name,
                x.Trip.TripId,
                x.Trip.Code,
                // A COUNT, never the documents: the register may name the evidence, but the bytes
                // stay behind Manager's access policy (spec 11 section 13, SC-06).
                DocumentCount = context.TripDocuments.Count(d => d.ProofOfDeliveryId == x.Pod.ProofOfDeliveryId),
            })
            .ToListAsync(cancellationToken);

        return new TripPodReportPageVm(
            [.. rows.Select(r => new TripPodReportRowVm(
                r.Pod.ProofOfDeliveryId,
                r.TripId,
                r.Code,
                r.Pod.TripStopId,
                r.Sequence,
                r.StopName,
                r.Pod.ReceiverName,
                r.Pod.ReceiverDocument,
                r.Pod.CapturedAt,
                r.Pod.Latitude,
                r.Pod.Longitude,
                r.DocumentCount))],
            totalCount,
            skip + rows.Count,
            skip + rows.Count < totalCount);
    }

    /// <summary>Whole minutes between two measurements, or null when either was never taken.</summary>
    private static int? Minutes(DateTimeOffset? from, DateTimeOffset? to)
        => from is { } start && to is { } end && end >= start
            ? (int)(end - start).TotalMinutes
            : null;

    /// <summary>
    /// Time actually spent moving: from the origin departure to the last measured arrival, less the
    /// dwell at every stop in between (spec 11a §4.3).
    /// <para>
    /// Subtracting the intermediate dwells is what makes the number mean "driving". A five-stop
    /// delivery run that spent three hours on the road and two at docks would otherwise report five
    /// hours of transit, and the loading/unloading columns beside it would double-count the same
    /// two hours.
    /// </para>
    /// </summary>
    private static int? TransitMinutes(Trip trip, (DateTimeOffset? LastArrivalAt, double IntermediateDwellMinutes) measured)
    {
        if (trip.OriginDepartedAt is not { } departed || measured.LastArrivalAt is not { } lastArrival)
        {
            return null;
        }

        var gross = (lastArrival - departed).TotalMinutes;
        return gross <= 0 ? 0 : (int)Math.Max(gross - measured.IntermediateDwellMinutes, 0d);
    }

    /// <summary>
    /// The per-trip stop measurements the transit calculation needs, for a whole report page in one
    /// query — one round trip, not one per row.
    /// </summary>
    private async Task<Dictionary<Guid, (DateTimeOffset? LastArrivalAt, double IntermediateDwellMinutes)>> MeasuredDurationsAsync(
        Guid accountId, IReadOnlyCollection<Guid> tripIds, CancellationToken cancellationToken)
    {
        if (tripIds.Count == 0)
        {
            return [];
        }

        var ids = tripIds.ToList();
        var stops = await context.TripStops
            .Where(s => ids.Contains(s.TripId) && s.AccountId == accountId && s.ActualArrivalAt != null)
            .Select(s => new { s.TripId, s.ActualArrivalAt, s.ActualDepartureAt })
            .ToListAsync(cancellationToken);

        return stops
            .GroupBy(s => s.TripId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var lastArrival = g.Max(s => s.ActualArrivalAt);

                    // "Intermediate" is every visit that CLOSED before the trip's last arrival. The
                    // final stop's own dwell is not transit time and is reported separately as its
                    // activity's loading/unloading duration.
                    var dwell = g
                        .Where(s => s.ActualDepartureAt.HasValue && s.ActualArrivalAt < lastArrival)
                        .Sum(s => (s.ActualDepartureAt!.Value - s.ActualArrivalAt!.Value).TotalMinutes);

                    return (lastArrival, dwell);
                });
    }

    /// <summary>
    /// The shared scope for all four report feeds: the same account boundary, the same group
    /// visibility and the same filters as the list and detail paths (acceptance 3).
    /// </summary>
    private IQueryable<Trip> ReportScope(
        Guid accountId,
        Guid? userId,
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? transporterId,
        Guid? staffId)
    {
        var query = Visible(accountId, userId)
            .Where(t => t.PlannedStartAt >= from && t.PlannedStartAt <= to);

        if (transporterId.HasValue)
        {
            query = query.Where(t => t.TransporterId == transporterId.Value);
        }

        if (staffId.HasValue)
        {
            query = query.Where(t => t.StaffId == staffId.Value);
        }

        return query;
    }

    /// <summary>
    /// The one place group visibility is applied. An EXISTS predicate rather than a join, because
    /// the view repeats a (user, transporter) pair once per shared group and a join would multiply
    /// the trip rows.
    /// </summary>
    private IQueryable<Trip> Visible(Guid accountId, Guid? userId)
    {
        var query = context.Trips.Where(t => t.AccountId == accountId);

        if (userId is { } actingUserId)
        {
            query = query.Where(t => context.VisibleTransporters.Any(v =>
                v.AccountId == accountId
                && v.UserId == actingUserId
                && v.TransporterId == t.TransporterId));
        }

        return query;
    }

    private async Task<RoutePlan?> LatestPlanAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
        => await context.RoutePlans
            .Where(p => p.TripId == tripId && p.AccountId == accountId)
            .OrderByDescending(p => p.ComputedAt)
            .ThenByDescending(p => p.Created)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<TripsPageVm> PageAsync(IQueryable<Trip> query, int skip, int take, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var trips = await query
            .OrderByDescending(t => t.PlannedStartAt)
            .ThenBy(t => t.Code)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new TripsPageVm(await WithStopCountsAsync(trips, cancellationToken), totalCount);
    }

    private async Task<IReadOnlyCollection<TripVm>> WithStopCountsAsync(List<Trip> trips, CancellationToken cancellationToken)
    {
        if (trips.Count == 0)
        {
            return [];
        }

        var tripIds = trips.ConvertAll(t => t.TripId);
        var (overdueGraceMinutes, stopsByTrip) = await PhaseContextAsync(trips[0].AccountId, tripIds, cancellationToken);

        return [.. trips.Select(t =>
        {
            var stops = stopsByTrip.TryGetValue(t.TripId, out var found) ? found : [];
            return TripMapper.ToVm(t, stops.Count, stops, overdueGraceMinutes);
        })];
    }
}
