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

namespace TrackHub.TripManagement.Application.Trips.Queries.GetTripDetail;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>Trip, stops, deliveries, assignment, route plan with toll breakdown, POD and shares.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct GetTripDetailQuery(Guid TripId) : IRequest<TripDetailVm>;

public sealed class GetTripDetailQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetTripDetailQuery, TripDetailVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<TripDetailVm> Handle(GetTripDetailQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        return await reader.GetTripDetailAsync(
            request.TripId, caller.AccountId, TripVisibility.ResolveScopeUserId(user, UserId), cancellationToken);
    }
}

public sealed class GetTripDetailValidator : AbstractValidator<GetTripDetailQuery>
{
    public GetTripDetailValidator()
        => RuleFor(v => v.TripId).NotEmpty();
}
