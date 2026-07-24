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

using NetTopologySuite.Geometries;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

/// <summary>
/// Platform toll reference data. No <c>accountId</c> parameter anywhere by design - this is road
/// infrastructure, readable by any authenticated account user (spec 11 section 5).
/// </summary>
public sealed class TollCatalogReader(IApplicationDbContext context) : ITollCatalogReader
{
    public async Task<TollStationsPageVm> GetStationsPageAsync(
        string? search,
        string? country,
        bool? active,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = context.TollStations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{search}%")
                || (s.Code != null && EF.Functions.ILike(s.Code, $"%{search}%"))
                || (s.RoadName != null && EF.Functions.ILike(s.RoadName, $"%{search}%")));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(s => s.Country == country);
        }

        if (active.HasValue)
        {
            query = query.Where(s => s.Active == active.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Ordering on entity columns, before projection (rules.md, Forbidden patterns).
        var stations = await query
            .OrderBy(s => s.Name)
            .ThenBy(s => s.TollStationId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new TollStationsPageVm([.. stations.Select(TripMapper.ToVm)], totalCount);
    }

    public async Task<TollStationDetailVm> GetStationDetailAsync(Guid tollStationId, CancellationToken cancellationToken)
    {
        var station = await context.TollStations
            .FirstOrDefaultAsync(s => s.TollStationId == tollStationId, cancellationToken)
            ?? throw new NotFoundException($"{tollStationId}", nameof(TollStation));

        // Full history, newest window first: tariffs are append-only so an operator can see
        // exactly which price a historical estimate used (acceptance 21).
        var tariffs = await context.TollTariffs
            .Where(t => t.TollStationId == tollStationId)
            .OrderBy(t => t.TollVehicleClassCode)
            .ThenByDescending(t => t.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return new TollStationDetailVm(
            TripMapper.ToVm(station),
            [.. tariffs.Select(TripMapper.ToVm)]);
    }

    public async Task<IReadOnlyCollection<TollVehicleClassVm>> GetVehicleClassesAsync(CancellationToken cancellationToken)
    {
        var classes = await context.TollVehicleClasses
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);

        return [.. classes.Select(TripMapper.ToVm)];
    }

    public async Task<IReadOnlyCollection<TollStationMatchVm>> MatchStationsAsync(
        IReadOnlyCollection<CoordinateVm> route,
        double toleranceMeters,
        string vehicleClassCode,
        DateOnly onDate,
        CancellationToken cancellationToken)
    {
        var line = BuildRouteLine(route);
        if (line is null)
        {
            return [];
        }

        // ST_DWithin over the geography cast (useSpheroid: true) so the tolerance really is metres.
        var rows = await context.TollStations
            .Where(s => s.Active && EF.Functions.IsWithinDistance(s.Point, line, toleranceMeters, true))
            .OrderBy(s => s.Name)
            .ThenBy(s => s.TollStationId)
            .Select(s => new
            {
                s.TollStationId,
                s.Name,
                s.Code,
                s.Point,
                s.RoadName,
                s.Direction,
                Tariff = context.TollTariffs
                    .Where(t => t.TollStationId == s.TollStationId
                        && t.TollVehicleClassCode == vehicleClassCode
                        && t.EffectiveFrom <= onDate
                        && (t.EffectiveTo == null || t.EffectiveTo >= onDate))
                    .OrderByDescending(t => t.EffectiveFrom)
                    .Select(t => new { t.Amount, t.Currency })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        // De-duplicate per station per plan (spec 11 section 6.2). A key-based DistinctBy, never
        // Distinct() over a projection - see rules.md on set operations and untyped columns.
        return [.. rows
            .DistinctBy(r => r.TollStationId)
            .Select(r => new TollStationMatchVm(
                r.TollStationId,
                r.Name,
                r.Code,
                r.Point.Y,
                r.Point.X,
                r.RoadName,
                r.Direction,
                // Null, NEVER zero: a matched station with no tariff for this class on this date
                // makes the estimate PartialNoTariff instead of silently understating cost.
                r.Tariff?.Amount,
                r.Tariff?.Currency,
                r.Tariff is not null))];
    }

    public async Task<bool> HasOverlappingTariffAsync(
        Guid tollStationId,
        string vehicleClassCode,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeTariffId,
        CancellationToken cancellationToken)
    {
        var query = context.TollTariffs.Where(t =>
            t.TollStationId == tollStationId
            && t.TollVehicleClassCode == vehicleClassCode);

        if (excludeTariffId.HasValue)
        {
            query = query.Where(t => t.TollTariffId != excludeTariffId.Value);
        }

        // Half-open window overlap: existing.From <= new.To AND (existing.To is null OR
        // existing.To >= new.From). An open-ended candidate overlaps everything from its start.
        return await query.AnyAsync(t =>
            (effectiveTo == null || t.EffectiveFrom <= effectiveTo)
            && (t.EffectiveTo == null || t.EffectiveTo >= effectiveFrom),
            cancellationToken);
    }

    /// <summary>
    /// Builds the planned line the match runs against. Fewer than two distinct vertices is not a
    /// route, so it matches nothing rather than throwing - an unplanned trip still estimates as
    /// <c>NoStations</c> (spec 11 section 7.7).
    /// </summary>
    private static LineString? BuildRouteLine(IReadOnlyCollection<CoordinateVm> route)
    {
        if (route.Count < 2)
        {
            return null;
        }

        var coordinates = route.Select(c => new Coordinate(c.Longitude, c.Latitude)).ToArray();
        return new LineString(coordinates) { SRID = TripGeometryDefaults.Srid };
    }
}
