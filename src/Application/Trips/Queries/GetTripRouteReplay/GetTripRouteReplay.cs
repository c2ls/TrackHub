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

using Common.Application.Interfaces;
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTripRouteReplay;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>
/// Replays the trip's real track from Telemetry. Telemetry owns the 31-day window and the
/// 10 000-point cap; this clamps to them and reports truncation EXPLICITLY — a silently shortened
/// route would read as a vehicle that stopped moving (acceptance 22).
/// <para>
/// Doubly gated: the module's own key plus Telemetry's <c>gps.positionHistory</c>, because position
/// history is a separately licensed capability.
/// </para>
/// </summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
[RequireFeature(FeatureKeys.GpsPositionHistory)]
public readonly record struct GetTripRouteReplayQuery(Guid TripId, int? MaxPoints) : IRequest<RouteReplayVm>;

public sealed class GetTripRouteReplayQueryHandler(
    ITripReader reader,
    IPositionHistoryClient positionHistoryClient,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripRouteReplayQuery, RouteReplayVm>
{
    private const int DefaultMaxPoints = 2000;
    private const int TelemetryMaxPoints = 10000;
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<RouteReplayVm> Handle(GetTripRouteReplayQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);

        // Group visibility matters especially here: the replay returns the transporter's full
        // Telemetry position history, so a missing predicate handed one group's dispatcher another
        // group's vehicle track.
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);
        var trip = await reader.GetTripAsync(request.TripId, caller.AccountId, scopeUserId, cancellationToken);

        var to = trip.ActualEndAt ?? DateTimeOffset.UtcNow;
        var from = trip.ActualStartAt ?? trip.PlannedStartAt;

        // The full window is passed through DELIBERATELY. PositionHistoryClient owns the 31-day
        // clamp and is the only thing that can report it, via the `truncated` flag. Pre-clamping
        // here made `windowClamped` permanently unreachable, so a 45-day trip silently lost its
        // first 14 days and still answered truncated = false — precisely the silent cut §7.5 and
        // acceptance 22 forbid. Do not re-add a clamp on this side.
        var maxPoints = Math.Clamp(request.MaxPoints ?? DefaultMaxPoints, 1, TelemetryMaxPoints);

        return await positionHistoryClient.GetRangeAsync(
            caller.AccountId, trip.TransporterId, from, to, maxPoints, cancellationToken);
    }
}

public sealed class GetTripRouteReplayValidator : AbstractValidator<GetTripRouteReplayQuery>
{
    public GetTripRouteReplayValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.MaxPoints).InclusiveBetween(1, 10000).When(v => v.MaxPoints.HasValue);
    }
}
