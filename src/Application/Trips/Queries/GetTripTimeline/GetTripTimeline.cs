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

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTripTimeline;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>Paged <c>TripEvent</c> history: manual overrides and detections in one log.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct GetTripTimelineQuery(Guid TripId, int? Skip, int? Take) : IRequest<TripTimelinePageVm>;

public sealed class GetTripTimelineQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripTimelineQuery, TripTimelinePageVm>
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripTimelinePageVm> Handle(GetTripTimelineQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var skip = Math.Max(request.Skip ?? 0, 0);
        var take = Math.Clamp(request.Take ?? DefaultPageSize, 1, MaxPageSize);

        return await reader.GetTimelineAsync(
            request.TripId, caller.AccountId, TripVisibility.ResolveScopeUserId(user, UserId), skip, take, cancellationToken);
    }
}

public sealed class GetTripTimelineValidator : AbstractValidator<GetTripTimelineQuery>
{
    public GetTripTimelineValidator()
    {
        RuleFor(v => v.TripId).NotEmpty();
        RuleFor(v => v.Skip).GreaterThanOrEqualTo(0).When(v => v.Skip.HasValue);
        RuleFor(v => v.Take).InclusiveBetween(1, 200).When(v => v.Take.HasValue);
    }
}
