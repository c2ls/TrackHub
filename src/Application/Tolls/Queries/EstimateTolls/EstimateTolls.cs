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

using Ardalis.GuardClauses;
using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Common;
using TrackHub.TripManagement.Application.Tolls.Services.Interfaces;

namespace TrackHub.TripManagement.Application.Tolls.Queries.EstimateTolls;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>
/// The planner's "what-if": re-runs toll matching over a stored plan for a different vehicle class
/// WITHOUT persisting anything, so an operator can compare classes before committing.
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct EstimateTollsQuery(Guid RoutePlanId, string? TollVehicleClass) : IRequest<TollEstimateVm>;

public sealed class EstimateTollsQueryHandler(
    IRoutePlanReader routePlanReader,
    ITripReader tripReader,
    ITollEstimationService tollEstimationService,
    IAccountFeatureReader accountFeatureReader,
    IUserReader userReader,
    IUser user) : IRequestHandler<EstimateTollsQuery, TollEstimateVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TollEstimateVm> Handle(EstimateTollsQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var plan = await routePlanReader.GetPlanAsync(request.RoutePlanId, caller.AccountId, cancellationToken)
            ?? throw new NotFoundException(request.RoutePlanId.ToString(), nameof(RoutePlanVm));

        // The plan lookup is account-scoped but NOT group-scoped, so on its own a dispatcher
        // holding another group's routePlanId got that route's toll total and its per-station
        // breakdown. Composed from the existing mechanism rather than adding a visibility
        // predicate to IRoutePlanReader: the plan already carries its TripId, and GetTripAsync
        // applies the one Visible() predicate (acceptance 4) and raises NotFoundException for a
        // trip outside the caller's groups — so an unknown plan and an invisible one are
        // indistinguishable, which is the point (non-disclosure).
        var trip = await tripReader.GetTripAsync(plan.TripId, caller.AccountId, TripVisibility.ResolveScopeUserId(user, UserId), cancellationToken);

        var geometry = await routePlanReader.GetPlanGeometryAsync(request.RoutePlanId, caller.AccountId, cancellationToken);
        var config = await accountFeatureReader.GetAccountConfigAsync(caller.AccountId, cancellationToken);

        return await tollEstimationService.EstimateAsync(
            geometry,
            request.TollVehicleClass ?? plan.TollVehicleClass,
            // Priced on the trip's PLANNED START, exactly as PlanTripRouteCommand prices the stored
            // estimate. Pricing this "what-if" on the plan's ComputedAt instead made the planner
            // panel and the persisted figure disagree for the same vehicle class whenever a tariff
            // changed between planning and departure, with nothing on screen saying which date was
            // used. Tariffs are temporal (§6.2), so the date is part of the answer.
            DateOnly.FromDateTime(trip.PlannedStartAt.UtcDateTime),
            config.TollMatchToleranceMeters,
            cancellationToken);
    }
}

public sealed class EstimateTollsValidator : AbstractValidator<EstimateTollsQuery>
{
    public EstimateTollsValidator()
    {
        RuleFor(v => v.RoutePlanId).NotEmpty();
        RuleFor(v => v.TollVehicleClass).MaximumLength(20);
    }
}
