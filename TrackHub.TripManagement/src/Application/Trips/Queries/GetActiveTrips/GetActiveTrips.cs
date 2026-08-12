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

namespace TrackHub.TripManagement.Application.Trips.Queries.GetActiveTrips;

// No [Caching] — scope comes from the caller identity (SVD-09). See GetTripsQuery.
/// <summary>Dispatch-board live feed: the account's <c>InProgress</c> and <c>Paused</c> trips.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Read)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct GetActiveTripsQuery : IRequest<IReadOnlyCollection<TripVm>>;

public sealed class GetActiveTripsQueryHandler(
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<GetActiveTripsQuery, IReadOnlyCollection<TripVm>>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<IReadOnlyCollection<TripVm>> Handle(GetActiveTripsQuery request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        return await reader.GetActiveTripsAsync(
            caller.AccountId, TripVisibility.ResolveScopeUserId(user, UserId), cancellationToken);
    }
}
