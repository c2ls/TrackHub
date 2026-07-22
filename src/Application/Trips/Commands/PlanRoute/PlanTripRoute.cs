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

using System.Text.Json;
using Common.Application.Interfaces;
using Microsoft.Extensions.Logging;
using TrackHub.TripManagement.Application.Common;
using TrackHub.TripManagement.Application.Tolls.Services.Interfaces;
using TrackHub.TripManagement.Domain.Exceptions;

namespace TrackHub.TripManagement.Application.Trips.Commands.PlanRoute;

/// <summary>
/// Plans the trip's route through OpenRouteService and stores the result with its corridor and
/// toll estimate.
/// <para>
/// <b>No routing failure ever reaches this command's caller</b> (acceptance 18). An unconfigured
/// provider or a provider outage is persisted as a <c>Failed</c> <see cref="RoutePlanVm"/> carrying
/// an error code and RETURNED — the trip stays fully usable, ETA falls back to the planned
/// schedule, and the dispatcher sees why rather than a stack trace.
/// </para>
/// Rate-limited because each call spends external provider quota.
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Custom)]
[RequireFeature(FeatureKeys.TripManagement)]
[RateLimiting(PermitLimit = 10, WindowSeconds = 60)]
public readonly record struct PlanTripRouteCommand(Guid TripId, int? CorridorMeters, string? TollVehicleClass) : IRequest<RoutePlanVm>;

public sealed class PlanTripRouteCommandHandler(
    ITripReader reader,
    IRoutePlanWriter routePlanWriter,
    IRoutingProvider routingProvider,
    ITollEstimationService tollEstimationService,
    ITripEventWriter tripEventWriter,
    IAccountFeatureReader accountFeatureReader,
    IUserReader userReader,
    IUser user,
    ILogger<PlanTripRouteCommandHandler> logger) : IRequestHandler<PlanTripRouteCommand, RoutePlanVm>
{
    private const int DefaultCorridorMeters = 500;
    private const int MinCorridorMeters = 100;
    private const int MaxCorridorMeters = 5000;

    /// <summary>camelCase, matching the <c>AlertEmitter</c> payload precedent so every JSON column
    /// in this module reads the same way to a consumer.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<RoutePlanVm> Handle(PlanTripRouteCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var detail = await reader.GetTripDetailAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);
        var trip = detail.Trip;

        var corridorMeters = Math.Clamp(request.CorridorMeters ?? DefaultCorridorMeters, MinCorridorMeters, MaxCorridorMeters);
        var vehicleClass = request.TollVehicleClass ?? trip.TollVehicleClass;

        if (!routingProvider.IsConfigured)
        {
            return await FailAsync(
                request.TripId, caller.AccountId, corridorMeters,
                TripErrorCodes.RoutingNotConfigured,
                "No routing provider is configured for this deployment.",
                cancellationToken);
        }

        var waypoints = BuildWaypoints(trip, detail.Stops);

        RouteResultVm route;
        try
        {
            route = await routingProvider.GetRouteAsync(waypoints, cancellationToken);
        }
        catch (RoutingUnavailableException ex)
        {
            logger.LogWarning(ex, "Routing provider unavailable while planning trip {TripId}; recording a Failed plan", request.TripId);
            return await FailAsync(request.TripId, caller.AccountId, corridorMeters, ex.ErrorCode, ex.Message, cancellationToken);
        }

        var estimate = await EstimateTollsAsync(caller.AccountId, trip, route, vehicleClass, cancellationToken);

        // The per-leg breakdown and the ordered waypoints are part of what acceptance 17 requires
        // route planning to RETURN. The provider already parses them; passing nulls here threw them
        // away and left §6.1's LegsJson/WaypointsJson permanently empty.
        var plan = await routePlanWriter.SaveReadyPlanAsync(
            request.TripId,
            caller.AccountId,
            routingProvider.Name,
            route.Geometry,
            corridorMeters,
            route.DistanceMeters,
            route.DurationSeconds,
            JsonSerializer.Serialize(waypoints, JsonOptions),
            JsonSerializer.Serialize(route.Legs, JsonOptions),
            estimate,
            cancellationToken);

        await tripEventWriter.AppendAsync(
            caller.AccountId, request.TripId, null, TripEventTypes.TripRoutePlanned,
            plan.ComputedAt, TripEventSources.Portal, null,
            $"trip-plan:{plan.RoutePlanId:N}", cancellationToken);

        return plan;
    }

    /// <summary>
    /// Toll estimation is a best-effort enrichment of a plan, never a precondition for having one.
    /// It reads account config and runs an <c>ST_DWithin</c> match over the toll catalog, so a
    /// PostGIS hiccup or a catalog read failure used to surface as an unhandled 500 from a ROUTE
    /// PLANNING call - the trip lost its plan because a price lookup failed. It now degrades to a
    /// plan saved with <c>TollStatus = NotComputed</c>: no estimate, and the status says so rather
    /// than implying a zero (spec 11 §18.9's "never silently understated" applied to the failure
    /// path as well as the missing-tariff one).
    /// </summary>
    private async Task<TollEstimateVm> EstimateTollsAsync(
        Guid accountId, TripVm trip, RouteResultVm route, string? vehicleClass, CancellationToken cancellationToken)
    {
        try
        {
            var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);
            return await tollEstimationService.EstimateAsync(
                route.Geometry,
                vehicleClass,
                DateOnly.FromDateTime(trip.PlannedStartAt.UtcDateTime),
                config.TollMatchToleranceMeters,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Toll estimation failed while planning trip {TripId}; saving the plan without an estimate", trip.TripId);
            return new TollEstimateVm(vehicleClass ?? string.Empty, null, null, TollStatuses.NotComputed, []);
        }
    }

    private async Task<RoutePlanVm> FailAsync(
        Guid tripId, Guid accountId, int corridorMeters, string errorCode, string errorMessage, CancellationToken cancellationToken)
    {
        var plan = await routePlanWriter.SaveFailedPlanAsync(
            tripId, accountId, routingProvider.Name, corridorMeters, errorCode, errorMessage, cancellationToken);

        await tripEventWriter.AppendAsync(
            accountId, tripId, null, TripEventTypes.TripRoutePlanFailed,
            plan.ComputedAt, TripEventSources.Portal, $$"""{"errorCode":"{{errorCode}}"}""",
            $"trip-plan-failed:{plan.RoutePlanId:N}", cancellationToken);

        return plan;
    }

    private static List<CoordinateVm> BuildWaypoints(TripVm trip, IReadOnlyCollection<TripStopVm> stops)
    {
        var waypoints = new List<CoordinateVm>(stops.Count + 1)
        {
            new(trip.OriginLatitude, trip.OriginLongitude),
        };

        foreach (var stop in stops.OrderBy(s => s.Sequence))
            waypoints.Add(new CoordinateVm(stop.Latitude, stop.Longitude));

        return waypoints;
    }
}

public sealed class PlanTripRouteValidator : AbstractValidator<PlanTripRouteCommand>
{
    public PlanTripRouteValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.CorridorMeters).InclusiveBetween(100, 5000).When(v => v.CorridorMeters.HasValue);
        RuleFor(v => v.TollVehicleClass).MaximumLength(20);
    }
}
